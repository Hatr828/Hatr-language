using Best_language.src.middle;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace Best_language.src.front
{
    public partial class Parser
    {
        private List<Token> _tokens = new();
        public static TypeTable typeTable = new();
        private ParserScopes _scopes = new();
        private int _idx;

        public ProgramNode GetAstNodes(List<Token> tokens)
        {
            _tokens = tokens;
            _scopes = new();
            _idx = 0;

            List<AstNode> res = new List<AstNode>();

            _scopes.AddScope();
            while (_idx < _tokens.Count)
            {
                res.Add(GetAstNode());
            }
            _scopes.RemoveScope();
#if DEBUG
            if (!_scopes.IsEmpty()) throw new ArgumentException();
#endif

            return new(res);
        }
        private AstNode GetAstNode()
        {

            while (_idx < _tokens.Count)
            {
                switch (_tokens[_idx].kind)
                {
                    case TokenKind.Struct:
                    case TokenKind.Enum:
                    case TokenKind.Signed:
                    case TokenKind.UnSigned:
                    case TokenKind.Bool:
                    case TokenKind.Long:
                    case TokenKind.Char:
                    case TokenKind.Void:
                    case TokenKind.Short:
                    case TokenKind.Int:
                    case TokenKind.Double:
                    case TokenKind.Float:
                        {
                            var type = ParseType();
                            return ParseSymbol(type);
                        }
                    case TokenKind.Typedef:
                        {
                            return ParseTypedef();          
                        }
                    case TokenKind.Indent when _scopes.TryGetVariable(_tokens[_idx].val!, out var kind) && kind is NameKind.TypeDef:
                        {
                            _scopes.TryGetTypedef(_tokens[_idx++].val!, out var type);
                            return ParseSymbol(type);
                        }
                    case TokenKind.Switch:
                        {
                            return ParseSwitch();
                        }
                    case TokenKind.Do:
                        {
                            _idx++;
                            List<AstNode> block = new();

                            _scopes.AddScope();
                            GetBlockCode(block);
                            _scopes.RemoveScope();

                            Requires(TokenKind.While, TokenKind.RightParen);

                            Expression ex = GetExpression();

                            Requires(TokenKind.LeftParen, TokenKind.SemiColon);

                            return new WhileDeclaration(ex, block, true);
                        }
                    case TokenKind.While:
                        {
                            _idx++;
                            Requires(TokenKind.RightParen);

                            Expression ex = GetExpression();
                            List<AstNode> block = new();

                            Requires(TokenKind.LeftParen);

                            _scopes.AddScope();
                            GetBlockCode(block);
                            _scopes.RemoveScope();

                            return new WhileDeclaration(ex, block);
                        }
                    case TokenKind.For:
                        {

                            _idx++;
                            Requires(TokenKind.RightParen);

                            AstNode? initializer;
                            Expression? ex, step;

                            if (_tokens[_idx].kind is TokenKind.SemiColon) { initializer = null; _idx++; }
                            else initializer = GetAstNode();

                            if (_tokens[_idx].kind is TokenKind.SemiColon) { ex = null; }
                            else ex = GetExpression();

                            Requires(TokenKind.SemiColon);

                            if (_tokens[_idx].kind is TokenKind.LeftParen) { step = null; }
                            else step = GetExpression();

                            List<AstNode> block = new();

                            Requires(TokenKind.LeftParen);

                            _scopes.AddScope();
                            GetBlockCode(block);
                            _scopes.RemoveScope();

                            return new ForDeclaration(initializer, ex, step, block);
                        }
                    case TokenKind.ElseIf:
                        {
                            _idx++;
                            Requires(TokenKind.RightParen);

                            Expression ex = GetExpression();
                            List<AstNode> block = new();

                            Requires(TokenKind.LeftParen);

                            _scopes.AddScope();
                            GetBlockCode(block);
                            _scopes.RemoveScope();

                            return new ElseIfDeclaration(ex, block);
                        }
                    case TokenKind.If:
                        {
                            return ParseIf();
                        }
                    case TokenKind.Goto:
                        {
                            _idx++;
                            Requires(TokenKind.Indent, TokenKind.SemiColon);
                            return new GotoStatement(_tokens[_idx - 2].val!);
                        }
                    case TokenKind.Indent when _tokens[_idx + 1].kind is TokenKind.Dot or TokenKind.Arrow:
                        {
                            string target = _tokens[_idx++].val!;
                            bool isArrow = CheckKind(TokenKind.Arrow);
                            string name = _tokens[++_idx].val!;
                            _idx++;
                            Operation? op = null;

                            int tmp2 = -1;

                            if (CheckKind(TokenKind.Equals)) tmp2 = _idx++;
                            else if (IsOperatorEquals()) op = GetOperatorEquals();

                            if (op is not null || tmp2 != -1)
                            {
                                var temp = new MemberAssignment(isArrow ? new UnaryExpression(Operation.Dereference, new VariableExpression(target)) 
                                    : new VariableExpression(target), name, null, GetExpression());

                                Requires(TokenKind.SemiColon);

                                return temp;
                            }
                            else
                            {
                                var tmp1 = GetExpression();
                                Requires(TokenKind.SemiColon);

                                return new ExpressionStatement(tmp1);
                            }
                        }
                    case TokenKind.Indent when _tokens[_idx + 1].kind is TokenKind.Colon:
                        {
                            _idx += 2;
                            return new GotoDeclaration(_tokens[_idx - 2].val!);
                        }
                    case TokenKind.Indent or TokenKind.Multiply:
                        {
                            Expression ex;
                            Expression? idx = null;
                            Expression left;
                            Operation? op = null;

                            left = GetExpression();

                            if (Consume(TokenKind.RightBracket))
                            {
                                idx = GetExpression();
                                Requires(TokenKind.LeftBracket);
                            }

                            if (Consume(TokenKind.Equals)) { }
                            else if (IsOperatorEquals()) op = GetOperatorEquals();
                            else
                            {
                                Requires(TokenKind.SemiColon);
                                return new ExpressionStatement(left);
                            }

                            ex = GetExpression();
                            Requires(TokenKind.SemiColon);

                            if (idx is not null) return new ArrayAssignment(left, idx, op, ex);
                            else return new AssignmentStatement(left, op, ex);
                        }
                    case TokenKind.Return:
                        {
                            _idx++;
                            if (Consume(TokenKind.SemiColon))
                            {
                                return new ReturnStatement();
                            }

                            var ex = GetExpression();
                            Requires(TokenKind.SemiColon);

                            return new ReturnStatement(ex);
                        }
                    case TokenKind.Break:
                        {
                            _idx++;
                            Requires(TokenKind.SemiColon);
                            
                            return new BreakStatement();
                        }
                    case TokenKind.Continue:
                        {
                            _idx++;
                            Requires(TokenKind.SemiColon);

                            return new ContinueStatement();
                        }
                    case var kind when IsExpression(kind):
                        var tmp = GetExpression();
                        Requires(TokenKind.SemiColon);

                        return new ExpressionStatement(tmp);

                    default: throw new Exception("Error in Parser, unknown token: " + _tokens[_idx].kind);
                }
            }

            throw new Exception("Parser error, unknown token");
        }

        private Expression GetExpression(int priority = 0)
        {
            Expression left;

            switch (_tokens[_idx].kind)
            {
                case TokenKind.Plus:
                case TokenKind.Minus:
                case TokenKind.Multiply:
                case TokenKind.Not:
                case TokenKind.BitNot:
                case TokenKind.BitAnd:
                case TokenKind.PlusPlus:
                case TokenKind.MinusMinus:
                    {
                        Operation op = GetOperatorAndPriorityUnary().Item1;
                        var right = GetExpression(100);
                        left = new UnaryExpression(op, right);

                        if (right is NumberExpression num) left = CombineUnary(op, num);
                        else if (right is DoubleExpression fp) left = CombineUnary(op, fp);

                        break;
                    }
                case TokenKind.RightParen:
                    {
                        _idx++;
                        left = GetExpression();

                        Requires(TokenKind.LeftParen);
                        break;
                    }
                case TokenKind.String:
                    {
                        string @string = _tokens[_idx++].val!;

                        List<Expression> expressions = new();

                        foreach (char s in @string)
                        {
                            expressions.Add(new NumberExpression((byte)s, typeTable.Char));
                        }

                        expressions.Add(new NumberExpression((byte)'\0', typeTable.Char));

                        return new ArrayExpression(expressions);
                    }
                case TokenKind.RightBrace:
                    {
                        _idx++;

                        List<Expression> expressions = new();

                        while (!Consume(TokenKind.LeftBrace))
                        {
                            expressions.Add(GetExpression());

                            if (Consume(TokenKind.Comma)) { }
                            else if (!CheckKind(TokenKind.LeftBrace)) throw new ArgumentException("Expected left brace or comma");
                        }

                        return new ArrayExpression(expressions);
                    }
                case TokenKind.Number:
                    {
                        if(_tokens[_idx + 1].kind is TokenKind.Dot)
                        {
                            var @double = _tokens[_idx].val! + "." + _tokens[_idx + 2].val!;
                            if (_tokens[_idx + 3].kind is TokenKind.F)
                            {
                                left = new DoubleExpression(float.Parse(@double), typeTable.Float);
                                _idx++;
                            }
                            else left = new DoubleExpression(double.Parse(@double), typeTable.Double);
                            _idx += 3;
                        }
                        else left = ParseNumber();
                        break;
                    }
                case TokenKind.Indent when _tokens[_idx + 1].kind is TokenKind.RightParen:
                    {
                        string name = _tokens[_idx++].val!;
                        _idx++;
                        List<Expression> @params = new();

                        while (!Consume(TokenKind.LeftParen))
                        {
                            @params.Add(GetExpression());
                            if (!CheckKind(TokenKind.LeftParen)) Requires(TokenKind.Comma);
                        }
                        left = new FuncExpression(name, @params);
                        break;
                    }
                case TokenKind.Indent when _tokens[_idx + 1].kind is TokenKind.Dot or TokenKind.Arrow:
                    {
                        string target = _tokens[_idx++].val!;
                        bool isArrow = CheckKind(TokenKind.Arrow);
                        string name = _tokens[++_idx].val!;
                        _idx++;

                        left = new MemberAccessExpression(isArrow ? new UnaryExpression(Operation.Dereference, new VariableExpression(target))
                                    : new VariableExpression(target), name);
                        break;
                    }
                case TokenKind.Indent when _tokens[_idx + 1].kind is TokenKind.RightBracket:
                    {
                        string name = _tokens[_idx++].val!;
                        _idx++;

                        Expression idx = GetExpression();

                        Requires(TokenKind.LeftBracket);

                        left = new ArrayAccessExpression(name, idx);
                        break;
                    }
                case TokenKind.Indent:
                    {
                        string name = _tokens[_idx++].val!;

                        if (_scopes.TryGetEnumMember(name, out var val))
                        {
                            left = new NumberExpression(val.Item1, val.Item2);
                            break;
                        }
                        left = new VariableExpression(name);
                        break;
                    }
                case TokenKind.True:
                case TokenKind.False:
                    {
                        left = new BoolExpression(_tokens[_idx++].kind is TokenKind.True ? true : false, typeTable.Bool);
                        break;
                    }
                default:
                    throw new Exception("Error in Parser, unknown token: " + _tokens[_idx].kind);
            }

            while (_idx < _tokens.Count && _tokens[_idx].kind is not TokenKind.SemiColon and not TokenKind.LeftParen and not TokenKind.Colon and not TokenKind.Comma and not TokenKind.LeftBrace and not TokenKind.LeftBracket and not TokenKind.Equals)
            {
                if (_tokens[_idx].kind is TokenKind.PlusPlus or TokenKind.MinusMinus)
                {
                    var op2 = _tokens[_idx++].kind switch
                    {
                        TokenKind.PlusPlus => Operation.PlusPlusRight,
                        TokenKind.MinusMinus => Operation.MinusMinusRight,
                        _ => throw new Exception()
                    };

                    left = new UnaryExpression(op2, left);
                    continue;
                }

                (var op, int opPriority) = GetOperatorAndPriority();
                if (priority > opPriority) break;

                _idx++;

                var right = GetExpression(++opPriority);
                var tmp = new BinaryExpression(left, op, right);

                if (left is NumberExpression && right is NumberExpression) left = CombineBinary(op, (NumberExpression)left, (NumberExpression)right);
                else if (left is DoubleExpression && right is NumberExpression) left = CombineBinary(op, (DoubleExpression)left, (NumberExpression)right);
                else if (left is NumberExpression && right is DoubleExpression) left = CombineBinary(op, (NumberExpression)left, (DoubleExpression)right);
                else if (left is DoubleExpression && right is DoubleExpression) left = CombineBinary(op, (DoubleExpression)left, (DoubleExpression)right);
                else left = tmp;
            }

            return left;
        }

        private NumberExpression ParseNumber()
        {
            NumberExpression res;
            switch (_tokens[_idx + 1].kind)
            {
                case TokenKind.F: 
                    throw new ArgumentException("cannot have XF, should be: X.XF");
                case TokenKind.U:
                    res = new NumberExpression(uint.Parse(_tokens[_idx++].val!), typeTable.UInt32);
                    break;
                case TokenKind.L:
                    res = new NumberExpression(unchecked((uint)int.Parse(_tokens[_idx++].val!)), typeTable.Int32);
                    break;
                case TokenKind.UL:
                    res = new NumberExpression(uint.Parse(_tokens[_idx++].val!), typeTable.UInt32);
                    break;
                case TokenKind.LU:
                    res = new NumberExpression(uint.Parse(_tokens[_idx++].val!), typeTable.UInt32);
                    break;
                case TokenKind.LL:
                    res = new NumberExpression(unchecked((ulong)long.Parse(_tokens[_idx++].val!)), typeTable.Int64);
                    break;
                case TokenKind.LLU:
                    res = new NumberExpression(ulong.Parse(_tokens[_idx++].val!), typeTable.UInt64);
                    break;
                case TokenKind.ULL:
                    res = new NumberExpression(ulong.Parse(_tokens[_idx++].val!), typeTable.UInt64);
                    break;
                default:
                    res = new NumberExpression(unchecked((uint)int.Parse(_tokens[_idx].val!)), typeTable.Int32);
                    break;
            }
            _idx++;
            return res;
        }

        private Statement ParseSymbol(TypeSymbol type)
        {
            EnumTypeSymbol? enumType = null;
            StructDeclaration? @struct = null;

            if (CheckKind(TokenKind.Indent, TokenKind.RightParen)) return ParseFunction(type);
            else if (type == typeTable.Enum && (CheckKind(TokenKind.Indent, TokenKind.RightBrace) || CheckKind(TokenKind.RightBrace)))
            {
                enumType = ParseEnum();
                type = enumType.EnumType;
            }
            else if (type == typeTable.Struct && (CheckKind(TokenKind.Indent, TokenKind.RightBrace) || CheckKind(TokenKind.RightBrace)))
            {
                @struct = ParseStruct();
            }
            else if (CheckKind(TokenKind.Indent) && type == typeTable.Enum)
            {
                type = new EnumTypeSymbol(_tokens[_idx++].val!, null!);
            }
            else if (CheckKind(TokenKind.Indent) && type == typeTable.Struct)
            {
                type = new StructTypeSymbol(_tokens[_idx++].val!, -1, 0, null!);
            }

            var variables = ParseVariables(type);

            if (enumType is not null) return new EnumDeclaration(enumType, variables.Variables);
            else if (@struct is not null)
            {
                @struct.Variables = variables.Variables;
                return @struct;
            } 

            return variables;
        }

        private VariableDeclarations ParseVariables(TypeSymbol type = null!) // needs refactoring
        {
            VariableDeclarations variables = new();
            bool isType = type is not null;


            while (!Consume(TokenKind.SemiColon))
            {
                if (!isType)
                {
                    if (Consume(TokenKind.LeftParen)) return variables;
                    type = ParseType();
                    if (CheckKind(TokenKind.Indent) && type == typeTable.Enum)
                    {
                        type = new EnumTypeSymbol(_tokens[_idx++].val!, null!);
                    }
                    else if (CheckKind(TokenKind.Indent) && type == typeTable.Struct)
                    {
                        type = new StructTypeSymbol(_tokens[_idx++].val!, -1, 0, null!);
                    }
                }

                string name;
                TypeSymbol? subType = null;
                bool unknownLength = false;
                Expression? ex = null;

                if (Consume(TokenKind.Multiply)) // pointer
                {
                    name = Requires(true, TokenKind.Indent);
                    _scopes.TryAddVariable(name, NameKind.Variable);

                    if (Consume(TokenKind.RightBracket))
                    {
                        if (Consume(TokenKind.LeftBracket)) unknownLength = true;
                        else
                        {
                            NumberExpression num = (NumberExpression)GetExpression();
                            Requires(TokenKind.LeftBracket);
                            subType = new ArrayTypeSymbol(name, (long)num.Value, type!);
                        }
                    }

                    if (Consume(TokenKind.Equals)) ex = GetExpression();

                    if (unknownLength)
                    {
                        if (ex is null) throw new ArgumentException("Cannot declare array");
                        if (ex is not ArrayExpression arr) throw new ArgumentException("Expected array expression");
                        subType = new ArrayTypeSymbol(name, (long)arr.Right.Count, type!);
                    }

                    variables.Variables.Add(new VariableDeclaration(name, typeTable.GetPointer(subType is not null ? subType : type!), ex));

                    if (Consume(TokenKind.SemiColon)) return variables;
                    else if (Consume(TokenKind.Comma)) continue;
                    else if (!isType && Consume(TokenKind.LeftParen)) return variables;
                    else throw new ArgumentException("requires semicolon or comma");
                }

                name = Requires(true, TokenKind.Indent);
                _scopes.TryAddVariable(name, NameKind.Variable);

                if (Consume(TokenKind.RightBracket))
                {
                    if (Consume(TokenKind.LeftBracket)) unknownLength = true;
                    else
                    {
                        NumberExpression num = (NumberExpression)GetExpression();
                        Requires(TokenKind.LeftBracket);
                        subType = new ArrayTypeSymbol(name, (long)num.Value, type!);
                    }
                }

                if (Consume(TokenKind.Equals)) ex = GetExpression();

                if (unknownLength)
                {
                    if (ex is null) throw new ArgumentException("Cannot declare array");
                    if (ex is not ArrayExpression arr) throw new ArgumentException("Expected array expression");
                    subType = new ArrayTypeSymbol(name, (long)arr.Right.Count, type!);
                }

                variables.Variables.Add(new VariableDeclaration(name, subType is not null ? subType : type!, ex));

                if (Consume(TokenKind.SemiColon)) return variables;

                if (!isType && Consume(TokenKind.LeftParen)) return variables;

                Requires(TokenKind.Comma);
            }

            return variables;
        }

        private EnumTypeSymbol ParseEnum() // needs refactoring
        {
            EnumTypeSymbol @enum;

            if (CheckKind(TokenKind.Indent))
            {
                string enumName = _tokens[_idx++].val!;
                @enum = new(enumName, typeTable.Int32);

                _scopes.TryAddEnum(enumName, @enum);
            }
            else @enum = new(null, typeTable.Int32);

            Requires(TokenKind.RightBrace);

            ulong prevIndex = 0;
            while(!CheckKind(TokenKind.LeftBrace))
            {
                Requires(TokenKind.Indent);
                string name = _tokens[_idx - 1].val!;
                if (!CheckKind(TokenKind.Equals))
                {
                    if (!CheckKind(TokenKind.LeftBrace))
                        Requires(TokenKind.Comma);
                    @enum.Members.Add(name, prevIndex);

                    if (!_scopes.TryAddEnumMember(name, (prevIndex++, typeTable.Int32))) throw new ArgumentException($"EnumMember {name} already exist");

                    if (!_scopes.TryAddVariable(name, NameKind.Enum)) throw new ArgumentException($"EnumMember {name} cannot be declared");
                }
                else
                {
                    _idx++;
                    var tmp = GetExpression();
                    if (tmp is not NumberExpression ex) throw new ArgumentException("Expected integer constant");
                    @enum.Members.Add(name, ex.Value);

                    prevIndex = ex.Value;

                    if (!_scopes.TryAddEnumMember(name, (ex.Value, typeTable.Int32))) throw new ArgumentException($"EnumMember {name} already exist");

                    if (!_scopes.TryAddVariable(name, NameKind.Enum)) throw new ArgumentException($"EnumMember {name} cannot be declared");

                    if (!CheckKind(TokenKind.LeftBrace))
                        Requires(TokenKind.Comma);
                }
            }
            _idx++;

            return @enum;
        }

        private StructDeclaration ParseStruct() // needs refactoring
        {
            string? structName = null;

            if (CheckKind(TokenKind.Indent))
            {
                structName = _tokens[_idx++].val!;
            }

            Requires(TokenKind.RightBrace);

            List<TypeSymbol> types = new();
            List<string> names = new();

            TypeSymbol type = ParseType();

            while (!CheckKind(TokenKind.LeftBrace))
            {

                if (CheckKind(TokenKind.Multiply)) // pointer
                {
                    _idx++;
                    Requires(TokenKind.Indent);
                    names.Add(_tokens[_idx - 1].val!);

                    type = typeTable.GetPointer(type);
                    types.Add(type);

                    if (CheckKind(TokenKind.Comma))
                    {
                        _idx++;
                        continue;
                    }
                    else if (CheckKind(TokenKind.SemiColon))
                    {
                        _idx++;
                        if(!CheckKind(TokenKind.LeftBrace)) type = ParseType();

                        continue;
                    }
                    else throw new ArgumentException("requires semiColon or comma");
                }

                Requires(TokenKind.Indent);
                if (CheckKind(TokenKind.SemiColon))
                {
                    _idx++;
                    names.Add(_tokens[_idx - 2].val!);
                    types.Add(type);

                    if (!CheckKind(TokenKind.LeftBrace)) type = ParseType();

                    continue;
                }
                else if (CheckKind(TokenKind.Comma))
                {
                    _idx++;
                    names.Add(_tokens[_idx - 2].val!);
                    types.Add(type);
                    continue;
                }

                throw new ArgumentException("Unknown syntax for struct");
            }
            _idx++;

            return new StructDeclaration(new(structName, GetSizeForStruct(types, out var alignment), alignment, types), names);
        }                                     

        private Statement ParseFunction(TypeSymbol type) // needs refactoring
        {
            if (Consume(TokenKind.Indent, TokenKind.RightParen)) // function
            {
                string name = _tokens[_idx - 2].val!;

                var @params = ParseVariables();
                List<TypeSymbol> types = new();

                foreach (var param in @params.Variables) types.Add(param.Type);

                List<AstNode> block = new();

                _scopes.AddScope();
                GetBlockCode(block);
                _scopes.RemoveScope();

                return new FuncDeclaration(new(type, types), name, @params.Variables, block);
            }

            throw new ArgumentException("Not function, expected function");
        }

        private IfDeclaration ParseIf()
        {

            _idx++;
            Requires(TokenKind.RightParen);

            Expression ex = GetExpression();
            List<AstNode> block = new();

            Requires(TokenKind.LeftParen);

            _scopes.AddScope();
            GetBlockCode(block);
            _scopes.RemoveScope();

            if (_idx >= _tokens.Count) return new IfDeclaration(ex, block);

            var @if = new IfDeclaration(ex, block);

            while (_tokens.Count > _idx && _tokens[_idx].kind is TokenKind.ElseIf)
            {
                @if.ElseIfs ??= new();
                @if.ElseIfs.Add((ElseIfDeclaration)GetAstNode());
            }

            if (_idx >= _tokens.Count || _tokens[_idx].kind is not TokenKind.Else) return @if;
            @if.Else = new();
            _idx++;

            _scopes.AddScope();
            GetBlockCode(@if.Else.CodeBlock);
            _scopes.RemoveScope();

            return @if;
        }

        private SwitchDeclaration ParseSwitch() // needs refactoring
        {
            _scopes.AddScope();

            _idx++;
            Requires(TokenKind.RightParen);

            Expression ex = GetExpression();
            List<CaseDeclaration> cases = new();

            Requires(TokenKind.LeftParen, TokenKind.RightBrace);

            while (_tokens.Count > _idx && _tokens[_idx].kind != TokenKind.LeftBrace)
            {
                List<AstNode> block = new();
                Expression? caseEx = null;

                if (_tokens.Count > _idx && _tokens[_idx].kind != TokenKind.Default)
                {
                    Requires(TokenKind.Case);
                    caseEx = GetExpression();
                }
                else Requires(TokenKind.Default);

                Requires(TokenKind.Colon);

                bool wasBrace = false;

                if (_tokens.Count > _idx && _tokens[_idx].kind is TokenKind.RightBrace)
                {
                    _scopes.AddScope();
                    wasBrace = true; _idx++;
                }

                while (_tokens.Count > _idx && _tokens[_idx].kind is not TokenKind.Case and not TokenKind.Default and not TokenKind.LeftBrace)
                {
                    block.Add(GetAstNode()!);
                }

                if (_tokens.Count > _idx && wasBrace && _tokens[_idx].kind is TokenKind.LeftBrace)
                {
                    _scopes.RemoveScope();
                    _idx++;
                }

                if (caseEx is null) cases.Add(new(caseEx, block, true));
                else cases.Add(new(caseEx, block));
            }

            Requires(TokenKind.LeftBrace);

            _scopes.RemoveScope();

            return new SwitchDeclaration(ex, cases);
        }

        private AstNode ParseTypedef() // needs refactoring
        {
            _idx++;
            TypeSymbol type;
            bool declareEnum = false;


            string typeName = _tokens[_idx].val!;

            if (typeName is not null)
            {
                if (!_scopes.TryGetTypedef(typeName, out type)) throw new ArgumentException($"Unknown type: {typeName}");
                _idx++;
            }
            else if (CheckKind(TokenKind.Enum, TokenKind.Indent, TokenKind.Indent))
            {
                _idx++;
                string enumName2 = _tokens[_idx++].val!;
                if (!_scopes.TryGetEnum(enumName2, out var enumType)) throw new ArgumentException($"Unknown type: {typeName}");
                type = enumType;
            }
            else if (CheckKind(TokenKind.Enum))
            {
                _idx++;
                declareEnum = true;
                type = ParseEnum();
            }
            else type = ParseType();

            string? enumName = null;


            while (!CheckKind(TokenKind.SemiColon))
            {
                Requires(TokenKind.Indent);

                string name = _tokens[_idx - 1].val!;

                if (enumName is null) enumName = name;

                if (!_scopes.TryAddVariable(name, NameKind.TypeDef)) throw new ArgumentException($"Cannot define {name} because variable with the same name in the same scope already exist");

                if (type is TypedefTypeSymbol tmp2)
                    _scopes.TryAddTypedef(name, tmp2.ElementType);
                else
                    _scopes.TryAddTypedef(name, type);

                if (CheckKind(TokenKind.Comma)) _idx++;
            }
            _idx++;

            if (declareEnum)
            {
                if (type.Name is null)
                    type.Name = enumName!;
                return new EnumDeclaration((EnumTypeSymbol)type, new());
            }
            else return new EmptyNode();
        }
    }
}

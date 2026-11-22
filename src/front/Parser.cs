using System.Diagnostics;
using System.Xml.Linq;

namespace Best_language.src.front
{
    public partial class Parser
    {
        private List<Token> _tokens = new();
        public static TypeTable _typeTable = new();
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
                            return ParseVariableOrFunctionOrEnum(type);
                        }
                    case TokenKind.Typedef:
                        {
                            return ParseTypedef();          
                        }
                    case TokenKind.Indent when _scopes.TryGetVariable(_tokens[_idx].val!, out var kind):
                        {
                            _idx++;
                            _scopes.TryGetTypedef(_tokens[_idx - 1].val!, out var type);
                            return ParseVariableOrFunctionOrEnum(type);
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
                    case TokenKind.Indent when _tokens[_idx + 1].kind is TokenKind.Colon:
                        {
                            _idx += 2;
                            return new GotoDeclaration(_tokens[_idx - 2].val!);
                        }
                    case TokenKind.Indent:
                        {
                            string name = _tokens[_idx].val!;

                            if (_tokens[_idx + 1].kind is TokenKind.Equals)
                            {
                                _idx += 2;
                                var temp = new AssignmentStatement(name, null, GetExpression());

                                Requires(TokenKind.SemiColon);

                                return temp;
                            }
                            else if (IsOperatorEquals())
                            {
                                var op = GetOperatorEquals();
                                var temp2 = new AssignmentStatement(name, op, GetExpression());

                                Requires(TokenKind.SemiColon);
                                return temp2;
                            }
                            else
                            {
                                var tmp1 = GetExpression();
                                Requires(TokenKind.SemiColon);

                                return new ExpressionStatement(tmp1);
                            } 
                        }
                    case TokenKind.Return:
                        {
                            if (_tokens[_idx].kind is TokenKind.SemiColon)
                            {
                                _idx += 2;
                                return new ReturnStatement();
                            }

                            _idx++;

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
                case TokenKind.Number:
                    {
                        if(_tokens[_idx + 1].kind is TokenKind.Dot)
                        {
                            var @double = _tokens[_idx].val! + "." + _tokens[_idx + 2].val!;
                            if (_tokens[_idx + 3].kind is TokenKind.F)
                            {
                                left = new DoubleExpression(float.Parse(@double), _typeTable.Float);
                                _idx++;
                            }
                            else left = new DoubleExpression(double.Parse(@double), _typeTable.Double);
                            _idx += 3;
                        }
                        else left = ParseNumber();
                        break;
                    }
                case TokenKind.Indent:
                    {
                        left = new VariableExpression(_tokens[_idx++].val!);
                        break;
                    }
                case TokenKind.True:
                case TokenKind.False:
                    {
                        left = new BoolExpression(_tokens[_idx++].kind is TokenKind.True ? true : false, _typeTable.Bool);
                        break;
                    }
                default:
                    throw new Exception("Error in Parser, unknown token: " + _tokens[_idx].kind);
            }

            while (_idx < _tokens.Count && _tokens[_idx].kind is not TokenKind.SemiColon and not TokenKind.LeftParen and not TokenKind.Colon and not TokenKind.Comma and not TokenKind.LeftBrace)
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
                    res = new NumberExpression(uint.Parse(_tokens[_idx++].val!), _typeTable.UInt32);
                    break;
                case TokenKind.L:
                    res = new NumberExpression(unchecked((uint)int.Parse(_tokens[_idx++].val!)), _typeTable.Int32);
                    break;
                case TokenKind.UL:
                    res = new NumberExpression(uint.Parse(_tokens[_idx++].val!), _typeTable.UInt32);
                    break;
                case TokenKind.LU:
                    res = new NumberExpression(uint.Parse(_tokens[_idx++].val!), _typeTable.UInt32);
                    break;
                case TokenKind.LL:
                    res = new NumberExpression(unchecked((ulong)long.Parse(_tokens[_idx++].val!)), _typeTable.Int64);
                    break;
                case TokenKind.LLU:
                    res = new NumberExpression(ulong.Parse(_tokens[_idx++].val!), _typeTable.UInt64);
                    break;
                case TokenKind.ULL:
                    res = new NumberExpression(ulong.Parse(_tokens[_idx++].val!), _typeTable.UInt64);
                    break;
                default:
                    res = new NumberExpression(unchecked((uint)int.Parse(_tokens[_idx].val!)), _typeTable.Int32);
                    break;
            }
            _idx++;
            return res;
        }

        private Statement ParseVariableOrFunctionOrEnum(TypeSymbol type)
        {
            EnumTypeSymbol? enumType = null;

            if (CheckKind(TokenKind.Indent, TokenKind.RightParen)) return ParseFunction(type);
            if (CheckKind(TokenKind.Indent, TokenKind.RightBrace) || CheckKind(TokenKind.RightBrace))
            {
                
                enumType = ParseEnum();
                type = enumType.EnumType;
            }
            if (CheckKind(TokenKind.Indent) && type == _typeTable.Enum)
            {
                type = new EnumTypeSymbol(_tokens[_idx].val!, null!);
                _idx++;
            }

            var variables = ParseVariables(type);

            if (enumType is not null)
            {
                return new EnumDeclaration(enumType, variables.Variables);
            }

            return variables;
        }

        private VariableDeclarations ParseVariables(TypeSymbol type)
        {
            VariableDeclarations variables = new();

            while (!CheckKind(TokenKind.SemiColon))
            {
                if (CheckKind(TokenKind.Multiply)) // pointer
                {
                    _idx++;
                    Requires(TokenKind.Indent);

                    if (CheckKind(TokenKind.Equals)) // pointer with initialization
                    {
                        _idx++;
                        _scopes.TryAddVariable(_tokens[_idx - 2].val!, NameKind.Variable);
                        variables.Variables.Add(new VariableDeclaration(_tokens[_idx - 2].val!, _typeTable.GetPointer(type), GetExpression()));
                    }
                    else
                    {
                        _scopes.TryAddVariable(_tokens[_idx - 1].val!, NameKind.Variable);
                        variables.Variables.Add(new VariableDeclaration(_tokens[_idx - 1].val!, _typeTable.GetPointer(type)));
                    }

                    if (CheckKind(TokenKind.SemiColon))
                    {
                        _idx++;
                        return variables;
                    }
                    else if (CheckKind(TokenKind.Comma))
                    {
                        _idx++;
                        continue;
                    }
                    else throw new ArgumentException("requires semiColon or comma");
                }

                Requires(TokenKind.Indent);
                if (CheckKind(TokenKind.Equals)) // variable
                {
                    _idx++;
                    _scopes.TryAddVariable(_tokens[_idx - 2].val!, NameKind.Variable);
                    variables.Variables.Add(new VariableDeclaration(_tokens[_idx - 2].val!, type, GetExpression()));

                    if (CheckKind(TokenKind.SemiColon))
                    {
                        _idx++;
                        return variables;
                    }
                    else if (CheckKind(TokenKind.Comma))
                    {
                        _idx++;
                        continue;
                    }
                    else throw new ArgumentException("requires semiColon or comma");
                }
                else if (CheckKind(TokenKind.SemiColon)) // variable without initialization 
                {
                    _idx++;
                    _scopes.TryAddVariable(_tokens[_idx - 2].val!, NameKind.Variable);
                    variables.Variables.Add(new VariableDeclaration(_tokens[_idx - 2].val!, type));
                    return variables;
                }
                else
                {
                    Requires(TokenKind.Comma);
                    _scopes.TryAddVariable(_tokens[_idx - 2].val!, NameKind.Variable);
                    variables.Variables.Add(new VariableDeclaration(_tokens[_idx - 2].val!, type));
                }

            }

            _idx++;

            return variables;
        }

        private EnumTypeSymbol ParseEnum()
        {
            EnumTypeSymbol @enum;

            if (CheckKind(TokenKind.Indent))
            {
                _idx++;

                string enumName = _tokens[_idx - 1].val!;
                @enum = new(enumName, _typeTable.Int32);

                _scopes.TryAddEnum(enumName, @enum);
            }
            else @enum = new(null, _typeTable.Int32);

            Requires(TokenKind.RightBrace);

            ulong prevIndex = 0;
            while(!CheckKind(TokenKind.LeftBrace))
            {
                if (!CheckKind(TokenKind.Indent)) throw new ArgumentException("Expected indent");
                string name = _tokens[_idx++].val!;
                if (!CheckKind(TokenKind.Equals))
                {
                    if (!CheckKind(TokenKind.LeftBrace))
                        Requires(TokenKind.Comma);
                    @enum.Members.Add(name, prevIndex++);

                }
                else
                {
                    _idx++;
                    var tmp = GetExpression();
                    if (tmp is not NumberExpression ex) throw new ArgumentException("Expected integer constant");
                    @enum.Members.Add(name, ex.Value);

                    prevIndex = ex.Value;

                    if (!CheckKind(TokenKind.LeftBrace))
                        Requires(TokenKind.Comma);
                }
            }
            _idx++;

            return @enum;
        }

        private Statement ParseFunction(TypeSymbol type)
        {
            if (CheckKind(TokenKind.Indent, TokenKind.RightParen)) // function
            {
                _idx += 2;
                string name = _tokens[_idx - 2].val!;

                List<ParamDeclaration> @params = new();

                while (!CheckKind(TokenKind.LeftParen))
                {
                    var typeParam = ParseType();
                    var NameParam = _tokens[_idx++].val!;
                    Expression? initializer = null;

                    if (CheckKind(TokenKind.Equals))
                    {
                        _idx++;

                        initializer = GetExpression();
                    }

                    if (_tokens[_idx].kind != TokenKind.LeftParen)
                        Requires(TokenKind.Comma);

                    @params.Add(new(NameParam, typeParam, initializer));
                }

                _idx++; // LeftParen
                List<AstNode> block = new();

                _scopes.AddScope();
                GetBlockCode(block);
                _scopes.RemoveScope();

                return new FuncDeclaration(type, name, @params, block);
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

        private SwitchDeclaration ParseSwitch()
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
                type.Name = enumName!;
                return new EnumDeclaration((EnumTypeSymbol)type, new());
            }
            else return new EmptyNode();
        }
    }
}

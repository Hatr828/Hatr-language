namespace Best_language.src.front
{
    public partial class Parser
    {
        private List<Token> _tokens = new List<Token>();
        public static TypeTable _typeTable = new();
        private int _idx;

        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
        }

        public ProgramNode GetAstNodes()
        {
            List<AstNode> res = new List<AstNode>();
            _typeTable = new();

            while (_idx < _tokens.Count)
            {
                res.Add(GetAstNode());
            }

            return new(res);
        }
        private AstNode GetAstNode()
        {

            while (_idx < _tokens.Count)
            {
                switch (_tokens[_idx].kind)
                {
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
                            return ParseVariableOrFunction();
                        }
                    case TokenKind.Switch:
                        {
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
                                    wasBrace = true; _idx++;
                                }

                                while (_tokens.Count > _idx && _tokens[_idx].kind is not TokenKind.Case and not TokenKind.Default and not TokenKind.LeftBrace)
                                {
                                    block.Add(GetAstNode());
                                }

                                if (_tokens.Count > _idx && wasBrace && _tokens[_idx].kind is TokenKind.LeftBrace) _idx++;

                                if (caseEx is null) cases.Add(new(caseEx, block, true));
                                else  cases.Add(new(caseEx, block));
                            }

                            Requires(TokenKind.LeftBrace);

                            return new SwitchDeclaration(ex, cases);
                        }
                    case TokenKind.Do:
                        {
                            _idx++;
                            List<AstNode> block = new();

                            GetBlockCode(block);

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

                            GetBlockCode(block);

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

                            GetBlockCode(block);

                            return new ForDeclaration(initializer, ex, step, block);
                        }
                    case TokenKind.ElseIf:
                        {
                            _idx++;
                            Requires(TokenKind.RightParen);

                            Expression ex = GetExpression();
                            List<AstNode> block = new();

                            Requires(TokenKind.LeftParen);

                            GetBlockCode(block);

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
                        left = new UnaryExpression(GetOperatorAndPriorityUnary().Item1, GetExpression(100));
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

            while (_idx < _tokens.Count && _tokens[_idx].kind is not TokenKind.SemiColon and not TokenKind.LeftParen and not TokenKind.Colon)
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

                left = new BinaryExpression(left, op, GetExpression(++opPriority));
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

        private Statement ParseVariableOrFunction()
        {
            var type = ParseType();

            if (_tokens[_idx].kind is TokenKind.Multiply) // pointer
            {
                _idx++;
                Requires(TokenKind.Indent);

                if (_tokens[_idx].kind is TokenKind.Equals) // pointer with initialization
                {
                    _idx++;
                    var temp = new VariableDeclaration(_tokens[_idx - 2].val!, _typeTable.GetPointer(type), GetExpression());

                    Requires(TokenKind.SemiColon);
                    return temp;
                }
                
                return new VariableDeclaration(_tokens[_idx - 2].val!, _typeTable.GetPointer(type));
            }

            Requires(TokenKind.Indent);
            if (_tokens[_idx].kind is TokenKind.Equals) // variable
            {
                _idx++;
                var temp = new VariableDeclaration(_tokens[_idx - 2].val!, type, GetExpression());

                Requires(TokenKind.SemiColon);

                return temp;
            }
            else if (_tokens[_idx].kind is TokenKind.SemiColon) // variable without initialization 
            {
                _idx++;
                return new VariableDeclaration(_tokens[_idx - 2].val!, type);
            }
            else if (_tokens[_idx].kind is TokenKind.RightParen) // function
            {
                _idx++;
                string name = _tokens[_idx - 2].val!;

                List<ParamDeclaration> @params = new();

                while (_tokens[_idx].kind != TokenKind.LeftParen)
                {
                    var typeParam = ParseType();
                    var NameParam = _tokens[_idx++].val!;
                    Expression? initializer = null;

                    if (_tokens[_idx].kind is TokenKind.Equals)
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
                GetBlockCode(block);

                return new FuncDeclaration(type, name, @params, block);
            }

            throw new ArgumentException("Unknown syntax for variable or function.");
        }

        private IfDeclaration ParseIf()
        {
            _idx++;
            Requires(TokenKind.RightParen);

            Expression ex = GetExpression();
            List<AstNode> block = new();

            Requires(TokenKind.LeftParen);

            GetBlockCode(block);

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

            GetBlockCode(@if.Else.CodeBlock);

            return @if;
        }
    }
}

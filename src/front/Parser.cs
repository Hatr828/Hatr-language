namespace Best_language.src.front
{
    class Parser
    {
        private List<Token> _tokens = new List<Token>();
        private TypeTable _typeTable = new();

        private int _idx;

        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
        }

        public ProgramNode GetAstNodes()
        {
            List<AstNode> res = new List<AstNode>();

            while (_idx < _tokens.Count)
            {
                res.Add(GetAstNode());
            }

            return new(res);
        }
        public AstNode GetAstNode()
        {

            while (_idx < _tokens.Count)
            {
                switch (_tokens[_idx].kind)
                {
                    case TokenKind.PlusPlus:
                    case TokenKind.MinusMinus:
                        {
                            Operations op = Operations.MinusMinusLeft;
                            if (_tokens[_idx].kind is TokenKind.PlusPlus) op = Operations.PlusPlusLeft;

                            _idx++;

                            if (_tokens[_idx].kind is not TokenKind.Indent) throw new ArgumentException($"Requires indent after ++ or --, got: {_tokens[_idx].kind}");

                            string name = _tokens[_idx].val!;
                            Requires(TokenKind.SemiColon);

                            return new UnaryExpression(op, new VariableExpression(name));
                        }
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

                            Requires(TokenKind.Indent);
                            if (_tokens[_idx].kind is TokenKind.Equals)
                            {
                                _idx++;
                                var temp = new VariableDeclaration(_tokens[_idx - 2].val!, type, GetExpression());

                                Requires(TokenKind.SemiColon);

                                return temp;
                            }
                            else if (_tokens[_idx].kind is TokenKind.SemiColon)
                            {
                                _idx++;
                                return new VariableDeclaration(_tokens[_idx - 2].val!, type);
                            }
                            else if(_tokens[_idx].kind is TokenKind.RightParen)
                            {
                                _idx++;
                                string name = _tokens[_idx - 2].val!;

                                List<ParamDeclaration> @params = new();

                                while (_tokens[_idx].kind != TokenKind.LeftParen)
                                {
                                    var typeParam = ParseType();
                                    var NameParam = _tokens[_idx++].val!;
                                    Expression? initializer = null;

                                    if(_tokens[_idx].kind is TokenKind.Equals)
                                    {
                                        _idx++;

                                        initializer = GetExpression();
                                    }

                                    if(_tokens[_idx].kind != TokenKind.LeftParen)
                                        Requires(TokenKind.Comma);

                                    @params.Add(new(NameParam, typeParam, initializer));
                                }

                                _idx++; // LeftParen
                                Requires(TokenKind.RightBrace);
                                List<AstNode> block = new();
                                GetBlockCode(block);

                                return new FuncDeclaration(type, name, @params, block); 
                            }
                            break;
                        }
                    case TokenKind.Do:
                        {
                            _idx++;
                            List<AstNode> block = new();

                            Requires(TokenKind.RightBrace);

                            GetBlockCode(block);

                            Requires(TokenKind.While);
                            Requires(TokenKind.RightParen);

                            Expression ex = GetExpression();

                            Requires(TokenKind.LeftParen);
                            Requires(TokenKind.SemiColon);

                            return new WhileDeclaration(ex, block, true);
                        }
                    case TokenKind.While:
                        {
                            _idx++;
                            Requires(TokenKind.RightParen);

                            Expression ex = GetExpression();
                            List<AstNode> block = new();

                            Requires(TokenKind.LeftParen);
                            Requires(TokenKind.RightBrace);

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
                            Requires(TokenKind.RightBrace);

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
                            Requires(TokenKind.RightBrace);

                            GetBlockCode(block);

                            return new ElseIfDeclaration(ex, block);
                        }
                    case TokenKind.If:
                        {
                            _idx++;
                            Requires(TokenKind.RightParen);

                            Expression ex = GetExpression();
                            List<AstNode> block = new();

                            Requires(TokenKind.LeftParen);
                            Requires(TokenKind.RightBrace);

                            GetBlockCode(block);

                            if (_idx >= _tokens.Count) return new IfDeclaration(ex, block);

                            if (_tokens[_idx].kind is not TokenKind.ElseIf and not TokenKind.Else) return new IfDeclaration(ex, block);

                            var @if = new IfDeclaration(ex, block);

                            while (_tokens[_idx].kind is TokenKind.ElseIf)
                            {
                                @if.ElseIfs ??= new();
                                @if.ElseIfs.Add((ElseIfDeclaration)GetAstNode());
                            }

                            if (_tokens[_idx].kind is not TokenKind.Else) return @if;
                            @if.Else = new();
                            _idx++;

                            Requires(TokenKind.RightBrace);
                            GetBlockCode(@if.Else.CodeBlock);

                            return @if;
                        }
                    case TokenKind.Goto:
                        {
                            _idx++;
                            Requires(TokenKind.Indent);
                            Requires(TokenKind.SemiColon);
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

                            if (_tokens[++_idx].kind is TokenKind.Equals)
                            {
                                _idx++;
                                var temp = new AssignmentStatement(name, null, GetExpression());

                                Requires(TokenKind.SemiColon);

                                return temp;
                            }
                            else if (_tokens[_idx].kind is TokenKind.PlusPlus)
                            {
                                _idx++;
                                Requires(TokenKind.SemiColon);
                                return new UnaryExpression(Operations.PlusPlusRight, new VariableExpression(name));
                            }
                            else if (_tokens[_idx].kind is TokenKind.MinusMinus)
                            {
                                _idx++;
                                Requires(TokenKind.SemiColon);
                                return new UnaryExpression(Operations.MinusMinusLeft, new VariableExpression(name));
                            }

                            var op = GetOperatorEquals(); _idx++;
                            var temp2 = new AssignmentStatement(name, op, GetExpression());

                            Requires(TokenKind.SemiColon);
                            return temp2;
                        }
                    case TokenKind.Return:
                        {
                            _idx++;

                            var ex = GetExpression();
                            Requires(TokenKind.SemiColon);

                            return new ReturnStatement(ex);
                        }

                    default: throw new Exception("Error in Parser, unknown token: " + _tokens[_idx].kind);
                }
            }

            throw new Exception("Parser error, unknown token");
        }

        public Expression GetExpression(int priority = 0)
        {
            Expression left;

            switch (_tokens[_idx].kind)
            {
                case TokenKind.Plus:
                case TokenKind.Minus:
                case TokenKind.Not:
                case TokenKind.BitNot:
                case TokenKind.PlusPlus:
                case TokenKind.MinusMinus:
                    {
                        Operations op = _tokens[_idx++].kind switch
                        {
                            TokenKind.Plus => Operations.Plus,
                            TokenKind.Minus => Operations.Minus,
                            TokenKind.MinusMinus => Operations.MinusMinusLeft,
                            TokenKind.PlusPlus => Operations.PlusPlusLeft,
                            TokenKind.BitNot => Operations.BitNot,
                            TokenKind.Not => Operations.Not,
                            _ => throw new Exception("Unknown unary operator.")
                        };

                        left = new UnaryExpression(op, GetExpression(100));
                        break;
                    }
                case TokenKind.RightParen:
                    {
                        _idx++;
                        left = GetExpression();

                        if (_idx >= _tokens.Count || _tokens[_idx].kind != TokenKind.LeftParen)
                        {
                            throw new Exception("Parser error expected )");
                        }

                        _idx++;
                        break;
                    }
                case TokenKind.Number:
                    {
                        if(_tokens[_idx + 1].kind is TokenKind.Dot)
                        {
                            var @double = _tokens[_idx].val! + _tokens[_idx + 1].val! + _tokens[_idx + 2].val!;
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
                case TokenKind.False:
                    {
                        left = new BoolExpression(false, _typeTable.Bool);
                        _idx++;
                        break;
                    }
                case TokenKind.True:
                    {
                        left = new BoolExpression(true, _typeTable.Bool);
                        _idx++;
                        break;
                    }
                default:
                    throw new Exception("Error in Parser, unknown token: " + _tokens[_idx].kind);
            }

            while (_idx < _tokens.Count && _tokens[_idx].kind is not TokenKind.SemiColon and not TokenKind.LeftParen)
            {
                if (_tokens[_idx].kind is TokenKind.PlusPlus or TokenKind.MinusMinus)
                {
                    var op2 = _tokens[_idx++].kind switch
                    {
                        TokenKind.PlusPlus => Operations.PlusPlusRight,
                        TokenKind.MinusMinus => Operations.MinusMinusRight,
                        _ => throw new Exception()
                    };

                    left = new UnaryExpression(op2, left);
                    continue;
                }

                (var op, int opPriority) = GetOperatorAndPriority();
                if (priority > opPriority) break;
                _idx++;

                left = new BinaryExpression(left, op, GetExpression(++opPriority));
            }

            return left;
        }
       
        public TypeSymbol ParseType()
        {
            bool hasInt = false, hasChar = false, hasShort = false, hasDouble = false;
            int longCount = 0;
            bool? isSigned = null;

            int start = _idx;

            while(_tokens[_idx].kind is TokenKind.Signed or TokenKind.UnSigned or TokenKind.Long or TokenKind.Int or TokenKind.Char or TokenKind.Short or TokenKind.Short)
            {
                if (_tokens[_idx].kind is TokenKind.Int)
                {
                    if (hasInt) throw new ArgumentException("cannot have two ints");
                    hasInt = true;
                }
                if (_tokens[_idx].kind is TokenKind.Double)
                {
                    if (hasDouble) throw new ArgumentException("cannot have two doubles");
                    hasDouble = true;
                }
                else if (_tokens[_idx].kind is TokenKind.Short)
                {
                    if (hasShort) throw new ArgumentException("cannot have two shorts");
                    hasShort = true;
                }
                else if (_tokens[_idx].kind is TokenKind.Char)
                {
                    if (hasChar) throw new ArgumentException("cannot have two chars");
                    hasChar = true;
                }
                else if (_tokens[_idx].kind is TokenKind.Long) longCount++;
                else if (_tokens[_idx].kind is TokenKind.Signed)
                {
                    if (isSigned is not null) throw new ArgumentException("cannot have two signed/unsigned");
                    isSigned = true;
                }
                else if (_tokens[_idx].kind is TokenKind.UnSigned)
                {
                    if (isSigned is not null) throw new ArgumentException("cannot have two signed/unsigned");
                    isSigned = false;
                }
                _idx++;
            }

            if(start == _idx)
            {
                if (_tokens[_idx].kind is TokenKind.Bool)
                {
                    _idx++;
                    return _typeTable.Bool;
                }
                else if (_tokens[_idx].kind is TokenKind.Void)
                {
                    _idx++;
                    return _typeTable.Void;
                }
                else if (_tokens[_idx].kind is TokenKind.Float)
                {
                    _idx++;
                    return _typeTable.Float;
                }
            }

            if ((hasChar || hasShort) && longCount != 0) throw new ArgumentException("Cannot have char/short with long");

            if (isSigned is not null && hasDouble) throw new ArgumentException("Cannot have signed/unsigned with double");

            if (longCount > 2) throw new ArgumentException("Cannot have more than 2 longs");

            bool signed = isSigned ?? true;

            if(hasChar)
            {
                if (isSigned is null) return _typeTable.Char;

                return signed ? _typeTable.Int8 : _typeTable.UInt8;
            }

            if(hasShort) return signed ? _typeTable.Int16 : _typeTable.UInt16;

            if(longCount == 2) return signed ? _typeTable.Int64 : _typeTable.UInt64;

            if (hasDouble) return _typeTable.Double;

            if (longCount == 1) return signed ? _typeTable.Long : _typeTable.ULong;

            return signed ? _typeTable.Int32 : _typeTable.UInt32;
        }

        public NumberExpression ParseNumber()
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

        private void Requires(TokenKind kind)
        {
            if (_tokens[_idx++].kind != kind) throw new Exception($"Requires {kind}");
        }
        private void GetBlockCode<T>(List<T> list) where T : AstNode
        {
            while (_tokens[_idx].kind is not TokenKind.LeftBrace)
            {
                list.Add((T)GetAstNode());
            }
            _idx++;
        }
        private (Operations, int) GetOperatorAndPriority() => _tokens[_idx].kind switch
        {
            TokenKind.Multiply => (Operations.Multiply, 70),
            TokenKind.Divide => (Operations.Divide, 70),
            TokenKind.DivisionRem => (Operations.DivisionRem, 70),

            TokenKind.Plus => (Operations.Plus, 60),
            TokenKind.Minus => (Operations.Minus, 60),

            TokenKind.LeftShift => (Operations.LeftShift, 50),
            TokenKind.RightShift => (Operations.RightShift, 50),

            TokenKind.GreaterThan => (Operations.GreaterThan, 40),
            TokenKind.GreaterThanOrEquals => (Operations.GreaterThanOrEqual, 40),
            TokenKind.LessThan => (Operations.LessThan, 40),
            TokenKind.LessThanOrEquals => (Operations.LessThanOrEqual, 40),

            TokenKind.EqualsEquals => (Operations.EqualsEquals, 30),
            TokenKind.NotEquals => (Operations.NotEquals, 30),

            TokenKind.BitAnd => (Operations.BitAnd, 25),
            TokenKind.BitXor => (Operations.BitXor, 20),
            TokenKind.BitOr => (Operations.BitOr, 15),

            TokenKind.And => (Operations.And, 10),
            TokenKind.Or => (Operations.Or, 5),

            _ => throw new Exception("Unknown Operation")
        };

        private Operations GetOperatorEquals() => _tokens[_idx].kind switch
        {
            TokenKind.PlusEquals => Operations.PlusEquals,
            TokenKind.MinusEquals => Operations.MinusEquals,
            TokenKind.MultiplyEquals => Operations.MultiplyEquals,
            TokenKind.DivideEquals => Operations.DivideEquals,
            TokenKind.DivisionRemEquals => Operations.DivisionRemEquals,

            TokenKind.BitAndEquals => Operations.BitAndEquals,
            TokenKind.BitOrEquals => Operations.BitOrEquals,
            TokenKind.BitXorEquals => Operations.BitXorEquals,

            TokenKind.RightShiftEquals => Operations.RightShiftEquals,
            TokenKind.LeftShiftEquals => Operations.LeftShiftEquals,

            _ => throw new Exception("Unknown Operation")
        };
        public TypeSymbol GetTypeFromToken(TokenKind kind, bool isSigned) => (kind, isSigned) switch
        {
            (TokenKind.Bool, false) => _typeTable.Bool,

            (TokenKind.Long, true) => _typeTable.Int64,
            (TokenKind.Int, true) => _typeTable.Int32,
            (TokenKind.Short, true) => _typeTable.Int16,
            (TokenKind.Char, true) => _typeTable.Int8,

            (TokenKind.Long, false) => _typeTable.UInt64,
            (TokenKind.Int, false) => _typeTable.UInt32,
            (TokenKind.Short, false) => _typeTable.UInt16,
            (TokenKind.Char, false) => _typeTable.UInt8,
            _ => throw new Exception("Unknown type")
        };
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Best_language.src.front
{
    class Parser
    {
        private List<Token> _tokens = new List<Token>();

        private int _idx;

        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
        }

        public ProgramNode GetAstNodes()
        {
             List<AstNode> res = new List<AstNode>();

             while(_idx < _tokens.Count)
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
                    case TokenKind.Bool:
                    case TokenKind.Int:
                        {
                            var type = _tokens[_idx++].kind;
                            Requires(TokenKind.Indent);
                            if (_tokens[_idx].kind is TokenKind.Equals)
                            {
                                _idx++;
                                var temp = new VariableDeclaration(_tokens[_idx - 2].val!, GetTypeFromToken(type), GetExpression());

                                Requires(TokenKind.QuestionMark);

                                return temp;
                            }
                            else if (_tokens[_idx].kind is TokenKind.QuestionMark)
                            {
                                _idx++;
                                return new VariableDeclaration(_tokens[_idx - 2].val!, VariableType.Int);
                            }
                            break;
                        }
                    case TokenKind.If:
                        {
                            _idx++;
                            Requires(TokenKind.LeftParen);

                            Expression ex = GetExpression();
                            List<AstNode> block = new();

                            Requires(TokenKind.RightParen);
                            Requires(TokenKind.LeftBrace);

                            while (_tokens[_idx].kind is not TokenKind.RightBrace)
                            {
                                block.Add(GetAstNode());
                            }

                            _idx++;
                            return new IfDeclaration(ex, block);
                        }
                    case TokenKind.Goto:
                        {
                            _idx++;
                            Requires(TokenKind.Indent);
                            Requires(TokenKind.QuestionMark);
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

                                Requires(TokenKind.QuestionMark);

                                return temp;
                            }
                            var op = GetOperatorEquals(); _idx++;
                            var temp2 = new AssignmentStatement(name, op, GetExpression());

                            Requires(TokenKind.QuestionMark);
                            return temp2;
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
                case TokenKind.LeftParen:
                    {
                        _idx++; 
                        left = GetExpression();

                        if (_idx >= _tokens.Count || _tokens[_idx].kind != TokenKind.RightParen)
                        {
                            throw new Exception("Parser error expected )");
                        }

                        _idx++;
                        break;
                    }
                case TokenKind.Number:
                    {    
                        left = new NumberExpression(int.Parse(_tokens[_idx++].val!), VariableType.Int);
                        break;
                    }
                case TokenKind.Indent:
                    {
                        left = new VariableExpression(_tokens[_idx++].val!);
                        break;
                    }
                case TokenKind.False:
                    {
                        left = new BoolExpression(false);
                        _idx++;
                        break;
                    }
                case TokenKind.True:
                    {
                        left = new BoolExpression(true);
                        _idx++;
                        break;
                    }
                default:
                    throw new Exception("Error in Parser, unknown token: " + _tokens[_idx].kind);
            }

            while(_idx < _tokens.Count && _tokens[_idx].kind is not TokenKind.QuestionMark and not TokenKind.RightParen)
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

        private void Requires(TokenKind kind)
        {
            if (_tokens[_idx++].kind != kind) throw new Exception($"Requires {kind}");
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
        public VariableType GetTypeFromToken(TokenKind kind) => kind switch { 
            TokenKind.Bool => VariableType.Bool,
            TokenKind.Int => VariableType.Int,
            _ => throw new Exception("Unknown type")
        };
    }
}

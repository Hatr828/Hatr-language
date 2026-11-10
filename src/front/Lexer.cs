using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Text;

namespace Best_language.src.front
{
    public class Lexer
    {
        private readonly string _code;

        private int _idx;

        private Dictionary<string, TokenKind> _keywords = new()
        {
            ["INT"] = TokenKind.Int,
            ["boom"] = TokenKind.Bool,
            ["fi"] = TokenKind.If,
            ["ttrruuee"] = TokenKind.True,
            ["ffaallssee"] = TokenKind.False,
            ["goat"] = TokenKind.Goto,
        };

        private Dictionary<char, TokenKind> _chars = new()
        {
            ['*'] = TokenKind.Multiply,
            ['+'] = TokenKind.Plus,
            ['-'] = TokenKind.Minus,
            ['/'] = TokenKind.Divide,
            ['%'] = TokenKind.DivisionRem,

            ['('] = TokenKind.RightParen,
            [')'] = TokenKind.LeftParen,
            ['{'] = TokenKind.RightBrace,
            ['}'] = TokenKind.LeftBrace,

            ['='] = TokenKind.Equals,
            ['?'] = TokenKind.QuestionMark,
            ['>'] = TokenKind.GreaterThan,
            ['<'] = TokenKind.LessThan,
            ['!'] = TokenKind.Not,

            [':'] = TokenKind.Colon,

            ['~'] = TokenKind.BitNot,
            ['|'] = TokenKind.BitOr,
            ['&'] = TokenKind.BitAnd,
            ['^'] = TokenKind.BitXor,
        };

        private Dictionary<string, TokenKind> _doubleChars = new()
        {
            [">="] = TokenKind.GreaterThanOrEquals,
            ["=="] = TokenKind.EqualsEquals,
            ["!="] = TokenKind.NotEquals,
            ["<="] = TokenKind.LessThanOrEquals,
            ["&&"] = TokenKind.And,
            ["||"] = TokenKind.Or,

            ["++"] = TokenKind.PlusPlus,
            ["--"] = TokenKind.MinusMinus,

            ["<<"] = TokenKind.LeftShift,
            [">>"] = TokenKind.RightShift,

            ["+="] = TokenKind.PlusEquals,
            ["-="] = TokenKind.MinusEquals,
            ["*="] = TokenKind.MultiplyEquals,
            ["/="] = TokenKind.DivideEquals,
            ["%="] = TokenKind.DivisionRemEquals,

            ["^="] = TokenKind.BitXorEquals,
            ["|="] = TokenKind.BitOrEquals,
            ["&="] = TokenKind.BitAndEquals,
        };

        public Lexer(string code) {
            _code = code; 
        }
         
        public List<Token> GetTokens()
        {
            List<Token> res = new List<Token>();

            while(_idx < _code.Length)
            {
                char temp = _code[_idx];
                
                if(temp is '\n' or ' ' or '\t' or '\r')
                {
                    _idx++;
                    continue;
                }
                if(temp is >= '0' and <= '9')
                {
                    string num = GetNumber();
                    res.Add(new Token(TokenKind.Number, num));
                    continue;
                }

                ReadOnlySpan<char> word;

                if (char.IsLetter(temp))
                {
                    word = GetWord();
                }
                else {
                    if (_idx + 1 < _code.Length)
                    {
                        if (_idx + 3 < _code.Length)
                        {
                            if(_code[_idx..(_idx + 3)] is "<<=")
                            {
                                res.Add(new(TokenKind.LeftShiftEquals));
                                _idx += 3;
                                continue;
                            }
                            else if (_code[_idx..(_idx + 3)] is ">>=")
                            {
                                res.Add(new(TokenKind.RightShiftEquals));
                                _idx += 3;
                                continue;
                            }
                        }
                        if (_doubleChars.TryGetValue(_code[_idx..(_idx + 2)], out TokenKind token1))
                        {
                            res.Add(new(token1));
                            _idx += 2;
                            continue;
                        }
                    }

                    if (_chars.TryGetValue(temp, out TokenKind token2))
                    {
                        res.Add(new(token2));
                    }
                    else throw new ArgumentException("Unknown token: " + temp);
                    _idx++;
                    continue;
                }
                if (_keywords.GetAlternateLookup<ReadOnlySpan<char>>().TryGetValue(word, out TokenKind token))
                {
                    res.Add(new(token));
                    continue;
                }

                res.Add(new Token(TokenKind.Indent, word.ToString()));
            }

            return res;
        }
        public string GetNumber()
        {
            int startIdx = _idx;
            while (_idx < _code.Length && _code[_idx] is >= '0' and <= '9')
            {
                _idx++;
            }

            return _code[startIdx.._idx];             }
        public ReadOnlySpan<char> GetWord()
        {
            int startIdx = _idx;
            while (_idx < _code.Length && char.IsLetterOrDigit(_code[_idx]) || _code[_idx] is '_')
            {
                _idx++;
            }

            return _code.AsSpan(startIdx.._idx);
        }
    }
}

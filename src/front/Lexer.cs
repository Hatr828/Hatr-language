using System.Collections.Frozen;
using System.Data;

namespace Best_language.src.front
{
    public sealed class Lexer
    {
        private readonly string _code;

        private int _idx;

        private readonly Dictionary<string, TokenKind> _keywords;

        private readonly Dictionary<char, TokenKind> _chars;

        private readonly Dictionary<string, TokenKind> _doubleChars;

        private readonly Dictionary<(string, string), TokenKind> _specialKeywords;

        public Lexer(string code)
        {
            _code = code;

#if DEBUG
            _keywords = CSyntax.keywords;
            _chars = CSyntax.chars;
            _doubleChars = CSyntax.doubleChars;
            _specialKeywords = CSyntax.specialKeywords;
#else
            _keywords = HatrSyntax.keywords;
            _chars = HatrSyntax.chars;
            _doubleChars = HatrSyntax.doubleChars;
            _specialKeywords = HatrSyntax.specialKeywords;
#endif
        }

        public List<Token> GetTokens()
        {
            List<Token> res = new List<Token>();

            while (_idx < _code.Length)
            {
                char temp = _code[_idx];

                if (temp is '\n' or ' ' or '\t' or '\r')
                {
                    _idx++;
                    continue;
                }
                if (temp is '\'')
                {
                    _idx++;
                    char num = GetChar();
                    _idx++;
                    res.Add(new Token(TokenKind.Number, ((int)num).ToString()));
                    continue;
                }
                if (temp is '\"')
                {
                    _idx++;
                    ReadOnlySpan<char> s = GetString();
                    _idx++;
                    res.Add(new Token(TokenKind.String, s.ToString()));
                    continue;
                }
                if (temp is >= '0' and <= '9')
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
                else
                {
                    if (_idx + 1 < _code.Length)
                    {
                        if (_idx + 3 < _code.Length)
                        {
                            if (_code[_idx..(_idx + 3)] is "<<=")
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
                if (_specialKeywords.Count > 0)
                {
                    var secondWord = GetSecondWordWithoutChangingIdx(out int idx);
                    if (_specialKeywords.TryGetValue((word.ToString(), secondWord.ToString()), out TokenKind token2))
                    {
                        _idx = idx;
                        res.Add(new(token2));
                        continue;
                    }
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

            return _code[startIdx.._idx];
        }
        public ReadOnlySpan<char> GetWord()
        {
            int startIdx = _idx;
            while (_idx < _code.Length && char.IsLetterOrDigit(_code[_idx]) || _code[_idx] is '_')
            {
                _idx++;
            }

            return _code.AsSpan(startIdx.._idx);
        }
        public ReadOnlySpan<char> GetSecondWordWithoutChangingIdx(out int idx)
        {
            int startIdx = _idx;
            idx = _idx;

            if (_code[_idx] is '\n' or ' ' or '\t' or '\r')
            {
                idx++; startIdx++;
            }

            while (idx < _code.Length && char.IsLetterOrDigit(_code[idx]) || _code[idx] is '_')
            {
                idx++;
            }

            return _code.AsSpan(startIdx..idx);
        }

        public char GetChar()
        {
            int startIdx = _idx;
            if (_code[_idx++] is '\\')
            {
                return _code[_idx++] switch
                {
                    'n' => '\n',
                    'r' => '\r',
                    '\\' => '\\',
                    '\'' => '\'',
                    _ => throw new ArgumentException("Unknown sequence")
                }; 
            }
            else return _code[startIdx];
        }

        public ReadOnlySpan<char> GetString()
        {
            int startIdx = _idx;
            while (_idx < _code.Length && _code[_idx] is not '\"') _idx++;

            return _code.AsSpan(startIdx.._idx);
        }
    }
}

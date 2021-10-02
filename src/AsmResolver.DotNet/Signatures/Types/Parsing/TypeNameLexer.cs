using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AsmResolver.DotNet.Signatures.Types.Parsing
{
    internal class TypeNameLexer
    {
        internal static readonly ISet<char> ReservedChars = new HashSet<char>("*+.,&[]…");

        private readonly TextReader _reader;
        private readonly StringBuilder _buffer = new();
        private TypeNameToken? _bufferedToken;

        public TypeNameLexer(TextReader reader)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        public bool HasConsumedTypeName
        {
            get;
            set;
        }

        public TypeNameToken Peek()
        {
            _bufferedToken ??= ReadNextToken();
            return _bufferedToken.GetValueOrDefault();
        }

        public TypeNameToken Next()
        {
            if (_bufferedToken.HasValue)
            {
                var next = _bufferedToken.Value;
                _bufferedToken = null;
                return next;
            }

            return ReadNextToken() ?? throw new EndOfStreamException();
        }

        private TypeNameToken? ReadNextToken()
        {
            SkipWhitespaces();
            _buffer.Clear();

            int c = _reader.Peek();
            if (c == -1)
                return null;

            char currentChar = (char) c;
            return currentChar switch
            {
                '*' => ReadSymbolToken(TypeNameTerminal.Star),
                '+' => ReadSymbolToken(TypeNameTerminal.Plus),
                '=' => ReadSymbolToken(TypeNameTerminal.Equals),
                '.' => ReadDotToken(),
                ',' => ReadSymbolToken(TypeNameTerminal.Comma),
                '&' => ReadSymbolToken(TypeNameTerminal.Ampersand),
                '[' => ReadSymbolToken(TypeNameTerminal.OpenBracket),
                ']' => ReadSymbolToken(TypeNameTerminal.CloseBracket),
                '…' => ReadSymbolToken(TypeNameTerminal.Ellipsis),
                _ => char.IsDigit(currentChar) ? ReadNumberOrIdentifierToken() : ReadIdentifierToken()
            };
        }

        private TypeNameToken ReadDotToken()
        {
            // Consume first dot.
            _reader.Read();

            // See if there's a second one.
            if (_reader.Peek() == '.')
            {
                _reader.Read();
                return new TypeNameToken(TypeNameTerminal.DoubleDot, "..");
            }

            return new TypeNameToken(TypeNameTerminal.Dot, ".");
        }

        private TypeNameToken ReadNumberOrIdentifierToken()
        {
            bool escape = false;

            TypeNameTerminal terminal = TypeNameTerminal.Number;
            while (true)
            {
                int c = _reader.Peek();
                if (c == -1)
                    break;

                char currentChar = (char) c;

                if (escape)
                {
                    escape = false;
                }
                else
                {
                    if (currentChar == '\\')
                    {
                        escape = true;
                    }
                    else if (currentChar == '=' && HasConsumedTypeName)
                    {
                        break;
                    }
                    else if (terminal == TypeNameTerminal.Number
                             && (char.IsWhiteSpace(currentChar) || ReservedChars.Contains(currentChar)))
                    {
                        break;
                    }
                }

                if (!char.IsDigit(currentChar))
                    terminal = TypeNameTerminal.Identifier;

                _reader.Read();

                if (!escape)
                    _buffer.Append(currentChar);
            }

            return new TypeNameToken(terminal, _buffer.ToString().Trim(' '));
        }

        private TypeNameToken ReadIdentifierToken()
        {
            bool escape = false;
            while (true)
            {
                int c = _reader.Peek();
                if (c == -1)
                    break;

                char currentChar = (char) c;

                if (escape)
                {
                    escape = false;
                }
                else
                {
                    if (currentChar == '\\')
                        escape = true;
                    else if (currentChar == '=' && HasConsumedTypeName)
                        break;
                    else if (ReservedChars.Contains(currentChar))
                        break;
                }

                _reader.Read();

                if (!escape)
                    _buffer.Append(currentChar);
            }

            return new TypeNameToken(TypeNameTerminal.Identifier, _buffer.ToString().Trim(' '));
        }

        private TypeNameToken ReadSymbolToken(TypeNameTerminal terminal)
        {
            string text = ((char) _reader.Read()).ToString();
            return new TypeNameToken(terminal, text);
        }

        private void SkipWhitespaces()
        {
            while (true)
            {
                int c = _reader.Peek();
                if (c == -1 || !char.IsWhiteSpace((char) c))
                    break;
                _reader.Read();
            }
        }

    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AsmResolver.Shims;

namespace AsmResolver.DotNet.Signatures.Parsing
{
    /// <summary>
    /// Provides a mechanism for parsing a fully assembly qualified name of a type.
    /// </summary>
    public struct TypeNameParser
    {
        // src/coreclr/src/vm/typeparse.cpp
        // https://docs.microsoft.com/en-us/dotnet/framework/reflection-and-codedom/specifying-fully-qualified-type-names

        private static readonly SignatureComparer Comparer = new();
        private readonly ModuleDefinition _module;
        private TypeNameLexer _lexer;

        private TypeNameParser(ModuleDefinition module, TypeNameLexer lexer)
        {
            _module = module ?? throw new ArgumentNullException(nameof(module));
            _lexer = lexer;
        }

        /// <summary>
        /// Parses a single fully assembly qualified name.
        /// </summary>
        /// <param name="module">The module containing the assembly qualified name.</param>
        /// <param name="canonicalName">The fully qualified assembly name of the type.</param>
        /// <returns>The parsed type.</returns>
        public static TypeSignature Parse(ModuleDefinition module, string canonicalName)
        {
            var lexer = new TypeNameLexer(new StringReader(canonicalName));
            var parser = new TypeNameParser(module, lexer);
            return parser.ParseTypeSpec().ToTypeSignature(module);
        }

        private ParsedTypeFullName ParseTypeSpec()
        {
            bool lastHasConsumedTypeName = _lexer.HasConsumedTypeName;

            // Parse type signature.
            _lexer.HasConsumedTypeName = false;
            var typeSpec = ParseSimpleTypeSpec();
            _lexer.HasConsumedTypeName = true;

            // See if the type full name contains an assembly ref.
            typeSpec.Scope = TryExpect(TypeNameTerminal.Comma).HasValue
                ? (IResolutionScope) ParseAssemblyNameSpec()
                : null;

            _lexer.HasConsumedTypeName = lastHasConsumedTypeName;

            return typeSpec;
        }

        private ParsedTypeFullName ParseSimpleTypeSpec()
        {
            // Parse type name.
            var result = new ParsedTypeFullName(ParseTypeName());

            // Check for annotations.
            while (true)
            {
                switch (_lexer.Peek().Terminal)
                {
                    case TypeNameTerminal.Ampersand:
                        ParseByReferenceTypeSpec(result);
                        break;

                    case TypeNameTerminal.Star:
                        ParsePointerTypeSpec(result);
                        break;

                    case TypeNameTerminal.OpenBracket:
                        ParseArrayOrGenericTypeSpec(result);
                        break;

                    default:
                        return result;
                }
            }
        }

        private void ParseByReferenceTypeSpec(ParsedTypeFullName result)
        {
            Expect(TypeNameTerminal.Ampersand);
            result.Annotations.Add(new TypeAnnotation(TypeAnnotationType.ByReference));
        }

        private void ParsePointerTypeSpec(ParsedTypeFullName result)
        {
            Expect(TypeNameTerminal.Star);
            result.Annotations.Add(new TypeAnnotation(TypeAnnotationType.Pointer));
        }

        private void ParseArrayOrGenericTypeSpec(ParsedTypeFullName result)
        {
            Expect(TypeNameTerminal.OpenBracket);

            switch (_lexer.Peek().Terminal)
            {
                case TypeNameTerminal.OpenBracket:
                case TypeNameTerminal.Identifier:
                    ParseGenericTypeSpec(result);
                    break;

                default:
                    ParseArrayTypeSpec(result);
                    break;
            }
        }

        private void ParseArrayTypeSpec(ParsedTypeFullName result)
        {
            var dimension = ParseArrayDimension();

            // Fast path: optimize for SZ arrays (to avoid list allocation).
            if (_lexer.Peek().Terminal == TypeNameTerminal.CloseBracket
                && dimension is { Size: null, LowerBound: null })
            {
                _lexer.Next();
                result.Annotations.Add(new TypeAnnotation(TypeAnnotationType.SzArray));
                return;
            }

            // Slow path: full array specification.
            var dimensions = new List<ArrayDimension> { dimension };

            bool stop = false;
            while (!stop)
            {
                var nextToken = Expect(TypeNameTerminal.CloseBracket, TypeNameTerminal.Comma);
                switch (nextToken.Terminal)
                {
                    case TypeNameTerminal.CloseBracket:
                        stop = true;
                        break;

                    case TypeNameTerminal.Comma:
                        dimensions.Add(ParseArrayDimension());
                        break;
                }
            }

            result.Annotations.Add(new TypeAnnotation(dimensions));
        }

        private ArrayDimension ParseArrayDimension()
        {
            switch (_lexer.Peek().Terminal)
            {
                case TypeNameTerminal.CloseBracket:
                case TypeNameTerminal.Comma:
                    return new ArrayDimension();

                case TypeNameTerminal.Number:
                    int? size = null;
                    int? lowerBound = null;

                    int firstNumber = int.Parse(_lexer.Next().Text);
                    var dots = TryExpect(TypeNameTerminal.Ellipsis, TypeNameTerminal.DoubleDot);
                    if (dots.HasValue)
                    {
                        var secondNumberToken = TryExpect(TypeNameTerminal.Number);
                        if (secondNumberToken.HasValue)
                        {
                            int secondNumber = int.Parse(secondNumberToken.Value.Text);
                            size = secondNumber - firstNumber;
                        }

                        lowerBound = firstNumber;
                    }

                    return new ArrayDimension(size, lowerBound);

                default:
                    // Fail intentionally:
                    Expect(TypeNameTerminal.CloseBracket, TypeNameTerminal.Comma, TypeNameTerminal.Number);
                    return new ArrayDimension();
            }
        }

        private void ParseGenericTypeSpec(ParsedTypeFullName result)
        {
            var arguments = new List<ParsedTypeFullName>();
            arguments.Add(ParseGenericTypeArgument());

            bool stop = false;
            while (!stop)
            {
                var nextToken = Expect(TypeNameTerminal.CloseBracket, TypeNameTerminal.Comma);
                switch (nextToken.Terminal)
                {
                    case TypeNameTerminal.CloseBracket:
                        stop = true;
                        break;
                    case TypeNameTerminal.Comma:
                        arguments.Add(ParseGenericTypeArgument());
                        break;
                }
            }

            result.Annotations.Add(new TypeAnnotation(arguments));
        }

        private ParsedTypeFullName ParseGenericTypeArgument()
        {
            var extraBracketToken = TryExpect(TypeNameTerminal.OpenBracket);

            var result = !extraBracketToken.HasValue
                ? ParseSimpleTypeSpec()
                : ParseTypeSpec();

            // If we started with double brackets, then we should end with double brackets.
            if (extraBracketToken.HasValue)
                Expect(TypeNameTerminal.CloseBracket);

            return result;
        }

        private TypeName ParseTypeName()
        {
            var names = ParseDottedExpression(TypeNameTerminal.Identifier);

            // The namespace is every name concatenated except for the last one.
            string? ns;
            if (names.Count > 1)
            {
                ns = StringShim.Join(".", names.Take(names.Count - 1));
                names.RemoveRange(0, names.Count - 1);
            }
            else
            {
                ns = null;
            }

            // Check if we have any nested identifiers.
            while (TryExpect(TypeNameTerminal.Plus).HasValue)
            {
                var nextIdentifier = Expect(TypeNameTerminal.Identifier);
                names.Add(nextIdentifier.Text);
            }

            return new TypeName(ns, names);
        }

        private List<string> ParseDottedExpression(TypeNameTerminal terminal)
        {
            var result = new List<string>();

            while (true)
            {
                var nextIdentifier = TryExpect(terminal);
                if (!nextIdentifier.HasValue)
                    break;

                result.Add(nextIdentifier.Value.Text);

                if (!TryExpect(TypeNameTerminal.Dot).HasValue)
                    break;
            }

            if (result.Count == 0)
                throw new FormatException($"Expected {terminal}.");

            return result;
        }

        private AssemblyReference ParseAssemblyNameSpec()
        {
            string assemblyName = StringShim.Join(".", ParseDottedExpression(TypeNameTerminal.Identifier));
            var newReference = new AssemblyReference(assemblyName, new Version());

            while (TryExpect(TypeNameTerminal.Comma).HasValue)
            {
                string propertyName = Expect(TypeNameTerminal.Identifier).Text;
                Expect(TypeNameTerminal.Equals);
                if (propertyName.Equals("version", StringComparison.OrdinalIgnoreCase))
                {
                    newReference.Version = ParseVersion();
                }
                else if (propertyName.Equals("culture", StringComparison.OrdinalIgnoreCase))
                {
                    string culture = ParseCulture();
                    newReference.Culture = !culture.Equals("neutral", StringComparison.OrdinalIgnoreCase)
                        ? culture
                        : null;
                }
                else if (propertyName.Equals("publickey", StringComparison.OrdinalIgnoreCase))
                {
                    newReference.PublicKeyOrToken = ParseHexBlob();
                    newReference.HasPublicKey = true;
                }
                else if (propertyName.Equals("publickeytoken", StringComparison.OrdinalIgnoreCase))
                {
                    newReference.PublicKeyOrToken = ParseHexBlob();
                    newReference.HasPublicKey = false;
                }
                else
                {
                    throw new FormatException($"Unsupported {propertyName} assembly property.");
                }
            }

            // Reuse imported assembly reference instance if possible.
            for (int i = 0; i < _module.AssemblyReferences.Count; i++)
            {
                var existingReference = _module.AssemblyReferences[i];
                if (Comparer.Equals((AssemblyDescriptor) existingReference, newReference))
                    return existingReference;
            }

            return newReference;
        }

        private Version ParseVersion()
        {
            string versionString = StringShim.Join(".", ParseDottedExpression(TypeNameTerminal.Number));
            return VersionShim.Parse(versionString);
        }

        private byte[]? ParseHexBlob()
        {
            string hexString = Expect(TypeNameTerminal.Identifier, TypeNameTerminal.Number).Text;
            if (hexString == "null")
                return null;
            if (hexString.Length % 2 != 0)
                throw new FormatException("Provided hex string does not have an even length.");

            byte[] result = new byte[hexString.Length / 2];
            for (int i = 0; i < hexString.Length; i += 2)
                result[i / 2] = ParseHexByte(hexString, i);
            return result;
        }

        private static byte ParseHexByte(string hexString, int index)
        {
            return (byte) ((ParseHexNibble(hexString[index]) << 4) | ParseHexNibble(hexString[index + 1]));
        }

        private static byte ParseHexNibble(char nibble) => nibble switch
        {
            >= '0' and <= '9' => (byte) (nibble - '0'),
            >= 'A' and <= 'F' => (byte) (nibble - 'A' + 10),
            >= 'a' and <= 'f' => (byte) (nibble - 'a' + 10),
            _ => throw new FormatException()
        };

        private string ParseCulture()
        {
            return Expect(TypeNameTerminal.Identifier).Text;
        }

        private TypeNameToken Expect(TypeNameTerminal terminal)
        {
            return TryExpect(terminal)
                   ?? throw new FormatException($"Expected {terminal}.");
        }

        private TypeNameToken? TryExpect(TypeNameTerminal terminal)
        {
            var token = _lexer.Peek();

            if (terminal != token.Terminal)
                return null;

            _lexer.Next();
            return token;
        }

        private TypeNameToken Expect(params TypeNameTerminal[] terminals)
        {
            return TryExpect(terminals)
                ?? throw new FormatException(
                    $"Expected one of {StringShim.Join(", ", terminals.Select(x => x.ToString()))}.");
        }

        private TypeNameToken? TryExpect(params TypeNameTerminal[] terminals)
        {
            var token = _lexer.Peek();

            if (!terminals.Contains(token.Terminal))
                return null;

            _lexer.Next();
            return token;
        }
    }
}

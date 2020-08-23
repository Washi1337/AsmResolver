using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace AsmResolver.DotNet.Signatures.Types.Parsing
{
    /// <summary>
    /// Provides a mechanism for parsing a fully assembly qualified name of a type. 
    /// </summary>
    public sealed class TypeNameParser
    {
        private static readonly SignatureComparer Comparer = new SignatureComparer();
        
        // https://docs.microsoft.com/en-us/dotnet/framework/reflection-and-codedom/specifying-fully-qualified-type-names
        
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
            return parser.ParseTypeSpec();
        }

        private readonly ModuleDefinition _module;
        private readonly TypeNameLexer _lexer;

        private TypeNameParser(ModuleDefinition module, TypeNameLexer lexer)
        {
            _module = module ?? throw new ArgumentNullException(nameof(module));
            _lexer = lexer ?? throw new ArgumentNullException(nameof(lexer));
        }

        private TypeSignature ParseTypeSpec()
        {
            // Parse type signature.
            var typeSpec = ParseSimpleTypeSpec();
            
            // See if the type full name contains an assembly ref.
            var scope = TryExpect(TypeNameTerminal.Comma).HasValue
                ? (IResolutionScope) ParseAssemblyNameSpec()
                : _module;

            // Ensure corlib type sigs are used.
            if (Comparer.Equals(scope, _module.CorLibTypeFactory.CorLibScope))
            {
                var corlibType = _module.CorLibTypeFactory.FromType(typeSpec);
                if (corlibType != null)
                    return corlibType;
            }
            
            // Update scope.
            var reference = (TypeReference) typeSpec.GetUnderlyingTypeDefOrRef();
            while (reference.Scope is TypeReference parent)
                reference = parent;
            reference.Scope = scope;

            return typeSpec;
        }

        private TypeSignature ParseSimpleTypeSpec()
        {
            // Parse type name.
            var typeName = ParseTypeName();
            
            // Check for annotations.
            while (true)
            {
                switch (_lexer.Peek().Terminal)
                {
                    case TypeNameTerminal.Ampersand:
                        typeName = ParseByReferenceTypeSpec(typeName);
                        break;
                    
                    case TypeNameTerminal.Star:
                        typeName = ParsePointerTypeSpec(typeName);
                        break;
                    
                    case TypeNameTerminal.OpenBracket:
                        typeName = ParseArrayOrGenericTypeSpec(typeName);
                        break;
                    
                    default:
                        return typeName;
                }
            }
        }

        private TypeSignature ParseByReferenceTypeSpec(TypeSignature typeName)
        {
            Expect(TypeNameTerminal.Ampersand);
            return new ByReferenceTypeSignature(typeName);
        }

        private TypeSignature ParsePointerTypeSpec(TypeSignature typeName)
        {
            Expect(TypeNameTerminal.Star);
            return new PointerTypeSignature(typeName);
        }

        private TypeSignature ParseArrayOrGenericTypeSpec(TypeSignature typeName)
        {
            Expect(TypeNameTerminal.OpenBracket);

            switch (_lexer.Peek().Terminal)
            {
                case TypeNameTerminal.OpenBracket:
                    return ParseGenericTypeSpec(typeName);
                
                default:
                    return ParseArrayTypeSpec(typeName);
            }
        }

        private TypeSignature ParseArrayTypeSpec(TypeSignature typeName)
        {
            var dimensions = new List<ArrayDimension>
            {
                ParseArrayDimension()
            };

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

            if (dimensions.Count == 1 && dimensions[0].Size == null && dimensions[0].LowerBound == null)
                return new SzArrayTypeSignature(typeName);

            var result = new ArrayTypeSignature(typeName);
            foreach (var dimension in dimensions)
                result.Dimensions.Add(dimension);
            return result;
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
                            lowerBound = firstNumber;
                        }
                        else
                        {
                            lowerBound = firstNumber;
                        }
                    }

                    return new ArrayDimension(size, lowerBound);
                
                default:
                    // Fail intentionally:
                    Expect(TypeNameTerminal.CloseBracket, TypeNameTerminal.Comma, TypeNameTerminal.Number);
                    return new ArrayDimension();
            }
        }

        private TypeSignature ParseGenericTypeSpec(TypeSignature typeName)
        {
            var result = new GenericInstanceTypeSignature(typeName.ToTypeDefOrRef(), typeName.IsValueType);
            result.TypeArguments.Add(ParseGenericTypeArgument(result));

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
                        result.TypeArguments.Add(ParseGenericTypeArgument(result));
                        break;
                }
            }

            return result;
        }

        private TypeSignature ParseGenericTypeArgument(GenericInstanceTypeSignature genericInstance)
        {
            Expect(TypeNameTerminal.OpenBracket);
            var result = ParseTypeSpec();
            Expect(TypeNameTerminal.CloseBracket);
            return result;
        }

        private TypeSignature ParseTypeName()
        {
            // Note: This is a slight deviation from grammar (but is equivalent), to make the parsing easier.
            //       We read all components
            (string ns, var names) = ParseNamespaceTypeName();
            
            TypeReference result = null; 
            for (int i = 0; i < names.Count; i++)
            {
                result = result is null
                    ? new TypeReference(_module, _module, ns, names[i])
                    : new TypeReference(_module, result, null, names[i]);
            }

            if (result is null)
                throw new FormatException();

            return result.ToTypeSignature();
        }

        private (string Namespace, IList<string> TypeNames) ParseNamespaceTypeName()
        {
            var names = ParseDottedExpression(TypeNameTerminal.Identifier);
            
            // The namespace is every name concatenated except for the last one.
            string ns;
            if (names.Count > 1)
            {
                ns = string.Join(".", names.Take(names.Count - 1));
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

            return (ns, names);
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
                throw new FormatException($"Expected {string.Join(", ",terminal)}.");

            return result;
        }
        
        private AssemblyReference ParseAssemblyNameSpec()
        {
            string assemblyName = string.Join(".", ParseDottedExpression(TypeNameTerminal.Identifier));
            var assemblyRef = new AssemblyReference(assemblyName, new Version());
            
            while (TryExpect(TypeNameTerminal.Comma).HasValue)
            {
                var propertyToken = Expect(TypeNameTerminal.Identifier);
                Expect(TypeNameTerminal.Equals);
                switch (propertyToken.Text.ToLowerInvariant())
                {
                    case "version":
                        assemblyRef.Version = ParseVersion();
                        break;
                    
                    case "publickey":
                        assemblyRef.PublicKeyOrToken = ParseHexBlob();
                        assemblyRef.HasPublicKey = true;
                        break;
                    
                    case "publickeytoken":
                        assemblyRef.PublicKeyOrToken = ParseHexBlob();
                        assemblyRef.HasPublicKey = false;
                        break;
                    
                    case "culture":
                        assemblyRef.Culture = ParseCulture();
                        if (assemblyRef.Culture.Equals("neutral", StringComparison.OrdinalIgnoreCase))
                            assemblyRef.Culture = null;
                        break;
                    
                    default:
                        throw new FormatException($"Unsupported {propertyToken.Text} assembly property.");
                }
            }

            return assemblyRef;
        }

        private Version ParseVersion()
        {
            string versionString = string.Join(".", ParseDottedExpression(TypeNameTerminal.Number));
            return Version.Parse(versionString);
        }

        private byte[] ParseHexBlob()
        {
            var hexString = Expect(TypeNameTerminal.Identifier, TypeNameTerminal.Number).Text;
            if (hexString == "null")
                return null;
            if (hexString.Length % 2 != 0)
                throw new FormatException("Provided hex string does not have an even length.");

            var result = new byte[hexString.Length / 2];
            for (int i = 0; i < hexString.Length; i+=2)
                result[i / 2] = byte.Parse(hexString.Substring(i, 2), NumberStyles.HexNumber);
            return result;
        }

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
                   ?? throw new FormatException($"Expected one of {string.Join(", ", terminals)}.");
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
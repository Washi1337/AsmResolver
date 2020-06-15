using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace AsmResolver.DotNet.Signatures.Types.Parsing
{
    public class TypeNameParser
    {
        // https://docs.microsoft.com/en-us/dotnet/framework/reflection-and-codedom/specifying-fully-qualified-type-names

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
            var typeSpec = ParseSimpleTypeSpec();
            
            if (TryExpect(TypeNameTerminal.Ampersand).HasValue)
                typeSpec = new ByReferenceTypeSignature(typeSpec);

            return typeSpec;
        }

        private TypeSignature ParseSimpleTypeSpec()
        {
            var typeName = ParseTypeName();
            return _lexer.Peek().Terminal switch
            {
                TypeNameTerminal.Star => ParsePointerTypeSpec(typeName),
                TypeNameTerminal.OpenBracket => ParseArrayOrGenericTypeSpec(typeName),
                _ => typeName.ToTypeSignature()
            };
        }

        private TypeSignature ParsePointerTypeSpec(TypeReference typeName)
        {
            Expect(TypeNameTerminal.Star);
            return new PointerTypeSignature(typeName.ToTypeSignature());
        }

        private TypeSignature ParseArrayOrGenericTypeSpec(TypeReference typeName)
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

        private TypeSignature ParseArrayTypeSpec(TypeReference typeName)
        {
            switch (_lexer.Peek().Terminal)
            {
                case TypeNameTerminal.CloseBracket:
                    _lexer.Next();
                    return new SzArrayTypeSignature(typeName.ToTypeSignature());
                
                default:
                    throw new NotImplementedException();
            }
        }

        private TypeSignature ParseGenericTypeSpec(TypeReference typeName)
        {
            throw new NotImplementedException();
        }

        private TypeReference ParseTypeName()
        {
            // Note: This is a slight deviation from grammar (but is equivalent), to make the parsing easier.
            //       We read all components
            (string ns, var names) = ParseNamespaceTypeName();
            var scope = TryExpect(TypeNameTerminal.Comma).HasValue
                ? (IResolutionScope) ParseAssemblyNameSpec()
                : _module;

            TypeReference result = null; 
            for (int i = 0; i < names.Count; i++)
            {
                result = result is null
                    ? new TypeReference(_module, scope, ns, names[i])
                    : new TypeReference(_module, result, null, names[i]);
            }

            return result;
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

        private List<string> ParseDottedExpression(params TypeNameTerminal[] terminals)
        {
            var result = new List<string>();
            
            while (true)
            {
                var nextIdentifier = TryExpect(terminals);
                if (!nextIdentifier.HasValue)
                    break;

                result.Add(nextIdentifier.Value.Text);

                if (!TryExpect(TypeNameTerminal.Dot).HasValue)
                    break;
            }
            
            if (result.Count == 0)
                throw new FormatException($"Expected one of {string.Join(", ",terminals)}.");

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
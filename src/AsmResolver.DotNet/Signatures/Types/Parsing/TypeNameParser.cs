using System;
using System.Collections.Generic;
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
                TypeNameTerminal.Tick => ParseGenericTypeSpec(typeName),
                TypeNameTerminal.Star => ParsePointerTypeSpec(typeName),
                _ => typeName.ToTypeSignature()
            };
        }

        private TypeSignature ParseGenericTypeSpec(TypeReference typeSpec)
        {
            throw new NotImplementedException();
        }

        private TypeSignature ParsePointerTypeSpec(TypeReference typeName)
        {
            Expect(TypeNameTerminal.Star);
            return new PointerTypeSignature(typeName.ToTypeSignature());
        }

        private TypeReference ParseTypeName()
        {
            (string ns, var names) = ParseNamespaceTypeName();
            var scope = TryExpect(TypeNameTerminal.Comma).HasValue
                ? (IResolutionScope) ParseAssemblyNameSpec()
                : _module;
            return new TypeReference(_module, scope, ns, names[0]);
        }

        private (string Namespace, IList<string> TypeNames) ParseNamespaceTypeName()
        {
            string ns = null;
            var names = new List<string>();
            
            var nextToken = Expect(TypeNameTerminal.Identifier);
            
            switch (_lexer.Peek().Terminal)
            {
                case TypeNameTerminal.Dot:
                    ns = ParseNamespaceSpec(nextToken);
                    break;
                case TypeNameTerminal.Plus:
                    break;
                default:
                    names.Add(nextToken.Text);
                    break;
            }

            return (ns, names);
        }

        private string ParseNamespaceSpec(TypeNameToken initialToken)
        {
            var builder = new StringBuilder();
            builder.Append(initialToken.Text);
            
            while (TryExpect(TypeNameTerminal.Dot).HasValue)
            {
                var identifier = Expect(TypeNameTerminal.Identifier);
                builder.Append('.');
                builder.Append(identifier.Text);
            }

            return builder.ToString();
        }
        

        private AssemblyReference ParseAssemblyNameSpec()
        {
            return null;
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
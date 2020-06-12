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
            var names = new List<string>();

            (string ns, string name) = ParseTopLevelTypeName();

            names.Add(name);
            return (ns, names);
        }

        private (string Namespace, string Name) ParseTopLevelTypeName()
        {
            var namespaceBuilder = new StringBuilder();

            string name = null;
            
            while (true)
            {
                var nextIdentifier = TryExpect(TypeNameTerminal.Identifier);
                if (!nextIdentifier.HasValue)
                    break;
                
                name = nextIdentifier.Value.Text;

                if (!TryExpect(TypeNameTerminal.Dot).HasValue)
                    break;
                
                if (namespaceBuilder.Length > 0)
                    namespaceBuilder.Append('.');
                namespaceBuilder.Append(name);
            }
            
            if (name is null)
                throw new FormatException("Expected identifier.");

            string ns = namespaceBuilder.Length == 0 ? null : namespaceBuilder.ToString();
            return (ns, name);
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
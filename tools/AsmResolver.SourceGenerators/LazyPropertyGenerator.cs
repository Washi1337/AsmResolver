using System.CodeDom.Compiler;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AsmResolver.SourceGenerators;

[Generator]
public class LazyPropertyGenerator : IIncrementalGenerator
{
    public const string LazyPropertyAttribute =
        """
        namespace AsmResolver
        {
            [global::System.AttributeUsage(global::System.AttributeTargets.Property)]
            internal sealed class LazyPropertyAttribute : global::System.Attribute { }
        }
        """;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static ctx =>
        {
            ctx.AddSource("LazyPropertyAttribute.g.cs", LazyPropertyAttribute);
        });

        var entries = context.SyntaxProvider
            .ForAttributeWithMetadataName<object?>(
                fullyQualifiedMetadataName: "AsmResolver.LazyPropertyAttribute",
                predicate: (node, _) => node is PropertyDeclarationSyntax,
                transform: (syntaxContext, token) =>
                {
                    var propertySyntax = (PropertyDeclarationSyntax) syntaxContext.TargetNode;
                    var propertySymbol = (IPropertySymbol) syntaxContext.TargetSymbol;
                    if (!propertySymbol.IsPartialDefinition)
                    {
                        return new DiagnosticInfo(
                            LazyPropertyDiagnostics.PropertyMustBePartial,
                            syntaxContext.TargetNode.GetLocation(),
                            [propertySymbol.Name]
                        );
                    }

                    string ns = propertySymbol.ContainingNamespace.ToDisplayString(
                        SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(
                            SymbolDisplayGlobalNamespaceStyle.Omitted
                        )
                    );

                    return new LazyPropertyInfo(
                        Namespace: ns,
                        TypeName: propertySymbol.ContainingType.Name,
                        Modifiers: propertySyntax.Modifiers.ToString(),
                        PropertyType: propertySymbol.Type.ToDisplayString(),
                        RequiresNullableSpecifier: propertySymbol.Type.IsReferenceType && propertySymbol.Type.NullableAnnotation != NullableAnnotation.Annotated,
                        PropertyName: propertySymbol.Name,
                        Getter: GetAccessor(propertySyntax, false),
                        Setter: GetAccessor(propertySyntax, true)
                    );

                    static PropertyAccessor? GetAccessor(PropertyDeclarationSyntax syntax, bool setter)
                    {
                        var kind = setter
                            ? SyntaxKind.SetAccessorDeclaration
                            : SyntaxKind.GetAccessorDeclaration;

                        if (syntax.AccessorList is null)
                            return null;

                        foreach (var accessor in syntax.AccessorList.Accessors)
                        {
                            if (accessor.Kind() == kind)
                                return new PropertyAccessor(accessor.Modifiers.ToString());
                        }

                        return null;
                    }
                }
            );

        context.RegisterSourceOutput(
            entries,
            (productionContext, entry) =>
            {
                switch (entry)
                {
                    case DiagnosticInfo diagnostic:
                        productionContext.ReportDiagnostic(diagnostic.ToDiagnostic());
                        break;

                    case LazyPropertyInfo property:
                        productionContext.AddSource(
                            $"{property.Namespace}_{property.TypeName}_{property.PropertyName}.g.cs",
                            property.GenerateSourceCode()
                        );
                        break;
                }
            }
        );
    }

    private sealed record DiagnosticInfo(
        DiagnosticDescriptor Descriptor,
        Location? Location,
        object[] Arguments
    )
    {
        public Diagnostic ToDiagnostic() => Diagnostic.Create(Descriptor, Location, Arguments);
    }

    private record struct PropertyAccessor(string Modifiers);

    private sealed record LazyPropertyInfo(
        string? Namespace,
        string TypeName,
        string Modifiers,
        string PropertyType,
        bool RequiresNullableSpecifier,
        string PropertyName,
        PropertyAccessor? Getter,
        PropertyAccessor? Setter)
    {
        public string GenerateSourceCode()
        {
            string fieldName = GetFieldName(PropertyName);
            string initializedFieldName = $"{fieldName}Initialized";
            string factoryName = GetFactoryName(PropertyName);

            var source = new StringWriter();
            var writer = new IndentedTextWriter(source);

            // File header
            writer.WriteLine("// This file is auto-generated by AsmResolver.SourceGenerators");
            writer.WriteLine();
            writer.WriteLine("#nullable enable");

            // Enclosing Namespace
            if (!string.IsNullOrEmpty(Namespace))
            {
                writer.WriteLine($"namespace {Namespace}");
                writer.OpenBrace();
            }

            // Enclosing class
            writer.WriteLine($"partial class {TypeName}");
            writer.OpenBrace();

            // Backing storage field
            writer.WriteLine(RequiresNullableSpecifier
                ? $"private {PropertyType}? {fieldName};"
                : $"private {PropertyType} {fieldName};"
            );

            // IsInitialized field
            writer.WriteLine($"private bool {initializedFieldName};");
            writer.WriteLine();

            // Property
            writer.WriteLine($"{Modifiers} {PropertyType} {PropertyName}");
            writer.OpenBrace();

            if (Getter is { } getter)
            {
                if (!string.IsNullOrEmpty(getter.Modifiers))
                {
                    writer.Write(getter.Modifiers);
                    writer.Write(' ');
                }

                // We use a separate local function for InitializeValue to allow for better inlining by the JIT.
                writer.WriteLines(
                    $$"""
                      get
                      {
                          if (!{{initializedFieldName}})
                              InitializeValue();
                          return {{fieldName}}!;

                          void InitializeValue()
                          {
                              lock (_lock)
                              {
                                  if (!{{initializedFieldName}})
                                  {
                                      {{fieldName}} = {{factoryName}}();
                                      {{initializedFieldName}} = true;
                                  }
                              }
                          }
                      }
                      """
                );
            }

            if (Setter is { } setter)
            {
                if (!string.IsNullOrEmpty(setter.Modifiers))
                {
                    writer.Write(setter.Modifiers);
                    writer.Write(' ');
                }

                writer.WriteLines(
                    $$"""
                      set
                      {
                          lock (_lock)
                          {
                              {{fieldName}} = value;
                              {{initializedFieldName}} = true;
                          }
                      }
                      """
                );
            }

            // Close property
            writer.CloseBrace();

            // Close enclosing class
            writer.CloseBrace();

            // Close enclosing namespace
            if (!string.IsNullOrEmpty(Namespace))
                writer.CloseBrace();

            return source.ToString();
        }

        static string GetFieldName(string propertyName) => $"_{char.ToLowerInvariant(propertyName[0])}{propertyName.Substring(1)}";
        static string GetFactoryName(string propertyName) => $"Get{propertyName}";
    }
}

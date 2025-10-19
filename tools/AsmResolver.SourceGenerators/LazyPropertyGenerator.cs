using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
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
            .ForAttributeWithMetadataName<Entry>(
                fullyQualifiedMetadataName: "AsmResolver.LazyPropertyAttribute",
                predicate: (node, _) => node is PropertyDeclarationSyntax,
                transform: (syntaxContext, _) =>
                {
                    var propertySyntax = (PropertyDeclarationSyntax) syntaxContext.TargetNode;
                    var propertySymbol = (IPropertySymbol) syntaxContext.TargetSymbol;

                    string ns = propertySymbol.ContainingNamespace.ToDisplayString(
                        SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(
                            SymbolDisplayGlobalNamespaceStyle.Omitted
                        )
                    );

                    // Check if the property is partial and not abstract.
                    if (!propertySymbol.IsPartialDefinition || propertySymbol.IsAbstract)
                    {
                        return new DiagnosticInfo(
                            Namespace: ns,
                            TypeName: propertySymbol.ContainingType.Name,
                            Descriptor: LazyPropertyDiagnostics.LazyPropertyMustBePartial,
                            Location: syntaxContext.TargetNode.GetLocation(),
                            Arguments: new EquatableArray<object>([propertySymbol.Name])
                        );
                    }

                    // Wrap all info in an equatable instance.
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
            )
            .Collect()
            .SelectMany((entries, _) => entries
                .GroupBy(entry => (entry!.Namespace, entry.TypeName)) // Create one combined file per type
                .Select(group => new LazyClassInfo(group.Key.Namespace, group.Key.TypeName, group.ToEquatableArray()))
                .ToEquatableArray()
            );

        context.RegisterSourceOutput(
            entries,
            (productionContext, info) =>
            {
                var source = new StringWriter();
                var writer = new IndentedTextWriter(source);

                info.GenerateSourceCode(productionContext, writer);

                productionContext.AddSource($"{info.TypeName}.g.cs", source.ToString());
            }
        );
    }

    private abstract record Entry(
        string? Namespace,
        string? TypeName
    );

    private sealed record DiagnosticInfo(
        string? Namespace,
        string? TypeName,
        DiagnosticDescriptor Descriptor,
        Location? Location,
        EquatableArray<object> Arguments
    ) : Entry(Namespace, TypeName)
    {
        public Diagnostic ToDiagnostic() => Diagnostic.Create(Descriptor, Location, Arguments.Array.ToArray());
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
        PropertyAccessor? Setter) : Entry(Namespace, TypeName)
    {
        public void GenerateSourceCode(IndentedTextWriter writer)
        {
            string fieldName = GetFieldName();
            string initializedFieldName = $"{fieldName}Initialized";
            string factoryName = GetFactoryName();

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
        }

        public string GetFieldName() => GetFieldName(PropertyName);

        public string GetFactoryName() => GetFactoryName(PropertyName);

        public static string GetFieldName(string propertyName) => $"_{char.ToLowerInvariant(propertyName[0])}{propertyName.Substring(1)}";

        public static string GetFactoryName(string propertyName) => $"Get{propertyName}";
    }

    private sealed record LazyClassInfo(
        string? Namespace,
        string? TypeName,
        EquatableArray<Entry> Entries
    ) : Entry(Namespace, TypeName)
    {
        public void GenerateSourceCode(SourceProductionContext productionContext, IndentedTextWriter writer)
        {
            // File header
            writer.WriteLine("// This file was auto-generated by AsmResolver.SourceGenerators");
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

            writer.WriteLine("private readonly object _lock = new();");
            writer.WriteLine();

            foreach (var entry in Entries)
            {
                switch (entry)
                {
                    case DiagnosticInfo diagnostic:
                        productionContext.ReportDiagnostic(diagnostic.ToDiagnostic());
                        break;

                    case LazyPropertyInfo property:
                        property.GenerateSourceCode(writer);
                        writer.WriteLine();
                        break;
                }
            }

            // Enclosing class
            writer.CloseBrace();

            // Enclosing Namespace
            if (!string.IsNullOrEmpty(Namespace))
                writer.CloseBrace();
        }
    }
}

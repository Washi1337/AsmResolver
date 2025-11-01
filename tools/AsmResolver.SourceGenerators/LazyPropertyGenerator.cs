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
        #nullable enable

        namespace AsmResolver
        {
            /// <summary>
            /// Specifies an auto property is lazily initialized and that this implementation is provided by a source generator
            /// </summary>
            /// <remarks>
            /// The source generator generates for every property <c>X</c> marked with this attribute the following:
            /// <list type="bullet">
            ///    <item><description>A private field <c>_x</c> to store the value of the property</description></item>
            ///    <item><description>A private constant <c>XMask</c> specifying the bit mask within the initialization vector <c>_initialized</c></description></item>
            ///    <item><description>A call to a parameterless method <c>GetX()</c></description></item>
            /// </list>
            /// In case <see cref="LazyPropertyAttribute.OwnerProperty" /> is set, the source generator will also generate code to automatically test and update exclusive ownership of the property's value.
            /// </remarks>
            [global::System.AttributeUsage(global::System.AttributeTargets.Property)]
            internal sealed class LazyPropertyAttribute : global::System.Attribute
            {
                /// <summary>
                /// When non-null, gets the name of the property that reflects the owner of the marked property's value.
                /// </summary>
                /// <remarks>
                /// Use this to indicate the value of the property is exclusively owned by at most one instance of the enclosing class.
                /// </remarks>
                public string? OwnerProperty { get; set; }
            }
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

                    // Should this property generate exclusive ownership code?
                    string? ownerProperty = syntaxContext.Attributes[0]
                        .NamedArguments.FirstOrDefault(x => x.Key == "OwnerProperty")
                        .Value.Value?.ToString();

                    // Wrap all info in an equatable instance.
                    return new LazyPropertyInfo(
                        Namespace: ns,
                        TypeName: propertySymbol.ContainingType.Name,
                        Modifiers: propertySyntax.Modifiers.ToString(),
                        PropertyType: propertySymbol.Type.ToDisplayString(),
                        RequiresNullableSpecifier: propertySymbol.Type.IsReferenceType && propertySymbol.Type.NullableAnnotation != NullableAnnotation.Annotated,
                        PropertyName: propertySymbol.Name,
                        OwnerPropertyName: ownerProperty,
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
        string? OwnerPropertyName,
        PropertyAccessor? Getter,
        PropertyAccessor? Setter) : Entry(Namespace, TypeName)
    {
        public void GenerateSourceCode(IndentedTextWriter writer, int index)
        {
            string fieldName = GetFieldName();
            string factoryName = GetFactoryName();

            // Backing storage field
            writer.WriteLine(RequiresNullableSpecifier
                ? $"private {PropertyType}? {fieldName};"
                : $"private {PropertyType} {fieldName};"
            );

            writer.WriteLine($"private const int {PropertyName}InitMask = 1 << {index};");

            // Property
            writer.WriteLine($"{Modifiers} {PropertyType} {PropertyName}");
            writer.OpenBrace();

            if (Getter is { } getter)
                GenerateGetterCode(writer, getter, fieldName, factoryName);

            if (Setter is { } setter)
                GenerateSetterCode(writer, setter, fieldName);

            // Close property
            writer.CloseBrace();
        }

        private void GenerateGetterCode(
            IndentedTextWriter writer,
            PropertyAccessor getter,
            string fieldName,
            string factoryName)
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
                      if (!_initialized[{{PropertyName}}InitMask])
                          InitializeValue();
                      return {{fieldName}}!;

                  """
            );

            if (string.IsNullOrEmpty(OwnerPropertyName))
            {
                writer.WriteLines(
                    $$"""
                          void InitializeValue()
                          {
                              lock (_lock)
                              {
                                  if (!_initialized[{{PropertyName}}InitMask])
                                  {
                                      {{fieldName}} = {{factoryName}}();
                                      _initialized[{{PropertyName}}InitMask] = true;
                                  }
                              }
                          }
                      """
                );
            }
            else
            {
                writer.WriteLines(
                    $$"""
                          void InitializeValue()
                          {
                              lock (_lock)
                              {
                                  if (!_initialized[{{PropertyName}}InitMask])
                                  {
                                      {{fieldName}} = {{factoryName}}();
                                      if ({{fieldName}} is not null)
                                          {{fieldName}}.{{OwnerPropertyName}} = this;
                                      _initialized[{{PropertyName}}InitMask] = true;
                                  }
                              }
                          }
                      """
                );
            }

            writer.WriteLine('}');
        }

        private void GenerateSetterCode(
            IndentedTextWriter writer,
            PropertyAccessor setter,
            string fieldName)
        {
            if (!string.IsNullOrEmpty(setter.Modifiers))
            {
                writer.Write(setter.Modifiers);
                writer.Write(' ');
            }

            if (string.IsNullOrEmpty(OwnerPropertyName))
            {
                writer.WriteLines(
                    $$"""
                      set
                      {
                          lock (_lock)
                          {
                              {{fieldName}} = value;
                              _initialized[{{PropertyName}}InitMask] = true;
                          }
                      }
                      """
                );
            }
            else
            {
                writer.WriteLines(
                    $$"""
                      set
                      {
                          lock (_lock)
                          {
                              if (value is { {{OwnerPropertyName}}: { } originalOwner })
                                  throw new global::System.ArgumentException($"{{PropertyName}} is already assigned to {originalOwner.SafeToString()}.");

                              if (_initialized[{{PropertyName}}InitMask] && {{fieldName}} is { } originalBody)
                                  originalBody.{{OwnerPropertyName}} = null;

                              {{fieldName}} = value;

                              if (value is not null)
                                  value.{{OwnerPropertyName}} = this;

                              _initialized[{{PropertyName}}InitMask] = true;
                          }
                      }
                      """
                );
            }
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
            int count = Entries.OfType<LazyPropertyInfo>().Count();
            if (count > 32)
            {
                productionContext.ReportDiagnostic(Diagnostic.Create(
                    LazyPropertyDiagnostics.TooManyLazyProperties,
                    null,
                    [TypeName, count]
                ));
                return;
            }

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

            writer.WriteLine("#if NET9_0_OR_GREATER");
            writer.WriteLine("private readonly global::System.Threading.Lock _lock = new();");
            writer.WriteLine("#else");
            writer.WriteLine("private readonly object _lock = new();");
            writer.WriteLine("#endif");
            writer.WriteLine("private global::System.Collections.Specialized.BitVector32 _initialized = new();");
            writer.WriteLine();

            int index = 0;
            foreach (var entry in Entries)
            {
                switch (entry)
                {
                    case DiagnosticInfo diagnostic:
                        productionContext.ReportDiagnostic(diagnostic.ToDiagnostic());
                        break;

                    case LazyPropertyInfo property:
                        property.GenerateSourceCode(writer, index++);
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

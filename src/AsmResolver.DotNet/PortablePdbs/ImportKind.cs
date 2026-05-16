namespace AsmResolver.DotNet.PortablePdbs;

public enum ImportKind
{
    // these names are taken from https://github.com/dotnet/runtime/blob/732f597ad3b75f1384eefa10cf5484ce3b221087/src/libraries/System.Reflection.Metadata/src/System/Reflection/Metadata/PortablePdb/ImportDefinitionKind.cs
    ImportNamespace = 1,
    ImportAssemblyNamespace = 2,
    ImportType = 3,
    ImportXmlNamespace = 4,
    ImportAssemblyReferenceAlias = 5,
    AliasAssemblyReference = 6,
    AliasNamespace = 7,
    AliasAssemblyNamespace = 8,
    AliasType = 9
}
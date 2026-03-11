namespace AsmResolver.DotNet.PortablePdbs;

public class Import
{
    public ImportKind Kind { get; set; }
    public Utf8String? Alias { get; set; }
    public AssemblyReference? Assembly { get; set; }
    public Utf8String? TargetNamespace { get; set; }
    public ITypeDefOrRef? TargetType { get; set; }
}
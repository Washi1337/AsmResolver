namespace AsmResolver.PE.DotNet.Metadata.Tables.Rows
{
    public enum DeltaFunctionCode : uint
    {
        FuncDefault = 0,
        MethodCreate,
        FieldCreate,
        ParamCreate,
        PropertyCreate,
        EventCreate,
    };
}
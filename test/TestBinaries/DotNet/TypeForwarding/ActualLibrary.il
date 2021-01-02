.assembly ActualLibrary {}

.class interface public auto ansi abstract IMyModel
{
    .method public abstract virtual hidebysig void MyMethod() {}
}

.class public abstract sealed MyClass
    extends [mscorlib] System.Object 
{
    .method public static hidebysig void StaticMethod(class IMyModel argument)
    {
        ldarg.0
        call instance void IMyModel::MyMethod()
        ret 
    }
}
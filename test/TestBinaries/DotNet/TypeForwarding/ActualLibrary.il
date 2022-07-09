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

    .class nested public abstract sealed MyNestedClass
        extends [mscorlib] System.Object
    {
        .method public static hidebysig void NestedStaticMethod()
        {
            ldstr "MyClass+MyNestedClass::NestedStaticMethod"
            call void [mscorlib] System.Console::WriteLine(string)
            ret
        }
    }
}

.class public abstract sealed MyClass2
    extends [mscorlib] System.Object
{
    .class nested public abstract sealed MyNestedClass
        extends [mscorlib] System.Object
    {
        .method public static hidebysig void NestedStaticMethod()
        {
            ldstr "MyClass2+MyNestedClass::NestedStaticMethod"
            call void [mscorlib] System.Console::WriteLine(string)
            ret
        }
    }
}

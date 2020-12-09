.assembly Test {}

.assembly extern ForwarderLibrary
{
}

.class public auto ansi MyModel
    extends [mscorlib] System.Object 
    implements [ForwarderLibrary] IMyModel
{
    .method public hidebysig specialname rtspecialname void .ctor()
    {
        ldarg.0
        call instance void System.Object::.ctor()
        ret
    }

    .method public virtual hidebysig void MyMethod() 
    {
        ldstr "Hello, world!"
        call void [mscorlib] System.Console::WriteLine(string)
        ret
    }
}

.method public static void Main()
{
    .entrypoint

    newobj void MyModel::.ctor()
    call void [ForwarderLibrary] MyClass::StaticMethod(class [ForwarderLibrary] IMyModel argument)
    ret
}
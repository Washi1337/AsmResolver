.assembly MsilConsoleApplication
{
}

.method public static void Main() cil managed
{
	.entrypoint
	
	ldstr "Hello world!"
	call void [mscorlib]System.Console::WriteLine(string)
	call valuetype [mscorlib]System.ConsoleKeyInfo [mscorlib]System.Console::ReadKey()
	pop

	ret
}
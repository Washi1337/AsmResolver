' https://github.com/Washi1337/AsmResolver/issues/241

Public Module Class1

    Public Sub Test(arg As String)
        Console.WriteLine(arg)
    End Sub

    Public Sub Test(arg As String, arg2 As String)
        Console.WriteLine(arg)
        Console.WriteLine(arg2)
    End Sub

End Module

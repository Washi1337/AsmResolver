namespace AsmResolver.X86
{
    public enum X86Register : byte
    {
        Al = 0,
        Cl,
        Dl,
        Bl,
        Ah,
        Ch,
        Dh,
        Bh,

        Ax = 0x8,
        Cx,
        Dx,
        Bx,
        Sp,
        Bp,
        Si,
        Di,

        Es = 0x10,
        Cs,
        Ss,
        Ds,
        Fs,
        Gs,
        
        Eax = 0x20,
        Ecx,
        Edx,
        Ebx,
        Esp,
        Ebp,
        Esi,
        Edi,
    }

    public enum X86RegisterSize : byte
    {
        Byte,
        Word,
        Dword,
    }

    public static class X86RegisterExtensions
    {
        public static X86RegisterSize GetRegisterSize(this X86Register register)
        {
            var value = (int)register;
            if (value > (int)X86Register.Eax)
                return X86RegisterSize.Dword;
            if (value > (int)X86Register.Ax)
                return X86RegisterSize.Word;
            return X86RegisterSize.Byte;
        }
    }


}

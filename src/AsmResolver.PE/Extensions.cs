namespace AsmResolver.PE;

internal static class Extensions
{
    public static uint GetFlags(this uint self, int index, uint mask) => (self & mask) >> index;

    public static uint SetFlags(this uint self, int index, uint mask, uint value) => (self & ~mask) | ((value << index) & mask);
}

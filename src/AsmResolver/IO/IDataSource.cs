namespace AsmResolver.IO
{
    public interface IDataSource
    {
        byte this[ulong address]
        {
            get;
        }

        ulong Length
        {
            get;
        }

        bool IsValidAddress(ulong address);

        int ReadBytes(ulong address, byte[] buffer, int index, int count);
    }
}

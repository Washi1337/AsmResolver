namespace AsmResolver.PE.Exceptions.X64
{
    /// <summary>
    /// Encodes the effects a function has on the stack pointer, and where the nonvolatile
    /// registers are saved on the stack.
    /// </summary>
    public class X64UnwindInfo : SegmentBase
    {
        /// <inheritdoc />
        public override uint GetPhysicalSize()
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
        {
            throw new System.NotImplementedException();
        }
    }
}

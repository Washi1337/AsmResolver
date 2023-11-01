using System.Collections.Generic;

namespace AsmResolver.PE.DotNet.ReadyToRun
{
    /// <summary>
    /// Provides information about a native entry point for a managed method that was compiled ahead-of-time.
    /// </summary>
    public class MethodEntryPoint
    {
        /// <summary>
        /// Constructs a new entry point for a method.
        /// </summary>
        /// <param name="functionIndex">The index of the <c>RUNTIME_FUNCTION</c> this method starts at.</param>
        public MethodEntryPoint(uint functionIndex)
        {
            RuntimeFunctionIndex = functionIndex;
        }

        /// <summary>
        /// Gets or sets the index to the <c>RUNTIME_FUNCTION</c> the method starts at.
        /// </summary>
        public uint RuntimeFunctionIndex
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a collection of fixups that need to be applied before the method can be executed natively.
        /// </summary>
        public IList<MethodFixup> Fixups
        {
            get;
        } = new List<MethodFixup>();
    }
}

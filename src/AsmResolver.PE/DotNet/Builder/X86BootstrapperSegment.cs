using System;
using System.Collections.Generic;
using AsmResolver.PE.Imports;
using AsmResolver.PE.Imports.Builder;
using AsmResolver.PE.Relocations;

namespace AsmResolver.PE.DotNet.Builder
{
    /// <summary>
    /// Provides an implementation of an x86 (32-bit) bootstrapper code segment that jumps to either
    /// mscoree.dll!_CorDllMain or mscoree.dll!_CorExeMain.
    /// </summary>
    public class X86BootstrapperSegment : BootstrapperSegment
    {
        private readonly bool _isDll;
        private readonly uint _imageBase;
        private readonly IImportAddressProvider _thunkProvider;

        /// <summary>
        /// Creates a new x86 CLR bootstrapper segment.
        /// </summary>
        /// <param name="isDll">Indicates the executable file is a library or not.</param>
        /// <param name="imageBase">The image base of the executable file.</param>
        /// <param name="thunkProvider">
        /// The instance to use for resolving the RVA to the thunk of either mscoree.dll!_CorDllMain or
        /// mscoree.dll!_CorExeMain.
        /// </param>
        public X86BootstrapperSegment(bool isDll, uint imageBase, IImportAddressProvider thunkProvider)
        {
            _isDll = isDll;
            _imageBase = imageBase;
            _thunkProvider = thunkProvider ?? throw new ArgumentNullException(nameof(thunkProvider));
        }

        /// <inheritdoc />
        public override byte[] GetNativeCode()
        {
            string entrypointName = _isDll ? "_CorDllMain" : "_CorExeMain";
            uint address = _imageBase + _thunkProvider.GetThunkRva("mscoree.dll", entrypointName);
            
            return new byte[]
            {
                0xFF, 0x25, 
                (byte) (address & 0xFF), 
                (byte) ((address >> 8) & 0xFF), 
                (byte) ((address >> 16) & 0xFF),
                (byte) ((address >> 24) & 0xFF)
            };
        }

        /// <inheritdoc />
        public override uint GetPhysicalSize() => 6;

        /// <inheritdoc />
        public override IEnumerable<BaseRelocation> GetRelocations()
        {
            return new[]
            {
                new BaseRelocation(RelocationType.HighLow, new RelativeReference(this, 2)), 
            };
        }
        
    }
}
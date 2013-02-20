using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver
{
    /// <summary>
    /// A class that is able to calculate different kinds of offsets.
    /// </summary>
    public class OffsetConverter
    {
     
        /// <summary>
        /// Creates a new instance of an offset converter.
        /// </summary>
        /// <param name="targetSection"></param>
        public OffsetConverter(Section targetSection)
        {
            if (targetSection == null)
                throw new ArgumentNullException();
            TargetSection = targetSection;

        }

        /// <summary>
        /// Gets the target section.
        /// </summary>
        public Section TargetSection { get; private set; }

        /// <summary>
        /// Transforms a relative virtual address to a virtual address.
        /// </summary>
        /// <param name="rva">The relative virtual address to convert.</param>
        /// <returns></returns>
        public ulong RvaToVa(uint rva)
        {
            return rva + TargetSection.ParentAssembly.ntheader.OptionalHeader.ImageBase;
        }
        /// <summary>
        /// Transforms a virtual address to a relative virtual address.
        /// </summary>
        /// <param name="va">The virtual address to convert.</param>
        /// <returns></returns>
        public uint VaToRva(ulong va)
        {
            return (uint)(va - TargetSection.ParentAssembly.ntheader.OptionalHeader.ImageBase);
        }
        /// <summary>
        /// Transforms a relative virtual address to a physical file offset.
        /// </summary>
        /// <param name="rva">The relative virtual address to convert.</param>
        /// <returns></returns>
        public uint RvaToFileOffset(uint rva)
        {
            return rva - TargetSection.RVA + TargetSection.RawOffset;
        }        
        /// <summary>
        /// Transforms a physical file offset to a relative virtual address.
        /// </summary>
        /// <param name="fileoffset">The physical file offset to convert.</param>
        /// <returns></returns>
        public uint FileOffsetToRva(uint fileoffset)
        {
            return fileoffset - TargetSection.RawOffset + TargetSection.RVA;
        }
        /// <summary>
        /// Transforms a physical file offset to a virtual address.
        /// </summary>
        /// <param name="fileoffset">The physical file offset to convert.</param>
        /// <returns></returns>
        public ulong FileOffsetToVa(uint fileoffset)
        {
            return fileoffset - TargetSection.RawOffset + TargetSection.RVA + TargetSection.ParentAssembly.ntheader.OptionalHeader.ImageBase;
        }
        /// <summary>
        /// Transforms a virtual address to a physical file offset.
        /// </summary>
        /// <param name="va">The virtual address to convert.</param>
        /// <returns></returns>
        public uint VaToFileOffset(ulong va)
        {
            return ((uint)(va - TargetSection.ParentAssembly.ntheader.OptionalHeader.ImageBase)) - TargetSection.RVA + TargetSection.RawOffset;
        }
    }
}

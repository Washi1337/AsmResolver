using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TUP.AsmResolver.ASM;
namespace TUP.AsmResolver
{
    /// <summary>
    /// Represents an offset to a value or structure in an assembly, containing the raw, virtual and relative virtual address.
    /// </summary>
    public class Offset
    {
        /// <summary>
        /// Creates a new instance of an offset.
        /// </summary>
        /// <param name="offset">The raw offset.</param>
        /// <param name="rva">The virtual address that is relative to a section.</param>
        /// <param name="va">The virtual address.</param>
        /// <param name="offsettype">The type of offset.</param>
        public Offset(uint offset, uint rva, ulong va)
        {
            this.FileOffset = offset;
            this.Rva = rva;
            this.Va = va;
        }
        /// <summary>
        /// Creates an instance of an offset by specifying a raw offset. 
        /// </summary>
        /// <param name="rawoffset">The file offset.</param>
        /// <param name="assembly">The assembly containing the offset.</param>
        /// <returns></returns>
        public static Offset FromFileOffset(uint rawoffset, Win32Assembly assembly)
        {
            if (rawoffset == 0)
                return new Offset(0, 0, 0);
            if (assembly == null)
                return new Offset(rawoffset, 0, 0);
            OffsetConverter offsetconverter = CreateConverter(assembly, rawoffset, 1);
            return new Offset(rawoffset, offsetconverter.FileOffsetToRva(rawoffset), offsetconverter.FileOffsetToVa(rawoffset));
        }
        /// <summary>
        /// Creates an instance of an offset by specifying a virtual address.
        /// </summary>
        /// <param name="va">The virtual address.</param>
        /// <param name="assembly">The assembly containing the offset.</param>
        /// <returns></returns>
        public static Offset FromVa(ulong va, Win32Assembly assembly)
        {
            if (va == 0)
                return new Offset(0, 0, 0);
            if (assembly == null)
                return new Offset(0, 0, va);
            OffsetConverter offsetconverter = CreateConverter(assembly, va, 3);
            return new Offset(offsetconverter.VaToFileOffset(va), offsetconverter.VaToRva(va), va);
        }
        /// <summary>
        /// Creates an instance of an offset by specifying a virtual address that is relative to a section.
        /// </summary>
        /// <param name="rva">The relative virtual address.</param>
        /// <param name="assembly">The assembly containing the offset.</param>
        /// <returns></returns>
        public static Offset FromRva(uint rva, Win32Assembly assembly)
        {
            if (rva == 0)
                return new Offset(0, 0, 0);
            if (assembly == null)
                return new Offset(0, rva, 0);
            OffsetConverter offsetconverter = CreateConverter(assembly, rva, 2);
            return new Offset(offsetconverter.RvaToFileOffset(rva), rva, offsetconverter.RvaToVa(rva));
       
        }

        private static OffsetConverter CreateConverter(Win32Assembly assembly, ulong offset, int type)
        {
            OffsetConverter converter;
            
            switch (type)
            {
                case 2:
                    converter = new OffsetConverter(Section.GetSectionByRva(assembly, (uint)offset));
                    break;
                case 3:
                    converter = new OffsetConverter(Section.GetSectionByRva(assembly, (uint)(offset - assembly._ntHeader.OptionalHeader.ImageBase)));
                    break;

                default: // case 1:
                    converter = new OffsetConverter(Section.GetSectionByFileOffset(assembly, (uint)offset));
                    break;
            }
            if (converter.TargetSection == null)
                converter = new OffsetConverter(assembly);

            return converter;
        }

        /// <summary>
        /// Gets the file offset.
        /// </summary>
        public uint FileOffset
        {
            get;
            internal set;
        }
        /// <summary>
        /// Gets the virtual offset that is relative to a section.
        /// </summary>
        public uint Rva
        {
            get;
            internal set;
        }
        /// <summary>
        /// Gets the virtual offset.
        /// </summary>
        public ulong Va
        {
            get;
            internal set;
        }
        /// <summary>
        /// Returns a string representation of the offset value.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ToString("o");
        }
        /// <summary>
        /// Returns a string representation of the offset value.
        /// </summary>
        /// <returns></returns>
        public string ToString(string format)
        {
            ulong returningOffset;

            if (format == "v")
                returningOffset = Va;
            else if (format == "r")
                returningOffset = Rva;
            else if (format == "o")
                returningOffset = FileOffset;
            else
                throw new ArgumentException("Format can be either v (va), r (rva), or o (offset).");

            return returningOffset.ToString("X8"); 

        }
        /// <summary>
        /// Converts the offset to an instruction.
        /// </summary>
        /// <param name="assembly">The assembly that contains the offset</param>
        /// <returns></returns>
        public x86Instruction ToInstruction(Win32Assembly assembly)
        {
            return assembly._disassembler.Disassemble(FileOffset, 10)[0];
        }
        /// <summary>
        /// Converts the offset to an Ascii String pointer.
        /// </summary>     
        /// <param name="assembly">The assembly that contains the offset</param>
        /// <returns></returns>
        public ulong ToAsciiStringPtr(Win32Assembly assembly)
        {

            Section targetsection = Section.GetSectionByRva(assembly, Rva);
            ulong stroffset = Va - assembly._ntHeader.OptionalHeader.ImageBase - targetsection.RVA + targetsection.RawOffset;
           // if (stroffset < 0)
           //     throw new ArgumentException("The target offset is not a valid offset to a string");
            return stroffset;
        }
        /// <summary>
        /// Converts the offset to an Ascii string.
        /// </summary>     
        /// <param name="assembly">The assembly that contains the offset</param>
        /// <returns></returns>
        public string ToAsciiString(Win32Assembly assembly)
        {
            return assembly._peImage.ReadZeroTerminatedString((uint)this.ToAsciiStringPtr(assembly));
        }
       /// <summary>
       /// Converts the offset to an imported or exported method/
        /// </summary>     
        /// <param name="assembly">The assembly that contains the offset</param>
       /// <returns></returns>
        public IMethod ToMethod(Win32Assembly assembly)
        {
            foreach (LibraryReference lib in assembly.LibraryImports)
                foreach (ImportMethod method in lib.ImportMethods)
                {
                    if (method.RVA + assembly._ntHeader.OptionalHeader.ImageBase == Va)
                        return method;
                }
            foreach (ExportMethod method in assembly.LibraryExports)
            {
                if (Va == method.RVA + assembly._ntHeader.OptionalHeader.ImageBase)
                    return method;
            }

            throw new ArgumentException("No matching method has been found.");

        }


    }
}

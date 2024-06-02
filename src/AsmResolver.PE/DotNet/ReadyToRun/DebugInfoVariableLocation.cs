using System;
using AsmResolver.PE.File;

namespace AsmResolver.PE.DotNet.ReadyToRun
{
    /// <summary>
    /// Describes the location of a native variable in a precompiled method.
    /// </summary>
    public readonly struct DebugInfoVariableLocation : IEquatable<DebugInfoVariableLocation>
    {
        /// <summary>
        /// Creates a new location description for a variable.
        /// </summary>
        /// <param name="type">The type of location.</param>
        /// <param name="data1">The first parameter further specifying the location.</param>
        public DebugInfoVariableLocation(DebugInfoVariableLocationType type, uint data1)
        {
            Type = type;
            Data1 = data1;
            Data2 = 0;
            Data3 = 0;
        }

        /// <summary>
        /// Creates a new location description for a variable.
        /// </summary>
        /// <param name="type">The type of location.</param>
        /// <param name="data1">The first parameter further specifying the location.</param>
        /// <param name="data2">The second parameter further specifying the location.</param>
        public DebugInfoVariableLocation(DebugInfoVariableLocationType type, uint data1, uint data2)
        {
            Type = type;
            Data1 = data1;
            Data2 = data2;
            Data3 = 0;
        }

        /// <summary>
        /// Creates a new location description for a variable.
        /// </summary>
        /// <param name="type">The type of location.</param>
        /// <param name="data1">The first parameter further specifying the location.</param>
        /// <param name="data2">The second parameter further specifying the location.</param>
        /// <param name="data3">The third parameter further specifying the location.</param>
        public DebugInfoVariableLocation(DebugInfoVariableLocationType type, uint data1, uint data2, uint data3)
        {
            Type = type;
            Data1 = data1;
            Data2 = data2;
            Data3 = data3;
        }

        /// <summary>
        /// Gets the type of location.
        /// </summary>
        public DebugInfoVariableLocationType Type
        {
            get;
        }

        /// <summary>
        /// Gets the first parameter further specifying the location.
        /// </summary>
        public uint Data1
        {
            get;
        }

        /// <summary>
        /// Gets the second parameter further specifying the location.
        /// </summary>
        public uint Data2
        {
            get;
        }

        /// <summary>
        /// Gets the third parameter further specifying the location.
        /// </summary>
        public uint Data3
        {
            get;
        }

        internal static DebugInfoVariableLocation FromReader(PEReaderContext context, ref NibbleReader reader)
        {
            var type = (DebugInfoVariableLocationType) reader.Read3BitEncodedUInt();

            uint data1 = 0, data2 = 0, data3 = 0;

            switch (type)
            {
                case DebugInfoVariableLocationType.Register:
                case DebugInfoVariableLocationType.RegisterFP:
                case DebugInfoVariableLocationType.RegisterByReference:
                case DebugInfoVariableLocationType.FPStack:
                case DebugInfoVariableLocationType.FixedVA:
                    data1 = reader.Read3BitEncodedUInt();
                    break;

                case DebugInfoVariableLocationType.Stack:
                case DebugInfoVariableLocationType.StackByReference:
                    data1 = reader.Read3BitEncodedUInt();
                    data2 = ToStackOffset(reader.Read3BitEncodedUInt());
                    break;

                case DebugInfoVariableLocationType.RegisterRegister:
                    data1 = reader.Read3BitEncodedUInt();
                    data2 = reader.Read3BitEncodedUInt();
                    break;

                case DebugInfoVariableLocationType.RegisterStack:
                    data1 = reader.Read3BitEncodedUInt();
                    data2 = reader.Read3BitEncodedUInt();
                    data3 = ToStackOffset(reader.Read3BitEncodedUInt());
                    break;

                case DebugInfoVariableLocationType.StackRegister:
                    data1 = ToStackOffset(reader.Read3BitEncodedUInt());
                    data2 = reader.Read3BitEncodedUInt();
                    data3 = reader.Read3BitEncodedUInt();
                    break;

                case DebugInfoVariableLocationType.Stack2:
                    data1 = reader.Read3BitEncodedUInt();
                    data2 = ToStackOffset(reader.Read3BitEncodedUInt());
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return new DebugInfoVariableLocation(type, data1, data2, data3);

            uint ToStackOffset(uint value)
            {
                if (context.File.FileHeader.Machine == MachineType.I386)
                    return value * sizeof(uint);

                return value;
            }
        }

        internal void Write(MachineType machineType, ref NibbleWriter writer)
        {
            writer.Write3BitEncodedUInt((uint) Type);

            switch (Type)
            {
                case DebugInfoVariableLocationType.Register:
                case DebugInfoVariableLocationType.RegisterFP:
                case DebugInfoVariableLocationType.RegisterByReference:
                case DebugInfoVariableLocationType.FPStack:
                case DebugInfoVariableLocationType.FixedVA:
                    writer.Write3BitEncodedUInt(Data1);
                    break;

                case DebugInfoVariableLocationType.Stack:
                case DebugInfoVariableLocationType.StackByReference:
                    writer.Write3BitEncodedUInt(Data1);
                    writer.Write3BitEncodedUInt(EncodeStackOffset(Data2));
                    break;

                case DebugInfoVariableLocationType.RegisterRegister:
                    writer.Write3BitEncodedUInt(Data1);
                    writer.Write3BitEncodedUInt(Data2);
                    break;

                case DebugInfoVariableLocationType.RegisterStack:
                    writer.Write3BitEncodedUInt(Data1);
                    writer.Write3BitEncodedUInt(Data2);
                    writer.Write3BitEncodedUInt(EncodeStackOffset(Data3));
                    break;

                case DebugInfoVariableLocationType.StackRegister:
                    writer.Write3BitEncodedUInt(EncodeStackOffset(Data1));
                    writer.Write3BitEncodedUInt(Data2);
                    writer.Write3BitEncodedUInt(Data3);
                    break;

                case DebugInfoVariableLocationType.Stack2:
                    writer.Write3BitEncodedUInt(Data1);
                    writer.Write3BitEncodedUInt(EncodeStackOffset(Data2));
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return;

            uint EncodeStackOffset(uint value)
            {
                if (machineType == MachineType.I386)
                    return value / sizeof(uint);
                return value;
            }
        }

        /// <inheritdoc />
        public bool Equals(DebugInfoVariableLocation other)
        {
            return Type == other.Type
                && Data1 == other.Data1
                && Data2 == other.Data2
                && Data3 == other.Data3;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is DebugInfoVariableLocation other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (int) Type;
                hashCode = (hashCode * 397) ^ (int) Data1;
                hashCode = (hashCode * 397) ^ (int) Data2;
                hashCode = (hashCode * 397) ^ (int) Data3;
                return hashCode;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            switch (Type)
            {
                case DebugInfoVariableLocationType.Register:
                case DebugInfoVariableLocationType.RegisterFP:
                case DebugInfoVariableLocationType.RegisterByReference:
                case DebugInfoVariableLocationType.FPStack:
                case DebugInfoVariableLocationType.FixedVA:
                    return $"{Type} {Data1:X}";

                case DebugInfoVariableLocationType.Stack:
                case DebugInfoVariableLocationType.StackByReference:
                case DebugInfoVariableLocationType.RegisterRegister:
                case DebugInfoVariableLocationType.Stack2:
                    return $"{Type} {Data1:X}, {Data2:X}";

                case DebugInfoVariableLocationType.RegisterStack:
                case DebugInfoVariableLocationType.StackRegister:
                    return $"{Type} {Data1:X}, {Data2:X}, {Data3:X}";

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}

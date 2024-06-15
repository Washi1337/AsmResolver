using System;
using AsmResolver.PE.File;

namespace AsmResolver.PE.DotNet.ReadyToRun
{
    /// <summary>
    /// Provides debugging information about a single native variable in a precompiled method.
    /// </summary>
    public readonly struct DebugInfoVariable : IEquatable<DebugInfoVariable>
    {
        /// <summary>
        /// The special VARARGS handle variable index.
        /// </summary>
        public const uint VarArgsHandle = 0xFFFFFFFF;

        /// <summary>
        /// The special return buffer variable index.
        /// </summary>
        public const uint ReturnBuffer = 0xFFFFFFFE;

        /// <summary>
        /// The special type context variable index.
        /// </summary>
        public const uint TypeContext = 0xFFFFFFFD;

        internal const uint Unknown = 0xFFFFFFFC;

        /// <summary>
        /// Creates new debugging information for the specified native variable.
        /// </summary>
        /// <param name="startOffset">The start offset the variable is live at.</param>
        /// <param name="endOffset">The (exclusive) end offset the variable is live at.</param>
        /// <param name="index">The index of the variable.</param>
        /// <param name="location">The location of the variable.</param>
        public DebugInfoVariable(uint startOffset, uint endOffset, uint index, DebugInfoVariableLocation location)
        {
            StartOffset = startOffset;
            EndOffset = endOffset;
            Index = index;
            Location = location;
        }

        /// <summary>
        /// Gets the start offset the variable is live at.
        /// </summary>
        public uint StartOffset
        {
            get;
        }

        /// <summary>
        /// Gets the (exclusive) end offset the variable is live at.
        /// </summary>
        public uint EndOffset
        {
            get;
        }

        /// <summary>
        /// Gets the index of the variable.
        /// </summary>
        public uint Index
        {
            get;
        }

        /// <summary>
        /// Gets the location of the variable.
        /// </summary>
        public DebugInfoVariableLocation Location
        {
            get;
        }

        internal static DebugInfoVariable FromReader(PEReaderContext context, ref NibbleReader reader)
        {
            uint start = reader.Read3BitEncodedUInt();
            uint end = start + reader.Read3BitEncodedUInt();
            uint index = reader.Read3BitEncodedUInt() + Unknown;
            var location = DebugInfoVariableLocation.FromReader(context, ref reader);

            return new DebugInfoVariable(start, end, index, location);
        }

        internal void Write(MachineType machineType, ref NibbleWriter writer)
        {
            writer.Write3BitEncodedUInt(StartOffset);
            writer.Write3BitEncodedUInt(EndOffset - StartOffset);
            writer.Write3BitEncodedUInt(Index - Unknown);
            Location.Write(machineType, ref writer);
        }

        /// <inheritdoc />
        public bool Equals(DebugInfoVariable other)
        {
            return StartOffset == other.StartOffset
                && EndOffset == other.EndOffset
                && Index == other.Index
                && Location.Equals(other.Location);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is DebugInfoVariable other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (int) StartOffset;
                hashCode = (hashCode * 397) ^ (int) EndOffset;
                hashCode = (hashCode * 397) ^ (int) Index;
                hashCode = (hashCode * 397) ^ Location.GetHashCode();
                return hashCode;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{StartOffset:X4}, {EndOffset:X4}), {nameof(Index)}: {Index}, {nameof(Location)}: {Location}";
        }
    }

}

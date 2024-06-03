using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using AsmResolver.Collections;
using AsmResolver.IO;
using AsmResolver.PE.File;
using AsmResolver.Shims;

namespace AsmResolver.PE.DotNet.ReadyToRun
{
    /// <summary>
    /// Provides additional debug information to a precompiled method body.
    /// </summary>
    public class DebugInfo : SegmentBase
    {
        private IList<DebugInfoBounds>? _bounds;
        private IList<DebugInfoVariable>? _variables;
        private byte[]? _serialized;
        private bool _is32Bit;

        /// <summary>
        /// Gets a collection of bounds information associated to the method.
        /// </summary>
        public IList<DebugInfoBounds> Bounds
        {
            get
            {
                if (_bounds is null)
                    Interlocked.CompareExchange(ref _bounds, GetBounds(), null);
                return _bounds;
            }
        }

        /// <summary>
        /// Gets a collection of native variable information associated to the method.
        /// </summary>
        public IList<DebugInfoVariable> Variables
        {
            get
            {
                if (_variables is null)
                    Interlocked.CompareExchange(ref _variables, GetVariables(), null);
                return _variables;
            }
        }

        /// <summary>
        /// Obtains the bounds information of the method.
        /// </summary>
        /// <returns>The bounds.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Bounds"/> property.
        /// </remarks>
        protected virtual IList<DebugInfoBounds> GetBounds() => new List<DebugInfoBounds>();

        /// <summary>
        /// Obtains the native variable information of the method.
        /// </summary>
        /// <returns>The variables.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Variables"/> property.
        /// </remarks>
        protected virtual IList<DebugInfoVariable> GetVariables() => new List<DebugInfoVariable>();

        /// <inheritdoc />
        public override void UpdateOffsets(in RelocationParameters parameters)
        {
            base.UpdateOffsets(in parameters);
            _serialized = Serialize();
            _is32Bit = parameters.Is32Bit;
        }

        /// <inheritdoc />
        public override uint GetPhysicalSize()
        {
            _serialized ??= Serialize();
            return (uint) _serialized.Length;
        }

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
        {
            _serialized ??= Serialize();
            writer.WriteBytes(_serialized);
        }

        private byte[] Serialize()
        {
            byte[] bounds = SerializeBounds();
            byte[] variables = SerializeVariables();

            using var stream = new MemoryStream();
            var writer = new BinaryStreamWriter(stream);

            NativeFormat.EncodeUnsigned(writer, 0); // lookback

            var nibbleWriter = new NibbleWriter(writer);
            nibbleWriter.Write3BitEncodedUInt((uint) bounds.Length);
            nibbleWriter.Write3BitEncodedUInt((uint) variables.Length);
            nibbleWriter.Flush();

            writer.WriteBytes(bounds);
            writer.WriteBytes(variables);

            return stream.ToArray();
        }

        private byte[] SerializeBounds()
        {
            if (Bounds.Count == 0)
                return ArrayShim.Empty<byte>();

            using var stream = new MemoryStream();
            var writer = new NibbleWriter(new BinaryStreamWriter(stream));

            writer.Write3BitEncodedUInt((uint) Bounds.Count);

            uint nativeOffset = 0;
            for (int i = 0; i < Bounds.Count; i++)
            {
                var bound = Bounds[i];

                writer.Write3BitEncodedUInt(bound.NativeOffset - nativeOffset);
                writer.Write3BitEncodedUInt(bound.ILOffset - DebugInfoBounds.EpilogOffset);
                writer.Write3BitEncodedUInt((uint) bound.Attributes);

                nativeOffset = bound.NativeOffset;
            }

            writer.Flush();
            return stream.ToArray();
        }

        private byte[] SerializeVariables()
        {
            if (Variables.Count == 0)
                return ArrayShim.Empty<byte>();

            using var stream = new MemoryStream();
            var writer = new NibbleWriter(new BinaryStreamWriter(stream));

            writer.Write3BitEncodedUInt((uint) Variables.Count);

            for (int i = 0; i < Variables.Count; i++)
                Variables[i].Write(_is32Bit ? MachineType.I386 : MachineType.Amd64, ref writer);

            writer.Flush();
            return stream.ToArray();
        }

    }

}

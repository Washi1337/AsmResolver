using AsmResolver.PE.Code;
using AsmResolver.PE.Relocations;

namespace AsmResolver.DotNet.Code.Native
{
    /// <summary>
    /// Provides an implementation for the <see cref="IMethodBodySerializer"/> that serializes method bodies written
    /// in a native and unmanaged language.
    /// </summary>
    public class NativeMethodBodySerializer : IMethodBodySerializer
    {
        /// <inheritdoc />
        public ISegmentReference SerializeMethodBody(MethodBodySerializationContext context, MethodDefinition method)
        {
            // We treat any non-conventional bounded method body as a plain native method body that we
            // don't need to process further.
            if (method.MethodBody is {Address.IsBounded: true} plainBody)
                return plainBody.Address;

            // We only support special treatment of native method bodies.
            if (method.MethodBody is not NativeMethodBody nativeMethodBody)
                return SegmentReference.Null;

            var provider = context.SymbolsProvider;

            // Create new raw code segment containing the native code.
            var segment = new DataSegment(nativeMethodBody.Code).AsPatchedSegment();

            // Process address fixups.
            for (int i = 0; i < nativeMethodBody.AddressFixups.Count; i++)
            {
                // Import symbol.
                var fixup = nativeMethodBody.AddressFixups[i];
                var symbol = FinalizeSymbol(segment, provider, fixup.Symbol);

                // Fixup address.
                segment.Patch(fixup.Offset, fixup.Type, symbol);

                // Add base relocation when necessary.
                AddBaseRelocations(segment, provider, fixup);
            }

            return segment.ToReference();
        }

        /// <summary>
        /// Registers base relocations for the provided address fixup, if required.
        /// </summary>
        /// <param name="segment">The code segment that is being constructed.</param>
        /// <param name="provider">The object responsible for providing symbols referenced by the native method body.</param>
        /// <param name="fixup">The fixup to build base relocations for.</param>
        protected virtual void AddBaseRelocations(ISegment segment, INativeSymbolsProvider provider, AddressFixup fixup)
        {
            switch (fixup.Type)
            {
                case AddressFixupType.Absolute32BitAddress:
                    provider.RegisterBaseRelocation(new BaseRelocation(
                        RelocationType.HighLow,
                        segment.ToReference((int) fixup.Offset)));
                    break;

                case AddressFixupType.Absolute64BitAddress:
                    provider.RegisterBaseRelocation(new BaseRelocation(
                        RelocationType.Dir64,
                        segment.ToReference((int) fixup.Offset)));
                    break;
            }
        }

        /// <summary>
        /// Ensures the right symbol is used within the method body.
        /// </summary>
        /// <param name="result">The code segment that is being constructed.</param>
        /// <param name="provider">The object responsible for providing symbols referenced by the native method body.</param>
        /// <param name="symbol">The symbol to reference.</param>
        /// <returns>The symbol.</returns>
        protected virtual ISymbol FinalizeSymbol(ISegment result, INativeSymbolsProvider provider, ISymbol symbol)
        {
            if (symbol is NativeLocalSymbol local)
                return new Symbol(result.ToReference((int) local.Offset));

            return provider.ImportSymbol(symbol);
        }
    }
}

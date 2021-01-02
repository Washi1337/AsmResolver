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
            if (!(method.MethodBody is NativeMethodBody nativeMethodBody))
                return SegmentReference.Null;

            var provider = context.SymbolsProvider;

            // Create new raw code segment containing the native code.
            var segment = new CodeSegment(provider.ImageBase, nativeMethodBody.Code);

            // Process fixups.
            for (int i = 0; i < nativeMethodBody.AddressFixups.Count; i++)
            {
                // Import symbol.
                var fixup = nativeMethodBody.AddressFixups[i];
                var symbol = provider.ImportSymbol(fixup.Symbol);

                // Create new fixup with imported symbol.
                segment.AddressFixups.Add(new AddressFixup(fixup.Offset, fixup.Type, symbol));

                // Add base relocation when necessary.
                // TODO: keep architecture into account..
                if (fixup.Type == AddressFixupType.Absolute32BitAddress)
                {
                    var relocation = new BaseRelocation(RelocationType.HighLow,
                        new RelativeReference(segment, (int) fixup.Offset));
                    provider.RegisterBaseRelocation(relocation);
                }
            }

            return new SegmentReference(segment);
        }
    }
}

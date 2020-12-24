using AsmResolver.PE.Code;

namespace AsmResolver.DotNet.Code.Native
{
    public class NativeMethodBodySerializer : IMethodBodySerializer
    {
        /// <inheritdoc />
        public ISegmentReference SerializeMethodBody(MethodBodySerializationContext context, MethodDefinition method)
        {
            if (!(method.MethodBody is NativeMethodBody nativeMethodBody))
                return SegmentReference.Null;

            var provider = context.SymbolsProvider;
            
            var segment = new CodeSegment(provider.ImageBase, nativeMethodBody.Code);

            foreach (var fixup in nativeMethodBody.AddressFixups)
            {
                var symbol = provider.ImportSymbol(fixup.Symbol);
                segment.AddressFixups.Add(new AddressFixup(fixup.Offset, fixup.Type, symbol));
            }

            return new SegmentReference(segment);
        }
    }
}

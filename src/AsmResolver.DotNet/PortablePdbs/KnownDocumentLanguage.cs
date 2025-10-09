using System;

namespace AsmResolver.DotNet.PortablePdbs
{
    public static class KnownDocumentLanguage
    {
        public static Guid CSharp { get; } = new("3f5162f8-07c6-11d3-9053-00c04fa302a1");
        public static Guid VisualBasic { get; } = new("3a12d0b8-c26c-11d0-b442-00a0244a1dd2");
        public static Guid FSharp { get; } = new("ab4f38c9-b6e6-43ba-be3b-58080b2ccce3");
    }
}

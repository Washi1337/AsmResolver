using System;

namespace AsmResolver.DotNet.TestCases.Fields
{
    public class RVAField
    {
        // the RVA used to initialize the array in InitialValues.cs is placed before this one
        // since that data is 50 bytes, if we don't explicitly check the alignment requirements for this RVA, it will be misaligned by 2
        public static ReadOnlySpan<long> ForRVA => new long[] { 1, 2, 3, 4 };
    }
}

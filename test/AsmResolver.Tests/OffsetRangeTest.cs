using Xunit;

namespace AsmResolver.Tests
{
    public class OffsetRangeTest
    {
        [Theory]
        [InlineData(0, 100, 10, true)]
        [InlineData(0, 100, 100, false)]
        [InlineData(0, 100, 101, false)]
        public void ContainsOffset(uint start, uint end, uint offset, bool expected)
        {
            var range = new OffsetRange(start, end);
            Assert.Equal(expected, range.Contains(offset));
        }
        
        [Theory]
        [InlineData(0, 100, 0, 0, true)]
        [InlineData(0, 100, 99, 99, true)]
        [InlineData(0, 100, 25, 75, true)]
        [InlineData(0, 100, 25, 125, false)]
        [InlineData(50, 100, 25, 75, false)]
        [InlineData(0, 100, 100, 125, false)]
        public void ContainsRange(uint start, uint end, uint subStart, uint subEnd, bool expected)
        {
            var range = new OffsetRange(start, end);
            var subRange = new OffsetRange(subStart, subEnd);
            Assert.Equal(expected, range.Contains(subRange));
        }
        
        [Theory]
        [InlineData(0, 100, 0, 0, true)]
        [InlineData(0, 100, 99, 99, true)]
        [InlineData(0, 100, 25, 75, true)]
        [InlineData(0, 100, 25, 125, true)]
        [InlineData(50, 100, 25, 75, true)]
        [InlineData(0, 100, 100, 125, true)]
        [InlineData(0, 100, 101, 125, false)]
        [InlineData(0, 100, 200, 225, false)]
        public void IntersectsRange(uint start, uint end, uint subStart, uint subEnd, bool expected)
        {
            var range = new OffsetRange(start, end);
            var subRange = new OffsetRange(subStart, subEnd);
            Assert.Equal(expected, range.Intersects(subRange));
        }
        
        [Theory]
        [InlineData(0, 100, 25, 75, 25, 75)]
        [InlineData(0, 100, 25, 125, 25, 100)]
        [InlineData(25, 125, 0, 100, 25, 100)]
        public void Intersection(uint start1, uint end1, uint start2, uint end2, uint expectedStart, uint expectedEnd)
        {
            var range = new OffsetRange(start1, end1);
            var subRange = new OffsetRange(start2, end2);
            Assert.Equal((expectedStart, expectedEnd), range.Intersect(subRange));
        }
    }
}
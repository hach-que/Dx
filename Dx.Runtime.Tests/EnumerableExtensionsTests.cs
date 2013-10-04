using System.Linq;
using Xunit;

namespace Dx.Runtime.Tests
{
    public class EnumerableExtensionsTests
    {
        [Fact]
        public void TestAppend()
        {
            var a = new[] { 1, 2, 3 };
            var b = new[] { 4, 5, 6 };
            
            var result = a.Append(b).ToArray();
            Assert.Contains(1, result);
            Assert.Contains(2, result);
            Assert.Contains(3, result);
            Assert.Contains(4, result);
            Assert.Contains(5, result);
            Assert.Contains(6, result);
            Assert.Equal(6, result.Length);
            Assert.Equal(1, result[0]);
            Assert.Equal(2, result[1]);
            Assert.Equal(3, result[2]);
            Assert.Equal(4, result[3]);
            Assert.Equal(5, result[4]);
            Assert.Equal(6, result[5]);
        }
    }
}


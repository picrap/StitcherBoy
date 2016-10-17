#region Arx One
// Stitcher Boy - a small library to help building post-build taks
// https://github.com/picrap/StitcherBoy
// MIT License - http://opensource.org/licenses/MIT
#endregion

namespace StitcherBoyTests
{
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using StitcherBoy.Utility;

    [TestClass]
    public class StringTest
    {
        [TestMethod]
        public void SplitArgumentsNoQuoteTest()
        {
            var a = "a b c".SplitArguments().ToArray();
            Assert.AreEqual(3, a.Length);
            Assert.AreEqual("a", a[0]);
            Assert.AreEqual("b", a[1]);
            Assert.AreEqual("c", a[2]);
        }

        [TestMethod]
        public void SplitArgumentsQuotesTest()
        {
            var a = "d \"e f\" g".SplitArguments().ToArray();
            Assert.AreEqual(3, a.Length);
            Assert.AreEqual("d", a[0]);
            Assert.AreEqual("\"e f\"", a[1]);
            Assert.AreEqual("g", a[2]);
        }

        [TestMethod]
        public void SplitArgumentsNonSpaceQuotesTest()
        {
            var a = "h i\"j k\" l".SplitArguments().ToArray();
            Assert.AreEqual(3, a.Length);
            Assert.AreEqual("h", a[0]);
            Assert.AreEqual("i\"j k\"", a[1]);
            Assert.AreEqual("l", a[2]);
        }
    }
}
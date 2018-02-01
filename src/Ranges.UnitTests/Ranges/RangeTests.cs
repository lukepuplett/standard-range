using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Evoq.Ranges.UnitTests
{
    [TestClass]
    public class RangeTests
    {
        protected virtual Range<int> CreateTestRange(int start, int stop)
        {
            return new Range<int>(start, stop);
        }

        [TestMethod]
        public void Range_Constructor_StartLessThanStop_CreatesValidRange()
        {
            var r = this.CreateTestRange(0, 10);

            Assert.AreEqual(0, r.Start);
            Assert.AreEqual(10, r.Stop);
        }

        [TestMethod]
        public void Range_Constructor_StartMoreThanStop_CreatesValidRange()
        {
            var r = this.CreateTestRange(10, 0);

            Assert.AreEqual(10, r.Start);
            Assert.AreEqual(0, r.Stop);
        }

        [TestMethod]
        public void Range_0_1000_IsSpot_ReturnsFalse()
        {
            var r = this.CreateTestRange(0, 1000);

            Assert.IsFalse(r.IsSpot);
        }

        [TestMethod]
        public void Range_1000_1000_IsSpot_ReturnsTrue()
        {
            var r = this.CreateTestRange(1000, 1000);

            Assert.IsTrue(r.IsSpot);
        }

        [TestMethod]
        public void Range_0_1000_IsIncreasing_ReturnsTrue()
        {
            var r = this.CreateTestRange(0, 1000);

            Assert.IsTrue(r.IsIncreasing);
        }

        [TestMethod]
        public void Range_1000_1000_IsIncreasing_ReturnsFalse()
        {
            var r = this.CreateTestRange(1000, 1000);

            Assert.IsFalse(r.IsIncreasing);
        }

        [TestMethod]
        public void Range_1000_1000_IsEnveloping_1000_ReturnsTrue()
        {
            var r = this.CreateTestRange(1000, 1000);

            Assert.IsTrue(r.IsEnveloping(1000));
        }

        [TestMethod]
        public void Range_1000_1000_IsEnveloping_500_ReturnsFalse()
        {
            var r = this.CreateTestRange(1000, 1000);

            Assert.IsFalse(r.IsEnveloping(500));
        }

        [TestMethod]
        public void Range_0_1000_IsEnveloping_500_ReturnsTrue()
        {
            var r = this.CreateTestRange(0, 1000);

            Assert.IsTrue(r.IsEnveloping(500));
        }

        [TestMethod]
        public void Range_0_1000_IsEnveloping_0_ReturnsTrue()
        {
            var r = this.CreateTestRange(0, 1000);

            Assert.IsTrue(r.IsEnveloping(0));
        }

        [TestMethod]
        public void Range_0_1000_IsEnveloping_1000_ReturnsTrue()
        {
            var r = this.CreateTestRange(0, 1000);

            Assert.IsTrue(r.IsEnveloping(1000));
        }

        [TestMethod]
        public void Range_1000_0_IsEnveloping_500_ReturnsTrue()
        {
            var r = this.CreateTestRange(1000, 0);

            Assert.IsTrue(r.IsEnveloping(500));
        }

        [TestMethod]
        public void Range_1000_0_IsEnveloping_0_ReturnsTrue()
        {
            var r = this.CreateTestRange(1000, 0);

            Assert.IsTrue(r.IsEnveloping(0));
        }

        [TestMethod]
        public void Range_1000_0_IsEnveloping_1000_ReturnsTrue()
        {
            var r = this.CreateTestRange(1000, 0);

            Assert.IsTrue(r.IsEnveloping(1000));
        }

        [TestMethod]
        public void Range_0_1000_IsEnveloping_1001_ReturnsFalse()
        {
            var r = this.CreateTestRange(0, 1000);

            Assert.IsFalse(r.IsEnveloping(1001));
        }

        [TestMethod]
        public void Range_1000_0_IsEnveloping_1001_ReturnsTrue()
        {
            var r = this.CreateTestRange(1000, 0);

            Assert.IsFalse(r.IsEnveloping(1001));
        }

        [TestMethod]
        public void Range_0_1000_IsEnveloping_0_1000_ReturnsTrue()
        {
            var r = this.CreateTestRange(0, 1000);

            Assert.IsTrue(r.IsEnveloping(0, 1000));
        }

        [TestMethod]
        public void Range_1000_0_IsEnveloping_0_1000_ReturnsTrue()
        {
            var r = this.CreateTestRange(1000, 0);

            Assert.IsTrue(r.IsEnveloping(0, 1000));
        }

        [TestMethod]
        public void Range_0_1000_IsEnveloping_0_2000_ReturnsFalse()
        {
            var r = this.CreateTestRange(0, 1000);

            Assert.IsFalse(r.IsEnveloping(0, 2000));
        }

        [TestMethod]
        public void Range_0_1000_IsOverlapping_0_ReturnsTrue()
        {
            var r = this.CreateTestRange(0, 1000);

            Assert.IsTrue(r.IsOverlapping(0));
        }

        [TestMethod]
        public void Range_1000_0_IsOverlapping_0_ReturnsTrue()
        {
            var r = this.CreateTestRange(1000, 0);

            Assert.IsTrue(r.IsOverlapping(0));
        }

        [TestMethod]
        public void Range_1000_0_IsOverlapping_Minus12_ReturnsFalse()
        {
            var r = this.CreateTestRange(1000, 0);

            Assert.IsFalse(r.IsOverlapping(-12));
        }

        [TestMethod]
        public void Range_0_1000_IsOverlapping_0_1000_ReturnsTrue()
        {
            var r = this.CreateTestRange(0, 1000);

            Assert.IsTrue(r.IsOverlapping(0, 1000));
        }

        [TestMethod]
        public void Range_0_1000_IsOverlapping_1000_1000_ReturnsTrue()
        {
            var r = this.CreateTestRange(0, 1000);

            Assert.IsTrue(r.IsOverlapping(1000, 1000));
        }

        [TestMethod]
        public void Range_0_1000_IsOverlapping_1001_1003_ReturnsFalse()
        {
            var r = this.CreateTestRange(0, 1000);

            Assert.IsFalse(r.IsOverlapping(1001, 1003));
        }

        [TestMethod]
        public void Range_500_600_IsOverlapping_1000_550_ReturnsTrue()
        {
            var r = this.CreateTestRange(500, 600);

            Assert.IsTrue(r.IsOverlapping(1000, 550));
        }

        [TestMethod]
        public void Range_3_3_IsOverlapping_1_4_ReturnsTrue()
        {
            var r = this.CreateTestRange(3, 3);

            Assert.IsTrue(r.IsOverlapping(1, 4));
        }
    }
}

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Evoq.Ranges.UnitTests
{
    [TestClass]
    public class TimeRangeTests
    {
        public static DateTime PointInTime1 = new DateTime(2020, 1, 1, 10, 00, 00);
        public static DateTime PointInTime2 = new DateTime(2030, 1, 1, 10, 00, 00);
        public static DateTime PointInTime3 = new DateTime(2030, 1, 1, 12, 00, 00);
        public static DateTime PointInTime4 = new DateTime(2030, 1, 1, 13, 00, 00);

        [TestMethod]
        public void TimeRange_Constructor_UsingDateTimes_ReturnsValidRange()
        {
            var r = new TimeRange(PointInTime1, PointInTime2);

            Assert.AreEqual(PointInTime1, r.Start);
            Assert.AreEqual(PointInTime2, r.Stop);
        }

        [TestMethod]
        public void TimeRange_Constructor_UsingDurationMinutes_ReturnsValidRange()
        {
            var r = new TimeRange(PointInTime1, 20);

            Assert.AreEqual(PointInTime1, r.Start);
            Assert.AreEqual(PointInTime1.Add(TimeSpan.FromMinutes(20)), r.Stop);
        }

        [TestMethod]
        public void TimeRange_Constructor_UsingDurationTimeSpan_ReturnsValidRange()
        {
            var r = new TimeRange(PointInTime1, TimeSpan.FromMinutes(20));

            Assert.AreEqual(PointInTime1, r.Start);
            Assert.AreEqual(PointInTime1.Add(TimeSpan.FromMinutes(20)), r.Stop);
        }

        [TestMethod]
        public void TimeRange_Constructor_ThrowsOnWonkyOffsets()
        {
            try
            {
                var _ = new TimeRange(new DateTimeOffset(new DateTime(2017, 12, 25), TimeSpan.Zero), new DateTimeOffset(new DateTime(2017, 12, 26), TimeSpan.FromHours(1)));
            }
            catch (ArgumentException)
            {
                Assert.IsTrue(true);
                return;
            }
        }

        [TestMethod]
        public void TimeRange_FromDay_ReturnsValidRange()
        {
            var r = TimeRange.FromDay(PointInTime1);

            Assert.AreEqual(PointInTime1.Date, r.Start);
            Assert.AreEqual(PointInTime1.Date, r.Stop.Date);
            Assert.AreEqual(24 * 60, r.DurationMins);
        }

        [TestMethod]
        public void TimeRange_FromDaysCovered_ReturnsValidRange()
        {
            var r = TimeRange.FromDaysCovered(PointInTime1.Date, 40);

            Assert.AreEqual(40 * 24 * 60, r.DurationMins);
        }

        [TestMethod]
        public void TimeRange_FromMonth_ReturnsExpectedRange()
        {
            var range = TimeRange.FromMonthsCovered(2017, 12, 1);

            Assert.AreEqual(range.Start.Year, 2017);
            Assert.AreEqual(range.Stop.Year, 2017);

            Assert.AreEqual(range.Start.Month, 12);
            Assert.AreEqual(range.Stop.Month, 12);

            Assert.AreEqual(range.Start.Day, 1);
            Assert.AreEqual(range.Stop.Day, 31);
        }

        [TestMethod]
        public void TimeRange_FromMonth_ReturnsFalseForIsEnvelopingMidnightTheNextMonth()
        {
            var range = TimeRange.FromMonthsCovered(2017, 12, 1);

            Assert.IsFalse(range.IsEnveloping(new DateTimeOffset(new DateTime(2018, 1, 1))));
        }

        [TestMethod]
        public void TimeRange_FromDatesCovered_ReturnsExpectedInstance()
        {
            var a = new DateTime(2017, 12, 1, 13, 45, 00);
            var b = new DateTime(2017, 12, 31, 09, 00, 00);
            //
            //  Times specified just for test. Expecting only the date information to be retained.

            var range = TimeRange.FromDatesCovered(a, b, TimeSpan.Zero);

            Assert.IsTrue(range.IsIncreasing);
            Assert.AreEqual(1, range.Start.Day);
            Assert.AreEqual(31, range.Stop.Day);
        }

        [TestMethod]
        public void TimeRange_FromDatesCovered_ReturnsExpectedInstanceWhenDecreasing()
        {
            var a = new DateTime(2017, 12, 31, 09, 00, 00);
            var b = new DateTime(2017, 12, 1, 13, 45, 00);
            //
            //  Times specified just for test. Expecting only the date information to be retained.

            var range = TimeRange.FromDatesCovered(a, b, TimeSpan.Zero);

            Assert.IsFalse(range.IsIncreasing);
            Assert.AreEqual(31, range.Start.Day);
            Assert.AreEqual(1, range.Stop.Day);
        }

        [TestMethod]
        public void TimeRange_FromDatesCovered_ReturnsExpectedInstanceWithExclusiveStop()
        {
            var a = new DateTime(2017, 12, 1, 13, 45, 00);
            var b = new DateTime(2018, 1, 1, 09, 00, 00);
            //
            //  Times specified just for test. Expecting only the date information to be retained.

            var range = TimeRange.FromDatesCovered(a, b, TimeSpan.Zero, true);

            Assert.AreEqual(1, range.Start.Day);
            Assert.AreEqual(31, range.Stop.Day);
        }

        [TestMethod]
        public void TimeRange_Last_ReturnsValidRange()
        {
            var r = TimeRange.Last(TimeSpan.FromHours(1));

            var elapsed = DateTimeOffset.Now - r.Stop;
            Assert.IsTrue(elapsed.TotalMilliseconds < 10);

            Assert.AreEqual(1 * 60 * 60 * 1000, r.Duration.TotalMilliseconds);
            Assert.IsTrue(r.IsIncreasing);
        }

        [TestMethod]
        public void TimeRange_Next_ReturnsValidRange()
        {
            var r = TimeRange.Next(TimeSpan.FromHours(1));

            var elapsed = DateTimeOffset.Now - r.Start;
            Assert.IsTrue(elapsed.TotalMilliseconds < 10);

            Assert.AreEqual(1 * 60 * 60 * 1000, r.Duration.TotalMilliseconds);
            Assert.IsTrue(r.IsIncreasing);
        }

        [TestMethod]
        public void TimeRange_Next_DoesntOverlapLast()
        {
            var last = TimeRange.Last(TimeSpan.FromHours(1));
            var next = TimeRange.Next(TimeSpan.FromHours(1));

            Assert.IsFalse(last.IsOverlapping(next));
        }

        [TestMethod]
        public void TimeRange_Next_OverlapsLast()
        {
            var next = TimeRange.Next(TimeSpan.FromHours(1)); // If we make next first
            var last = TimeRange.Last(TimeSpan.FromHours(1)); // then last will overlap.

            Assert.IsTrue(last.IsOverlapping(next));
        }

        [TestMethod]
        public void TimeRange_MaxRange_ReturnsValidRange()
        {
            var r = TimeRange.MaxRange();

            Assert.AreEqual(DateTime.MinValue, r.Start);
            Assert.AreEqual(DateTime.MaxValue, r.Stop);
        }

        [TestMethod]
        public void TimeRange_StartingToday_ReturnsValidRange()
        {
            var r = TimeRange.StartingToday(new TimeSpan(23, 00, 00), 10);

            Assert.AreEqual(DateTime.Now.Date, r.Start.Date);
            Assert.AreEqual(23, r.Start.TimeOfDay.TotalHours);
            Assert.AreEqual(10, r.DurationMins);
        }

        [TestMethod]
        public void TimeRange_TotalDuration_ReturnsValidRange()
        {
            var ranges = new TimeRange[]
            {
                new TimeRange(PointInTime3, PointInTime4), // 1 hour
                new TimeRange(PointInTime3, PointInTime4) // 1 hour
            };

            var r = TimeRange.TotalDuration(ranges);

            Assert.AreEqual(2, r.TotalHours);
        }

        [TestMethod]
        public void TimeRange_WithExtension_ReturnsValidRange()
        {
            var o = new TimeRange(PointInTime3, PointInTime4);
            var r = TimeRange.WithExtension(o, TimeSpan.FromMinutes(1));

            Assert.AreEqual(61, r.DurationMins);
        }

        [TestMethod]
        public void TimeRange_WithPadding_ReturnsValidRange()
        {
            var o = new TimeRange(PointInTime3, PointInTime4);
            var r = TimeRange.WithPadding(o, 10, 10);

            Assert.AreEqual(80, r.DurationMins);
            Assert.AreEqual(o.Start.Subtract(TimeSpan.FromMinutes(10)), r.Start);
            Assert.AreEqual(o.Stop.Add(TimeSpan.FromMinutes(10)), r.Stop);
        }

        [TestMethod]
        public void TimeRange_AdjustedBy_ReturnsValidRange()
        {
            var r = new TimeRange(PointInTime1, PointInTime2);

            var s = r.AdjustedBy(TimeSpan.FromDays(366));

            Assert.AreEqual(2021, s.Start.Year);
        }

        [TestMethod]
        public void TimeRange_AsToday_ReturnsRangeStartingTodayOfSameDuration()
        {
            var r1 = new TimeRange(PointInTime1, PointInTime2);
            var r2 = r1.AsToday();

            Assert.AreEqual(DateTime.Now.Date, r2.Start.Date);
            Assert.AreEqual(r2.Duration, r2.Duration);
        }

        [TestMethod]
        public void TimeRange_IsIncreasing_ReturnsTrueWhenStopIsHigherValue()
        {
            var r1 = new TimeRange(PointInTime1, PointInTime2);

            Assert.IsTrue(r1.IsIncreasing);
        }

        [TestMethod]
        public void TimeRange_IsIncreasing_ReturnsFalseWhenStopIsLowerValue()
        {
            var r1 = new TimeRange(PointInTime2, PointInTime1);

            Assert.IsFalse(r1.IsIncreasing);
        }

        [TestMethod]
        public void TimeRange_IsSpot_ReturnsTrueForSpot()
        {
            var r1 = new TimeRange(PointInTime1, PointInTime1);

            Assert.IsTrue(r1.IsSpot);
        }

        [TestMethod]
        public void TimeRange_IsSpot_ReturnsFalseForARange()
        {
            var r1 = new TimeRange(PointInTime2, PointInTime1);

            Assert.IsFalse(r1.IsSpot);
        }

        [TestMethod]
        public void TimeRange_SpansMidnight_ReturnsTrueWhenItDoes()
        {
            var r1 = TimeRange.FromNowAndNextHours(25);

            Assert.IsTrue(r1.SpansMidnight);
        }

        [TestMethod]
        public void TimeRange_SpansMidnight_ReturnsFalseWhenItDoesNot()
        {
            var r1 = new TimeRange(PointInTime1, PointInTime1.AddHours(1));

            Assert.IsFalse(r1.SpansMidnight);
        }

        [TestMethod]
        public void TimeRange_GetSubdivisions_Returns11MinutesWithin10MinutesInclusive()
        {
            TimeRange range = new TimeRange(DateTime.Now.Date, 10.0);

            int c = range.GetSubdivisions(TimeSpan.FromMinutes(1)).Count();

            Assert.AreEqual(11, c);
        }

        [TestMethod]
        public void TimeRange_GetSubdivisions_Returns9MinutesWithin10MinutesExclusive()
        {
            TimeRange range = new TimeRange(DateTime.Now.Date, 10.0);

            int c = range.GetSubdivisions(TimeSpan.FromMinutes(1), true).Count();

            Assert.AreEqual(9, c);
        }

        [TestMethod]
        public void TimeRange_GetSubdivisions_Returns5TimesWithin10MinutesInclusive()
        {
            TimeRange range = new TimeRange(DateTime.Now.Date, 10.0);

            int c = range.GetSubdivisions(TimeSpan.FromMinutes(3), false).Count();

            Assert.AreEqual(5, c);
        }

        [TestMethod]
        public void TimeRange_GetSubdivisions_Returns3TimesWithin10MinutesExclusive()
        {
            TimeRange range = new TimeRange(DateTime.Now.Date, 10.0);

            int c = range.GetSubdivisions(TimeSpan.FromMinutes(3), true).Count();

            Assert.AreEqual(3, c);
        }

        [TestMethod]
        public void TimeRange_GetSubdivisions_Returns2TimesWithinZeroMinutesInclusive()
        {
            TimeRange range = new TimeRange(DateTime.Now.Date, 0);

            int c = range.GetSubdivisions(TimeSpan.FromMinutes(3), false).Count();

            Assert.AreEqual(2, c);
        }

        [TestMethod]
        public void TimeRange_GetSubdivisions_ReturnsZeroTimesWithinZeroMinutesExclusive()
        {
            TimeRange range = new TimeRange(DateTime.Now.Date, 0);

            int c = range.GetSubdivisions(TimeSpan.FromMinutes(3), true).Count();

            Assert.AreEqual(0, c);
        }

        [TestMethod]
        public void TimeRange_IsEnveloping_ReturnsTrueForEndSpotTime()
        {
            var range = TimeRange.FromDay(new DateTime(2017, 12, 25));

            Assert.IsTrue(range.IsEnveloping(range.Stop));
        }

        [TestMethod]
        public void TimeRange_TryParse_ReturnsTrueAndExpectedRange()
        {
            TimeRange december;
            bool ok = TimeRange.TryParse("2017-12-01", "2017-12-31", out december);

            Assert.IsTrue(ok);
            Assert.AreEqual(2017, december.Start.Year);
            Assert.AreEqual(12, december.Start.Month);
            Assert.AreEqual(1, december.Start.Day);

            Assert.AreEqual(2017, december.Stop.Year);
            Assert.AreEqual(12, december.Stop.Month);
            Assert.AreEqual(31, december.Stop.Day);

            Assert.IsTrue(december.IsEnveloping(new DateTimeOffset(2017, 12, 31, 23, 59, 59, TimeSpan.Zero))); // Just before midnight at the end of the range.
            Assert.IsFalse(december.IsEnveloping(new DateTimeOffset(2018, 1, 1, 0, 0, 0, TimeSpan.Zero))); // Bang on new year.
        }

        [TestMethod]
        public void TimeRange_TryParse_ReturnsTrueAndExpectedRangeUsingISO8601Utc()
        {
            TimeRange range;
            bool ok = TimeRange.TryParse("2017-08-01T09:30:00Z", "2017-08-01T10:30:00Z", out range);

            Assert.IsTrue(ok);
            Assert.AreEqual(2017, range.Start.Year);
            Assert.AreEqual(8, range.Start.Month);
            Assert.AreEqual(1, range.Start.Day);

            Assert.AreEqual(2017, range.Stop.Year);
            Assert.AreEqual(8, range.Stop.Month);
            Assert.AreEqual(1, range.Stop.Day);

            Assert.IsTrue(range.IsEnveloping(new DateTimeOffset(2017, 8, 1, 9, 59, 59, TimeSpan.Zero))); // August 1st, 09:59:59
            Assert.IsFalse(range.IsEnveloping(new DateTimeOffset(2017, 8, 1, 10, 59, 59, TimeSpan.Zero))); // August 1st, 10:59:59
        }

        [TestMethod]
        public void TimeRange_TryParse_ReturnsFalse()
        {
            TimeRange range;
            bool ok = TimeRange.TryParse("2017-08-01T09:30:00Z", "lewjhfhwfeO", out range);

            Assert.IsFalse(ok);
            Assert.IsNull(range);
        }
    }
}

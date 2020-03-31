namespace Evoq.Ranges
{
    using System;
    using System.Runtime.Serialization;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a finite period of time with a start and end.
    /// </summary>
    // This code analysis error is odd because this type is not actually marked ComVisible.
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Interoperability", "CA1405:ComVisibleTypeBaseTypesShouldBeComVisible")]
    [DataContract(Name = "WorldTimeRange", Namespace = "http://schemas.evoq.co.uk/datastructures/2016/06")]
    public sealed class TimeRange : Range<DateTimeOffset>
    {
        // Constructors

        /// <summary>
        /// Creates a new TimeRange given a start time and duration.
        /// </summary>
        /// <remarks>
        /// Because the TimeRange type deals with the UI it is perfectly feasible to need to use local time zoned
        /// DateTime values. Utc is only required when sending time information to services.
        /// </remarks>
        public TimeRange(DateTimeOffset start, TimeSpan duration)
        {
            base.Start = start;
            base.Stop = start.Add(duration);
        }

        /// <summary>
        /// Creates a new TimeRange given a start time and duration.
        /// </summary>
        public TimeRange(DateTimeOffset start, double durationMins)
        {
            base.Start = start;
            base.Stop = start.AddMinutes(durationMins);
        }

        /// <summary>
        /// Creates a new TimeRange with the start and stop times, where each end is inclusive.
        /// </summary>
        /// <param name="start">Inclusive period start time.</param>
        /// <param name="stop">Inclusive period end time.</param>
        public TimeRange(DateTimeOffset start, DateTimeOffset stop)
        {
            base.Start = start;
            base.Stop = stop;
        }

        /// <summary>
        /// Creates a new TimeRange occuring between two times in a single day.
        /// </summary>
        /// <param name="date">The date of the day in which the range occurs - time value will be ignored.</param>
        /// <param name="start">The start time data.</param>
        /// <param name="durationMins">The duration in minutes.</param>
        public TimeRange(DateTime date, TimeSpan start, double durationMins)
        {
            this.Start = date.Date.Add(start);
            this.Stop = this.Start.AddMinutes(durationMins);
        }

        // Properties

        /// <summary>
        /// Returns the time-span covered by the range.
        /// </summary>
        public TimeSpan Duration
        {
            get
            {
                return this.Stop.Subtract(this.Start);
            }
        }

        /// <summary>
        /// Returns true if this TimeRange crosses one or more midnight hours.
        /// </summary>        
        public bool SpansMidnight
        {
            get
            {
                DateTime midnight = this.Stop.Date;
                return this.IsEnveloping(midnight);
            }
        }

        /// <summary>
        /// The duration rounded to the nearest minute.
        /// </summary>        
        public int DurationMins
        {
            get
            {
                var mins = this.Duration.TotalMinutes;
                var d = (int)Math.Round(mins, 0);
                return d;
            }
        }

        /// <summary>
        /// Returns the point halfway between the start and stop.
        /// </summary>
        public DateTimeOffset MidwayPoint
        {
            get
            {
                long halfDuration = this.Duration.Ticks / 2;
                return this.Start.AddTicks(halfDuration);
            }
        }

        // Methods

        /// <summary>
        /// Returns a copy of the TimeRange but where the start time occurs today.
        /// </summary>
        public TimeRange AsToday()
        {
            return TimeRange.StartingToday(this.Start.TimeOfDay, this.DurationMins);
        }

        /// <summary>
        /// Returns a copy of this TimeRange extended by the duration specified.
        /// </summary>        
        public TimeRange WithExtension(TimeSpan extendedBy)
        {
            return TimeRange.WithExtension(this, extendedBy);
        }

        /// <summary>
        /// Returns a TimeRange based on this instance but with an earlier Start and later Stop.
        /// </summary>        
        /// <param name="startPaddingMins">How many minutes to make the start time earlier by</param>
        /// <param name="stopPaddingMins">How many minutes to make the stop time later by</param>        
        public TimeRange WithPadding(double startPaddingMins, double stopPaddingMins)
        {
            return TimeRange.WithPadding(this, startPaddingMins, stopPaddingMins);
        }

        /// <summary>
        /// Adjusts the TimeRange by the amount specified.
        /// </summary>    
        /// <param name="amount">Accepts positive and negative TimeSpans</param>
        /// <returns>Returns a new TimeRange reflecting the changes.</returns>
        public TimeRange AdjustedBy(TimeSpan amount)
        {
            return new TimeRange(this.Start.Add(amount), this.Stop.Add(amount));
        }

        /// <summary>
        /// DEFUNCT - Please use AdjustedBy. Returns a TimeRange based on this instance but with start and stop times shifted later.
        /// </summary>
        /// <param name="amount">The amount by which your new TimeRange will be shifted by.</param>
        public TimeRange MadeLater(TimeSpan amount)
        {
            return new TimeRange(this.Start.Add(amount), this.Stop.Add(amount));
        }

        /// <summary>
        /// DEFUNCT - Please use AdjustedBy. Returns a TimeRange based on this instance but with start and stop times shifted sooner.
        /// </summary>
        /// <param name="amount">The amount by which your new TimeRange will be shifted by.</param>
        public TimeRange MadeSooner(TimeSpan amount)
        {
            return new TimeRange(this.Start.Subtract(amount), this.Stop.Subtract(amount));
        }

        protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Start")
            {
                base.OnPropertyChanged(e);
                base.OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("Duration"));
            }
            else if (e.PropertyName == "Stop")
            {
                base.OnPropertyChanged(e);
                base.OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("Duration"));
            }
        }

        /// <summary>
        /// Indicates if the range overlaps the one provided.
        /// </summary>
        public bool IsOverlapping(TimeRange range)
        {
            return this.IsOverlapping(range.Start, range.Stop);
        }

        /// <summary>
        /// Indicates if the time occurs within the time range, ignoring dates.
        /// </summary>
        public bool IsOverlapping(TimeSpan timeOfDay)
        {
            return ((timeOfDay >= this.Start.TimeOfDay) && (timeOfDay <= this.Stop.TimeOfDay));
        }

        /// <summary>
        /// Returns a random point in time within the time range.
        /// </summary>
        public DateTimeOffset RandomPoint(Random rng)
        {
            if (rng == null)
                throw new ArgumentNullException("rng");


            double randomPercent = rng.NextDouble();
            double portionOfDuration = randomPercent * this.Duration.TotalSeconds;

            return this.Start.AddSeconds(portionOfDuration);
        }

        /// <summary>
        /// Gets an enumerator over each of the subdivisions of time within the range.
        /// </summary>
        /// <param name="division">The length of each division.</param>
        /// <param name="excludeEachEnd">Set true to exclude prevent start and stop being returned, otherwise they will always be returned regardless of the division length.</param>
        /// <returns>An enumerable set of dates at each division within the range.</returns>
        public IEnumerable<DateTimeOffset> GetSubdivisions(TimeSpan division, bool excludeEachEnd = false)
        {
            if (division == null)
                throw new ArgumentNullException(nameof(division));

            if (division.Ticks == 0)
                throw new ArgumentOutOfRangeException("division");


            if (this.IsSpot)
            {
                if (!excludeEachEnd)
                {
                    yield return this.Start;
                    yield return this.Stop;
                    yield break;
                }
                else
                {
                    yield break;
                }
            }
            else
            {
                if (!excludeEachEnd)
                {
                    yield return this.Start;
                }

                var current = this.Start.Add(division);

                while (true)
                {
                    if (current >= this.Stop)
                    {
                        if (!excludeEachEnd)
                        {
                            yield return this.Stop;
                            yield break;
                        }
                        else
                        {
                            yield break;
                        }
                    }
                    else
                    {
                        yield return current;
                    }

                    current = current + division;
                }
            }
        }

        // Statics and Factories

        /// <summary>
        /// Gets a TimeRange for the maximum span possible.
        /// </summary>
        public static TimeRange MaxRange()
        {
            return new TimeRange(DateTime.MinValue, DateTime.MaxValue);
        }

        /// <summary>
        /// Returns a TimeSpan object representing the total duration of all TimeRanges specified.
        /// </summary>
        public static TimeSpan TotalDuration(IEnumerable<TimeRange> ranges)
        {
            TimeRange total = new TimeRange(DateTime.Today, 0);
            foreach (TimeRange t in ranges)
            {
                total = total.WithExtension(t.Duration);
            }
            return total.Duration;
        }

        /// <summary>
        /// Returns a TimeRange and extends its duration by the time specified.
        /// </summary>
        public static TimeRange WithExtension(TimeRange original, TimeSpan by)
        {
            return new TimeRange(original.Start, original.Duration.Add(by));
        }

        /// <summary>
        /// Takes a TimeRange instance and returns a copy with the start earlier and stop later.
        /// </summary>
        /// <param name="original">The TimeRange to base on.</param>
        /// <param name="startPaddingMins">How many minutes to make the start time earlier by</param>
        /// <param name="stopPaddingMins">How many minutes to make the stop time later by</param>        
        public static TimeRange WithPadding(TimeRange original, double startPaddingMins, double stopPaddingMins)
        {
            var subtract = TimeSpan.FromMinutes(startPaddingMins);
            return new TimeRange(original.Start.Subtract(subtract), original.Stop.AddMinutes(stopPaddingMins));
        }

        /// <summary>
        /// Attempts to create a new TimeRange instance from start and stop date-time string representations.
        /// </summary>
        /// <param name="start">A string representation of the start date-time.</param>
        /// <param name="stop">A string representation of the stop date-time.</param>
        /// <param name="range">The produced TimeRange instance or null.</param>
        /// <returns>True if a TimeRange was produced.</returns>
        public static bool TryParse(string start, string stop, out TimeRange range)
        {
            DateTimeOffset startDto;
            DateTimeOffset stopDto;
            if (DateTimeOffset.TryParse(start, out startDto) && DateTimeOffset.TryParse(stop, out stopDto))
            {
                if (startDto.TimeOfDay == TimeSpan.Zero && stopDto.TimeOfDay == TimeSpan.Zero)
                {
                    // Treat strings that do not include time information as date ranges. This means the end date should be adjusted to 
                    // have time information that extends into the full day to within a tick of the next day.

                    range = TimeRange.FromDatesCovered(startDto.Date, stopDto.Date, TimeSpan.Zero);
                    return true;
                }
                else
                {
                    // Treat as verbatim.

                    range = new TimeRange(startDto, stopDto);
                    return true;
                }
            }
            else
            {
                range = null;
                return false;
            }
        }

        /// <summary>
        /// Returns a TimeRange covering midnight to one second before midnight on a specified day.
        /// </summary>
        /// <param name="dayDate">The date of the day.</param>        
        public static TimeRange FromDay(DateTime dayDate)
        {
            DateTime midnight = dayDate.Date;
            DateTime secBeforeMidnight = midnight.Add(new TimeSpan(23, 59, 59));
            return new TimeRange(midnight, secBeforeMidnight);
        }

        /// <summary>
        /// Createsa a TimeRange from specific start and end DateTime instances.
        /// </summary>
        /// <param name="start">The starting DateTime.</param>
        /// <param name="stop">The ending DateTime which is included in the range by default.</param>
        /// <param name="offset">The offset from UTC for the start and end times.</param>
        /// <param name="exclusiveStop">By default the range will include the entire end date.</param>
        /// <returns>A new TimeRange instance.</returns>
        public static TimeRange FromDatesCovered(DateTime start, DateTime stop, TimeSpan offset, bool exclusiveStop = false)
        {
            if (exclusiveStop)
                stop = stop.AddDays(-1);

            var duration = stop.Date - start.Date;

            return TimeRange.FromDaysCovered(new DateTimeOffset(start.Date, offset), duration.TotalDays + 1.0);
        }

        /// <summary>
        /// Creates a TimeRange from the start given and ending a second before midnight on the last day.
        /// </summary>
        /// <remarks>
        /// When specifying a round-back value, a TimeSpan of a single day will round the start back to the
        /// start of the nearest day. Likewise, specifying an hour will round-back to the nearest whole hour.
        /// </remarks>
        /// <param name="startAt">The starting date time.</param>
        /// <param name="days">The number of days to cover.</param>
        /// <param name="roundBack">A TimeSpan to which the start time will be rounded back to.</param>
        public static TimeRange FromDaysCovered(DateTimeOffset startAt, double days, TimeSpan roundBack)
        {
            // Example: 18 rounded back to a grain of 5 is 18 divided by 5, so 3 wholes. Then 3 * 5 = 15.
            //
            long wholeChunks = startAt.Ticks / roundBack.Ticks; // Int64, so loses decimal precision, so always less (rounded back).
            long ticks = wholeChunks * roundBack.Ticks;

            var roundedBackStart = new DateTimeOffset(ticks, startAt.Offset);

            return FromDaysCovered(roundedBackStart, days);
        }

        /// <summary>
        /// Creates a TimeRange from the start given and ending a second before midnight on the last day.
        /// </summary>
        /// <param name="startAt">The starting date time.</param>
        /// <param name="days">The number of days to cover.</param>        
        public static TimeRange FromDaysCovered(DateTimeOffset startAt, double days)
        {
            var secondsPerDay = 60 * 60 * 24;
            var seconds = (secondsPerDay * days) - 1;
            return new TimeRange(startAt, startAt.Date.AddSeconds(seconds));
        }

        /// <summary>
        /// Creates a new TimeRange covering the month given to within a single tick precision of the end time.
        /// </summary>
        /// <param name="year">The year of the month.</param>
        /// <param name="month">The month ordinal within the year.</param>
        /// <param name="months">The number of months to cover from the month going forward.</param>
        /// <returns></returns>
        public static TimeRange FromMonthsCovered(int year, int month, int months)
        {
            if (months < 1)
                throw new ArgumentOutOfRangeException(nameof(months));

            return FromMonthsCovered(year, month, months, TimeSpan.Zero);
        }

        /// <summary>
        /// Creates a new TimeRange covering the month given to within a single tick precision of the end time.
        /// </summary>
        /// <param name="year">The year of the month.</param>
        /// <param name="month">The month ordinal within the year.</param>
        /// <param name="months">The number of months to cover from the month going forward.</param>
        /// <param name="offset">The offset from UTC, when a range is in a timezone that is not UTC.</param>
        /// <returns></returns>1
        public static TimeRange FromMonthsCovered(int year, int month, int months, TimeSpan offset)
        {
            var start = new DateTimeOffset(year, month, 1, 0, 0, 0, offset);

            // The end is the next month but nipped back a touch to make it the last tick of the month.
            //
            var end = start.AddMonths(months).AddTicks(-1);

            return new TimeRange(start, end);
        }

        /// <summary>
        /// Creates a new range covering the current hour and the next given.
        /// </summary>
        public static TimeRange FromNowAndNextHours(double hours)
        {
            DateTime start = DateTime.UtcNow.Subtract(DateTime.UtcNow.TimeOfDay).AddHours(DateTime.UtcNow.TimeOfDay.Hours);
            if (DateTime.UtcNow.TimeOfDay.Minutes > 30)
            {
                //
                // Are we past halfway on this hour.

                start = start.AddMinutes(30);
                TimeSpan h = TimeSpan.FromHours(hours);
                if (h.TotalMinutes > 90) { h.Subtract(TimeSpan.FromMinutes(30)); }
                return new TimeRange(start, h);
            }
            return new TimeRange(start, TimeSpan.FromHours(hours));
        }

        /// <summary>
        /// Creates a new range covering the current hour and the next given.
        /// </summary>
        public static TimeRange Next(TimeSpan duration)
        {
            return new TimeRange(DateTimeOffset.Now, duration);
        }
        
        /// <summary>
        /// Returns a new TimeRange covering a specified period to now.
        /// </summary>
        /// <param name="period">A TimeSpan representing the period leading up until now.</param>
        /// <returns>A new TimeRange instance.</returns>
        public static TimeRange Last(TimeSpan period)
        {
            var now = DateTime.UtcNow;
            return new TimeRange(now.Subtract(period), now);
        }

        /// <summary>
        /// Returns a new TimeRange starting today at the time specified and lasting for the duration.
        /// </summary>
        /// <param name="at">A TimeSpan representing the start time.</param>
        public static TimeRange StartingToday(TimeSpan at, double duration)
        {
            var start = DateTime.Now.Date.Add(at);
            return new TimeRange(start, duration);
        }

        // Object Overrides

        /// <summary>
        /// Returns a string decription of the current range.
        /// </summary>
        /// <returns>A string.</returns>
        public override string ToString()
        {
            return this.ToString(System.Globalization.CultureInfo.CurrentUICulture);
        }

        /// <summary>
        /// Returns a string decription of the current range.
        /// </summary>
        /// <param name="format">A format provider instance used to format the start and stop values.</param>        
        /// <returns>A string.</returns>
        public string ToString(IFormatProvider format)
        {
            string start = this.Start.ToString(format);
            string stop = this.Stop.ToString(format);
            return String.Format(format, "{0} - {1}", start, stop);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public static bool operator ==(TimeRange a, TimeRange b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(TimeRange a, TimeRange b)
        {
            return !a.Equals(b);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}

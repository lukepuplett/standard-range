
namespace Evoq.Ranges
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents a range of some data type.
    /// </summary>
    /// <typeparam name="T">The type of data such as DateTime.</typeparam>
    [DataContract(Namespace = "http://schemas.evoq.co.uk/datastructures/2012/10")]
    public class Range<T> : System.ComponentModel.INotifyPropertyChanged where T : IComparable<T>
    {
        // Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Range&lt;T&gt;"/> class.
        /// </summary>
        protected Range() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Range&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="start">The start of the range.</param>
        /// <param name="stop">The stop/end of the range.</param>
        public Range(T start, T stop)
        {
            _start = start; _stop = stop;
        }
   
        // OnMethods

        /// <summary>
        /// Is called upon any property change.
        /// </summary>
        /// <param name="e">Args passed through to the PropertyChanged event.</param>
        protected virtual void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
        {
            //
            // Actually nothing here - this is more for derived types to override.

            this.RaisePropertyChanged(e);
        }

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
        protected void RaisePropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (this.PropertyChanged != null) { this.PropertyChanged(this, e); }
        }

        // Properties

        /// <summary>
        /// Gets or sets the start.
        /// </summary>
        /// <value>
        /// The start.
        /// </value>
        [DataMember(Order = 1)]
        public T Start
        {
            get
            {
                return _start;
            }
            protected internal set
            {
                _start = value;
                this.OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("Start"));
            }
        } private T _start;

        /// <summary>
        /// Gets or sets the stop.
        /// </summary>
        /// <value>
        /// The stop.
        /// </value>
        [DataMember(Order = 2)]
        public T Stop
        {
            get
            {
                return _stop;
            }
            protected internal set
            {
                _stop = value;
                this.OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("Stop"));
            }
        } private T _stop;

        /// <summary>
        /// Gets a value indicating whether the range increases in value, i.e. whether the Stop is greater than the Start.
        /// </summary>
        public bool IsIncreasing
        {
            get
            {
                return IsIncreasingRange(this.Start, this.Stop);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the range is effectively a spot point, i.e. Start and Stop are equal.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is point; otherwise, <c>false</c>.
        /// </value>
        public bool IsSpot
        {
            get
            {
                return IsSpotRange(this.Start, this.Stop);
            }
        }

        // Methods

        /// <summary>
        /// Determines whether the specified range is overlapping this one.
        /// </summary>
        /// <param name="range">The range.</param>
        /// <returns>
        ///   <c>true</c> if the specified range is overlapping; otherwise, <c>false</c>.
        /// </returns>
        public bool IsOverlapping(Range<T> range)
        {
            return this.IsOverlapping(range.Start, range.Stop);
        }

        /// <summary>
        /// Determines whether the specified range is overlapping this one.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="stop">The stop.</param>
        /// <returns>
        ///   <c>true</c> if the specified start is overlapping; otherwise, <c>false</c>.
        /// </returns>
        public bool IsOverlapping(T start, T stop)
        {
            if (this.IsSpot)
            {
                if (IsSpotRange(start, stop))
                {
                    return this.Start.CompareTo(start) == 0; // So if Start is equal to start then it must be overlapping.
                }
                else
                {
                    return this.IsEnvelopedBy(start, stop);
                }
            }
            else
            {
                return this.IsEnveloping(start) || this.IsEnveloping(stop);
            }
        }

        /// <summary>
        /// Determines whether this range is enveloped by the specified range.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="stop">The stop.</param>
        public bool IsEnvelopedBy(T start, T stop)
        {
            if (this.IsSpot)
            {
                return IsRangeEnvelopingSpot(start, stop, this.Start);
            }
            else
            {
                var r = new Range<T>(start, stop);

                return r.IsEnveloping(this);
            }
        }

        /// <summary>
        /// Determines whether the specified spot is overlapping this one.
        /// </summary>
        /// <param name="spot">The spot.</param>
        /// <returns>
        ///   <c>true</c> if the specified spot is overlapping; otherwise, <c>false</c>.
        /// </returns>
        public bool IsOverlapping(T spot)
        {
            return this.IsEnveloping(spot);
        }

        /// <summary>
        /// Determines whether this range is enveloping another one.
        /// </summary>
        /// <param name="range">The range.</param>
        /// <returns>
        ///   <c>true</c> if the specified range is enveloping; otherwise, <c>false</c>.
        /// </returns>
        public bool IsEnveloping(Range<T> range)
        {
            return this.IsEnveloping(range.Start, range.Stop);
        }

        /// <summary>
        /// Determines whether the this range is is enveloping a spot.
        /// </summary>
        /// <param name="spot">The spot.</param>
        /// <returns>
        ///   <c>true</c> if the specified spot is enveloping; otherwise, <c>false</c>.
        /// </returns>
        public bool IsEnveloping(T spot)
        {
            return IsRangeEnvelopingSpot(this.Start, this.Stop, spot);
        }

        /// <summary>
        /// Determines whether this range is enveloping an explicit range.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="stop">The stop.</param>
        /// <returns>
        ///   <c>true</c> if the specified start is enveloping; otherwise, <c>false</c>.
        /// </returns>
        public bool IsEnveloping(T start, T stop)
        {
            return this.IsEnveloping(start) && this.IsEnveloping(stop);
        }

        /// <summary>
        /// Determines whether the specified range is a spot range.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="stop">The stop.</param>
        /// <returns><c>true</c> if the specified start is spot; otherwise, <c>false</c>.</returns>
        private bool IsSpotRange(T start, T stop)
        {
            return start.CompareTo(stop) == 0;
        }

        /// <summary>
        /// Determines whether the range has a start that's a lower value than its end.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="stop">The stop.</param>
        /// <returns><c>true</c> if [is increasing range] [the specified start]; otherwise, <c>false</c>.</returns>
        private bool IsIncreasingRange(T start, T stop)
        {
            return start.CompareTo(stop) < 0;
        }

        /// <summary>
        /// Determines whether a range is enveloping a spot value.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="stop">The stop.</param>
        /// <param name="spot">The spot.</param>
        private bool IsRangeEnvelopingSpot(T start, T stop, T spot)
        {
            if (IsSpotRange(start, stop))
            {
                return spot.CompareTo(start) == 0;
            }
            else
            {
                if (IsIncreasingRange(start, stop))
                {
                    return (
                        start.CompareTo(spot) <= 0
                        &&
                        spot.CompareTo(stop) <= 0
                        );
                }
                else // Decreasing
                {
                    return (
                        stop.CompareTo(spot) <= 0
                        &&
                        start.CompareTo(spot) >= 0
                        );
                }
            }
        }

        // INotifyPropertyChanged Members

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        // Object Overrides

        /// <summary>
        /// Returns a string representation of the object.
        /// </summary>
        public override string ToString()
        {
            return String.Format("{0} - {1}", this.Start.ToString(), this.Stop.ToString());
        }

        /// <summary>
        /// Compares this with another for equality.
        /// </summary>        
        public override bool Equals(object obj)
        {
            Range<T> cast = obj as Range<T>;

            return this.Equals(cast);
        }

        /// <summary>
        /// Compares this with another for equality.
        /// </summary>
        public bool Equals(Range<T> other)
        {
            if (other == null)
                return false;

            return (this.Start.Equals(other.Start) && this.Stop.Equals(other.Stop));
        }

        // static Equals(obj, obj) actually calls the instance.Equals(obj) anyway so no need to implement.

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }
    }
}

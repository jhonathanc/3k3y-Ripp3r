//
// Copyright (c) 2008-2011, Kenneth Bell
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
//

namespace DiscUtils
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a range of bytes in a stream.
    /// </summary>
    /// <remarks>This is normally used to represent regions of a SparseStream that
    /// are actually stored in the underlying storage medium (rather than implied
    /// zero bytes).  Extents are stored as a zero-based byte offset (from the
    /// beginning of the stream), and a byte length</remarks>
    public sealed class StreamExtent : IEquatable<StreamExtent>, IComparable<StreamExtent>
    {
        private readonly long _start;
        private readonly long _length;

        /// <summary>
        /// Initializes a new instance of the StreamExtent class.
        /// </summary>
        /// <param name="start">The start of the extent</param>
        /// <param name="length">The length of the extent</param>
        public StreamExtent(long start, long length)
        {
            _start = start;
            _length = length;
        }

        /// <summary>
        /// Gets the start of the extent (in bytes).
        /// </summary>
        public long Start
        {
            get { return _start; }
        }

        /// <summary>
        /// Gets the start of the extent (in bytes).
        /// </summary>
        public long Length
        {
            get { return _length; }
        }

        /// <summary>
        /// Calculates the intersection of the extents of multiple streams.
        /// </summary>
        /// <param name="streams">The stream extents</param>
        /// <returns>The intersection of the extents from multiple streams.</returns>
        /// <remarks>A typical use of this method is to calculate the extents in a
        /// region of a stream..</remarks>
        public static IEnumerable<StreamExtent> Intersect(params IEnumerable<StreamExtent>[] streams)
        {
            long extentStart = long.MinValue;
            long extentEnd = long.MaxValue;

            IEnumerator<StreamExtent>[] enums = new IEnumerator<StreamExtent>[streams.Length];
            for (int i = 0; i < streams.Length; ++i)
            {
                enums[i] = streams[i].GetEnumerator();
                if (!enums[i].MoveNext())
                {
                    // Gone past end of one stream (in practice was empty), so no intersections
                    yield break;
                }
            }

            int overlapsFound = 0;
            while (true)
            {
                // We keep cycling round the streams, until we get streams.Length continuous overlaps
                for (int i = 0; i < streams.Length; ++i)
                {
                    // Move stream on past all extents that are earlier than our candidate start point
                    while (enums[i].Current.Length == 0
                        || enums[i].Current.Start + enums[i].Current.Length <= extentStart)
                    {
                        if (!enums[i].MoveNext())
                        {
                            // Gone past end of this stream, no more intersections possible
                            yield break;
                        }
                    }

                    // If this stream has an extent that spans over the candidate start point
                    if (enums[i].Current.Start <= extentStart)
                    {
                        extentEnd = Math.Min(extentEnd, enums[i].Current.Start + enums[i].Current.Length);
                        overlapsFound++;
                    }
                    else
                    {
                        extentStart = enums[i].Current.Start;
                        extentEnd = extentStart + enums[i].Current.Length;
                        overlapsFound = 1;
                    }

                    // We've just done a complete loop of all streams, they overlapped this start position
                    // and we've cut the extent's end down to the shortest run.
                    if (overlapsFound == streams.Length)
                    {
                        yield return new StreamExtent(extentStart, extentEnd - extentStart);
                        extentStart = extentEnd;
                        extentEnd = long.MaxValue;
                        overlapsFound = 0;
                    }
                }
            }
        }

        /// <summary>
        /// The equality operator.
        /// </summary>
        /// <param name="a">The first extent to compare</param>
        /// <param name="b">The second extent to compare</param>
        /// <returns>Whether the two extents are equal</returns>
        public static bool operator ==(StreamExtent a, StreamExtent b)
        {
            return ReferenceEquals(a, null) ? ReferenceEquals(b, null) : a.Equals(b);
        }

        /// <summary>
        /// The inequality operator.
        /// </summary>
        /// <param name="a">The first extent to compare</param>
        /// <param name="b">The second extent to compare</param>
        /// <returns>Whether the two extents are different</returns>
        public static bool operator !=(StreamExtent a, StreamExtent b)
        {
            return !(a == b);
        }

        /// <summary>
        /// The less-than operator.
        /// </summary>
        /// <param name="a">The first extent to compare</param>
        /// <param name="b">The second extent to compare</param>
        /// <returns>Whether a is less than b</returns>
        public static bool operator <(StreamExtent a, StreamExtent b)
        {
            return a.CompareTo(b) < 0;
        }

        /// <summary>
        /// The greater-than operator.
        /// </summary>
        /// <param name="a">The first extent to compare</param>
        /// <param name="b">The second extent to compare</param>
        /// <returns>Whether a is greather than b</returns>
        public static bool operator >(StreamExtent a, StreamExtent b)
        {
            return a.CompareTo(b) > 0;
        }

        /// <summary>
        /// Indicates if this StreamExtent is equal to another.
        /// </summary>
        /// <param name="other">The extent to compare</param>
        /// <returns><c>true</c> if the extents are equal, else <c>false</c></returns>
        public bool Equals(StreamExtent other)
        {
            return other != null && _start == other._start && _length == other._length;
        }

        /// <summary>
        /// Returns a string representation of the extent as [start:+length].
        /// </summary>
        /// <returns>The string representation</returns>
        public override string ToString()
        {
            return "[" + _start + ":+" + _length + "]";
        }

        /// <summary>
        /// Indicates if this stream extent is equal to another object.
        /// </summary>
        /// <param name="obj">The object to test</param>
        /// <returns><c>true</c> if <c>obj</c> is equivalent, else <c>false</c></returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as StreamExtent);
        }

        /// <summary>
        /// Gets a hash code for this extent.
        /// </summary>
        /// <returns>The extent's hash code.</returns>
        public override int GetHashCode()
        {
            return _start.GetHashCode() ^ _length.GetHashCode();
        }

        /// <summary>
        /// Compares this stream extent to another.
        /// </summary>
        /// <param name="other">The extent to compare.</param>
        /// <returns>Value greater than zero if this extent starts after
        /// <c>other</c>, zero if they start at the same position, else
        /// a value less than zero.</returns>
        public int CompareTo(StreamExtent other)
        {
            if (_start > other._start)
            {
                return 1;
            }
            return _start == other._start ? 0 : -1;
        }
    }
}

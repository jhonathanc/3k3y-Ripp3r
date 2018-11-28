using System;
using System.Collections.Generic;
using System.Linq;

namespace NUnrar
{
    internal class LazyReadOnlyCollection<T> : ICollection<T>
    {
        private LazyLoader loader;
        private List<T> backing = new List<T>();

        public LazyReadOnlyCollection(IEnumerable<T> source)
        {
            loader = new LazyLoader(source, backing);
        }

        private class LazyLoader : IEnumerator<T>, IEnumerable<T>
        {
            private IEnumerable<T> s;
            private IEnumerator<T> source;
            private ICollection<T> backing;

            internal LazyLoader(IEnumerable<T> s, ICollection<T> backing)
            {
                this.s = s;
                this.backing = backing;
            }

            internal bool Started { get; private set; }
            internal bool FullyLoaded { get; private set; }

            #region IEnumerator<T> Members

            public T Current
            {
                get { return source.Current; }
            }

            #endregion

            #region IDisposable Members

            public void Dispose()
            {
                if (FullyLoaded)
                {
                    source.Dispose();
                    source = null;
                }
            }

            #endregion

            #region IEnumerator Members

            object System.Collections.IEnumerator.Current
            {
                get { throw new NotImplementedException(); }
            }

            public bool MoveNext()
            {
                if (FullyLoaded)
                {
                    return false;
                }
                if (!Started)
                {
                    source = s.GetEnumerator();
                }
                Started = true;
                if (source.MoveNext())
                {
                    backing.Add(source.Current);
                    return true;
                }
                else
                {
                    FullyLoaded = true;
                    return false;
                }
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }

            #endregion

            #region IEnumerable<T> Members

            public IEnumerator<T> GetEnumerator()
            {
                return this;
            }

            #endregion

            #region IEnumerable Members

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }

            #endregion
        }

        #region ICollection<T> Members

        public void Add(T item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(T item)
        {
            if (loader.FullyLoaded)
            {
                return backing.Contains(item);
            }
            return backing.Concat(loader).Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (loader.FullyLoaded)
            {
                backing.CopyTo(array, arrayIndex);
            }
            backing.Concat(loader).ForEach(item => array[arrayIndex++] = item);
        }

        public int Count
        {
            get
            {
                if (loader.FullyLoaded)
                {
                    return backing.Count;
                }
                return backing.Concat(loader).Count();
            }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            if (loader.FullyLoaded)
            {
                return backing.GetEnumerator();
            }
            return backing.Concat(loader).GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

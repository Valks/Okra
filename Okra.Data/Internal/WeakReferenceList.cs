using System;
using System.Collections;
using System.Collections.Generic;

namespace Okra.Data.Internal
{
    internal class WeakReferenceList<T> : IList<T> where T : class
    {
        // *** Fields ***

        private readonly List<WeakReference<T>> _internalList = new List<WeakReference<T>>();
        
        // *** IList<T> Properties ***

        public T this[int index]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int Count
        {
            get
            {
                CleanInternalList();
                return _internalList.Count;
            }
        }

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        // *** IList<T> Methods ***

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
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int IndexOf(T item)
        {
            // Enumerate each item in the list, and for live references compare with the item

            for (int i = 0; i < _internalList.Count; i++)
            {
                T target;

                if (_internalList[i].TryGetTarget(out target) && item == target)
                    return i;
            }

            return -1;
        }

        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            // Get the index of the item to remove

            int index = IndexOf(item);

            // Remove the item if the index is found

            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }
          return false;
        }

        public void RemoveAt(int index)
        {
            // Delegate to the internal list

            _internalList.RemoveAt(index);
        }

        // *** IEnumerable<T> Methods ***

        public IEnumerator<T> GetEnumerator()
        {
            // Enumerate each reference and yield each value if it is live

            foreach (WeakReference<T> weakReference in _internalList)
            {
                T target;

                if (weakReference.TryGetTarget(out target))
                    yield return target;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        // *** Public Methods ***

        public WeakReference<T> AddAndReturnReference(T item)
        {
            // Clean the internal list of garbage collected references to stop this growing continually

            CleanInternalList();

            // Create and add the new reference

            WeakReference<T> weakReference = new WeakReference<T>(item);
            _internalList.Add(weakReference);

            // Return the weak reference

            return weakReference;
        }

        public bool Remove(WeakReference<T> reference)
        {
            return _internalList.Remove(reference);
        }

        // *** Private Methods ***

        private void CleanInternalList()
        {
            // Enumerate the internal list removing any items that have been garbage collected
            // NB: Have to use custom enumeration so that we can remove items whilst enumerating the list

            int i=0;

            while(i<_internalList.Count)
            {
                T target;

                if (_internalList[i].TryGetTarget(out target))
                    i++;
                else
                    _internalList.RemoveAt(i);
            }
        }
    }
}

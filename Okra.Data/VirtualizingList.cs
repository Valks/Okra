using System;
using System.Collections.Generic;
using System.Globalization;

namespace Okra.Data
{
    public class VirtualizingList<T> : IList<T>
    {
        // *** Fields ***

        private T[] _internalArray = new T[0];
        private int _count;

        // *** IList<T> Properties ***

        public T this[int index]
        {
            get
            {
                // Validate that the index is within range

              if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException(string.Format(CultureInfo.InvariantCulture,
                  "The specified index is outside the bounds of the array."));

                // If the index is in a virtual part of the list then return a placeholder value

                if (index >= _internalArray.Length)
                    return default(T);

                // Otherwise return the actual value

                return _internalArray[index];
            }
            set
            {
                // Validate that the index is within range

              if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException(string.Format(CultureInfo.InvariantCulture,
                  "The specified index is outside the bounds of the array."));

                // Expand the internal list with empty placeholders until the item will fit

                EnsureCapacity(index + 1);

                // Set the item

                _internalArray[index] = value;
            }
        }

        public int Count
        {
            get
            {
                return _count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        // *** Methods ***

        public void UpdateCount(int count)
        {
            // Validate that the index is within range

          if (count < 0)
            throw new ArgumentOutOfRangeException(string.Format(CultureInfo.InvariantCulture,
              "The parameter must be greater than or equal to zero."));

            // Update the count

            _count = count;
        }

        // *** IList<T> Methods ***

        public void Add(T item)
        {
            int newIndex = _count;

            _count++;
            this[newIndex] = item;
        }

        public void Clear()
        {
            _count = 0;
            _internalArray = new T[0];
        }

        public bool Contains(T item)
        {
            return IndexOf(item) != -1;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            // Copy the array that is available

            Array.Copy(_internalArray, 0, array, arrayIndex, _count);

            // Set the remaining items to the placeholder

            for (int i = _internalArray.Length + arrayIndex; i < _count + arrayIndex; i++)
                array[i] = default(T);
        }

        public int IndexOf(T item)
        {
            for (int i = 0; i < _internalArray.Length; i++)
            {
                T internalItem = _internalArray[i];

                if (internalItem != null && internalItem.Equals(item))
                    return i;
            }

            return -1;
        }

        public void Insert(int index, T item)
        {
            // Validate that the index is within range

          if (index < 0 || index > Count)
            throw new ArgumentOutOfRangeException(string.Format(CultureInfo.InvariantCulture,
              "The specified index is outside the bounds of the array."));

            // Ensure that there is enough room in the collection

            int requiredSize = Math.Max(index, _internalArray.Length + 1);
            EnsureCapacity(requiredSize);

            // If there are items after the inserted item them move them along

            Array.Copy(_internalArray, index, _internalArray, index + 1, _internalArray.Length - index - 1);

            // Insert the new item

            _count++;
            _internalArray[index] = item;
        }

        public bool Remove(T item)
        {
            int index = IndexOf(item);

            if (index == -1)
                return false;

            RemoveAt(index);
            return true;
        }

        public void RemoveAt(int index)
        {
            // Validate that the index is within range

          if (index < 0 || index >= Count)
            throw new ArgumentOutOfRangeException(string.Format(CultureInfo.InvariantCulture,
              "The specified index is outside the bounds of the array."));

            // Reduce the count

            _count--;

            // Move all items after the removed item to the left

            Array.Copy(_internalArray, index, _internalArray, index - 1, _internalArray.Length - index);
        }

        // *** IEnumerable<T> Methods ***

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < _count; i++)
                yield return this[i];
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<T>)this).GetEnumerator();
        }

        // *** Private Methods ***

        private void EnsureCapacity(int requiredSize)
        {
            if (_internalArray.Length < requiredSize)
            {
                // If the capacity is too small, then make the list required size x 2

                int desiredSize = requiredSize * 2;
                T[] newArray = new T[desiredSize];

                // Copy the existing data

                Array.Copy(_internalArray, 0, newArray, 0, _internalArray.Length);
                _internalArray = newArray;
            }
        }
    }
}

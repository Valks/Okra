using System;
using System.Collections;
using System.Collections.Generic;

namespace Okra.Data.Internal
{
  internal class WeakReferenceList<T> : IList<T> where T : class
  {
    // *** Fields ***

    private readonly List<WeakReference> internalList = new List<WeakReference>();

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
        return internalList.Count;
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

      for (int i = 0; i < internalList.Count; i++)
      {
        if (internalList[i].Target == item)
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
      else
      {
        return false;
      }
    }

    public void RemoveAt(int index)
    {
      // Delegate to the internal list

      internalList.RemoveAt(index);
    }

    // *** IEnumerable<T> Methods ***

    public IEnumerator<T> GetEnumerator()
    {
      // Enumerate each reference and yield each value if it is live

      foreach (WeakReference weakReference in internalList)
      {
        if (weakReference.Target is T)
          yield return (T)weakReference.Target;
      }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return this.GetEnumerator();
    }

    // *** Public Methods ***

    public WeakReference AddAndReturnReference(T item)
    {
      // Clean the internal list of garbage collected references to stop this growing continually

      CleanInternalList();

      // Create and add the new reference

      WeakReference weakReference = new WeakReference(item);
      internalList.Add(weakReference);

      // Return the weak reference

      return weakReference;
    }

    public bool Remove(WeakReference reference)
    {
      return internalList.Remove(reference);
    }

    // *** Private Methods ***

    private void CleanInternalList()
    {
      // Enumerate the internal list removing any items that have been garbage collected
      // NB: Have to use custom enumeration so that we can remove items whilst enumerating the list

      int i = 0;

      while (i < internalList.Count)
      {
        if (internalList[i].Target != null)
          i++;
        else
          internalList.RemoveAt(i);
      }
    }
  }
}
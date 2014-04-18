using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Okra.Data
{
  public abstract class VirtualizingVectorBase<T> : IList, IList<T>, INotifyPropertyChanged, INotifyCollectionChanged
  {
    // *** Constants ***

    private const string PropertyNameCount = "Count";
    private const string PropertyNameIndexer = "Item[]";

    // *** Fields ***

    private Task<int> _getCountTask;
    private int _count;
    private bool _isLoading;

    private readonly HashSet<int> _currentFetchingIndicies = new HashSet<int>();

    private int? _currentFetchedIndex;
    private T _currentFetchedItem;

    // *** Events ***

    public event PropertyChangedEventHandler PropertyChanged;
    public event NotifyCollectionChangedEventHandler CollectionChanged;

    // *** IList<T> Properties ***

    public T this[int index]
    {
      get { return GetItem(index); }
      set
      {
        throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
          "Cannot modify a read-only collection."));
      }
    }

    object IList.this[int index]
    {
      get { return GetItem(index); }
      set
      {
        throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
          "Cannot modify a read-only collection."));
      }
    }

    public int Count
    {
      get { return GetCount(); }
    }

    public bool IsFixedSize
    {
      get { return false; }
    }

    public bool IsLoading
    {
      get { return _isLoading; }
      protected set
      {
        if (_isLoading != value)
        {
          _isLoading = value;
          OnPropertyChanged("IsLoading");
        }
      }
    }

    public bool IsReadOnly
    {
      get { return true; }
    }

    public bool IsSynchronized
    {
      get { return false; }
    }

    public object SyncRoot
    {
      get { return this; }
    }

    // *** IList<T> Methods ***

    public void Add(T item)
    {
      throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
        "Cannot modify a read-only collection."));
    }

    int IList.Add(object value)
    {
      throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
        "Cannot modify a read-only collection."));
    }

    public void Clear()
    {
      throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
        "Cannot modify a read-only collection."));
    }

    public bool Contains(T item)
    {
      return IndexOf(item) != -1;
    }

    bool IList.Contains(object value)
    {
      if (value is T)
        return Contains((T) value);
      else
        return false;
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
      CopyToInternal(array, arrayIndex).Wait();
    }

    void ICollection.CopyTo(Array array, int index)
    {
      CopyToInternal(array, index).Wait();
    }

    public int IndexOf(T item)
    {
      return GetIndexOf(item);
    }

    int IList.IndexOf(object value)
    {
      if (value is T)
        return GetIndexOf((T) value);
      return -1;
    }

    public void Insert(int index, T item)
    {
      throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
        "Cannot modify a read-only collection."));
    }

    void IList.Insert(int index, object value)
    {
      throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
        "Cannot modify a read-only collection."));
    }

    public bool Remove(T item)
    {
      throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
        "Cannot modify a read-only collection."));
    }

    void IList.Remove(object value)
    {
      throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
        "Cannot modify a read-only collection."));
    }

    public void RemoveAt(int index)
    {
      throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
        "Cannot modify a read-only collection."));
    }

    // *** IEnumerable<T> Methods ***

    public IEnumerator<T> GetEnumerator()
    {
      for (int i = 0; i < Count; i++)
        yield return GetItemAsync(i).Result;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return ((IEnumerable<T>) this).GetEnumerator();
    }

    // *** Protected Methods ***

    protected void OnItemsAdded(int index, int count)
    {
      // If we are still awaiting the count for the collection then we can just ignore this

      if (IsLoading)
        return;

      // Get a list of all the items being fetched prior to the update

      int[] fetchingIndicies = _currentFetchingIndicies.Where(i => i >= index).ToArray();

      // Increment the cached count for this collection

      _count += count;

      // Raise property changed events

      OnPropertyChanged(PropertyNameCount);
      OnPropertyChanged(PropertyNameIndexer);

      // Raise collection changed events for each new item (in ascending order)

      for (int i = index; i < index + count; i++)
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
          new object[] {null}, i));

      // Raise collection changed events for each previously fetching item
      // For example: The bound list may have requested item 5 and recieved a placeholder - if two items are added we need to fetch item 7
      //              NB - It is assumed that when item 5 is returned it is the "new" item 5

      foreach (int i in fetchingIndicies)
      {
        int newIndex = i + count;
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace,
          new object[] {null}, new object[] {null}, newIndex));
      }
    }

    protected void OnItemsRemoved(int index, int count)
    {
      // If we are still awaiting the count for the collection then we can just ignore this

      if (IsLoading)
        return;

      // Get a list of all the items being fetched prior to the update

      int[] fetchingIndicies = _currentFetchingIndicies.Where(i => i >= index + count).ToArray();

      // Decrement the cached count for this collection

      _count -= count;

      // Raise property changed events

      OnPropertyChanged(PropertyNameCount);
      OnPropertyChanged(PropertyNameIndexer);

      // Raise collection changed events for each removed item (in decending order)

      for (int i = index + count - 1; i >= index; i--)
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove,
          new object[] {null}, i));

      // Raise collection changed events for each previously fetching item
      // For example: The bound list may have requested item 5 and recieved a placeholder - if two items are removed before this then we need to fetch item 3
      //              NB - It is assumed that when item 5 is returned it is the "new" item 5

      foreach (int i in fetchingIndicies)
      {
        int newIndex = i - count;
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace,
          new object[] {null}, new object[] {null}, newIndex));
      }
    }

    protected void Reset()
    {
      // If we are still awaiting the count for the collection then we can just ignore this

      if (IsLoading)
        return;

      // Reset the cached count and task count

      _count = 0;
      _getCountTask = null;

      // Raise property and collection changed events

      OnPropertyChanged(PropertyNameCount);
      OnPropertyChanged(PropertyNameIndexer);
      OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    // *** Protected Abstract Methods ***

    protected abstract Task<int> GetCountAsync();

    protected abstract Task<T> GetItemAsync(int index);

    protected abstract int GetIndexOf(T item);

    // *** Event Handlers ***

    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
    {
      PropertyChangedEventHandler eventHandler = PropertyChanged;

      if (eventHandler != null)
        eventHandler(this, e);
    }

    protected void OnPropertyChanged(string propertyName)
    {
      OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
    }

    protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
      NotifyCollectionChangedEventHandler eventHandler = CollectionChanged;

      if (eventHandler != null)
        eventHandler(this, e);
    }

    // *** Private Methods ***

    private int GetCount()
    {
      if (_getCountTask == null)
      {
        _getCountTask = GetCountAsync();

        // If the GetCountAsync call completed syncronously then return the count

        if (_getCountTask.IsCompleted)
          _count = _getCountTask.Result;

          // Otherwise fetch the count in the background (raising changed events)

        else
          GetCountBackground(_getCountTask);
      }

      // Return the cached value of count (or zero if this is still being fetched in the background)

      return _count;
    }

    private async void GetCountBackground(Task<int> getCountTask)
    {
      // Await for the count to be returned

      IsLoading = true;
      int newCount = await getCountTask;
      IsLoading = false;

      if (_count != newCount)
      {
        _count = newCount;

        // Raise property and collection changed events

        OnPropertyChanged(PropertyNameCount);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
      }
    }

    private T GetItem(int index)
    {
      // If we are within a property or collection changed event for this index then return the cached values

      if (_currentFetchedIndex != null)
        return _currentFetchedItem;

      // Otherwise retrieve the requested item

      Task<T> getItemTask = GetItemAsync(index);

      // If the GetItemAsync call completed syncronously then return the item directly

      if (getItemTask.IsCompleted)
        return getItemTask.Result;

      // Otherwise fetch the count in the background and return a placeholder (raising changed events)

      GetItemBackground(index, getItemTask);
      return default(T);
    }

    private async void GetItemBackground(int index, Task<T> getItemTask)
    {
      // If we are currently fetching this item then we can ignore this request

      if (_currentFetchingIndicies.Contains(index))
        return;

      // Get the item (making a note of the index as it is being retrieved)

      _currentFetchingIndicies.Add(index);
      T item = await getItemTask;
      _currentFetchingIndicies.Remove(index);

      // Store the current index and value
      // NB: The data bound item will be retrieved during the property and collection changes, so we can use these cached values

      _currentFetchedIndex = index;
      _currentFetchedItem = item;

      // Raise property and collection changed events

      OnPropertyChanged(PropertyNameIndexer);
      OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, default(T),
        index));

      // Reset the currently fetched state

      _currentFetchedIndex = null;
      _currentFetchedItem = default(T);
    }

    private async Task CopyToInternal(Array array, int index)
    {
      int count = await GetCountAsync();

      for (int i = 0; i < count; i++)
        array.SetValue(await GetItemAsync(i), new int[] {i + index});
    }
  }
}
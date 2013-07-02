using Okra.Data.Internal;
using System;
using System.Threading.Tasks;

namespace Okra.Data
{
  public abstract class DataListSourceBase<T> : IDataListSource<T>
  {
    // *** Fields ***

    private readonly WeakReferenceList<IUpdatableCollection> _updateSubscriptions = new WeakReferenceList<IUpdatableCollection>();

    // *** IDataListSource<T> Methods ***

    public abstract Task<int> GetCountAsync();

    public abstract Task<T> GetItemAsync(int index);

    public abstract int IndexOf(T item);

    public IDisposable Subscribe(IUpdatableCollection collection)
    {
      WeakReference collectionReference = _updateSubscriptions.AddAndReturnReference(collection);
      return new DelegateDisposable(() => _updateSubscriptions.Remove(collectionReference));
    }

    // *** Protected Methods ***

    protected void PostUpdate(DataListUpdate update)
    {
      foreach (IUpdatableCollection collection in _updateSubscriptions)
        collection.Update(update);
    }
  }
}
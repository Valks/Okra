using Okra.Data.Internal;
using System;
using System.Threading.Tasks;

namespace Okra.Data
{
  public abstract class DataListSourceBase<T> : IDataListSource<T>
  {
    // *** Fields ***

    private WeakReferenceList<IUpdatableCollection> updateSubscriptions = new WeakReferenceList<IUpdatableCollection>();

    // *** IDataListSource<T> Methods ***

    public abstract Task<int> GetCountAsync();

    public abstract Task<T> GetItemAsync(int index);

    public abstract int IndexOf(T item);

    public IDisposable Subscribe(IUpdatableCollection collection)
    {
      WeakReference collectionReference = updateSubscriptions.AddAndReturnReference(collection);
      return new DelegateDisposable(() => updateSubscriptions.Remove(collectionReference));
    }

    // *** Protected Methods ***

    protected void PostUpdate(DataListUpdate update)
    {
      foreach (IUpdatableCollection collection in updateSubscriptions)
        collection.Update(update);
    }
  }
}
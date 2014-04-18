using System;
using System.Threading.Tasks;
using Okra.Data.Internal;

namespace Okra.Data
{
    internal abstract class DataListSourceOperatorBase<T> : IDataListSource<T>, IUpdatableCollection
    {
        // *** Fields ***

        private readonly IDataListSource<T> _source;

        private readonly WeakReferenceList<IUpdatableCollection> _updateSubscriptions = new WeakReferenceList<IUpdatableCollection>();
        private IDisposable _sourceUpdateDisposable;

        // *** Constructors ***

      protected DataListSourceOperatorBase(IDataListSource<T> source)
      {
        _source = source;
      }

      // *** Properties ***

        protected IDataListSource<T> Source
        {
            get
            {
                return _source;
            }
        }

        // *** Methods ***

        public abstract Task<int> GetCountAsync();
        public abstract Task<T> GetItemAsync(int index);
        public abstract int IndexOf(T item);

        public IDisposable Subscribe(IUpdatableCollection collection)
        {
            // Store the IUpdatableCollection in a WeakReferenceList

            WeakReference<IUpdatableCollection> collectionReference = _updateSubscriptions.AddAndReturnReference(collection);

            // Subscribe to the source if there is not currently a subscription in place

            if (_sourceUpdateDisposable == null)
                _sourceUpdateDisposable = _source.Subscribe(this);
            
            // Return an IDisposable to remove the subscription

            return new DelegateDisposable(delegate()
            {
                _updateSubscriptions.Remove(collectionReference);
                UnsubscribeFromSourceIfNoSubscribers();
            });
        }

        // *** Protected Methods ***

        protected void PostUpdate(DataListUpdate update)
        {
            foreach (IUpdatableCollection collection in _updateSubscriptions)
                collection.Update(update);
        }

        protected virtual void ProcessUpdate(DataListUpdate update)
        {
            // Unsubscribe from future updates if all subscribers have been garbage collected or removed

            UnsubscribeFromSourceIfNoSubscribers();

            // If the derived class does not implement ProcessUpdate then simply forward the update

            PostUpdate(update);
        }

        // *** IUpdatableCollection Methods ***

        void IUpdatableCollection.Update(DataListUpdate update)
        {
            // Process the update to perform any side-effects or conversions required and forward to any subscribers

            ProcessUpdate(update);
        }

        // *** Private Methods ***

        private void UnsubscribeFromSourceIfNoSubscribers()
        {
          if (_updateSubscriptions.Count != 0 || _sourceUpdateDisposable == null) return;

          _sourceUpdateDisposable.Dispose();
          _sourceUpdateDisposable = null;
        }
    }
}

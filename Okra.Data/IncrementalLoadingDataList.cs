using System;
using System.Globalization;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Okra.Data
{
    public class IncrementalLoadingDataList<T> : VirtualizingVectorBase<T>, ISupportIncrementalLoading, IUpdatableCollection
    {
        // *** Fields ***

        private readonly IDataListSource<T> _dataListSource;

        private int _minimumPagingSize;
        private int _currentCount;
        private int? _sourceCount;

        // *** Constructors ***

        public IncrementalLoadingDataList(IDataListSource<T> dataListSource)
        {
            // Validate the parameters

            if (dataListSource == null)
                throw new ArgumentNullException("dataListSource");

            // Set the fields

            _dataListSource = dataListSource;
        }

        // *** Properties ***

        public int MinimumPagingSize
        {
            get
            {
                return _minimumPagingSize;
            }
            set
            {
                if (_minimumPagingSize != value)
                {
                    _minimumPagingSize = value;
                    OnPropertyChanged("MinimumPagingSize");
                }
            }
        }

        // *** IUpdatableCollection Members ***

        void IUpdatableCollection.Update(DataListUpdate update)
        {
            switch (update.Action)
            {
                case DataListUpdateAction.Add:
                    Update_Add(update);
                    break;
                case DataListUpdateAction.Remove:
                    Update_Remove(update);
                    break;
                case DataListUpdateAction.Reset:
                    Update_Reset();
                    break;
            }
        }

        // *** ISupportIncrementalLoading Members ***

        public bool HasMoreItems
        {
            get
            {
              // If we have not yet retrived the number of items from the data list source then return true

                if (_sourceCount == null)
                    return true;

                // Otherwise return true only if there are items yet to be retrived
              return _currentCount < _sourceCount;
            }
        }

        public async Task<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            return await LoadMoreItemsAsyncInternal((int)count);
        }

        // *** Overridden Base Methods ***

        protected override Task<int> GetCountAsync()
        {
            return Task.FromResult(_currentCount);
        }

        protected override Task<T> GetItemAsync(int index)
        {
            // Validate arguments

          if (index < 0 || index > _currentCount)
            throw new ArgumentOutOfRangeException("index",
              string.Format(CultureInfo.InvariantCulture, "The specified index is outside the bounds of the array."));

            // Return the value from the source

            return _dataListSource.GetItemAsync(index);
        }

        protected override int GetIndexOf(T item)
        {
            // Get the index from the source

            int index = _dataListSource.IndexOf(item);

            // If the index is in the currently visible region then return the index, otherwise return -1

            return index < _currentCount ? index : -1;
        }

        // *** Private Methods ***

        private async Task<LoadMoreItemsResult> LoadMoreItemsAsyncInternal(int count)
        {
            IsLoading = true;

            // If we currently do not know the number of items then fetch this

            if (_sourceCount == null)
                _sourceCount = await _dataListSource.GetCountAsync();

            // Set a minimum paging size if requested

            count = Math.Max(count, MinimumPagingSize);

            // Limit the number of items to fetch to the number of remaining items

            count = Math.Min(count, _sourceCount.Value - _currentCount);

            // Get all the items and wait until they are all fetched

            var fetchItemTasks = new Task<T>[count];
            int startIndex = _currentCount;

            for (int i = 0; i < count; i++)
                fetchItemTasks[i] = _dataListSource.GetItemAsync(startIndex + i);

            await Task.WhenAll(fetchItemTasks);

            // Increment the current count

            _currentCount += count;

            // Set properties and raise property changed for HasMoreItems if this is not false

            IsLoading = false;

            if (_currentCount == _sourceCount)
                OnPropertyChanged("HasMoreItems");

            // Raise collection changed events

            OnItemsAdded(startIndex, count);

            // Return the number of items added
            
            return new LoadMoreItemsResult { Count = (uint)count };
        }

        private void Update_Add(DataListUpdate update)
        {
            // If the entire update is outside of the visible collection then ignore it

            if (update.Index > _currentCount)
                return;

            _currentCount += update.Count;
            OnItemsAdded(update.Index, update.Count);
        }

        private void Update_Remove(DataListUpdate update)
        {
            // If the entire update is outside of the visible collection then ignore it

            if (update.Index >= _currentCount)
                return;

            // If the update overlaps the boundary of the visible collection then only remove visible items

            int removedItemCount = Math.Min(update.Count, _currentCount - update.Index);

            _currentCount -= removedItemCount;
            OnItemsRemoved(update.Index, removedItemCount);
        }

        private void Update_Reset()
        {
            // Set the count to zero

            _currentCount = 0;

            // Raise the Reset events

            Reset();

            // Raise a 'HasMoreItems' property changed event

            OnPropertyChanged("HasMoreItems");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Okra.Data
{
    public abstract class SimpleDataListSource<T> : DataListSourceBase<T>
    {
        // *** Fields ***

        private Task _fetchingTask;

        // *** Private Properties ***

        private IList<T> InternalList
        {
            get;
            set;
        }

        // *** IDataListSource<T> Methods ***

        public async override Task<int> GetCountAsync()
        {
            // If we are not initialized then await fetching the list

            if (InternalList == null)
                await GetFetchingTask();

            // Return the result from the cached list

            return InternalList.Count;
        }

        public async override Task<T> GetItemAsync(int index)
        {
            // Validate arguments

          if (index < 0)
            throw new ArgumentOutOfRangeException("index",
              string.Format(CultureInfo.InvariantCulture, "The specified index is outside the bounds of the array."));

            // If we are not initialized then await fetching the list

            if (InternalList == null)
                await GetFetchingTask();

            // Return the result from the cached list (or throw an exception if after the last item)

          if (index >= InternalList.Count)
            throw new ArgumentOutOfRangeException("index",
              string.Format(CultureInfo.InvariantCulture, "The specified index is outside the bounds of the array."));
          
          return InternalList[index];
        }

        public override int IndexOf(T item)
        {
            // If we have not yet fetched the items then return that the item is not in the list

            if (InternalList == null)
                return -1;

            // Otherwise delegate to the internal list

            return InternalList.IndexOf(item);
        }

        public void Refresh()
        {
            // TODO : Should 'await fetchingTask' here in case a fetch is currently in progress???

            _fetchingTask = null;
            InternalList = null;

            PostUpdate(new DataListUpdate(DataListUpdateAction.Reset));
        }

        // *** Protected Methods ***

        protected abstract Task<IList<T>> FetchItemsAsync();

        // *** Private Methods ***

        private Task GetFetchingTask()
        {
            if (_fetchingTask == null)
                _fetchingTask = FetchingTask();

            return _fetchingTask;
        }

        private async Task FetchingTask()
        {
            // Call the deriving class to get the items

            InternalList = await FetchItemsAsync();
        }
    }
}

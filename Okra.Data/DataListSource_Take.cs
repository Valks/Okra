using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Okra.Data
{
    internal class DataListSource_Take<T> : DataListSourceOperatorBase<T>
    {
        // *** Fields ***

        private readonly int _count;

        // NB: 'sourceCount' holds the last known value - if this gets out of sync with the source then this is because there
        //     is nobody observing the changes to the collection and any new observers will need to resync first by calling GetCountAsync()
        private int _sourceCount;

        // *** Constructors ***

        public DataListSource_Take(IDataListSource<T> source, int count)
            : base(source)
        {
            _count = count;
        }

        // *** Methods ***

        public override async Task<int> GetCountAsync()
        {
            // Get the source count

            _sourceCount = await Source.GetCountAsync();

            // Return the minimum value of source and 'count'

            return Math.Min(_sourceCount, _count);
        }

        public override Task<T> GetItemAsync(int index)
        {
            // If the index is outside of the bounds of the Take then throw an exception
            // NB: If the source is shorter than the count then it will handle throwing the exception

          if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(string.Format(CultureInfo.InvariantCulture,
              "The specified index is outside the bounds of the array."));

            // Otherwise defer to the source

            return Source.GetItemAsync(index);
        }

        public override int IndexOf(T item)
        {
            // Get the index from the source

            int index = Source.IndexOf(item);

            // Return the supplied value if within the bounds of the Take, otherwise return -1

            if (index >= _count)
                return -1;
          
          return index;
        }

        protected override void ProcessUpdate(DataListUpdate update)
        {
            switch (update.Action)
            {
                case DataListUpdateAction.Add:
                    ProcessUpdate_Add(update);
                    break;
                case DataListUpdateAction.Remove:
                    ProcessUpdate_Remove(update);
                    break;
                default:
                    PostUpdate(update);
                    break;
            }
        }

        // *** Private Methods ***

        private void ProcessUpdate_Add(DataListUpdate update)
        {
            // If the update is outside of the bounds of the 'count' then simply consume the event

            if (update.Index >= _count)
                return;

            // Forward the 'Add' update to any subscribers
            // NB: The number of items added may need to be trimmed if they will exceed the bounds of the Take

            int itemsAdded = Math.Min(update.Count, _count - update.Index);
            PostUpdate(new DataListUpdate(DataListUpdateAction.Add, update.Index, itemsAdded));

            // If the 'Add' operation has pushed some items outside of the bounds of the Take then these will need to be removed

            int oldCount = Math.Min(_sourceCount, _count);
            int itemsRemoved = oldCount + itemsAdded - _count;

            if (itemsRemoved > 0)
                PostUpdate(new DataListUpdate(DataListUpdateAction.Remove, _count, itemsRemoved));

            // Set the last known source count

            _sourceCount += update.Count;
        }

        private void ProcessUpdate_Remove(DataListUpdate update)
        {
            // If the update is outside of the bounds of the 'count' then simply consume the event

            if (update.Index >= _count)
                return;

            // Forward the 'Remove' update to any subscribers
            // NB: The number of items removed may need to be trimmed if they exceed the bounds of the Take

            int itemsRemoved = Math.Min(update.Count, _count - update.Index);
            PostUpdate(new DataListUpdate(DataListUpdateAction.Remove, update.Index, itemsRemoved));

            // If the 'Remove' operation has pulled in some items outside of the bounds of the Take then these will need to be added

            int itemsAvailable = _sourceCount - Math.Max(update.Index + update.Count, _count);
            int itemsAdded = Math.Min(itemsRemoved, itemsAvailable);

            if (itemsAdded > 0)
                PostUpdate(new DataListUpdate(DataListUpdateAction.Add, _count - itemsRemoved, itemsAdded));

            // Set the last known source count

            _sourceCount -= update.Count;
        }
    }
}

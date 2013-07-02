using Okra.Data.Helpers;
using System;
using System.Threading.Tasks;

namespace Okra.Data
{
  internal class DataListSourceSkip<T> : DataListSourceOperatorBase<T>
  {
    // *** Fields ***

    private readonly int _count;

    // NB: 'sourceCount' holds the last known value - if this gets out of sync with the source then this is because there
    //     is nobody observing the changes to the collection and any new observers will need to resync first by calling GetCountAsync()
    private int _sourceCount;

    // *** Constructors ***

    public DataListSourceSkip(IDataListSource<T> source, int count)
      : base(source)
    {
      this._count = count;
    }

    // *** Methods ***

    public override Task<int> GetCountAsync()
    {
      return TaskHelper.RunAsync(() =>
        {
          // Get the source count
          Task<int> task = Source.GetCountAsync();
          task.Start();
          task.Wait();
          _sourceCount = task.Result;

          // Return the source count, minus 'count' (or zero if negative)

          int resultCount = _sourceCount - _count;
          return resultCount > 0 ? resultCount : 0;
        });
    }

    public override Task<T> GetItemAsync(int index)
    {
      // If the index is outside of the bounds of the Skip then throw an exception
      // NB: Don't need to validate the upper bounds as the source will do this for us

      if (index < 0)
        throw new ArgumentOutOfRangeException(ResourceHelper.GetErrorResource("Exception_ArgumentOutOfRange_ArrayIndexOutOfRange"));

      // Otherwise defer to the source with the relevant offset

      return Source.GetItemAsync(index + _count);
    }

    public override int IndexOf(T item)
    {
      // Get the index from the source

      int index = Source.IndexOf(item);

      // Return the supplied value if within the bounds of the Skip, otherwise return -1

      if (index < _count)
        return -1;
      else
        return index - _count;
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
      // Determine the index that the subscribers will see items added at
      //   - If the update is before the start of the window, then items will flow into the start
      //   - If the update is within the window then the items will be placed there

      int oldCount = Math.Max(0, _sourceCount - _count);
      int newCount = Math.Max(0, _sourceCount + update.Count - _count);

      if (oldCount != newCount)
      {
        int addIndex = Math.Max(0, update.Index - _count);
        PostUpdate(new DataListUpdate(DataListUpdateAction.Add, addIndex, newCount - oldCount));
      }

      // Set the last known source count

      _sourceCount += update.Count;
    }

    private void ProcessUpdate_Remove(DataListUpdate update)
    {
      // Determine the index that the subscribers will see items removed from
      //   - If the update is before the start of the window, then items will flow out from the start
      //   - If the update is within the window then the items will be removed there

      int oldCount = Math.Max(0, _sourceCount - _count);
      int newCount = Math.Max(0, _sourceCount - update.Count - _count);

      if (oldCount != newCount)
      {
        int removeIndex = Math.Max(0, update.Index - _count);
        PostUpdate(new DataListUpdate(DataListUpdateAction.Remove, removeIndex, oldCount - newCount));
      }

      // Set the last known source count

      _sourceCount += update.Count;
    }
  }
}
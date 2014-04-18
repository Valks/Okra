using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Okra.Data
{
  public class SimplePagedDataListSource<T> : PagedDataListSource<T>
  {
    private const int PAGE_SIZE = 20;
    private readonly IList<Func<int, int, DataListPageResult<T>>> _requestFunc;
    private readonly IList<Func<int?>> _countFunc;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="requestFunc">pageNumber, pageSize, result</param>
    /// <param name="countFunc">result</param>
    public SimplePagedDataListSource(Func<int, int, DataListPageResult<T>> requestFunc, Func<int?> countFunc = null)
    {
      _requestFunc = new Func<int, int, DataListPageResult<T>>[1];
      _countFunc = new Func<int?>[1];

      _requestFunc[0] = requestFunc;
      _countFunc[0] = countFunc ?? (() => _requestFunc[0](0, 1).TotalItemCount);
    }

    public SimplePagedDataListSource(IList<Func<int, int, DataListPageResult<T>>> requestFunc, IList<Func<int?>> countFunc)
    {
      _requestFunc = requestFunc;
      if (countFunc == null)
      {
        countFunc = new Func<int?>[_requestFunc.Count()];
        for (int index = 0; index < _requestFunc.Count; index++)
        {
          var requestFunction = _requestFunc[index];
          countFunc[index] = () => requestFunction(0, 1).TotalItemCount;
        }
      }

      _countFunc = countFunc;
    }

    public SimplePagedDataListSource(IQueryable<T> query)
    {
      _requestFunc = new Func<int, int, DataListPageResult<T>>[1];
      _countFunc = new Func<int?>[1];

      _requestFunc[0] = (pageNumber, pageSize) =>
      {
        var result = query.Skip(pageNumber*pageSize).Take(pageSize).ToList();
        return new DataListPageResult<T>(result.Count,pageSize,pageNumber,result);
      };

      _countFunc[0] = () => query.Count();
    }

    public SimplePagedDataListSource(IList<IQueryable<T>> query)
    {
      _requestFunc = new Func<int, int, DataListPageResult<T>>[query.Count];
      _countFunc = new Func<int?>[query.Count];

      for (int i = 0; i < query.Count; i++)
      {
        int index = i;
        _requestFunc[i] = (pageNumber, pageSize) =>
        {
          var result = query[index].Skip(pageNumber * pageSize).Take(pageSize).ToList();
          return new DataListPageResult<T>(result.Count, pageSize, pageNumber, result);
        };

        _countFunc[index] = () => query[index].Count();
      }
    }

    protected override Task<DataListPageResult<T>> FetchCountAsync()
    {
      var count = _countFunc.AsParallel().WithDegreeOfParallelism(6).AsUnordered().Sum(countFunc => countFunc() ?? 0);
      return new Task<DataListPageResult<T>>(() => new DataListPageResult<T>(count, null, null, null));
    }

    protected override Task<DataListPageResult<T>> FetchPageAsync(int pageNumber)
    {
      return new Task<DataListPageResult<T>>(() =>
      {
        //Find the data to load.
        var dataSourceItemCounts =
          _countFunc.AsParallel().WithDegreeOfParallelism(6).AsOrdered().Select(countFunc => countFunc() ?? -1).ToList();

        int currentIndex = 0;
        int retrievedItems = 0;
        List<T> items = new List<T>(PAGE_SIZE);

        for (int index = 0; index < dataSourceItemCounts.Count; index++)
        {
          var sourceItemCount = dataSourceItemCounts[index];
          if (sourceItemCount == -1)
            throw new InvalidDataException("Error retrieving the item count.");

          if (currentIndex <= pageNumber*PAGE_SIZE)
          {
            DataListPageResult<T> result = _requestFunc[index](pageNumber, PAGE_SIZE - retrievedItems);
            items.AddRange(result.Page);
            retrievedItems += result.TotalItemCount ?? 0;

            if (result.TotalItemCount == PAGE_SIZE - retrievedItems)
              break;
          }

          currentIndex += sourceItemCount;
        }
        return new DataListPageResult<T>(retrievedItems, PAGE_SIZE, pageNumber, items);
      });
    }

    protected override Task<DataListPageResult<T>> FetchPageSizeAsync()
    {
      return new Task<DataListPageResult<T>>(() => new DataListPageResult<T>(null,PAGE_SIZE,null,null));
    }
  }

  
}
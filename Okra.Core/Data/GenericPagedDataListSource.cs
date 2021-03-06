﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Okra.Data
{
  public class GenericPagedDataListSource<T, U> : PagedDataListSource<T> where U : IGenericPagedRequestResult<T>
  {
    private const int PAGE_SIZE = 20;
    private readonly Func<int, int, U> _requestFunc;
    private readonly Func<int> _countFunc; 

    public GenericPagedDataListSource(Func<int, int, U> requestFunc, Func<int> countFunc)
    {
      _requestFunc = requestFunc;
      if (countFunc == null)
      {
        countFunc = () => requestFunc(0, 1).Count;
      }

      _countFunc = countFunc;
    }

    protected override Task<DataListPageResult<T>> FetchCountAsync()
    {
      return new Task<DataListPageResult<T>>(() => new DataListPageResult<T>(_countFunc(), null, null, null));
    }

    protected override Task<DataListPageResult<T>> FetchPageAsync(int pageNumber)
    {
      return new Task<DataListPageResult<T>>(() =>
      {
        U result = _requestFunc(pageNumber, PAGE_SIZE);

        return new DataListPageResult<T>(result.Count, PAGE_SIZE, pageNumber, result.Items);
      });
    }

    protected override Task<DataListPageResult<T>> FetchPageSizeAsync()
    {
      return new Task<DataListPageResult<T>>(() => new DataListPageResult<T>(null,PAGE_SIZE,null,null));
    }
  }

  
}
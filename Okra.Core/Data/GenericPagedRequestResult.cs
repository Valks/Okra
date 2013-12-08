using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Okra.Data
{
  public class GenericPagedRequestResult<T> : IGenericPagedRequestResult<T>
  {
    public GenericPagedRequestResult(IList<T> items, int count, int returnCount, uint updateId)
    {
      Items = items;
      Count = count;
      ReturnCount = returnCount;
      UpdateId = updateId;
    }

    public IList<T> Items { get; set; }
    public int Count { get; set; }
    public int ReturnCount { get; set; }
    public uint UpdateId { get; set; }
  }
}

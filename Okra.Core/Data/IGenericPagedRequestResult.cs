using System.Collections.Generic;

namespace Okra.Data
{
  public interface IGenericPagedRequestResult<T>
  {
    IList<T> Items { get; set; }
    int Count { get; set; }
  }
}
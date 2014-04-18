using System.Collections.Generic;

namespace Windows.Foundation.Collections
{
  public interface IObservableVector<T> : IList<T>
  {
    event VectorChangedEventHandler<T> VectorChanged;
  }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Okra.Data
{
  // Summary:
  //     Specifies a calling contract for collection views that support incremental
  //     loading.
  public interface ISupportIncrementalLoading
  {
    // Summary:
    //     Gets a sentinel value that supports incremental loading implementations.
    //
    // Returns:
    //     True if additional unloaded items remain in the view; otherwise, false.
    bool HasMoreItems { get; }

    // Summary:
    //     Initializes incremental loading from the view.
    //
    // Parameters:
    //   count:
    //     The number of items to load.
    //
    // Returns:
    //     The wrapped results of the load operation.
    Task<LoadMoreItemsResult> LoadMoreItemsAsync(uint count);
}
  }

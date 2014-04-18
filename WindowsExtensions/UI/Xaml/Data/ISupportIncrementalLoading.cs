using System.Threading.Tasks;

namespace Windows.UI.Xaml.Data
{
  public interface ISupportIncrementalLoading
  {
    Task<LoadMoreItemsResult> LoadMoreItemsAsync(uint count);
    bool HasMoreItems { get; }
  }
}

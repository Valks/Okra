using System;
using System.Threading;
using System.Threading.Tasks;

namespace Okra.Data.Helpers
{
  public static class TaskHelper
  {
    public static Task<T> RunAsync<T>(Func<T> function)
    {
      if (function == null) throw new ArgumentNullException("function");
      var tcs = new TaskCompletionSource<T>();
      ThreadPool.QueueUserWorkItem(_ =>
        {
          try
          {
            T result = function();
            tcs.SetResult(result);
          }
          catch (Exception exc)
          {
            tcs.SetException(exc);
          }
        });
      return tcs.Task;
    }

    public static Task RunAsync(Task action)
    {
      var tcs = new TaskCompletionSource<Object>();
      ThreadPool.QueueUserWorkItem(_ =>
        {
          try
          {
            action.Start();
            tcs.SetResult(null);
          }
          catch (Exception exc)
          {
            tcs.SetException(exc);
          }
        });
      return tcs.Task;
    }
  }
}
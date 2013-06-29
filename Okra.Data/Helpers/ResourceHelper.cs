using System;

namespace Okra.Data.Helpers
{
    internal static class ResourceHelper
    {
        // *** Methods ***

        public static string GetErrorResource(string resourceName)
        {
          throw new Exception(resourceName);
        }
    }
}

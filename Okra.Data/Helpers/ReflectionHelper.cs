using System;
using System.Collections.Generic;
using System.Linq;

namespace Okra.Data.Helpers
{
    internal static class ReflectionHelper
    {
        // *** Methods ***

      public static Type GetClosedGenericType(object obj, Type openGenericType)
      {
        // If the object is null then just return null

        if (obj == null)
          return null;

        // Otherwise use reflection to get all interfaces implemented by the type

        //IEnumerable<Type> implementedInterfaces = obj.GetType().GetInterfaces().GetTypeInfo().ImplementedInterfaces;
        IEnumerable<Type> implementedInterfaces = obj.GetType().GetInterfaces();

        // Return the first interface matching the specified open generic type (or null if this interface is not implemented)
        return
          implementedInterfaces.FirstOrDefault(
            i => i.IsConstructedGenericType && i.GetGenericTypeDefinition() == openGenericType);
      }
    }
}

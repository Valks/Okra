using System;
using System.Globalization;

namespace Okra.Data
{
    public struct DataListUpdate
    {
        // *** Fields ***

        private readonly DataListUpdateAction _action;
        private readonly int _index;
        private readonly int _count;

        // *** Constructors ***

        public DataListUpdate(DataListUpdateAction action)
        {
            // Validate Parameters

          if (action != DataListUpdateAction.Reset)
            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
              "The called constructor is not supported for the action '{0}'.", action));

            // Set Fields

            _action = action;
            _index = 0;
            _count = 0;
        }

        public DataListUpdate(DataListUpdateAction action, int index, int count)
        {
            // Validate Parameters

          if (action != DataListUpdateAction.Add && action != DataListUpdateAction.Remove)
            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
              "The called constructor is not supported for the action '{0}'.", action));

          if (index < 0)
            throw new ArgumentOutOfRangeException("count",
              string.Format(CultureInfo.InvariantCulture, "The parameter must be greater than or equal to zero."));

          if (count <= 0)
            throw new ArgumentOutOfRangeException("count",
              string.Format(CultureInfo.InvariantCulture, "The parameter must be greater than zero."));

            // Set Fields

            _action = action;
            _index = index;
            _count = count;
        }

        // *** Properties ***

        public DataListUpdateAction Action
        {
            get
            {
                return _action;
            }
        }

        public int Count
        {
            get
            {
                return _count;
            }
        }

        public int Index
        {
            get
            {
                return _index;
            }
        }
    }
}

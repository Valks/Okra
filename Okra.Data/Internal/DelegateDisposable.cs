using System;

namespace Okra.Data.Internal
{
    internal class DelegateDisposable : IDisposable
    {
        // *** Fields ***

        private readonly Action _disposeAction;
        private bool _isDisposed;

        // *** Constructors ***

        public DelegateDisposable(Action disposeAction)
        {
            _disposeAction = disposeAction;
        }

        // *** Methods ***

        public void Dispose()
        {
          if (_isDisposed) return;

          _disposeAction();
          _isDisposed = true;
        }
    }
}

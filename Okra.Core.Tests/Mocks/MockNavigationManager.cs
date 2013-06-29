using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Okra.Navigation;

namespace Okra.Tests.Mocks
{
    public class MockNavigationManager : INavigationManager
    {
        // *** Fields ***

        private readonly Func<string, INavigationEntry> pageEntryCreator;

        private readonly Stack<INavigationEntry> pageStack = new Stack<INavigationEntry>();

        public IList<Tuple<string, object>> NavigatedPages = new List<Tuple<string, object>>();
        public bool CanRestoreNavigationStack = false;

        // *** Events ***

        public event EventHandler CanGoBackChanged;
        public event EventHandler<PageNavigationEventArgs> NavigatingFrom;
        public event EventHandler<PageNavigationEventArgs> NavigatedTo;

        // *** Constructors ***

        public MockNavigationManager()
            :this(pageName => new MockNavigationEntry(pageName))
        {
        }

        public MockNavigationManager(Func<string, INavigationEntry> pageEntryCreator)
        {
            this.pageEntryCreator = pageEntryCreator;

            this.HomePageName = "Home";
        }

        // *** Properties ***

        public bool CanGoBack
        {
            get { throw new NotImplementedException(); }
        }

        public INavigationEntry CurrentPage
        {
            get
            {
                if (pageStack.Count == 0)
                    return null;
                else
                    return pageStack.Peek();
            }
        }

        public string HomePageName
        {
            get;
            set;
        }

        public NavigationStorageType NavigationStorageType
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        // *** Methods ***

        public bool CanNavigateTo(string pageName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<object> GetCurrentPageElements()
        {
            throw new NotImplementedException();
        }

        public void GoBack()
        {
            throw new NotImplementedException();
        }

        public void NavigateTo(string pageName)
        {
            NavigateTo(pageName, null);
        }

        public void NavigateTo(string pageName, object arguments)
        {
            NavigatedPages.Add(new Tuple<string, object>(pageName, arguments));
            pageStack.Push(pageEntryCreator(pageName));
        }

        public Task<bool> RestoreNavigationStack()
        {
            if (CanRestoreNavigationStack)
                NavigateTo("[Restored Pages]");

            return Task.FromResult(CanRestoreNavigationStack);
        }

        // *** Mock Methods ***

        public void RaiseNavigatedTo(PageNavigationEventArgs eventArgs)
        {
            if (NavigatedTo != null)
                NavigatedTo(this, eventArgs);
        }

        public void RaiseNavigatingFrom(PageNavigationEventArgs eventArgs)
        {
            if (NavigatingFrom != null)
                NavigatingFrom(this, eventArgs);
        }

        // *** Private sub-classes ***

        private class MockNavigationEntry : INavigationEntry
        {
            // *** Constructors ***

            public MockNavigationEntry(string pageName)
            {
                this.PageName = pageName;
            }

            // *** Properties ***

            public string PageName
            {
                get;
                private set;
            }

            // *** Methods ***

            public IEnumerable<object> GetElements()
            {
                return new object[0];
            }
        }
    }
}

using Okra.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;

namespace Okra.Navigation
{
    public abstract class NavigationBase : INavigationBase
    {
        // *** Fields ***

        private readonly IViewFactory viewFactory;

        private readonly Stack<INavigationEntry> navigationStack = new Stack<INavigationEntry>();
        private readonly NavigationContext navigationContext;

        // *** Events ***

        public event EventHandler CanGoBackChanged;
        public event EventHandler<PageNavigationEventArgs> NavigatingFrom;
        public event EventHandler<PageNavigationEventArgs> NavigatedTo;

        // *** Constructors ***

        public NavigationBase(IViewFactory viewFactory)
        {
            this.viewFactory = viewFactory;

            this.navigationContext = new NavigationContext(this);
        }

        // *** Properties ***

        public virtual bool CanGoBack
        {
            get
            {
                return NavigationStack.Count > 0;
            }
        }

        public INavigationEntry CurrentPage
        {
            get
            {
                if (navigationStack.Count > 0)
                    return navigationStack.Peek();
                else
                    return null;
            }
        }

        // *** Protected Properties ***

        protected Stack<INavigationEntry> NavigationStack
        {
            get
            {
                return navigationStack;
            }
        }

        // *** Private Properties ***

        private NavigationEntry CurrentNavigationEntry
        {
            get
            {
                return CurrentPage as NavigationEntry;
            }
        }

        // *** Methods ***

        public bool CanNavigateTo(string pageName)
        {
            // Query the underlying view factory to see if the page exists

            return viewFactory.IsViewDefined(pageName);
        }

        public void GoBack()
        {
            // Check that we can go back

            if (!CanGoBack)
                throw new InvalidOperationException(ResourceHelper.GetErrorResource("Exception_InvalidOperation_CannotGoBackWithEmptyBackStack"));

            // Pop the last page from the stack, call NavigationFrom() and dispose any cached items

            INavigationEntry oldNavigationEntry = NavigationStack.Pop();
            CallNavigatingFrom(oldNavigationEntry, NavigationMode.Back);

            if (oldNavigationEntry is NavigationEntry)
                ((NavigationEntry)oldNavigationEntry).DisposeCachedItems();

            // Display the new current page from the navigation stack

            DisplayNavigationEntry(CurrentNavigationEntry);

            // If the value of CanGoBack has changed then raise an event
            // NB: We can assume that the old value was true otherwise an exception is thrown on entry to this method

            if (!CanGoBack)
                OnCanGoBackChanged();

            // Call NavigatingTo()

            CallNavigatedTo(CurrentNavigationEntry, NavigationMode.Back);
        }

        public void NavigateTo(string pageName)
        {
            NavigateTo(pageName, null);
        }

        public void NavigateTo(string pageName, object arguments)
        {
            // Call NavigatingFrom on the existing navigation entry (if one exists)

            CallNavigatingFrom(CurrentNavigationEntry, NavigationMode.New);

            // Get the old value of CanGoBack

            bool oldCanGoBack = CanGoBack;

            // Create the new navigation entry and push it onto the navigation stack

            NavigationEntry navigationEntry = new NavigationEntry(pageName, arguments);
            navigationStack.Push(navigationEntry);

            // Navigate to the page

            DisplayNavigationEntry(navigationEntry);

            // If the value of CanGoBack has changed then raise an event

            if (CanGoBack != oldCanGoBack)
                OnCanGoBackChanged();

            // Call NavigatedTo on the new navigation entry

            CallNavigatedTo(navigationEntry, NavigationMode.New);
        }

        // *** Protected Methods ***

        protected void CallNavigatedTo(INavigationEntry entry, NavigationMode navigationMode)
        {
            if (entry == null)
                return;

            // Fire the NavigatedTo event

            OnNavigatedTo(new PageNavigationEventArgs(entry, navigationMode));

            // Call NavigatedTo on all page elements

            foreach (object element in entry.GetElements())
            {
                if (element is INavigationAware)
                    ((INavigationAware)element).NavigatedTo(navigationMode);
            }
        }

        protected void CallNavigatingFrom(INavigationEntry entry, NavigationMode navigationMode)
        {
            if (entry == null)
                return;

            // Fire the NavigatingFrom event

            OnNavigatingFrom(new PageNavigationEventArgs(entry, navigationMode));

            // Call NavigatingFrom on all page elements

            foreach (object element in entry.GetElements())
            {
                if (element is INavigationAware)
                    ((INavigationAware)element).NavigatingFrom(navigationMode);
            }
        }

        protected void DisplayNavigationEntry(INavigationEntry entry)
        {
            if (entry == null)
            {
                // If this entry is null then simply pass null to the deriving class

                DisplayPage(null);
            }
            else
            {
                // Cast to the internal NavigationEntry class so we can access all members
                // TODO : Try to get rid of the need for this cast? (or check that it is of the correct type and provide alternatives for derived classes)

                NavigationEntry internalEntry = (NavigationEntry)entry;

                // If the page and VM have not been created then do so

                if (internalEntry.ViewLifetimeContext == null)
                    CreatePage(internalEntry);

                // Navigate to the relevant page

                DisplayPage(internalEntry.ViewLifetimeContext.View);
            }
        }

        protected abstract void DisplayPage(object page);

        protected virtual void OnCanGoBackChanged()
        {
            EventHandler eventHandler = CanGoBackChanged;

            if (eventHandler != null)
                eventHandler(this, EventArgs.Empty);
        }

        protected virtual void OnNavigatingFrom(PageNavigationEventArgs args)
        {
            EventHandler<PageNavigationEventArgs> eventHandler = NavigatingFrom;

            if (eventHandler != null)
                eventHandler(this, args);
        }

        protected virtual void OnNavigatedTo(PageNavigationEventArgs args)
        {
            EventHandler<PageNavigationEventArgs> eventHandler = NavigatedTo;

            if (eventHandler != null)
                eventHandler(this, args);
        }

        protected void RestoreState(NavigationState state)
        {
            foreach (NavigationEntryState entryState in state.NavigationStack.Reverse())
            {
                // Push the restored navigation entry onto the stack

                NavigationStack.Push(new NavigationEntry(entryState.PageName, entryState.ArgumentsData, entryState.StateData));
            }

            // Display the last page in the stack

            DisplayNavigationEntry(CurrentPage);

            // Call NavigatedTo() on the restored page

            CallNavigatedTo(CurrentPage, NavigationMode.Refresh);
        }

        protected NavigationState StoreState()
        {
            // Create an object for storage of the navigation state

            NavigationState state = new NavigationState();

            // Enumerate all NavigationEntries in the navigation stack

            foreach (NavigationEntry entry in NavigationStack)
            {
                // Save the page state
                // TODO : Do this when navigating away from each page to save time when suspending

                SavePageState(entry);

                // Create an object for storage of this entry

                NavigationEntryState entryState = new NavigationEntryState(entry.PageName, entry.ArgumentsData, entry.StateData);
                state.NavigationStack.Add(entryState);
            }

            // Return the result

            return state;
        }

        // *** Private Methods ***

        private void CreatePage(NavigationEntry entry)
        {
            // Create the View

            IViewLifetimeContext viewLifetimeContext = viewFactory.CreateView(entry.PageName, navigationContext);
            entry.ViewLifetimeContext = viewLifetimeContext;

            // Activate the view model if it implements IActivatable<,>
            // NB: Use reflection as we do not know the generic parameter types

            object viewModel = entry.ViewLifetimeContext.ViewModel;
            Type activatableInterface = ReflectionHelper.GetClosedGenericType(viewModel, typeof(IActivatable<,>));

            if (activatableInterface != null)
            {
                // If required deserialize the arguments and state

                entry.DeserializeData(activatableInterface.GenericTypeArguments[0], activatableInterface.GenericTypeArguments[1]);

                // Activate the view model

                MethodInfo activateMethod = activatableInterface.GetTypeInfo().GetDeclaredMethod("Activate");
                activateMethod.Invoke(viewModel, new object[] { entry.Arguments, entry.State });
            }
        }

        private void SavePageState(NavigationEntry entry)
        {
            // If the view model is IActivatable<,> then use this to save the page state
            // NB: First check that the view has been created - this may still have state from a previous instance
            // NB: Use reflection as we do not know the generic parameter types

            if (entry.ViewLifetimeContext != null)
            {
                // Get the generic IActivatable<,> interface

                object viewModel = entry.ViewLifetimeContext.ViewModel;
                Type activatableInterface = ReflectionHelper.GetClosedGenericType(viewModel, typeof(IActivatable<,>));

                if (activatableInterface != null)
                {
                    // Save the state

                    MethodInfo saveStateMethod = activatableInterface.GetTypeInfo().GetDeclaredMethod("SaveState");
                    entry.State = saveStateMethod.Invoke(viewModel, null);

                    // Serialize the arguments and state

                    entry.SerializeData(activatableInterface.GenericTypeArguments[0], activatableInterface.GenericTypeArguments[1]);
                }
            }
        }
    }
}

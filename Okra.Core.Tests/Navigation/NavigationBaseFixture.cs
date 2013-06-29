using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Okra.Navigation;
using Okra.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;

namespace Okra.Core.Tests.Navigation
{
    [TestClass]
    public class NavigationBaseFixture
    {
        // *** Property Tests ***

        [TestMethod]
        public void CanGoBack_IsFalseIfNoPagesNavigated()
        {
            TestableNavigationBase navigationManager = CreateNavigationManager();

            Assert.AreEqual(false, navigationManager.CanGoBack);
        }

        [TestMethod]
        public void CanGoBack_IsTrueIfOnePageNavigated()
        {
            TestableNavigationBase navigationManager = CreateNavigationManager();

            navigationManager.NavigateTo("Page 1");

            Assert.AreEqual(true, navigationManager.CanGoBack);
        }

        [TestMethod]
        public void CanGoBack_IsFalseIfOnePageNavigatedThenBack()
        {
            TestableNavigationBase navigationManager = CreateNavigationManager();

            navigationManager.NavigateTo("Page 1");
            navigationManager.GoBack();

            Assert.AreEqual(false, navigationManager.CanGoBack);
        }

        // *** CurrentPage Tests ***

        [TestMethod]
        public void CurrentPage_IsInitiallyNull()
        {
            INavigationBase navigationManager = CreateNavigationManager();

            Assert.AreEqual(null, navigationManager.CurrentPage);
        }

        [TestMethod]
        public void CurrentPage_Name_IsSetIfOnePageNavigated()
        {
            TestableNavigationBase navigationManager = CreateNavigationManager();

            navigationManager.NavigateTo("Page 1");

            Assert.AreEqual("Page 1", navigationManager.CurrentPage.PageName);
        }

        [TestMethod]
        public void CurrentPage_Name_IsSetIfTwoPagesNavigatedThenBack()
        {
            TestableNavigationBase navigationManager = CreateNavigationManager();

            navigationManager.NavigateTo("Page 1");
            navigationManager.NavigateTo("Page 2");
            navigationManager.GoBack();

            Assert.AreEqual("Page 1", navigationManager.CurrentPage.PageName);
        }

        [TestMethod]
        public void CurrentPage_Name_IsSetIfTwoPagesNavigated()
        {
            TestableNavigationBase navigationManager = CreateNavigationManager();

            navigationManager.NavigateTo("Page 1");
            navigationManager.NavigateTo("Page 2");

            Assert.AreEqual("Page 2", navigationManager.CurrentPage.PageName);
        }

        [TestMethod]
        public void CurrentPage_GetElements_ReturnsViewModelThenPageIfBothPresent()
        {
            TestableNavigationBase navigationManager = CreateNavigationManager();

            navigationManager.NavigateTo("Page 1");
            MockPage currentPage = navigationManager.NavigatedPages.Cast<MockPage>().LastOrDefault();
            MockViewModel currentViewModel = (MockViewModel)currentPage.DataContext;

            CollectionAssert.AreEqual(new object[] { currentViewModel, currentPage }, navigationManager.CurrentPage.GetElements().ToList());
        }

        [TestMethod]
        public void CurrentPage_GetElements_ReturnsPageIfNoViewModelPresent()
        {
            TestableNavigationBase navigationManager = CreateNavigationManager();

            navigationManager.NavigateTo("Page 3");
            MockPage currentPage = navigationManager.NavigatedPages.Cast<MockPage>().LastOrDefault();

            CollectionAssert.AreEqual(new object[] { currentPage }, navigationManager.CurrentPage.GetElements().ToList());
        }

        // *** Method Tests ***

        [TestMethod]
        public void CanNavigateTo_ReturnsTrue_PageExistsWithSpecifiedName()
        {
            INavigationBase navigationManager = CreateNavigationManager();

            bool canNavigateTo = navigationManager.CanNavigateTo("Page 1");

            Assert.AreEqual(true, canNavigateTo);
        }

        [TestMethod]
        public void CanNavigateTo_ReturnsFalse_NoPageWithSpecifiedName()
        {
            INavigationBase navigationManager = CreateNavigationManager();

            bool canNavigateTo = navigationManager.CanNavigateTo("Page X");

            Assert.AreEqual(false, canNavigateTo);
        }

        [TestMethod]
        public void GoBack_NavigatesToNextPageInStack()
        {
            TestableNavigationBase navigationManager = CreateNavigationManager();

            navigationManager.NavigateTo("Page 1");
            navigationManager.NavigateTo("Page 2");
            navigationManager.GoBack();

            string[] pageNames = navigationManager.NavigatedPages.Cast<MockPage>().Select(page => page.PageName).ToArray();
            CollectionAssert.AreEqual(new string[] { "Page 1", "Page 2", "Page 1" }, pageNames);
        }

        [TestMethod]
        public void GoBack_DisposesCurrentPage_WithViewModel()
        {
            TestableNavigationBase navigationManager = CreateNavigationManager();

            navigationManager.NavigateTo("Page 1");
            navigationManager.NavigateTo("Page 2");
            MockPage currentPage = (MockPage)navigationManager.NavigatedPages.Last();
            navigationManager.GoBack();

            Assert.AreEqual(true, currentPage.IsDisposed);
        }

        [TestMethod]
        public void GoBack_DisposesCurrentViewModel()
        {
            TestableNavigationBase navigationManager = CreateNavigationManager();

            navigationManager.NavigateTo("Page 1");
            navigationManager.NavigateTo("Page 2");
            MockPage currentPage = (MockPage)navigationManager.NavigatedPages.Last();
            MockViewModel<string, string> currentViewModel = (MockViewModel<string, string>)currentPage.DataContext;
            navigationManager.GoBack();

            Assert.AreEqual(true, currentViewModel.IsDisposed);
        }

        [TestMethod]
        public void GoBack_RaisesNavigatingFromEvent()
        {
            TestableNavigationBase navigationManager = CreateNavigationManager();

            navigationManager.NavigateTo("Page 1");
            navigationManager.NavigateTo("Page 2");

            List<PageNavigationEventArgs> navigationEventArgs = new List<PageNavigationEventArgs>();
            navigationManager.NavigatingFrom += delegate(object sender, PageNavigationEventArgs e) { navigationEventArgs.Add(e); };

            navigationManager.GoBack();

            Assert.AreEqual(1, navigationEventArgs.Count);
            Assert.AreEqual("Page 2", navigationEventArgs[0].Page.PageName);
            Assert.AreEqual(NavigationMode.Back, navigationEventArgs[0].NavigationMode);
        }

        [TestMethod]
        public void GoBack_RaisesNavigatedToEvent()
        {
            TestableNavigationBase navigationManager = CreateNavigationManager();

            navigationManager.NavigateTo("Page 1");
            navigationManager.NavigateTo("Page 2");

            List<PageNavigationEventArgs> navigationEventArgs = new List<PageNavigationEventArgs>();
            navigationManager.NavigatedTo += delegate(object sender, PageNavigationEventArgs e) { navigationEventArgs.Add(e); };

            navigationManager.GoBack();

            Assert.AreEqual(1, navigationEventArgs.Count);
            Assert.AreEqual("Page 1", navigationEventArgs[0].Page.PageName);
            Assert.AreEqual(NavigationMode.Back, navigationEventArgs[0].NavigationMode);
        }

        [TestMethod]
        public void GoBack_CallsNavigatingFromOnPreviousViewModel()
        {
            MockViewFactory_WithNavigationAware viewFactory = new MockViewFactory_WithNavigationAware();
            TestableNavigationBase navigationManager = CreateNavigationManager(viewFactory);

            navigationManager.NavigateTo("Page 1");

            navigationManager.NavigateTo("Page 2");
            MockPage currentPage = (MockPage)navigationManager.NavigatedPages.Last();
            MockNavigationAwareViewModel<string, int> currentViewModel = (MockNavigationAwareViewModel<string, int>)currentPage.DataContext;
            currentViewModel.NavigationEvents.Clear();

            navigationManager.GoBack();

            CollectionAssert.AreEqual(new string[] { "NavigatingFrom(Back)" }, currentViewModel.NavigationEvents);
        }

        [TestMethod]
        public void GoBack_CallsNavigatingFromOnPreviousPage()
        {
            MockViewFactory_WithNavigationAware viewFactory = new MockViewFactory_WithNavigationAware();
            TestableNavigationBase navigationManager = CreateNavigationManager(viewFactory);

            navigationManager.NavigateTo("Page 1");

            navigationManager.NavigateTo("Page 2");
            MockNavigationAwarePage currentPage = (MockNavigationAwarePage)navigationManager.NavigatedPages.Last();
            currentPage.NavigationEvents.Clear();

            navigationManager.GoBack();

            CollectionAssert.AreEqual(new string[] { "NavigatingFrom(Back)" }, currentPage.NavigationEvents);
        }

        [TestMethod]
        public void GoBack_CallsNavigatingToOnNewViewModel()
        {
            MockViewFactory_WithNavigationAware viewFactory = new MockViewFactory_WithNavigationAware();
            TestableNavigationBase navigationManager = CreateNavigationManager(viewFactory);

            navigationManager.NavigateTo("Page 1");
            MockPage currentPage = (MockPage)navigationManager.NavigatedPages.Last();
            MockNavigationAwareViewModel<string, int> currentViewModel = (MockNavigationAwareViewModel<string, int>)currentPage.DataContext;

            navigationManager.NavigateTo("Page 2");
            currentViewModel.NavigationEvents.Clear();

            navigationManager.GoBack();

            CollectionAssert.AreEqual(new string[] { "NavigatedTo(Back)" }, currentViewModel.NavigationEvents);
        }

        [TestMethod]
        public void GoBack_CallsNavigatingToOnNewPage()
        {
            MockViewFactory_WithNavigationAware viewFactory = new MockViewFactory_WithNavigationAware();
            TestableNavigationBase navigationManager = CreateNavigationManager(viewFactory);

            navigationManager.NavigateTo("Page 1");
            MockNavigationAwarePage currentPage = (MockNavigationAwarePage)navigationManager.NavigatedPages.Last();

            navigationManager.NavigateTo("Page 2");
            currentPage.NavigationEvents.Clear();

            navigationManager.GoBack();

            CollectionAssert.AreEqual(new string[] { "NavigatedTo(Back)" }, currentPage.NavigationEvents);
        }

        [TestMethod]
        public void GoBack_ThrowsException_NoPageInBackStack()
        {
            TestableNavigationBase navigationManager = CreateNavigationManager();

            Assert.ThrowsException<InvalidOperationException>(() => navigationManager.GoBack());
        }

        [TestMethod]
        public void NavigateTo_NavigatesToSpecifiedPage_WithViewModel()
        {
            TestableNavigationBase navigationManager = CreateNavigationManager();

            navigationManager.NavigateTo("Page 2");

            string[] pageNames = navigationManager.NavigatedPages.Cast<MockPage>().Select(page => page.PageName).ToArray();
            CollectionAssert.AreEqual(new string[] { "Page 2" }, pageNames);
        }

        [TestMethod]
        public void NavigateTo_NavigatesToSpecifiedPage_WithoutViewModel()
        {
            TestableNavigationBase navigationManager = CreateNavigationManager();

            navigationManager.NavigateTo("Page 3");

            string[] pageNames = navigationManager.NavigatedPages.Cast<MockPage>().Select(page => page.PageName).ToArray();
            CollectionAssert.AreEqual(new string[] { "Page 3" }, pageNames);
        }

        [TestMethod]
        public void NavigateTo_NavigatedTwice_NavigatesToSpecifiedPage()
        {
            TestableNavigationBase navigationManager = CreateNavigationManager();

            navigationManager.NavigateTo("Page 1");
            navigationManager.NavigateTo("Page 2");

            string[] pageNames = navigationManager.NavigatedPages.Cast<MockPage>().Select(page => page.PageName).ToArray();
            CollectionAssert.AreEqual(new string[] { "Page 1", "Page 2" }, pageNames);
        }

        [TestMethod]
        public void NavigateTo_SetsSpecifiedViewModel_WithViewModel()
        {
            TestableNavigationBase navigationManager = CreateNavigationManager();

            navigationManager.NavigateTo("Page 2");

            MockPage navigatedPage = (MockPage)navigationManager.NavigatedPages.Last();
            object dataContext = navigatedPage.DataContext;

            Assert.IsNotNull(dataContext);
            Assert.IsInstanceOfType(dataContext, typeof(MockViewModel<string, string>));
            Assert.AreEqual("ViewModel 2", ((MockViewModel<string, string>)dataContext).Name);
        }

        [TestMethod]
        public void NavigateTo_SetsNullViewModel_WithoutViewModel()
        {
            TestableNavigationBase navigationManager = CreateNavigationManager();

            navigationManager.NavigateTo("Page 3");

            MockPage navigatedPage = (MockPage)navigationManager.NavigatedPages.Last();
            object dataContext = navigatedPage.DataContext;

            Assert.IsNull(dataContext);
        }

        [TestMethod]
        public void NavigateTo_PassesNullArgumentToViewModel()
        {
            TestableNavigationBase navigationManager = CreateNavigationManager();

            navigationManager.NavigateTo("Page 2");

            MockPage navigatedPage = (MockPage)navigationManager.NavigatedPages.Last();
            object dataContext = navigatedPage.DataContext;

            Assert.IsNotNull(dataContext);
            Assert.IsInstanceOfType(dataContext, typeof(MockViewModel<string, string>));
            Assert.AreEqual(true, ((MockViewModel<string, string>)dataContext).IsActivated);
            Assert.AreEqual(null, ((MockViewModel<string, string>)dataContext).ActivationArguments);
        }

        [TestMethod]
        public void NavigateTo_WithArguments_PassesArgumentToViewModel()
        {
            TestableNavigationBase navigationManager = CreateNavigationManager();

            navigationManager.NavigateTo("Page 2", "Test Argument");

            MockPage navigatedPage = (MockPage)navigationManager.NavigatedPages.Last();
            object dataContext = navigatedPage.DataContext;

            Assert.IsNotNull(dataContext);
            Assert.IsInstanceOfType(dataContext, typeof(MockViewModel<string, string>));
            Assert.AreEqual(true, ((MockViewModel<string, string>)dataContext).IsActivated);
            Assert.AreEqual("Test Argument", ((MockViewModel<string, string>)dataContext).ActivationArguments);
        }

        [TestMethod]
        public void NavigateTo_PassesNullStateToViewModel_WithStringState()
        {
            TestableNavigationBase navigationManager = CreateNavigationManager();

            navigationManager.NavigateTo("Page 2", "Test Argument");

            MockPage navigatedPage = (MockPage)navigationManager.NavigatedPages.Last();
            object dataContext = navigatedPage.DataContext;

            Assert.IsNotNull(dataContext);
            Assert.IsInstanceOfType(dataContext, typeof(MockViewModel<string, string>));
            Assert.AreEqual(true, ((MockViewModel<string, string>)dataContext).IsActivated);
            Assert.AreEqual(null, ((MockViewModel<string, string>)dataContext).ActivationState);
        }

        [TestMethod]
        public void NavigateTo_PassesNullStateToViewModel_WithComplexState()
        {
            IViewFactory viewFactory = new MockViewFactory_WithComplexState();
            TestableNavigationBase navigationManager = CreateNavigationManager(viewFactory: viewFactory);

            navigationManager.NavigateTo("Page 2", "Test Argument");

            MockPage navigatedPage = (MockPage)navigationManager.NavigatedPages.Last();
            object dataContext = navigatedPage.DataContext;

            Assert.IsNotNull(dataContext);
            Assert.IsInstanceOfType(dataContext, typeof(MockViewModel<string, TestData>));
            Assert.AreEqual(true, ((MockViewModel<string, TestData>)dataContext).IsActivated);
            Assert.AreEqual(null, ((MockViewModel<string, TestData>)dataContext).ActivationState);
        }

        [TestMethod]
        public void NavigateTo_PassesNullStateToViewModel_WithNonNullableState()
        {
            IViewFactory viewFactory = new MockViewFactory_WithNonNullableState();
            TestableNavigationBase navigationManager = CreateNavigationManager(viewFactory: viewFactory);

            navigationManager.NavigateTo("Page 2", "Test Argument");

            MockPage navigatedPage = (MockPage)navigationManager.NavigatedPages.Last();
            object dataContext = navigatedPage.DataContext;

            Assert.IsNotNull(dataContext);
            Assert.IsInstanceOfType(dataContext, typeof(MockViewModel<string, int>));
            Assert.AreEqual(true, ((MockViewModel<string, int>)dataContext).IsActivated);
            Assert.AreEqual(0, ((MockViewModel<string, int>)dataContext).ActivationState);
        }

        [TestMethod]
        public void NavigateTo_PassesNavigationContextToViewFactory()
        {
            TestableNavigationBase navigationManager = CreateNavigationManager();

            navigationManager.NavigateTo("Page 2");

            MockPage page = (MockPage)navigationManager.NavigatedPages.First();
            INavigationContext navigationContext = page.NavigationContext;

            Assert.IsNotNull(navigationContext);
            Assert.AreEqual(navigationManager, navigationContext.GetCurrent());
        }

        [TestMethod]
        public void NavigateTo_RaisesNavigatingFromEvent()
        {
            TestableNavigationBase navigationManager = CreateNavigationManager();

            navigationManager.NavigateTo("Page 1");

            List<PageNavigationEventArgs> navigationEventArgs = new List<PageNavigationEventArgs>();
            navigationManager.NavigatingFrom += delegate(object sender, PageNavigationEventArgs e) { navigationEventArgs.Add(e); };

            navigationManager.NavigateTo("Page 2");

            Assert.AreEqual(1, navigationEventArgs.Count);
            Assert.AreEqual("Page 1", navigationEventArgs[0].Page.PageName);
            Assert.AreEqual(NavigationMode.New, navigationEventArgs[0].NavigationMode);
        }

        [TestMethod]
        public void NavigateTo_RaisesNavigatedToEvent()
        {
            TestableNavigationBase navigationManager = CreateNavigationManager();

            navigationManager.NavigateTo("Page 1");

            List<PageNavigationEventArgs> navigationEventArgs = new List<PageNavigationEventArgs>();
            navigationManager.NavigatedTo += delegate(object sender, PageNavigationEventArgs e) { navigationEventArgs.Add(e); };

            navigationManager.NavigateTo("Page 2");

            Assert.AreEqual(1, navigationEventArgs.Count);
            Assert.AreEqual("Page 2", navigationEventArgs[0].Page.PageName);
            Assert.AreEqual(NavigationMode.New, navigationEventArgs[0].NavigationMode);
        }

        [TestMethod]
        public void NavigateTo_CallsNavigatingFromOnPreviousViewModel()
        {
            MockViewFactory_WithNavigationAware viewFactory = new MockViewFactory_WithNavigationAware();
            TestableNavigationBase navigationManager = CreateNavigationManager(viewFactory: viewFactory);

            navigationManager.NavigateTo("Page 1");
            MockPage currentPage = (MockPage)navigationManager.NavigatedPages.Last();
            MockNavigationAwareViewModel<string, int> currentViewModel = (MockNavigationAwareViewModel<string, int>)currentPage.DataContext;
            currentViewModel.NavigationEvents.Clear();

            navigationManager.NavigateTo("Page 2");

            CollectionAssert.AreEqual(new string[] { "NavigatingFrom(New)" }, currentViewModel.NavigationEvents);
        }

        [TestMethod]
        public void NavigateTo_CallsNavigatingFromOnPreviousPage()
        {
            MockViewFactory_WithNavigationAware viewFactory = new MockViewFactory_WithNavigationAware();
            TestableNavigationBase navigationManager = CreateNavigationManager(viewFactory: viewFactory);

            navigationManager.NavigateTo("Page 1");
            MockNavigationAwarePage currentPage = (MockNavigationAwarePage)navigationManager.NavigatedPages.Last();
            currentPage.NavigationEvents.Clear();

            navigationManager.NavigateTo("Page 2");

            CollectionAssert.AreEqual(new string[] { "NavigatingFrom(New)" }, currentPage.NavigationEvents);
        }

        [TestMethod]
        public void NavigateTo_CallsNavigatingToOnNewViewModel()
        {
            MockViewFactory_WithNavigationAware viewFactory = new MockViewFactory_WithNavigationAware();
            TestableNavigationBase navigationManager = CreateNavigationManager(viewFactory: viewFactory);

            navigationManager.NavigateTo("Page 1");

            navigationManager.NavigateTo("Page 2");
            MockPage currentPage = (MockPage)navigationManager.NavigatedPages.Last();
            MockNavigationAwareViewModel<string, int> currentViewModel = (MockNavigationAwareViewModel<string, int>)currentPage.DataContext;

            CollectionAssert.AreEqual(new string[] { "NavigatedTo(New)" }, currentViewModel.NavigationEvents);
        }

        [TestMethod]
        public void NavigateTo_CallsNavigatingToOnNewPage()
        {
            MockViewFactory_WithNavigationAware viewFactory = new MockViewFactory_WithNavigationAware();
            TestableNavigationBase navigationManager = CreateNavigationManager(viewFactory: viewFactory);

            navigationManager.NavigateTo("Page 1");

            navigationManager.NavigateTo("Page 2");
            MockNavigationAwarePage currentPage = (MockNavigationAwarePage)navigationManager.NavigatedPages.Last();

            CollectionAssert.AreEqual(new string[] { "NavigatedTo(New)" }, currentPage.NavigationEvents);
        }

        [TestMethod]
        public void NavigateTo_ThrowsException_NoPageWithSpecifiedName()
        {
            TestableNavigationBase navigationManager = CreateNavigationManager();

            Assert.ThrowsException<InvalidOperationException>(() => navigationManager.NavigateTo("Page X"));
        }

        [TestMethod]
        public void RestoreState_RestoresNavigationState()
        {
            TestableNavigationBase navigationManager = CreateNavigationManager();

            NavigationState state = new NavigationState()
            {
                NavigationStack =
                        {
                            new NavigationEntryState("Page 2", null, null),
                            new NavigationEntryState("Page 1", null, null),
                        }
            };

            navigationManager.RestoreState(state);

            // Step back through all pages in the navigation stack

            navigationManager.GoBack();

            // Assert that the current pages are restored

            string[] pageNames = navigationManager.NavigatedPages.Cast<MockPage>().Select(page => page.PageName).ToArray();
            string[] viewModels = navigationManager.NavigatedPages.Cast<MockPage>().Select(page => ((MockViewModel<string, string>)page.DataContext).Name).ToArray();
            CollectionAssert.AreEqual(new string[] { "Page 2", "Page 1" }, pageNames);
            CollectionAssert.AreEqual(new string[] { "ViewModel 2", "ViewModel 1" }, viewModels);
        }

        [TestMethod]
        public void RestoreState_RestoresStringArguments()
        {
            TestableNavigationBase navigationManager = CreateNavigationManager();

            NavigationState state = new NavigationState()
            {
                NavigationStack =
                        {
                            new NavigationEntryState("Page 2", SerializationHelper.SerializeToArray("Test Argument"), null)
                        }
            };

            navigationManager.RestoreState(state);
            MockPage page = navigationManager.NavigatedPages[0] as MockPage;
            MockViewModel<string, string> viewModel = page.DataContext as MockViewModel<string, string>;

            Assert.AreEqual(true, viewModel.IsActivated);
            Assert.AreEqual("Test Argument", viewModel.ActivationArguments);
        }

        [TestMethod]
        public void RestoreState_RestoresComplexArguments()
        {
            IViewFactory viewFactory = new MockViewFactory_WithComplexArguments();
            TestableNavigationBase navigationManager = CreateNavigationManager(viewFactory);

            NavigationState state = new NavigationState()
            {
                NavigationStack =
                        {
                            new NavigationEntryState("Page 2", SerializationHelper.SerializeToArray(new TestData { Text = "Test Text", Number = 42 }), null)
                        }
            };

            navigationManager.RestoreState(state);
            MockPage page = navigationManager.NavigatedPages[0] as MockPage;
            MockViewModel<TestData, string> viewModel = page.DataContext as MockViewModel<TestData, string>;

            Assert.AreEqual(true, viewModel.IsActivated);
            Assert.AreEqual("Test Text", viewModel.ActivationArguments.Text);
            Assert.AreEqual(42, viewModel.ActivationArguments.Number);
        }

        [TestMethod]
        public void RestoreState_RestoresNullArguments()
        {
            TestableNavigationBase navigationManager = CreateNavigationManager();

            NavigationState state = new NavigationState()
            {
                NavigationStack =
                        {
                            new NavigationEntryState("Page 2", null, null)
                        }
            };

            navigationManager.RestoreState(state);
            MockPage page = navigationManager.NavigatedPages[0] as MockPage;
            MockViewModel<string, string> viewModel = page.DataContext as MockViewModel<string, string>;

            Assert.AreEqual(true, viewModel.IsActivated);
            Assert.AreEqual(null, viewModel.ActivationArguments);
        }

        [TestMethod]
        public void RestoreState_RestoresStringState_ForLastPage()
        {
            TestableNavigationBase navigationManager = CreateNavigationManager();

            NavigationState state = new NavigationState()
            {
                NavigationStack =
                        {
                            new NavigationEntryState("Page 2", null, SerializationHelper.SerializeToArray("Test State"))
                        }
            };

            navigationManager.RestoreState(state);
            MockPage page = navigationManager.NavigatedPages[0] as MockPage;
            MockViewModel<string, string> viewModel = page.DataContext as MockViewModel<string, string>;

            Assert.AreEqual(true, viewModel.IsActivated);
            Assert.AreEqual("Test State", viewModel.ActivationState);
        }

        [TestMethod]
        public void RestoreState_RestoresComplexState_ForLastPage()
        {
            IViewFactory viewFactory = new MockViewFactory_WithComplexState();
            TestableNavigationBase navigationManager = CreateNavigationManager(viewFactory);

            NavigationState state = new NavigationState()
            {
                NavigationStack =
                        {
                            new NavigationEntryState("Page 2", null, SerializationHelper.SerializeToArray(new TestData { Text = "Test Text", Number = 42 }))
                        }
            };

            navigationManager.RestoreState(state);
            MockPage page = navigationManager.NavigatedPages[0] as MockPage;
            MockViewModel<string, TestData> viewModel = page.DataContext as MockViewModel<string, TestData>;

            Assert.AreEqual(true, viewModel.IsActivated);
            Assert.AreEqual("Test Text", viewModel.ActivationState.Text);
            Assert.AreEqual(42, viewModel.ActivationState.Number);
        }

        [TestMethod]
        public void RestoreState_RestoresNullState_ForLastPage()
        {
            TestableNavigationBase navigationManager = CreateNavigationManager();

            NavigationState state = new NavigationState()
            {
                NavigationStack =
                        {
                            new NavigationEntryState("Page 2", null, null)
                        }
            };

            navigationManager.RestoreState(state);
            MockPage page = navigationManager.NavigatedPages[0] as MockPage;
            MockViewModel<string, string> viewModel = page.DataContext as MockViewModel<string, string>;

            Assert.AreEqual(true, viewModel.IsActivated);
            Assert.AreEqual(null, viewModel.ActivationState);
        }

        [TestMethod]
        public void RestoreState_RestoresStringState_ForPreviousPage()
        {
            TestableNavigationBase navigationManager = CreateNavigationManager();

            NavigationState state = new NavigationState()
            {
                NavigationStack =
                        {
                            new NavigationEntryState("Page 2", null, SerializationHelper.SerializeToArray("Test State")),
                            new NavigationEntryState("Page 1", null, null)
                        }
            };

            navigationManager.RestoreState(state);
            navigationManager.GoBack();
            MockPage page = navigationManager.NavigatedPages[0] as MockPage;
            MockViewModel<string, string> viewModel = page.DataContext as MockViewModel<string, string>;

            Assert.AreEqual(true, viewModel.IsActivated);
            Assert.AreEqual("Test State", viewModel.ActivationState);
        }

        [TestMethod]
        public void RestoreState_RestoresComplexState_ForPreviousPage()
        {
            IViewFactory viewFactory = new MockViewFactory_WithComplexState();
            TestableNavigationBase navigationManager = CreateNavigationManager(viewFactory);

            NavigationState state = new NavigationState()
            {
                NavigationStack =
                        {
                            new NavigationEntryState("Page 2", null, SerializationHelper.SerializeToArray(new TestData { Text = "Test Text", Number = 42 })),
                            new NavigationEntryState("Page 1", null, null)
                        }
            };

            navigationManager.RestoreState(state);
            navigationManager.GoBack();
            MockPage page = navigationManager.NavigatedPages[0] as MockPage;
            MockViewModel<string, TestData> viewModel = page.DataContext as MockViewModel<string, TestData>;

            Assert.AreEqual(true, viewModel.IsActivated);
            Assert.AreEqual("Test Text", viewModel.ActivationState.Text);
            Assert.AreEqual(42, viewModel.ActivationState.Number);
        }

        [TestMethod]
        public void RestoreState_RestoresNullState_ForPreviousPage()
        {
            TestableNavigationBase navigationManager = CreateNavigationManager();

            NavigationState state = new NavigationState()
            {
                NavigationStack =
                        {
                            new NavigationEntryState("Page 2", null, null),
                            new NavigationEntryState("Page 1", null, null)
                        }
            };

            navigationManager.RestoreState(state);
            navigationManager.GoBack();
            MockPage page = navigationManager.NavigatedPages[0] as MockPage;
            MockViewModel<string, string> viewModel = page.DataContext as MockViewModel<string, string>;

            Assert.AreEqual(true, viewModel.IsActivated);
            Assert.AreEqual(null, viewModel.ActivationState);
        }

        [TestMethod]
        public void StoreState_StoresNavigationStack()
        {
            TestableNavigationBase navigationManager = CreateNavigationManager();

            navigationManager.NavigateTo("Page 1");
            navigationManager.NavigateTo("Page 2");

            NavigationState state = navigationManager.StoreState();

            Assert.AreEqual(2, state.NavigationStack.Count);
            Assert.AreEqual("Page 2", state.NavigationStack[0].PageName);
            Assert.AreEqual("Page 1", state.NavigationStack[1].PageName);
        }

        [TestMethod]
        public void StoreState_StoresStringArguments()
        {
            TestableNavigationBase navigationManager = CreateNavigationManager();

            navigationManager.NavigateTo("Page 2", "Test Argument");

            NavigationState state = navigationManager.StoreState();

            Assert.AreEqual(1, state.NavigationStack.Count);
            byte[] expectedBytes = SerializationHelper.SerializeToArray("Test Argument");
            CollectionAssert.AreEqual(expectedBytes, state.NavigationStack[0].ArgumentsData);
        }

        [TestMethod]
        public void StoreState_StoresComplexArguments()
        {
            IViewFactory viewFactory = new MockViewFactory_WithComplexArguments();
            TestableNavigationBase navigationManager = CreateNavigationManager(viewFactory);

            navigationManager.NavigateTo("Page 2", new TestData { Text = "Test Text", Number = 42 });

            NavigationState state = navigationManager.StoreState();

            Assert.AreEqual(1, state.NavigationStack.Count);
            byte[] expectedBytes = SerializationHelper.SerializeToArray(new TestData { Text = "Test Text", Number = 42 });
            CollectionAssert.AreEqual(expectedBytes, state.NavigationStack[0].ArgumentsData);
        }

        [TestMethod]
        public void StoreState_StoresNullArguments()
        {
            TestableNavigationBase navigationManager = CreateNavigationManager();

            navigationManager.NavigateTo("Page 2");

            NavigationState state = navigationManager.StoreState();

            Assert.AreEqual(1, state.NavigationStack.Count);
            CollectionAssert.AreEqual(null, state.NavigationStack[0].ArgumentsData);
        }

        [TestMethod]
        public void StoreState_StoresStringState_ForLastPage()
        {
            TestableNavigationBase navigationManager = CreateNavigationManager();

            navigationManager.NavigateTo("Page 2");
            MockPage page = navigationManager.NavigatedPages.Last() as MockPage;
            MockViewModel<string, string> viewModel = page.DataContext as MockViewModel<string, string>;
            viewModel.State = "Test State";

            NavigationState state = navigationManager.StoreState();

            Assert.AreEqual(1, state.NavigationStack.Count);
            byte[] expectedBytes = SerializationHelper.SerializeToArray("Test State");
            CollectionAssert.AreEqual(expectedBytes, state.NavigationStack[0].StateData);
        }

        [TestMethod]
        public void StoreState_StoresComplexState_ForLastPage()
        {
            IViewFactory viewFactory = new MockViewFactory_WithComplexState();
            TestableNavigationBase navigationManager = CreateNavigationManager(viewFactory);

            navigationManager.NavigateTo("Page 2");
            MockPage page = navigationManager.NavigatedPages.Last() as MockPage;
            MockViewModel<string, TestData> viewModel = page.DataContext as MockViewModel<string, TestData>;
            viewModel.State = new TestData { Text = "Test Text", Number = 42 };

            NavigationState state = navigationManager.StoreState();

            Assert.AreEqual(1, state.NavigationStack.Count);
            byte[] expectedBytes = SerializationHelper.SerializeToArray(new TestData { Text = "Test Text", Number = 42 });
            CollectionAssert.AreEqual(expectedBytes, state.NavigationStack[0].StateData);
        }

        [TestMethod]
        public void StoreState_StoresNullState_ForLastPage()
        {
            TestableNavigationBase navigationManager = CreateNavigationManager();

            navigationManager.NavigateTo("Page 2");

            NavigationState state = navigationManager.StoreState();

            Assert.AreEqual(1, state.NavigationStack.Count);
            CollectionAssert.AreEqual(null, state.NavigationStack[0].StateData);
        }

        [TestMethod]
        public void StoreState_StoresStringState_ForPreviousPage()
        {
            TestableNavigationBase navigationManager = CreateNavigationManager();

            navigationManager.NavigateTo("Page 2");
            MockPage page = navigationManager.NavigatedPages.Last() as MockPage;
            MockViewModel<string, string> viewModel = page.DataContext as MockViewModel<string, string>;
            viewModel.State = "Test State";

            navigationManager.NavigateTo("Page 1");

            NavigationState state = navigationManager.StoreState();

            Assert.AreEqual(2, state.NavigationStack.Count);
            byte[] expectedBytes = SerializationHelper.SerializeToArray("Test State");
            CollectionAssert.AreEqual(expectedBytes, state.NavigationStack[1].StateData);
        }

        [TestMethod]
        public void StoreState_StoresComplexState_ForPreviousPage()
        {
            IViewFactory viewFactory = new MockViewFactory_WithComplexState();
            TestableNavigationBase navigationManager = CreateNavigationManager(viewFactory);

            navigationManager.NavigateTo("Page 2");
            MockPage page = navigationManager.NavigatedPages.Last() as MockPage;
            MockViewModel<string, TestData> viewModel = page.DataContext as MockViewModel<string, TestData>;
            viewModel.State = new TestData { Text = "Test Text", Number = 42 };

            navigationManager.NavigateTo("Page 1");

            NavigationState state = navigationManager.StoreState();

            Assert.AreEqual(2, state.NavigationStack.Count);
            byte[] expectedBytes = SerializationHelper.SerializeToArray(new TestData { Text = "Test Text", Number = 42 });
            CollectionAssert.AreEqual(expectedBytes, state.NavigationStack[1].StateData);
        }

        [TestMethod]
        public void StoreState_StoresNullState_ForPreviousPage()
        {
            TestableNavigationBase navigationManager = CreateNavigationManager();

            navigationManager.NavigateTo("Page 2");
            navigationManager.NavigateTo("Page 1");

            NavigationState state = navigationManager.StoreState();

            Assert.AreEqual(2, state.NavigationStack.Count);
            CollectionAssert.AreEqual(null, state.NavigationStack[1].StateData);
        }

        // *** Behavior Tests ***

        [TestMethod]
        public void CanGoBackChanged_IsCalledWhenFirstPageNavigated()
        {
            INavigationBase navigationManager = CreateNavigationManager();

            int canGoBackChangedCount = 0;
            navigationManager.CanGoBackChanged += delegate(object sender, EventArgs e) { canGoBackChangedCount++; };

            navigationManager.NavigateTo("Page 1");

            Assert.AreEqual(1, canGoBackChangedCount);
        }

        [TestMethod]
        public void CanGoBackChanged_IsNotCalledWhenSecondPageNavigated()
        {
            INavigationBase navigationManager = CreateNavigationManager();

            navigationManager.NavigateTo("Page 1");

            int canGoBackChangedCount = 0;
            navigationManager.CanGoBackChanged += delegate(object sender, EventArgs e) { canGoBackChangedCount++; };

            navigationManager.NavigateTo("Page 2");

            Assert.AreEqual(0, canGoBackChangedCount);
        }

        [TestMethod]
        public void CanGoBackChanged_IsNotCalledWhenSecondPageNavigatedThenBack()
        {
            INavigationBase navigationManager = CreateNavigationManager();

            navigationManager.NavigateTo("Page 1");
            navigationManager.NavigateTo("Page 2");

            int canGoBackChangedCount = 0;
            navigationManager.CanGoBackChanged += delegate(object sender, EventArgs e) { canGoBackChangedCount++; };

            navigationManager.GoBack();

            Assert.AreEqual(0, canGoBackChangedCount);
        }

        [TestMethod]
        public void CanGoBackChanged_IsCalledWhenFirstPageNavigatedThenBack()
        {
            INavigationBase navigationManager = CreateNavigationManager();

            navigationManager.NavigateTo("Page 1");

            int canGoBackChangedCount = 0;
            navigationManager.CanGoBackChanged += delegate(object sender, EventArgs e) { canGoBackChangedCount++; };

            navigationManager.GoBack();

            Assert.AreEqual(1, canGoBackChangedCount);
        }

        // *** Private Methods ***

        private TestableNavigationBase CreateNavigationManager(IViewFactory viewFactory = null)
        {
            if (viewFactory == null)
                viewFactory = new MockViewFactory();

            TestableNavigationBase navigationManager = new TestableNavigationBase(viewFactory);

            return navigationManager;
        }

        // *** Private Sub-classes ***

        private class TestableNavigationBase : NavigationBase
        {
            // *** Fields ***

            public readonly List<object> NavigatedPages = new List<object>();

            // *** Constructors ***

            public TestableNavigationBase(IViewFactory viewFactory)
                : base(viewFactory)
            {
            }

            // *** Methods ***

            protected override void DisplayPage(object page)
            {
                NavigatedPages.Add(page);
            }

            public new void RestoreState(NavigationState state)
            {
                base.RestoreState(state);
            }

            public new NavigationState StoreState()
            {
                return base.StoreState();
            }
        }

        private class MockViewFactory : IViewFactory
        {
            // *** Methods ***

            public IViewLifetimeContext CreateView(string name, INavigationContext context)
            {
                switch (name)
                {
                    case "Home":
                        return new MockViewLifetimeContext<string, string>("Home", null, context);
                    case "Page 1":
                        return new MockViewLifetimeContext<string, string>("Page 1", "ViewModel 1", context);
                    case "Page 2":
                        return new MockViewLifetimeContext<string, string>("Page 2", "ViewModel 2", context);
                    case "Page 3":
                        return new MockViewLifetimeContext<string, string>("Page 3", null, context);
                    default:
                        throw new InvalidOperationException();
                }
            }

            public bool IsViewDefined(string name)
            {
                return new string[] { "Home", "Page 1", "Page 2", "Page 3" }.Contains(name);
            }
        }

        private class MockViewFactory_WithComplexArguments : IViewFactory
        {
            // *** Methods ***

            public IViewLifetimeContext CreateView(string name, INavigationContext context)
            {
                switch (name)
                {
                    case "Page 1":
                        return new MockViewLifetimeContext<TestData, string>("Page 1", "ViewModel 1");
                    case "Page 2":
                        return new MockViewLifetimeContext<TestData, string>("Page 2", "ViewModel 2");
                    case "Page 3":
                        return new MockViewLifetimeContext<TestData, string>("Page 3", null);
                    default:
                        throw new InvalidOperationException();
                }
            }

            public bool IsViewDefined(string name)
            {
                throw new NotImplementedException();
            }
        }

        private class MockViewFactory_WithComplexState : IViewFactory
        {
            // *** Methods ***

            public IViewLifetimeContext CreateView(string name, INavigationContext context)
            {
                switch (name)
                {
                    case "Page 1":
                        return new MockViewLifetimeContext<string, TestData>("Page 1", "ViewModel 1");
                    case "Page 2":
                        return new MockViewLifetimeContext<string, TestData>("Page 2", "ViewModel 2");
                    case "Page 3":
                        return new MockViewLifetimeContext<string, TestData>("Page 3", null);
                    default:
                        throw new InvalidOperationException();
                }
            }

            public bool IsViewDefined(string name)
            {
                throw new NotImplementedException();
            }
        }

        private class MockViewFactory_WithNonNullableState : IViewFactory
        {
            // *** Methods ***

            public IViewLifetimeContext CreateView(string name, INavigationContext context)
            {
                switch (name)
                {
                    case "Page 1":
                        return new MockViewLifetimeContext<string, int>("Page 1", "ViewModel 1");
                    case "Page 2":
                        return new MockViewLifetimeContext<string, int>("Page 2", "ViewModel 2");
                    case "Page 3":
                        return new MockViewLifetimeContext<string, int>("Page 3", null);
                    default:
                        throw new InvalidOperationException();
                }
            }

            public bool IsViewDefined(string name)
            {
                throw new NotImplementedException();
            }
        }

        private class MockViewFactory_WithNavigationAware : IViewFactory
        {
            // *** Methods ***

            public IViewLifetimeContext CreateView(string name, INavigationContext context)
            {
                switch (name)
                {
                    case "Page 1":
                        return new MockViewLifetimeContext<string, int>("Page 1", "ViewModel 1", pageType: typeof(MockNavigationAwarePage), viewModelType: typeof(MockNavigationAwareViewModel<string, int>));
                    case "Page 2":
                        return new MockViewLifetimeContext<string, int>("Page 2", "ViewModel 2", pageType: typeof(MockNavigationAwarePage), viewModelType: typeof(MockNavigationAwareViewModel<string, int>));
                    case "Page 3":
                        return new MockViewLifetimeContext<string, int>("Page 3", null, pageType: typeof(MockNavigationAwarePage), viewModelType: typeof(MockNavigationAwareViewModel<string, int>));
                    default:
                        throw new InvalidOperationException();
                }
            }

            public bool IsViewDefined(string name)
            {
                throw new NotImplementedException();
            }
        }

        private class MockViewLifetimeContext<TArguments, TState> : IViewLifetimeContext
        {
            // *** Constructors ***

            public MockViewLifetimeContext(string pageName, string viewModelName, INavigationContext navigationContext = null, Type pageType = null, Type viewModelType = null)
            {
                if (pageName != null)
                {
                    MockPage page = (MockPage)Activator.CreateInstance(pageType ?? typeof(MockPage));
                    page.PageName = pageName;
                    page.NavigationContext = navigationContext;

                    View = page;
                }

                if (viewModelName != null)
                {
                    MockViewModel<TArguments, TState> viewModel = (MockViewModel<TArguments, TState>)Activator.CreateInstance(viewModelType ?? typeof(MockViewModel<TArguments, TState>));
                    viewModel.Name = viewModelName;

                    ViewModel = viewModel;
                }

                if (pageName != null && viewModelName != null)
                    ((MockPage)View).DataContext = ViewModel;
            }

            // *** Properties ***

            public object View { get; set; }
            public object ViewModel { get; set; }

            // *** Methods ***

            public void Dispose()
            {
                if (View != null)
                    ((MockPage)View).IsDisposed = true;

                if (ViewModel != null)
                    ((MockViewModel<TArguments, TState>)ViewModel).IsDisposed = true;
            }
        }

        private class MockPage
        {
            // *** Properties ***

            public bool IsDisposed { get; set; }
            public string PageName { get; set; }
            public object DataContext { get; set; }
            public INavigationContext NavigationContext { get; set; }
        }

        private class MockViewModel
        {
        }

        private class MockViewModel<TArguments, TState> : MockViewModel, IActivatable<TArguments, TState>
        {
            // *** Properties ***

            public TArguments ActivationArguments { get; private set; }
            public TState ActivationState { get; private set; }
            public bool IsActivated { get; private set; }
            public bool IsDisposed { get; set; }
            public string Name { get; set; }
            public TState State { get; set; }

            // *** Methods ***

            public void Activate(TArguments arguments, TState state)
            {
                IsActivated = true;
                ActivationArguments = arguments;
                ActivationState = state;
            }

            public TState SaveState()
            {
                return State;
            }
        }

        private class MockNavigationAwarePage : MockPage, INavigationAware
        {
            // *** Fields ***

            public List<string> NavigationEvents = new List<string>();

            // *** Methods ***

            public void NavigatedTo(NavigationMode navigationMode)
            {
                NavigationEvents.Add(string.Format("NavigatedTo({0})", navigationMode));
            }

            public void NavigatingFrom(NavigationMode navigationMode)
            {
                NavigationEvents.Add(string.Format("NavigatingFrom({0})", navigationMode));
            }
        }

        private class MockNavigationAwareViewModel<TArguments, TState> : MockViewModel<TArguments, TState>, INavigationAware
        {
            // *** Fields ***

            public List<string> NavigationEvents = new List<string>();

            // *** Methods ***

            public void NavigatedTo(NavigationMode navigationMode)
            {
                NavigationEvents.Add(string.Format("NavigatedTo({0})", navigationMode));
            }

            public void NavigatingFrom(NavigationMode navigationMode)
            {
                NavigationEvents.Add(string.Format("NavigatingFrom({0})", navigationMode));
            }
        }

        [DataContract]
        private class TestData
        {
            [DataMember]
            public string Text { get; set; }
            [DataMember]
            public int Number { get; set; }
        }
    }
}

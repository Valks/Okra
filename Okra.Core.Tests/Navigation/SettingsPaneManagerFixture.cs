using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Okra.Navigation;
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
    public class SettingsPaneManagerFixture
    {
        // *** Property Tests ***

        [TestMethod]
        public void CanGoBack_IsFalseIfNoPagesNavigated()
        {
            ISettingsPaneManager settingsPaneManager = CreateSettingsPaneManager();

            Assert.AreEqual(false, settingsPaneManager.CanGoBack);
        }

        [TestMethod]
        public void CanGoBack_IsTrueIfOnePageNavigated()
        {
            ISettingsPaneManager settingsPaneManager = CreateSettingsPaneManager();

            settingsPaneManager.NavigateTo("Page 1");

            Assert.AreEqual(true, settingsPaneManager.CanGoBack);
        }

        [TestMethod]
        public void CanGoBack_IsFalseIfOnePagesNavigatedThenBack()
        {
            ISettingsPaneManager settingsPaneManager = CreateSettingsPaneManager();

            settingsPaneManager.NavigateTo("Page 1");
            settingsPaneManager.GoBack();

            Assert.AreEqual(false, settingsPaneManager.CanGoBack);
        }

        // *** CurrentPage Tests ***

        [TestMethod]
        public void CurrentPage_IsInitiallyNull()
        {
            ISettingsPaneManager settingsPaneManager = CreateSettingsPaneManager();

            Assert.AreEqual(null, settingsPaneManager.CurrentPage);
        }

        [TestMethod]
        public void CurrentPage_IsNullIfOnePageNavigatedThenBack()
        {
            ISettingsPaneManager settingsPaneManager = CreateSettingsPaneManager();

            settingsPaneManager.NavigateTo("Page 1");
            settingsPaneManager.GoBack();

            Assert.AreEqual(null, settingsPaneManager.CurrentPage);
        }

        [TestMethod]
        public void CurrentPage_Name_IsSetIfOnePageNavigated()
        {
            ISettingsPaneManager settingsPaneManager = CreateSettingsPaneManager();

            settingsPaneManager.NavigateTo("Page 1");

            Assert.AreEqual("Page 1", settingsPaneManager.CurrentPage.PageName);
        }

        [TestMethod]
        public void CurrentPage_Name_IsSetIfTwoPagesNavigatedThenBack()
        {
            ISettingsPaneManager settingsPaneManager = CreateSettingsPaneManager();

            settingsPaneManager.NavigateTo("Page 1");
            settingsPaneManager.NavigateTo("Page 2");
            settingsPaneManager.GoBack();

            Assert.AreEqual("Page 1", settingsPaneManager.CurrentPage.PageName);
        }

        [TestMethod]
        public void CurrentPage_Name_IsSetIfTwoPagesNavigated()
        {
            ISettingsPaneManager settingsPaneManager = CreateSettingsPaneManager();

            settingsPaneManager.NavigateTo("Page 1");
            settingsPaneManager.NavigateTo("Page 2");

            Assert.AreEqual("Page 2", settingsPaneManager.CurrentPage.PageName);
        }

        [TestMethod]
        public void CurrentPage_GetElements_ReturnsViewModelThenPageIfBothPresent()
        {
            TestableSettingsPaneManager settingsPaneManager = CreateSettingsPaneManager();

            settingsPaneManager.NavigateTo("Page 1");
            MockPage currentPage = settingsPaneManager.NavigatedPages.Cast<MockPage>().LastOrDefault();
            MockViewModel currentViewModel = (MockViewModel)currentPage.DataContext;

            CollectionAssert.AreEqual(new object[] { currentViewModel, currentPage }, settingsPaneManager.CurrentPage.GetElements().ToList());
        }

        [TestMethod]
        public void CurrentPage_GetElements_ReturnsPageIfNoViewModelPresent()
        {
            TestableSettingsPaneManager settingsPaneManager = CreateSettingsPaneManager();

            settingsPaneManager.NavigateTo("Page 3");
            MockPage currentPage = settingsPaneManager.NavigatedPages.Cast<MockPage>().LastOrDefault();

            CollectionAssert.AreEqual(new object[] { currentPage }, settingsPaneManager.CurrentPage.GetElements().ToList());
        }

        // *** Method Tests ***

        [TestMethod]
        public void CanNavigateTo_ReturnsTrue_PageExistsWithSpecifiedName()
        {
            ISettingsPaneManager navigationManager = CreateSettingsPaneManager();

            bool canNavigateTo = navigationManager.CanNavigateTo("Page 1");

            Assert.AreEqual(true, canNavigateTo);
        }

        [TestMethod]
        public void CanNavigateTo_ReturnsFalse_NoPageWithSpecifiedName()
        {
            ISettingsPaneManager navigationManager = CreateSettingsPaneManager();

            bool canNavigateTo = navigationManager.CanNavigateTo("Page X");

            Assert.AreEqual(false, canNavigateTo);
        }

        [TestMethod]
        public void GoBack_NavigatesToNextPageInStack()
        {
            TestableSettingsPaneManager settingsPaneManager = CreateSettingsPaneManager();

            settingsPaneManager.NavigateTo("Page 1");
            settingsPaneManager.NavigateTo("Page 2");
            settingsPaneManager.GoBack();

            string[] pageNames = settingsPaneManager.NavigatedPages.Cast<MockPage>().Select(page => page.PageName).ToArray();
            CollectionAssert.AreEqual(new string[] { "Page 1", "Page 2", "Page 1" }, pageNames);
        }

        [TestMethod]
        public void GoBack_ShowsSettingsPaneIfStackIsEmpty()
        {
            TestableSettingsPaneManager settingsPaneManager = CreateSettingsPaneManager();

            settingsPaneManager.NavigateTo("Page 1");
            settingsPaneManager.GoBack();

            string[] pageNames = settingsPaneManager.NavigatedPages.Cast<MockPage>().Select(page => page.PageName).ToArray();
            CollectionAssert.AreEqual(new string[] { "Page 1", "Settings" }, pageNames);
        }

        [TestMethod]
        public void GoBack_DisposesCurrentPage_WithViewModel()
        {
            TestableSettingsPaneManager settingsPaneManager = CreateSettingsPaneManager();

            settingsPaneManager.NavigateTo("Page 1");
            settingsPaneManager.NavigateTo("Page 2");
            MockPage currentPage = (MockPage)settingsPaneManager.NavigatedPages.Last();
            settingsPaneManager.GoBack();

            Assert.AreEqual(true, currentPage.IsDisposed);
        }

        [TestMethod]
        public void GoBack_DisposesCurrentViewModel()
        {
            TestableSettingsPaneManager settingsPaneManager = CreateSettingsPaneManager();

            settingsPaneManager.NavigateTo("Page 1");
            settingsPaneManager.NavigateTo("Page 2");
            MockPage currentPage = (MockPage)settingsPaneManager.NavigatedPages.Last();
            MockViewModel<string, string> currentViewModel = (MockViewModel<string, string>)currentPage.DataContext;
            settingsPaneManager.GoBack();

            Assert.AreEqual(true, currentViewModel.IsDisposed);
        }

        [TestMethod]
        public void GoBack_RaisesNavigatingFromEvent()
        {
            TestableSettingsPaneManager settingsPaneManager = CreateSettingsPaneManager();

            settingsPaneManager.NavigateTo("Page 1");
            settingsPaneManager.NavigateTo("Page 2");

            List<PageNavigationEventArgs> navigationEventArgs = new List<PageNavigationEventArgs>();
            settingsPaneManager.NavigatingFrom += delegate(object sender, PageNavigationEventArgs e) { navigationEventArgs.Add(e); };

            settingsPaneManager.GoBack();

            Assert.AreEqual(1, navigationEventArgs.Count);
            Assert.AreEqual("Page 2", navigationEventArgs[0].Page.PageName);
            Assert.AreEqual(NavigationMode.Back, navigationEventArgs[0].NavigationMode);
        }

        [TestMethod]
        public void GoBack_RaisesNavigatedToEvent()
        {
            TestableSettingsPaneManager settingsPaneManager = CreateSettingsPaneManager();

            settingsPaneManager.NavigateTo("Page 1");
            settingsPaneManager.NavigateTo("Page 2");

            List<PageNavigationEventArgs> navigationEventArgs = new List<PageNavigationEventArgs>();
            settingsPaneManager.NavigatedTo += delegate(object sender, PageNavigationEventArgs e) { navigationEventArgs.Add(e); };

            settingsPaneManager.GoBack();

            Assert.AreEqual(1, navigationEventArgs.Count);
            Assert.AreEqual("Page 1", navigationEventArgs[0].Page.PageName);
            Assert.AreEqual(NavigationMode.Back, navigationEventArgs[0].NavigationMode);
        }

        [TestMethod]
        public void GoBack_CallsNavigatingFromOnPreviousViewModel()
        {
            MockViewFactory_WithNavigationAware viewFactory = new MockViewFactory_WithNavigationAware();
            TestableSettingsPaneManager settingsPaneManager = CreateSettingsPaneManager(viewFactory);

            settingsPaneManager.NavigateTo("Page 1");

            settingsPaneManager.NavigateTo("Page 2");
            MockPage currentPage = (MockPage)settingsPaneManager.NavigatedPages.Last();
            MockNavigationAwareViewModel<string, int> currentViewModel = (MockNavigationAwareViewModel<string, int>)currentPage.DataContext;
            currentViewModel.NavigationEvents.Clear();

            settingsPaneManager.GoBack();

            CollectionAssert.AreEqual(new string[] { "NavigatingFrom(Back)" }, currentViewModel.NavigationEvents);
        }

        [TestMethod]
        public void GoBack_CallsNavigatingFromOnPreviousPage()
        {
            MockViewFactory_WithNavigationAware viewFactory = new MockViewFactory_WithNavigationAware();
            TestableSettingsPaneManager settingsPaneManager = CreateSettingsPaneManager(viewFactory);

            settingsPaneManager.NavigateTo("Page 1");

            settingsPaneManager.NavigateTo("Page 2");
            MockNavigationAwarePage currentPage = (MockNavigationAwarePage)settingsPaneManager.NavigatedPages.Last();
            currentPage.NavigationEvents.Clear();

            settingsPaneManager.GoBack();

            CollectionAssert.AreEqual(new string[] { "NavigatingFrom(Back)" }, currentPage.NavigationEvents);
        }

        [TestMethod]
        public void GoBack_CallsNavigatingToOnNewViewModel()
        {
            MockViewFactory_WithNavigationAware viewFactory = new MockViewFactory_WithNavigationAware();
            TestableSettingsPaneManager settingsPaneManager = CreateSettingsPaneManager(viewFactory);

            settingsPaneManager.NavigateTo("Page 1");
            MockPage currentPage = (MockPage)settingsPaneManager.NavigatedPages.Last();
            MockNavigationAwareViewModel<string, int> currentViewModel = (MockNavigationAwareViewModel<string, int>)currentPage.DataContext;

            settingsPaneManager.NavigateTo("Page 2");
            currentViewModel.NavigationEvents.Clear();

            settingsPaneManager.GoBack();

            CollectionAssert.AreEqual(new string[] { "NavigatedTo(Back)" }, currentViewModel.NavigationEvents);
        }

        [TestMethod]
        public void GoBack_CallsNavigatingToOnNewPage()
        {
            MockViewFactory_WithNavigationAware viewFactory = new MockViewFactory_WithNavigationAware();
            TestableSettingsPaneManager settingsPaneManager = CreateSettingsPaneManager(viewFactory);

            settingsPaneManager.NavigateTo("Page 1");
            MockNavigationAwarePage currentPage = (MockNavigationAwarePage)settingsPaneManager.NavigatedPages.Last();

            settingsPaneManager.NavigateTo("Page 2");
            currentPage.NavigationEvents.Clear();

            settingsPaneManager.GoBack();

            CollectionAssert.AreEqual(new string[] { "NavigatedTo(Back)" }, currentPage.NavigationEvents);
        }

        [TestMethod]
        public void GoBack_ThrowsException_NoPageInBackStack()
        {
            ISettingsPaneManager settingsPaneManager = CreateSettingsPaneManager();

            Assert.ThrowsException<InvalidOperationException>(() => settingsPaneManager.GoBack());
        }

        [TestMethod]
        public void NavigateTo_NavigatesToSpecifiedPage_WithViewModel()
        {
            TestableSettingsPaneManager settingsPaneManager = CreateSettingsPaneManager();

            settingsPaneManager.NavigateTo("Page 2");

            string[] pageNames = settingsPaneManager.NavigatedPages.Cast<MockPage>().Select(page => page.PageName).ToArray();
            CollectionAssert.AreEqual(new string[] { "Page 2" }, pageNames);
        }

        [TestMethod]
        public void NavigateTo_NavigatesToSpecifiedPage_WithoutViewModel()
        {
            TestableSettingsPaneManager settingsPaneManager = CreateSettingsPaneManager();

            settingsPaneManager.NavigateTo("Page 3");

            string[] pageNames = settingsPaneManager.NavigatedPages.Cast<MockPage>().Select(page => page.PageName).ToArray();
            CollectionAssert.AreEqual(new string[] { "Page 3" }, pageNames);
        }

        [TestMethod]
        public void NavigateTo_NavigatedTwice_NavigatesToSpecifiedPage()
        {
            TestableSettingsPaneManager settingsPaneManager = CreateSettingsPaneManager();

            settingsPaneManager.NavigateTo("Page 1");
            settingsPaneManager.NavigateTo("Page 2");

            string[] pageNames = settingsPaneManager.NavigatedPages.Cast<MockPage>().Select(page => page.PageName).ToArray();
            CollectionAssert.AreEqual(new string[] { "Page 1", "Page 2" }, pageNames);
        }

        [TestMethod]
        public void NavigateTo_SetsSpecifiedViewModel_WithViewModel()
        {
            TestableSettingsPaneManager settingsPaneManager = CreateSettingsPaneManager();

            settingsPaneManager.NavigateTo("Page 2");

            MockPage navigatedPage = (MockPage)settingsPaneManager.NavigatedPages.Last();
            object dataContext = navigatedPage.DataContext;

            Assert.IsNotNull(dataContext);
            Assert.IsInstanceOfType(dataContext, typeof(MockViewModel<string, string>));
            Assert.AreEqual("ViewModel 2", ((MockViewModel<string, string>)dataContext).Name);
        }

        [TestMethod]
        public void NavigateTo_SetsNullViewModel_WithoutViewModel()
        {
            TestableSettingsPaneManager settingsPaneManager = CreateSettingsPaneManager();

            settingsPaneManager.NavigateTo("Page 3");

            MockPage navigatedPage = (MockPage)settingsPaneManager.NavigatedPages.Last();
            object dataContext = navigatedPage.DataContext;

            Assert.IsNull(dataContext);
        }

        [TestMethod]
        public void NavigateTo_PassesNullArgumentToViewModel()
        {
            TestableSettingsPaneManager settingsPaneManager = CreateSettingsPaneManager();

            settingsPaneManager.NavigateTo("Page 2");

            MockPage navigatedPage = (MockPage)settingsPaneManager.NavigatedPages.Last();
            object dataContext = navigatedPage.DataContext;

            Assert.IsNotNull(dataContext);
            Assert.IsInstanceOfType(dataContext, typeof(MockViewModel<string, string>));
            Assert.AreEqual(true, ((MockViewModel<string, string>)dataContext).IsActivated);
            Assert.AreEqual(null, ((MockViewModel<string, string>)dataContext).ActivationArguments);
        }

        [TestMethod]
        public void NavigateTo_WithArguments_PassesArgumentToViewModel()
        {
            TestableSettingsPaneManager settingsPaneManager = CreateSettingsPaneManager();

            settingsPaneManager.NavigateTo("Page 2", "Test Argument");

            MockPage navigatedPage = (MockPage)settingsPaneManager.NavigatedPages.Last();
            object dataContext = navigatedPage.DataContext;

            Assert.IsNotNull(dataContext);
            Assert.IsInstanceOfType(dataContext, typeof(MockViewModel<string, string>));
            Assert.AreEqual(true, ((MockViewModel<string, string>)dataContext).IsActivated);
            Assert.AreEqual("Test Argument", ((MockViewModel<string, string>)dataContext).ActivationArguments);
        }

        [TestMethod]
        public void NavigateTo_PassesNullStateToViewModel_WithStringState()
        {
            TestableSettingsPaneManager settingsPaneManager = CreateSettingsPaneManager();

            settingsPaneManager.NavigateTo("Page 2", "Test Argument");

            MockPage navigatedPage = (MockPage)settingsPaneManager.NavigatedPages.Last();
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
            TestableSettingsPaneManager settingsPaneManager = CreateSettingsPaneManager(viewFactory);

            settingsPaneManager.NavigateTo("Page 2", "Test Argument");

            MockPage navigatedPage = (MockPage)settingsPaneManager.NavigatedPages.Last();
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
            TestableSettingsPaneManager settingsPaneManager = CreateSettingsPaneManager(viewFactory);

            settingsPaneManager.NavigateTo("Page 2", "Test Argument");

            MockPage navigatedPage = (MockPage)settingsPaneManager.NavigatedPages.Last();
            object dataContext = navigatedPage.DataContext;

            Assert.IsNotNull(dataContext);
            Assert.IsInstanceOfType(dataContext, typeof(MockViewModel<string, int>));
            Assert.AreEqual(true, ((MockViewModel<string, int>)dataContext).IsActivated);
            Assert.AreEqual(0, ((MockViewModel<string, int>)dataContext).ActivationState);
        }

        [TestMethod]
        public void NavigateTo_RaisesNavigatingFromEvent()
        {
            TestableSettingsPaneManager settingsPaneManager = CreateSettingsPaneManager();

            settingsPaneManager.NavigateTo("Page 1");

            List<PageNavigationEventArgs> navigationEventArgs = new List<PageNavigationEventArgs>();
            settingsPaneManager.NavigatingFrom += delegate(object sender, PageNavigationEventArgs e) { navigationEventArgs.Add(e); };

            settingsPaneManager.NavigateTo("Page 2");

            Assert.AreEqual(1, navigationEventArgs.Count);
            Assert.AreEqual("Page 1", navigationEventArgs[0].Page.PageName);
            Assert.AreEqual(NavigationMode.New, navigationEventArgs[0].NavigationMode);
        }

        [TestMethod]
        public void NavigateTo_RaisesNavigatedToEvent()
        {
            TestableSettingsPaneManager settingsPaneManager = CreateSettingsPaneManager();

            settingsPaneManager.NavigateTo("Page 1");

            List<PageNavigationEventArgs> navigationEventArgs = new List<PageNavigationEventArgs>();
            settingsPaneManager.NavigatedTo += delegate(object sender, PageNavigationEventArgs e) { navigationEventArgs.Add(e); };

            settingsPaneManager.NavigateTo("Page 2");

            Assert.AreEqual(1, navigationEventArgs.Count);
            Assert.AreEqual("Page 2", navigationEventArgs[0].Page.PageName);
            Assert.AreEqual(NavigationMode.New, navigationEventArgs[0].NavigationMode);
        }

        [TestMethod]
        public void NavigateTo_CallsNavigatingFromOnPreviousViewModel()
        {
            MockViewFactory_WithNavigationAware viewFactory = new MockViewFactory_WithNavigationAware();
            TestableSettingsPaneManager settingsPaneManager = CreateSettingsPaneManager(viewFactory);

            settingsPaneManager.NavigateTo("Page 1");
            MockPage currentPage = (MockPage)settingsPaneManager.NavigatedPages.Last();
            MockNavigationAwareViewModel<string, int> currentViewModel = (MockNavigationAwareViewModel<string, int>)currentPage.DataContext;
            currentViewModel.NavigationEvents.Clear();

            settingsPaneManager.NavigateTo("Page 2");

            CollectionAssert.AreEqual(new string[] { "NavigatingFrom(New)" }, currentViewModel.NavigationEvents);
        }

        [TestMethod]
        public void NavigateTo_CallsNavigatingFromOnPreviousPage()
        {
            MockViewFactory_WithNavigationAware viewFactory = new MockViewFactory_WithNavigationAware();
            TestableSettingsPaneManager settingsPaneManager = CreateSettingsPaneManager(viewFactory);

            settingsPaneManager.NavigateTo("Page 1");
            MockNavigationAwarePage currentPage = (MockNavigationAwarePage)settingsPaneManager.NavigatedPages.Last();
            currentPage.NavigationEvents.Clear();

            settingsPaneManager.NavigateTo("Page 2");

            CollectionAssert.AreEqual(new string[] { "NavigatingFrom(New)" }, currentPage.NavigationEvents);
        }

        [TestMethod]
        public void NavigateTo_CallsNavigatingToOnNewViewModel()
        {
            MockViewFactory_WithNavigationAware viewFactory = new MockViewFactory_WithNavigationAware();
            TestableSettingsPaneManager settingsPaneManager = CreateSettingsPaneManager(viewFactory);

            settingsPaneManager.NavigateTo("Page 1");

            settingsPaneManager.NavigateTo("Page 2");
            MockPage currentPage = (MockPage)settingsPaneManager.NavigatedPages.Last();
            MockNavigationAwareViewModel<string, int> currentViewModel = (MockNavigationAwareViewModel<string, int>)currentPage.DataContext;

            CollectionAssert.AreEqual(new string[] { "NavigatedTo(New)" }, currentViewModel.NavigationEvents);
        }

        [TestMethod]
        public void NavigateTo_CallsNavigatingToOnNewPage()
        {
            MockViewFactory_WithNavigationAware viewFactory = new MockViewFactory_WithNavigationAware();
            TestableSettingsPaneManager settingsPaneManager = CreateSettingsPaneManager(viewFactory);

            settingsPaneManager.NavigateTo("Page 1");

            settingsPaneManager.NavigateTo("Page 2");
            MockNavigationAwarePage currentPage = (MockNavigationAwarePage)settingsPaneManager.NavigatedPages.Last();

            CollectionAssert.AreEqual(new string[] { "NavigatedTo(New)" }, currentPage.NavigationEvents);
        }

        [TestMethod]
        public void NavigateTo_ThrowsException_NoPageWithSpecifiedName()
        {
            ISettingsPaneManager settingsPaneManager = CreateSettingsPaneManager();

            Assert.ThrowsException<InvalidOperationException>(() => settingsPaneManager.NavigateTo("Page X"));
        }

        // *** Behaviour Tests ***

        [TestMethod]
        public void FlyoutClosed_CanGoBackIsFalse()
        {
            TestableSettingsPaneManager settingsPaneManager = CreateSettingsPaneManager();

            settingsPaneManager.NavigateTo("Page 1");
            settingsPaneManager.NavigateTo("Page 2");
            settingsPaneManager.CallOnSettingsFlyoutClosed();

            Assert.AreEqual(false, settingsPaneManager.CanGoBack);
        }

        [TestMethod]
        public void FlyoutClosed_CurrentPageIsNull()
        {
            TestableSettingsPaneManager settingsPaneManager = CreateSettingsPaneManager();

            settingsPaneManager.NavigateTo("Page 1");
            settingsPaneManager.NavigateTo("Page 2");
            settingsPaneManager.CallOnSettingsFlyoutClosed();

            Assert.AreEqual(null, settingsPaneManager.CurrentPage);
        }

        [TestMethod]
        public void FlyoutClosed_NavigationStackIsEmpty()
        {
            TestableSettingsPaneManager settingsPaneManager = CreateSettingsPaneManager();

            settingsPaneManager.NavigateTo("Page 1");
            settingsPaneManager.NavigateTo("Page 2");
            settingsPaneManager.CallOnSettingsFlyoutClosed();

            Assert.AreEqual(0, settingsPaneManager.NavigationStack.Count);
        }

        [TestMethod]
        public void FlyoutClosed_FiresFlyoutClosedEvent()
        {
            TestableSettingsPaneManager settingsPaneManager = CreateSettingsPaneManager();

            int flyoutClosedCount = 0;
            int flyoutOpenedCount = 0;
            settingsPaneManager.FlyoutClosed += (sender, e) => { flyoutClosedCount++; };
            settingsPaneManager.FlyoutOpened += (sender, e) => { flyoutOpenedCount++; };

            settingsPaneManager.CallOnSettingsFlyoutClosed();

            Assert.AreEqual(1, flyoutClosedCount);
            Assert.AreEqual(0, flyoutOpenedCount);
        }

        [TestMethod]
        public void FlyoutOpened_FiresFlyoutOpenedEvent()
        {
            TestableSettingsPaneManager settingsPaneManager = CreateSettingsPaneManager();

            int flyoutClosedCount = 0;
            int flyoutOpenedCount = 0;
            settingsPaneManager.FlyoutClosed += (sender, e) => { flyoutClosedCount++; };
            settingsPaneManager.FlyoutOpened += (sender, e) => { flyoutOpenedCount++; };

            settingsPaneManager.CallOnSettingsFlyoutOpened();

            Assert.AreEqual(0, flyoutClosedCount);
            Assert.AreEqual(1, flyoutOpenedCount);
        }

        [TestMethod]
        public void CanGoBackChanged_IsCalledWhenFirstPageNavigated()
        {
            ISettingsPaneManager settingsPaneManager = CreateSettingsPaneManager();

            int canGoBackChangedCount = 0;
            settingsPaneManager.CanGoBackChanged += delegate(object sender, EventArgs e) { canGoBackChangedCount++; };

            settingsPaneManager.NavigateTo("Page 1");

            Assert.AreEqual(1, canGoBackChangedCount);
        }

        [TestMethod]
        public void CanGoBackChanged_IsNotCalledWhenSecondPageNavigated()
        {
            ISettingsPaneManager settingsPaneManager = CreateSettingsPaneManager();

            settingsPaneManager.NavigateTo("Page 1");

            int canGoBackChangedCount = 0;
            settingsPaneManager.CanGoBackChanged += delegate(object sender, EventArgs e) { canGoBackChangedCount++; };

            settingsPaneManager.NavigateTo("Page 2");

            Assert.AreEqual(0, canGoBackChangedCount);
        }

        [TestMethod]
        public void CanGoBackChanged_IsNotCalledWhenSecondPageNavigatedThenBack()
        {
            ISettingsPaneManager settingsPaneManager = CreateSettingsPaneManager();

            settingsPaneManager.NavigateTo("Page 1");
            settingsPaneManager.NavigateTo("Page 2");

            int canGoBackChangedCount = 0;
            settingsPaneManager.CanGoBackChanged += delegate(object sender, EventArgs e) { canGoBackChangedCount++; };

            settingsPaneManager.GoBack();

            Assert.AreEqual(0, canGoBackChangedCount);
        }

        [TestMethod]
        public void CanGoBackChanged_IsCalledWhenFirstPageNavigatedThenBack()
        {
            ISettingsPaneManager settingsPaneManager = CreateSettingsPaneManager();

            settingsPaneManager.NavigateTo("Page 1");

            int canGoBackChangedCount = 0;
            settingsPaneManager.CanGoBackChanged += delegate(object sender, EventArgs e) { canGoBackChangedCount++; };

            settingsPaneManager.GoBack();

            Assert.AreEqual(1, canGoBackChangedCount);
        }

        // *** Private Methods ***

        private TestableSettingsPaneManager CreateSettingsPaneManager(IViewFactory viewFactory = null)
        {
            if (viewFactory == null)
                viewFactory = new MockViewFactory();

            TestableSettingsPaneManager settingsPaneManager = new TestableSettingsPaneManager(viewFactory);

            return settingsPaneManager;
        }

        // *** Private Sub-classes ***

        private class TestableSettingsPaneManager : SettingsPaneManager
        {
            // *** Fields ***

            public IList<object> NavigatedPages = new List<object>();

            // *** Constructors ***

            public TestableSettingsPaneManager(IViewFactory viewFactory) : base(viewFactory)
            {
            }

            // *** Properties ***

            public new Stack<INavigationEntry> NavigationStack
            {
                get
                {
                    return base.NavigationStack;
                }
            }

            // *** Methods ***

            public void CallOnSettingsFlyoutClosed()
            {
                base.OnSettingsFlyoutClosed(null, null);
            }

            public void CallOnSettingsFlyoutOpened()
            {
                base.OnSettingsFlyoutOpened(null, null);
            }

            // *** Overriden Base Methods ***

            protected override void DisplayPage(object page)
            {
                if (page == null)
                    NavigatedPages.Add(new MockPage() { PageName = "Settings" });
                else
                    NavigatedPages.Add(page);
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
                        return new MockViewLifetimeContext<string, string>("Home", null);
                    case "Page 1":
                        return new MockViewLifetimeContext<string, string>("Page 1", "ViewModel 1");
                    case "Page 2":
                        return new MockViewLifetimeContext<string, string>("Page 2", "ViewModel 2");
                    case "Page 3":
                        return new MockViewLifetimeContext<string, string>("Page 3", null);
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

            public MockViewLifetimeContext(string pageName, string viewModelName, Type pageType = null, Type viewModelType = null)
            {
                if (pageName != null)
                {
                    MockPage page = (MockPage)Activator.CreateInstance(pageType ?? typeof(MockPage));
                    page.PageName = pageName;

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

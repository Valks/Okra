﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Okra.Navigation;
using Okra.Search;
using Okra.Tests.Mocks;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Search;
using Okra.Services;

namespace Okra.Tests.Search
{
    [TestClass]
    public class SearchManagerFixture
    {
        // *** Constructor Tests ***

        [TestMethod]
        public void Constructor_RegistersWithActivationManager()
        {
            MockActivationManager activationManager = new MockActivationManager();
            SearchManager searchManager = CreateSearchMananger(activationManager: activationManager);

            CollectionAssert.Contains(activationManager.RegisteredServices, searchManager);
        }

        // *** Property Tests ***

        [TestMethod]
        public void SearchPageName_IsInitiallySpecialPageName()
        {
            SearchManager searchManager = CreateSearchMananger(setSearchPageName: false);

            Assert.AreEqual(SpecialPageNames.Search, searchManager.SearchPageName);
        }

        [TestMethod]
        public void SearchPageName_CanSetValue()
        {
            SearchManager searchManager = CreateSearchMananger();

            searchManager.SearchPageName = "MySearchPage";

            Assert.AreEqual("MySearchPage", searchManager.SearchPageName);
        }

        [TestMethod]
        public void SearchPageName_Exception_CannotSetToNull()
        {
            SearchManager searchManager = CreateSearchMananger();

            Assert.ThrowsException<ArgumentException>(() => searchManager.SearchPageName = null);
        }

        [TestMethod]
        public void SearchPageName_Exception_CannotSetToEmptyString()
        {
            SearchManager searchManager = CreateSearchMananger();

            Assert.ThrowsException<ArgumentException>(() => searchManager.SearchPageName = "");
        }

        [TestMethod]
        public void SearchPageName_Exception_CannotSetValueOnceActivated()
        {
            MockActivationManager activationManager = new MockActivationManager();
            SearchManager searchManager = CreateSearchMananger(activationManager: activationManager);

            activationManager.RaiseActivatedEvent(new MockActivatedEventArgs());

            Assert.ThrowsException<InvalidOperationException>(() => searchManager.SearchPageName = "MySearchPage");
        }

        // *** Method Tests ***

        [TestMethod]
        public async Task Activate_ReturnsTrueIfActivationKindIsSearch()
        {
            SearchManager searchManager = CreateSearchMananger();

            // Activate the application

            bool result = await searchManager.Activate(new MockSearchActivatedEventArgs() { QueryText = "MyQuery", Language = "en-GB" });

            // Check the result

            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public async Task Activate_ReturnsFalseIfActivationKindIsNotLaunch()
        {
            SearchManager searchManager = CreateSearchMananger();

            // Activate the application

            bool result = await searchManager.Activate(new MockActivatedEventArgs() { Kind = ActivationKind.Launch });

            // Check the result

            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public async Task Activate_NavigatesToSearchPageIfPreviousExecutionRunning()
        {
            MockNavigationManager navigationManager = new MockNavigationManager() { CanRestoreNavigationStack = true };
            SearchManager searchManager = CreateSearchMananger(navigationManager: navigationManager);

            await searchManager.Activate(new MockSearchActivatedEventArgs() { QueryText = "MyQuery", Language = "en-GB", PreviousExecutionState = ApplicationExecutionState.Running });

            CollectionAssert.AreEqual(new[] { Tuple.Create("Search", (object)null) }, (ICollection)navigationManager.NavigatedPages);
        }

        [TestMethod]
        public async Task Activate_NavigatesToSearchPageIfPreviousExecutionSuspended()
        {
            MockNavigationManager navigationManager = new MockNavigationManager() { CanRestoreNavigationStack = true };
            SearchManager searchManager = CreateSearchMananger(navigationManager: navigationManager);

            await searchManager.Activate(new MockSearchActivatedEventArgs() { QueryText = "MyQuery", Language = "en-GB", PreviousExecutionState = ApplicationExecutionState.Suspended });

            CollectionAssert.AreEqual(new[] { Tuple.Create("Search", (object)null) }, (ICollection)navigationManager.NavigatedPages);
        }

        [TestMethod]
        public async Task Activate_NavigatesToSearchPageWithRestoredNavigationIfPreviousExecutionTerminated()
        {
            MockNavigationManager navigationManager = new MockNavigationManager() { CanRestoreNavigationStack = true };
            SearchManager searchManager = CreateSearchMananger(navigationManager: navigationManager);

            await searchManager.Activate(new MockSearchActivatedEventArgs() { QueryText = "MyQuery", Language = "en-GB", PreviousExecutionState = ApplicationExecutionState.Terminated });

            CollectionAssert.AreEqual(new[] { Tuple.Create("[Restored Pages]", (object)null), Tuple.Create("Search", (object)null) }, (ICollection)navigationManager.NavigatedPages);
        }

        [TestMethod]
        public async Task Activate_NavigatesToSearchPageAfterHomePageIfPreviousExecutionClosedByUser()
        {
            MockNavigationManager navigationManager = new MockNavigationManager() { CanRestoreNavigationStack = true };
            SearchManager searchManager = CreateSearchMananger(navigationManager: navigationManager);

            await searchManager.Activate(new MockSearchActivatedEventArgs() { QueryText = "MyQuery", Language = "en-GB", PreviousExecutionState = ApplicationExecutionState.ClosedByUser });

            CollectionAssert.AreEqual(new[] { Tuple.Create("Home", (object)null), Tuple.Create("Search", (object)null) }, (ICollection)navigationManager.NavigatedPages);
        }

        [TestMethod]
        public async Task Activate_NavigatesToSearchPageAfterHomePageIfPreviousExecutionNotRunning()
        {
            MockNavigationManager navigationManager = new MockNavigationManager() { CanRestoreNavigationStack = true };
            SearchManager searchManager = CreateSearchMananger(navigationManager: navigationManager);

            await searchManager.Activate(new MockSearchActivatedEventArgs() { QueryText = "MyQuery", Language = "en-GB", PreviousExecutionState = ApplicationExecutionState.NotRunning });

            CollectionAssert.AreEqual(new[] { Tuple.Create("Home", (object)null), Tuple.Create("Search", (object)null) }, (ICollection)navigationManager.NavigatedPages);
        }

        [TestMethod]
        public async Task Activate_EmptySearch_NoNavigationIfPreviousExecutionRunning()
        {
            MockNavigationManager navigationManager = new MockNavigationManager() { CanRestoreNavigationStack = true };
            SearchManager searchManager = CreateSearchMananger(navigationManager: navigationManager);

            await searchManager.Activate(new MockSearchActivatedEventArgs() { QueryText = "", Language = "en-GB", PreviousExecutionState = ApplicationExecutionState.Running });

            CollectionAssert.AreEqual(new Tuple<string, object>[] { }, (ICollection)navigationManager.NavigatedPages);
        }

        [TestMethod]
        public async Task Activate_EmptySearch_NoNavigationIfPreviousExecutionSuspended()
        {
            MockNavigationManager navigationManager = new MockNavigationManager() { CanRestoreNavigationStack = true };
            SearchManager searchManager = CreateSearchMananger(navigationManager: navigationManager);

            await searchManager.Activate(new MockSearchActivatedEventArgs() { QueryText = "", Language = "en-GB", PreviousExecutionState = ApplicationExecutionState.Suspended });

            CollectionAssert.AreEqual(new Tuple<string, object>[] { }, (ICollection)navigationManager.NavigatedPages);
        }

        [TestMethod]
        public async Task Activate_EmptySearch_RestoresNavigationIfPreviousExecutionTerminated()
        {
            MockNavigationManager navigationManager = new MockNavigationManager() { CanRestoreNavigationStack = true };
            SearchManager searchManager = CreateSearchMananger(navigationManager: navigationManager);

            await searchManager.Activate(new MockSearchActivatedEventArgs() { QueryText = "", Language = "en-GB", PreviousExecutionState = ApplicationExecutionState.Terminated });

            CollectionAssert.AreEqual(new[] { Tuple.Create("[Restored Pages]", (object)null) }, (ICollection)navigationManager.NavigatedPages);
        }

        [TestMethod]
        public async Task Activate_EmptySearch_NavigatesToHomePageIfPreviousExecutionClosedByUser()
        {
            MockNavigationManager navigationManager = new MockNavigationManager() { CanRestoreNavigationStack = true };
            SearchManager searchManager = CreateSearchMananger(navigationManager: navigationManager);

            await searchManager.Activate(new MockSearchActivatedEventArgs() { QueryText = "", Language = "en-GB", PreviousExecutionState = ApplicationExecutionState.ClosedByUser });

            CollectionAssert.AreEqual(new[] { Tuple.Create("Home", (object)null) }, (ICollection)navigationManager.NavigatedPages);
        }

        [TestMethod]
        public async Task Activate_EmptySearch_NavigatesToHomePageIfPreviousExecutionNotRunning()
        {
            MockNavigationManager navigationManager = new MockNavigationManager() { CanRestoreNavigationStack = true };
            SearchManager searchManager = CreateSearchMananger(navigationManager: navigationManager);

            await searchManager.Activate(new MockSearchActivatedEventArgs() { QueryText = "", Language = "en-GB", PreviousExecutionState = ApplicationExecutionState.NotRunning });

            CollectionAssert.AreEqual(new[] { Tuple.Create("Home", (object)null) }, (ICollection)navigationManager.NavigatedPages);
        }

        [TestMethod]
        public async Task Activate_DoesNotNavigateIfAlreadyShowingSearchPage()
        {
            MockNavigationManager navigationManager = new MockNavigationManager();
            SearchManager searchManager = CreateSearchMananger(navigationManager: navigationManager);

            navigationManager.NavigateTo("Search");
            navigationManager.NavigatedPages.Clear();

            await searchManager.Activate(new MockSearchActivatedEventArgs() { QueryText = "MyQuery", Language = "en-GB", PreviousExecutionState = ApplicationExecutionState.Running });

            CollectionAssert.AreEqual(new Tuple<string, object>[] { }, (ICollection)navigationManager.NavigatedPages);
        }

        [TestMethod]
        public async Task Activate_CallsPerformQueryOnAllElements()
        {
            MockNavigationEntry searchPageEntry = new MockNavigationEntry();
            MockNavigationManager navigationManager = new MockNavigationManager(_ => searchPageEntry);
            SearchManager searchManager = CreateSearchMananger(navigationManager: navigationManager);

            await searchManager.Activate(new MockSearchActivatedEventArgs() { QueryText = "MyQuery", Language = "en-GB" });

            CollectionAssert.AreEqual(new[] { Tuple.Create("MyQuery", "en-GB") }, (ICollection)((MockSearchPageElement)searchPageEntry.Page).Queries);
            CollectionAssert.AreEqual(new[] { Tuple.Create("MyQuery", "en-GB") }, (ICollection)((MockSearchPageElement)searchPageEntry.ViewModel).Queries);
        }

        [TestMethod]
        public async Task Activate_CallsPerformQueryOnlyOnSearchPageImplementors()
        {
            MockNavigationEntry searchPageEntry = new MockNavigationEntry() { Page = new MockPageElement() };
            MockNavigationManager navigationManager = new MockNavigationManager(_ => searchPageEntry);
            SearchManager searchManager = CreateSearchMananger(navigationManager: navigationManager);

            await searchManager.Activate(new MockSearchActivatedEventArgs() { QueryText = "MyQuery", Language = "en-GB" });

            CollectionAssert.AreEqual(new[] { Tuple.Create("MyQuery", "en-GB") }, (ICollection)((MockSearchPageElement)searchPageEntry.ViewModel).Queries);
        }

        [TestMethod]
        public async Task Activate_SuccessfulEvenWhenNoSearchPageImplementors()
        {
            MockNavigationEntry searchPageEntry = new MockNavigationEntry() { Page = new MockPageElement(), ViewModel = new MockPageElement() };
            MockNavigationManager navigationManager = new MockNavigationManager(_ => searchPageEntry);
            SearchManager searchManager = CreateSearchMananger(navigationManager: navigationManager);

            await searchManager.Activate(new MockSearchActivatedEventArgs() { QueryText = "MyQuery", Language = "en-GB" });
        }

        [TestMethod]
        public async Task Activate_DoesNotCallPerformQueryIfSameQueryAsVisible()
        {
            MockNavigationEntry searchPageEntry = new MockNavigationEntry();
            MockNavigationManager navigationManager = new MockNavigationManager(_ => searchPageEntry);
            SearchManager searchManager = CreateSearchMananger(navigationManager: navigationManager);

            await searchManager.Activate(new MockSearchActivatedEventArgs() { QueryText = "MyQuery", Language = "en-GB" });

            CollectionAssert.AreEqual(new[] { Tuple.Create("MyQuery", "en-GB") }, (ICollection)((MockSearchPageElement)searchPageEntry.Page).Queries);
            CollectionAssert.AreEqual(new[] { Tuple.Create("MyQuery", "en-GB") }, (ICollection)((MockSearchPageElement)searchPageEntry.ViewModel).Queries);
        }

        [TestMethod]
        public async Task Activate_DoesNotNavigateIfActivationKindIsNotSearch()
        {
            MockNavigationManager navigationManager = new MockNavigationManager() { CanRestoreNavigationStack = true };
            SearchManager searchManager = CreateSearchMananger(navigationManager: navigationManager);

            // Activate the application

            await searchManager.Activate(new MockActivatedEventArgs() { Kind = ActivationKind.Launch });

            // Assert that no pages were navigated

            CollectionAssert.AreEqual(new string[] { }, navigationManager.NavigatedPages.Select(t => t.Item1).ToArray());
        }

        // *** Behaviour Tests ***

        [TestMethod]
        public void BeforeFirstActivation_DoesNotCallRegisterQuerySubmitted()
        {
            MockActivationManager activationManager = new MockActivationManager();
            TestableSearchManager searchManager = CreateSearchMananger(activationManager: activationManager);
        
            Assert.AreEqual(0, searchManager.RegisterQuerySubmittedCount);
        }

        [TestMethod]
        public void OnFirstActivation_CallsRegisterQuerySubmitted()
        {
            MockActivationManager activationManager = new MockActivationManager();
            TestableSearchManager searchManager = CreateSearchMananger(activationManager: activationManager);

            activationManager.RaiseActivatedEvent(new MockActivatedEventArgs());

            Assert.AreEqual(1, searchManager.RegisterQuerySubmittedCount);
        }

        [TestMethod]
        public void OnMultipleActivations_CallsRegisterQuerySubmittedOnlyOnce()
        {
            MockActivationManager activationManager = new MockActivationManager();
            TestableSearchManager searchManager = CreateSearchMananger(activationManager: activationManager);

            activationManager.RaiseActivatedEvent(new MockActivatedEventArgs());
            activationManager.RaiseActivatedEvent(new MockActivatedEventArgs());

            Assert.AreEqual(1, searchManager.RegisterQuerySubmittedCount);
        }

        // *** Private Methods ***

        private TestableSearchManager CreateSearchMananger(MockNavigationManager navigationManager = null, MockActivationManager activationManager = null, bool setSearchPageName = true)
        {
            if (navigationManager == null)
                navigationManager = new MockNavigationManager();

            if (activationManager == null)
                activationManager = new MockActivationManager();

            TestableSearchManager searchManager = new TestableSearchManager(navigationManager, activationManager);

            if (setSearchPageName)
                searchManager.SearchPageName = "Search";

            return searchManager;
        }

        // *** Private sub-classes ***

        private class TestableSearchManager : SearchManager
        {
            // *** Fields ***

            public int RegisterQuerySubmittedCount;

            // *** Constructors ***

            public TestableSearchManager(INavigationManager navigationManager, IActivationManager activationManager)
                : base(navigationManager, activationManager)
            {
            }

            // *** Overriden base methods ***

            protected override void RegisterQuerySubmitted()
            {
                RegisterQuerySubmittedCount++;
            }
        }

        private class MockNavigationEntry : INavigationEntry
        {
            // *** Fields ***

            public MockPageElement Page = new MockSearchPageElement();
            public MockPageElement ViewModel = new MockSearchPageElement();

            public string PageName
            {
                get { throw new NotImplementedException(); }
            }

            public IEnumerable<object> GetElements()
            {
                return new object[] { ViewModel, Page };
            }
        }

        private class MockPageElement
        {
        }

        private class MockSearchPageElement : MockPageElement, ISearchPage
        {
            // *** Fields ***

            public IList<Tuple<string, string>> Queries = new List<Tuple<string, string>>();

            // *** Methods ***

            public void PerformQuery(string queryText, string language)
            {
                Queries.Add(Tuple.Create(queryText, language));
            }
        }

        private class MockSearchActivatedEventArgs : MockActivatedEventArgs, ISearchActivatedEventArgs
        {
            // *** Constructors ***

            public MockSearchActivatedEventArgs()
            {
                base.Kind = ActivationKind.Search;
                base.PreviousExecutionState = ApplicationExecutionState.Terminated;
            }

            // *** Propertes ***

            public string Language
            {
                get;
                set;
            }

            public string QueryText
            {
                get;
                set;
            }
        }
    }
}

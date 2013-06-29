using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Okra.Data;
using Okra.Tests.Helpers;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace Concoon.Tests.Data
{
    [TestClass]
    public class SimpleDataListSourceFixture
    {
        // *** Method Tests ***

        [TestMethod]
        public void GetCount_ReturnsNumberOfItems()
        {
            MockDataListSource dataListSource = new MockDataListSource();

            int count = dataListSource.GetCountAsync().Result;

            Assert.AreEqual(6, count);
        }

        [TestMethod]
        public void GetCount_ReturnsSameValueWithParallelRequests()
        {
            MockDataListSource dataListSource = new MockDataListSource();

            Task<int> countTask1 = dataListSource.GetCountAsync();
            Task<int> countTask2 = dataListSource.GetCountAsync();
            Task<int> countTask3 = dataListSource.GetCountAsync();

            Task.WaitAll(countTask1, countTask2, countTask3);

            Assert.AreEqual(6, countTask1.Result);
            Assert.AreEqual(6, countTask2.Result);
            Assert.AreEqual(6, countTask3.Result);
        }

        [TestMethod]
        public async Task GetCount_ReturnsSameValueWithSubsequentRequests()
        {
            MockDataListSource dataListSource = new MockDataListSource();

            Task<int> countTask1 = dataListSource.GetCountAsync();
            await countTask1;
            Task<int> countTask2 = dataListSource.GetCountAsync();
            await countTask2;
            Task<int> countTask3 = dataListSource.GetCountAsync();
            await countTask3;

            Assert.AreEqual(6, countTask1.Result);
            Assert.AreEqual(6, countTask2.Result);
            Assert.AreEqual(6, countTask3.Result);
        }

        [TestMethod]
        public void GetCount_OnlyFetchesItemsOnceWithParallelRequests()
        {
            MockDataListSource dataListSource = new MockDataListSource();

            Task<int> countTask1 = dataListSource.GetCountAsync();
            Task<int> countTask2 = dataListSource.GetCountAsync();
            Task<int> countTask3 = dataListSource.GetCountAsync();

            Task.WaitAll(countTask1, countTask2, countTask3);

            Assert.AreEqual(1, dataListSource.FetchItemsCallCount);
        }

        [TestMethod]
        public async Task GetCount_OnlyFetchesItemsOnceWithSubsequentRequests()
        {
            MockDataListSource dataListSource = new MockDataListSource();

            Task<int> countTask1 = dataListSource.GetCountAsync();
            await countTask1;
            Task<int> countTask2 = dataListSource.GetCountAsync();
            await countTask2;
            Task<int> countTask3 = dataListSource.GetCountAsync();
            await countTask3;

            Task.WaitAll(countTask1, countTask2, countTask3);

            Assert.AreEqual(1, dataListSource.FetchItemsCallCount);
        }

        [TestMethod]
        public void GetItem_ReturnsCorrectItem()
        {
            MockDataListSource dataListSource = new MockDataListSource();

            int item = dataListSource.GetItemAsync(3).Result;

            Assert.AreEqual(8, item);
        }

        [TestMethod]
        public void GetItem_ReturnsSameValueWithParallelRequests()
        {
            MockDataListSource dataListSource = new MockDataListSource();

            Task<int> itemTask1 = dataListSource.GetItemAsync(3);
            Task<int> itemTask2 = dataListSource.GetItemAsync(3);
            Task<int> itemTask3 = dataListSource.GetItemAsync(3);

            Task.WaitAll(itemTask1, itemTask2, itemTask3);

            Assert.AreEqual(8, itemTask1.Result);
            Assert.AreEqual(8, itemTask2.Result);
            Assert.AreEqual(8, itemTask3.Result);
        }

        [TestMethod]
        public async Task GetItem_ReturnsSameValueWithSubsequentRequests()
        {
            MockDataListSource dataListSource = new MockDataListSource();

            Task<int> itemTask1 = dataListSource.GetItemAsync(3);
            await itemTask1;
            Task<int> itemTask2 = dataListSource.GetItemAsync(3);
            await itemTask2;
            Task<int> itemTask3 = dataListSource.GetItemAsync(3);
            await itemTask3;

            Assert.AreEqual(8, itemTask1.Result);
            Assert.AreEqual(8, itemTask2.Result);
            Assert.AreEqual(8, itemTask3.Result);
        }

        [TestMethod]
        public void GetItem_OnlyFetchesItemsOnceWithParallelRequests()
        {
            MockDataListSource dataListSource = new MockDataListSource();

            Task<int> itemTask1 = dataListSource.GetItemAsync(3);
            Task<int> itemTask2 = dataListSource.GetItemAsync(3);
            Task<int> itemTask3 = dataListSource.GetItemAsync(3);

            Task.WaitAll(itemTask1, itemTask2, itemTask3);

            Assert.AreEqual(1, dataListSource.FetchItemsCallCount);
        }

        [TestMethod]
        public async Task GetItem_OnlyFetchesItemsOnceWithSubsequentRequests()
        {
            MockDataListSource dataListSource = new MockDataListSource();

            Task<int> itemTask1 = dataListSource.GetItemAsync(3);
            await itemTask1;
            Task<int> itemTask2 = dataListSource.GetItemAsync(3);
            await itemTask2;
            Task<int> itemTask3 = dataListSource.GetItemAsync(3);
            await itemTask3;

            Task.WaitAll(itemTask1, itemTask2, itemTask3);

            Assert.AreEqual(1, dataListSource.FetchItemsCallCount);
        }

        [TestMethod]
        public async Task GetItem_Exception_IndexIsGreaterThanListLength()
        {
            MockDataListSource dataListSource = new MockDataListSource();

            await AssertEx.ThrowsExceptionAsync<ArgumentOutOfRangeException>(() => dataListSource.GetItemAsync(6));
        }

        [TestMethod]
        public async Task GetItem_Exception_IndexIsNegative()
        {
            MockDataListSource dataListSource = new MockDataListSource();

            await AssertEx.ThrowsExceptionAsync<ArgumentOutOfRangeException>(() => dataListSource.GetItemAsync(-1));
        }

        [TestMethod]
        public void IndexOf_ReturnsMinusOneIfListIsNotFetched()
        {
            MockDataListSource dataListSource = new MockDataListSource();

            Assert.AreEqual(-1, dataListSource.IndexOf(6));
        }

        [TestMethod]
        public async Task IndexOf_ReturnsItemIndexIfItemIsInList()
        {
            MockDataListSource dataListSource = new MockDataListSource();
            await dataListSource.GetCountAsync();

            Assert.AreEqual(2, dataListSource.IndexOf(6));
        }

        [TestMethod]
        public async Task IndexOf_ReturnsMinusOneIfItemIsNotInList()
        {
            MockDataListSource dataListSource = new MockDataListSource();
            await dataListSource.GetCountAsync();

            Assert.AreEqual(-1, dataListSource.IndexOf(5));
        }

        // *** Update Method Tests ***

        [TestMethod]
        public void Refresh_UpdatesCount()
        {
            MockDataListSource dataListSource = new MockDataListSource();

            int oldCount = dataListSource.GetCountAsync().Result;

            dataListSource.Refresh(new int[] { 3, 6, 9 });
            int count = dataListSource.GetCountAsync().Result;

            Assert.AreEqual(3, count);
        }

        [TestMethod]
        public void Refresh_UpdatesItems()
        {
            MockDataListSource dataListSource = new MockDataListSource();

            int oldItem = dataListSource.GetItemAsync(1).Result;

            dataListSource.Refresh(new int[] { 3, 6, 9 });
            int item = dataListSource.GetItemAsync(1).Result;

            Assert.AreEqual(6, item);
        }

        [TestMethod]
        public void Refresh_UpdatesSubscribers()
        {
            MockUpdatableCollection updatableCollection = new MockUpdatableCollection();
            MockDataListSource dataListSource = new MockDataListSource();
            dataListSource.Subscribe(updatableCollection);

            int oldCount = dataListSource.GetCountAsync().Result;

            dataListSource.Refresh(new int[] { 3, 6, 9 });

            CollectionAssert.AreEqual(new[] { new DataListUpdate(DataListUpdateAction.Reset) }, (ICollection)updatableCollection.Updates);
        }

        [TestMethod]
        public void Refresh_UnsubscriptionDoesNotUpdateSubscribers()
        {
            MockUpdatableCollection updatableCollection = new MockUpdatableCollection();
            MockDataListSource dataListSource = new MockDataListSource();
            IDisposable unsubscribeDisposable = dataListSource.Subscribe(updatableCollection);

            int oldCount = dataListSource.GetCountAsync().Result;

            unsubscribeDisposable.Dispose();
            dataListSource.Refresh(new int[] { 3, 6, 9 });

            Assert.AreEqual(0, updatableCollection.Updates.Count);
        }

        // *** Private Sub-Classes ***

        private class MockDataListSource : SimpleDataListSource<int>
        {
            // *** Fields ***

            public int FetchItemsCallCount = 0;
            private int[] items = new int[] { 2, 4, 6, 8, 10, 12 };

            // *** Methods ***

            public void Refresh(int[] newItems)
            {
                items = newItems;
                base.Refresh();
            }

            // *** Overriden Base Methods ***

            protected async override Task<IList<int>> FetchItemsAsync()
            {
                FetchItemsCallCount++;
                await Task.Yield();

                return items;
            }
        }

        private class MockUpdatableCollection : IUpdatableCollection
        {
            // *** Fields ***

            public IList<DataListUpdate> Updates = new List<DataListUpdate>();

            // *** Methods ***

            public void Update(DataListUpdate update)
            {
                Updates.Add(update);
            }
        }
    }
}

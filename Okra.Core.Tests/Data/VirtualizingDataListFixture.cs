using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Okra.Data;
using Okra.Tests.Helpers;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace Concoon.Tests.Data
{
    [TestClass]
    public class VirtualizingDataListFixture
    {
        // *** Constructor Tests ***

        [TestMethod]
        public void Constructor_Exception_DataListSourceIsNull()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new VirtualizingDataList<int>(null));
        }

        // *** Property Tests ***

        [TestMethod]
        public void Indexer_IsPlaceholderWhilstAwaitingValue()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new VirtualizingDataList<int>(dataListSource);

            int count = dataList.Count;
            dataListSource.TriggerCount();

            Assert.AreEqual(0, dataList[4]);
        }

        [TestMethod]
        public void Indexer_IsValueOnceProvided()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new VirtualizingDataList<int>(dataListSource);

            int count = dataList.Count;
            dataListSource.TriggerCount();

            object item = dataList[4];
            dataListSource.TriggerItems();


            Assert.AreEqual(10, dataList[4]);
        }

        [TestMethod]
        public void Count_IsZeroWhilstLoading()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new VirtualizingDataList<int>(dataListSource);

            Assert.AreEqual(0, dataList.Count);
        }

        [TestMethod]
        public void Count_IsListLengthAfterLoading()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new VirtualizingDataList<int>(dataListSource);

            int count = dataList.Count;
            dataListSource.TriggerCount();

            Assert.AreEqual(10, dataList.Count);
        }

        // *** Method Tests ***

        [TestMethod]
        public void IndexOf_ReturnsItemIndexIfReportedInList()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new VirtualizingDataList<int>(dataListSource);

            Assert.AreEqual(3, ((IList<int>)dataList).IndexOf(8));
        }

        [TestMethod]
        public void IndexOf_ReturnsMinusOneIfNotReportedInList()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new VirtualizingDataList<int>(dataListSource);

            Assert.AreEqual(-1, ((IList<int>)dataList).IndexOf(9));
        }

        // *** Update Method Tests ***

        [TestMethod]
        public void Update_Reset_RaisesCountPropertyChanged()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new VirtualizingDataList<int>(dataListSource);
            int firstCountResult = dataList.Count;
            dataListSource.TriggerCount();

            int propertyChangedCount = 0;
            dataList.PropertyChanged += (sender, e) => { if (e.PropertyName == "Count") { propertyChangedCount++; } };

            ((IUpdatableCollection)dataList).Update(new DataListUpdate(DataListUpdateAction.Reset));

            Assert.AreEqual(1, propertyChangedCount);
        }

        [TestMethod]
        public void Update_Reset_RaisesItemsPropertyChanged()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new VirtualizingDataList<int>(dataListSource);
            int firstCountResult = dataList.Count;
            dataListSource.TriggerCount();

            int propertyChangedCount = 0;
            dataList.PropertyChanged += (sender, e) => { if (e.PropertyName == "Item[]") { propertyChangedCount++; } };

            ((IUpdatableCollection)dataList).Update(new DataListUpdate(DataListUpdateAction.Reset));

            Assert.AreEqual(1, propertyChangedCount);
        }

        [TestMethod]
        public void Update_Reset_RaisesCollectionChanged()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new VirtualizingDataList<int>(dataListSource);
            int firstCountResult = dataList.Count;
            dataListSource.TriggerCount();

            int collectionChangedCount = 0;
            dataList.CollectionChanged += (sender, e) => { if (e.Action == NotifyCollectionChangedAction.Reset) { collectionChangedCount++; } };

            ((IUpdatableCollection)dataList).Update(new DataListUpdate(DataListUpdateAction.Reset));

            Assert.AreEqual(1, collectionChangedCount);
        }

        [TestMethod]
        public void Update_Add_RaisesCountPropertyChanged()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new VirtualizingDataList<int>(dataListSource);
            int firstCountResult = dataList.Count;
            dataListSource.TriggerCount();

            int propertyChangedCount = 0;
            dataList.PropertyChanged += (sender, e) => { if (e.PropertyName == "Count") { propertyChangedCount++; } };

            ((IUpdatableCollection)dataList).Update(new DataListUpdate(DataListUpdateAction.Add, 2, 5));

            Assert.AreEqual(1, propertyChangedCount);
        }

        [TestMethod]
        public void Update_Add_RaisesItemsPropertyChanged()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new VirtualizingDataList<int>(dataListSource);
            int firstCountResult = dataList.Count;
            dataListSource.TriggerCount();

            int propertyChangedCount = 0;
            dataList.PropertyChanged += (sender, e) => { if (e.PropertyName == "Item[]") { propertyChangedCount++; } };

            ((IUpdatableCollection)dataList).Update(new DataListUpdate(DataListUpdateAction.Add, 2, 5));

            Assert.AreEqual(1, propertyChangedCount);
        }

        [TestMethod]
        public void Update_Add_RaisesCollectionChanged()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new VirtualizingDataList<int>(dataListSource);
            int firstCountResult = dataList.Count;
            dataListSource.TriggerCount();

            int collectionChangedCount = 0;
            dataList.CollectionChanged += (sender, e) => { if (e.Action == NotifyCollectionChangedAction.Add) { collectionChangedCount++; } };

            ((IUpdatableCollection)dataList).Update(new DataListUpdate(DataListUpdateAction.Add, 2, 5));

            Assert.AreEqual(5, collectionChangedCount);
        }

        [TestMethod]
        public void Update_Remove_RaisesCountPropertyChanged()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new VirtualizingDataList<int>(dataListSource);
            int firstCountResult = dataList.Count;
            dataListSource.TriggerCount();

            int propertyChangedCount = 0;
            dataList.PropertyChanged += (sender, e) => { if (e.PropertyName == "Count") { propertyChangedCount++; } };

            ((IUpdatableCollection)dataList).Update(new DataListUpdate(DataListUpdateAction.Remove, 2, 5));

            Assert.AreEqual(1, propertyChangedCount);
        }

        [TestMethod]
        public void Update_Remove_RaisesItemsPropertyChanged()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new VirtualizingDataList<int>(dataListSource);
            int firstCountResult = dataList.Count;
            dataListSource.TriggerCount();

            int propertyChangedCount = 0;
            dataList.PropertyChanged += (sender, e) => { if (e.PropertyName == "Item[]") { propertyChangedCount++; } };

            ((IUpdatableCollection)dataList).Update(new DataListUpdate(DataListUpdateAction.Remove, 2, 5));

            Assert.AreEqual(1, propertyChangedCount);
        }

        [TestMethod]
        public void Update_Remove_RaisesCollectionChanged()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new VirtualizingDataList<int>(dataListSource);
            int firstCountResult = dataList.Count;
            dataListSource.TriggerCount();

            int collectionChangedCount = 0;
            dataList.CollectionChanged += (sender, e) => { if (e.Action == NotifyCollectionChangedAction.Remove) { collectionChangedCount++; } };

            ((IUpdatableCollection)dataList).Update(new DataListUpdate(DataListUpdateAction.Remove, 2, 5));

            Assert.AreEqual(5, collectionChangedCount);
        }

        // *** Private Sub-Classes ***

        private class MockDataListSource : IDataListSource<int>
        {
            // *** Fields ***

            private TaskCompletionSource<bool> countCompletionSource = new TaskCompletionSource<bool>();
            private TaskCompletionSource<bool> itemsCompletionSource = new TaskCompletionSource<bool>();

            // *** Constructors ***

            public MockDataListSource()
            {
            }

            // *** Methods ***

            public void TriggerCount()
            {
                countCompletionSource.SetResult(true);
            }

            public void TriggerItems()
            {
                itemsCompletionSource.SetResult(true);
            }

            // *** IDataListSource<int> Methods ***

            public async Task<int> GetCountAsync()
            {
                await countCompletionSource.Task;
                return 10;
            }

            public async Task<int> GetItemAsync(int index)
            {
                await itemsCompletionSource.Task;
                return (index + 1) * 2;
            }

            public int IndexOf(int item)
            {
                if (item >= 2 && item <= 20 && item % 2 == 0)
                    return item / 2 - 1;
                else
                    return -1;
            }

            public IDisposable Subscribe(IUpdatableCollection collection)
            {
                return new MockDisposable();
            }
        }

        private class MockDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}

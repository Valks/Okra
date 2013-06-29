using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Okra.Data;
using Okra.Tests.Helpers;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Windows.UI.Xaml.Data;

namespace Okra.Tests.Data
{
    [TestClass]
    public class IncrementalLoadingDataListFixture
    {
        // *** Constructor Tests ***

        [TestMethod]
        public void Constructor_Exception_DataListSourceIsNull()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new IncrementalLoadingDataList<int>(null));
        }

        // *** Property Tests ***

        [TestMethod]
        public void Indexer_ReturnsItem()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new IncrementalLoadingDataList<int>(dataListSource);

            var loadMoreItemsTask = dataList.LoadMoreItemsAsync(10).AsTask();

            dataListSource.TriggerCount();
            dataListSource.TriggerItems(false);

            Assert.AreEqual(12, dataList[5]);
        }

        [TestMethod]
        public void Indexer_Exception_IndexLessThanZero()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new IncrementalLoadingDataList<int>(dataListSource);

            var loadMoreItemsTask = dataList.LoadMoreItemsAsync(10).AsTask();

            dataListSource.TriggerCount();

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => dataList[-1]);
        }

        [TestMethod]
        public void Indexer_Exception_IndexOutsideOfSourceCollection()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new IncrementalLoadingDataList<int>(dataListSource);

            var loadMoreItemsTask = dataList.LoadMoreItemsAsync(10).AsTask();

            dataListSource.TriggerCount();

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => dataList[20]);
        }

        [TestMethod]
        public void Indexer_Exception_IndexOutsideOfVisibleCollection()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new IncrementalLoadingDataList<int>(dataListSource);

            var loadMoreItemsTask = dataList.LoadMoreItemsAsync(10).AsTask();

            dataListSource.TriggerCount();

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => dataList[12]);
        }

        [TestMethod]
        public void Count_IsInitiallyZero()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new IncrementalLoadingDataList<int>(dataListSource);

            Assert.AreEqual(0, dataList.Count);
        }

        [TestMethod]
        public void Count_IsSetToDisplayedItems_WithFirstPortionOfDataList()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new IncrementalLoadingDataList<int>(dataListSource);

            dataList.LoadMoreItemsAsync(10).AsTask();

            dataListSource.TriggerCount();
            dataListSource.TriggerItems();

            Assert.AreEqual(10, dataList.Count);
        }

        [TestMethod]
        public void Count_IsSetToDisplayedItems_WithLastPortionOfDataList()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new IncrementalLoadingDataList<int>(dataListSource);

            dataList.LoadMoreItemsAsync(10).AsTask();

            dataListSource.TriggerCount();
            dataListSource.TriggerItems();

            dataList.LoadMoreItemsAsync(10).AsTask();

            dataListSource.TriggerItems();

            Assert.AreEqual(15, dataList.Count);
        }

        [TestMethod]
        public void Count_IsSetToDisplayedItems_WithCompletelyFetchedDataList()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new IncrementalLoadingDataList<int>(dataListSource);

            dataList.LoadMoreItemsAsync(10).AsTask();

            dataListSource.TriggerCount();
            dataListSource.TriggerItems();

            dataList.LoadMoreItemsAsync(10).AsTask();

            dataListSource.TriggerItems();

            dataList.LoadMoreItemsAsync(10).AsTask();

            dataListSource.TriggerItems();

            Assert.AreEqual(15, dataList.Count);
        }

        [TestMethod]
        public void HasMoreItems_IsInitiallyTrue()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new IncrementalLoadingDataList<int>(dataListSource);

            Assert.AreEqual(true, dataList.HasMoreItems);
        }

        [TestMethod]
        public void HasMoreItems_IsFalseWithEmptyDataListOnceCountIsRetrieved()
        {
            var dataListSource = new EmptyMockDataListSource();
            var dataList = new IncrementalLoadingDataList<int>(dataListSource);

            dataList.LoadMoreItemsAsync(10).AsTask();

            dataListSource.TriggerCount();

            Assert.AreEqual(false, dataList.HasMoreItems);
        }

        [TestMethod]
        public void HasMoreItems_IsTrueOncePartialItemsAreRetrieved()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new IncrementalLoadingDataList<int>(dataListSource);

            dataList.LoadMoreItemsAsync(10).AsTask();

            dataListSource.TriggerCount();
            dataListSource.TriggerItems();

            Assert.AreEqual(true, dataList.HasMoreItems);
        }

        [TestMethod]
        public void HasMoreItems_IsFalseOnceAllItemsAreRetrieved()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new IncrementalLoadingDataList<int>(dataListSource);

            dataList.LoadMoreItemsAsync(10).AsTask();

            dataListSource.TriggerCount();
            dataListSource.TriggerItems();

            dataList.LoadMoreItemsAsync(10).AsTask();

            dataListSource.TriggerItems();

            Assert.AreEqual(false, dataList.HasMoreItems);
        }

        [TestMethod]
        public void HasMoreItems_RaisesPropertyChangedToTrue()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new IncrementalLoadingDataList<int>(dataListSource);
            int hasMoreItemsChangedCount = 0;

            dataList.LoadMoreItemsAsync(10).AsTask();

            dataListSource.TriggerCount();
            dataListSource.TriggerItems();

            dataList.LoadMoreItemsAsync(10).AsTask();

            ((INotifyPropertyChanged)dataList).PropertyChanged += (object sender, PropertyChangedEventArgs e) =>
            {
                if (e.PropertyName == "HasMoreItems")
                    hasMoreItemsChangedCount++;
            };

            dataListSource.TriggerItems();

            Assert.AreEqual(1, hasMoreItemsChangedCount);
        }

        [TestMethod]
        public void IsLoading_IsInitiallyFalse()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new IncrementalLoadingDataList<int>(dataListSource);

            Assert.AreEqual(false, dataList.IsLoading);
        }

        [TestMethod]
        public void IsLoading_TrueAfterCallToLoadMoreItemsAsync()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new IncrementalLoadingDataList<int>(dataListSource);

            bool hasMoreItems = dataList.HasMoreItems;
            Task<LoadMoreItemsResult> task = dataList.LoadMoreItemsAsync(10).AsTask();

            Assert.AreEqual(true, dataList.IsLoading);
        }

        [TestMethod]
        public void IsLoading_RaisesPropertyChangedToTrue()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new IncrementalLoadingDataList<int>(dataListSource);

            int isLoadingChangedCount = 0;

            ((INotifyPropertyChanged)dataList).PropertyChanged += (object sender, PropertyChangedEventArgs e) =>
            {
                if (e.PropertyName == "IsLoading")
                    isLoadingChangedCount++;
            };

            bool hasMoreItems = dataList.HasMoreItems;
            Task<LoadMoreItemsResult> task = dataList.LoadMoreItemsAsync(10).AsTask();

            Assert.AreEqual(1, isLoadingChangedCount);
        }

        [TestMethod]
        public void IsLoading_FalseAfterItemsAreReturned()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new IncrementalLoadingDataList<int>(dataListSource);

            bool hasMoreItems = dataList.HasMoreItems;
            Task<LoadMoreItemsResult> task = dataList.LoadMoreItemsAsync(10).AsTask();
            dataListSource.TriggerCount();
            dataListSource.TriggerItems();

            Assert.AreEqual(false, dataList.IsLoading);
        }

        [TestMethod]
        public void IsLoading_RaisesPropertyChangedToFalse()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new IncrementalLoadingDataList<int>(dataListSource);
            int isLoadingChangedCount = 0;

            bool hasMoreItems = dataList.HasMoreItems;
            Task<LoadMoreItemsResult> task = dataList.LoadMoreItemsAsync(10).AsTask();

            ((INotifyPropertyChanged)dataList).PropertyChanged += (object sender, PropertyChangedEventArgs e) =>
            {
                if (e.PropertyName == "IsLoading")
                    isLoadingChangedCount++;
            };

            dataListSource.TriggerCount();
            dataListSource.TriggerItems();

            Assert.AreEqual(1, isLoadingChangedCount);
        }

        [TestMethod]
        public void MinimumPagingSize_IsInitiallyZero()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new IncrementalLoadingDataList<int>(dataListSource);

            Assert.AreEqual(0, dataList.MinimumPagingSize);
        }

        [TestMethod]
        public void MinimumPagingSize_SetterSetsValue()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new IncrementalLoadingDataList<int>(dataListSource);
            int minimumPagingSizeChangedCount = 0;

            ((INotifyPropertyChanged)dataList).PropertyChanged += (object sender, PropertyChangedEventArgs e) =>
            {
                if (e.PropertyName == "MinimumPagingSize")
                    minimumPagingSizeChangedCount++;
            };

            dataList.MinimumPagingSize = 42;

            Assert.AreEqual(1, minimumPagingSizeChangedCount);
        }

        [TestMethod]
        public void MinimumPagingSize_SetterRaisesPropertyChanged()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new IncrementalLoadingDataList<int>(dataListSource);

            dataList.MinimumPagingSize = 42;

            Assert.AreEqual(42, dataList.MinimumPagingSize);
        }

        // *** Method Tests ***

        [TestMethod]
        public void LoadMoreItemsAsync_ReturnsZeroItemsForAnEmptyDataList()
        {
            var dataListSource = new EmptyMockDataListSource();
            var dataList = new IncrementalLoadingDataList<int>(dataListSource);

            var loadMoreItemsTask = dataList.LoadMoreItemsAsync(10).AsTask();

            dataListSource.TriggerCount();

            Assert.AreEqual(true, loadMoreItemsTask.IsCompleted);
            Assert.AreEqual<uint>(0, loadMoreItemsTask.Result.Count);
        }

        [TestMethod]
        public void LoadMoreItemsAsync_ReturnsRequestedItemCount_ForAPartialDataList()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new IncrementalLoadingDataList<int>(dataListSource);

            var loadMoreItemsTask = dataList.LoadMoreItemsAsync(10).AsTask();

            dataListSource.TriggerCount();
            dataListSource.TriggerItems();

            Assert.AreEqual(true, loadMoreItemsTask.IsCompleted);
            Assert.AreEqual<uint>(10, loadMoreItemsTask.Result.Count);
        }

        [TestMethod]
        public void LoadMoreItemsAsync_ReturnsRemainingItemCount_ForLastPortionOfDataList()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new IncrementalLoadingDataList<int>(dataListSource);

            dataList.LoadMoreItemsAsync(10).AsTask();

            dataListSource.TriggerCount();
            dataListSource.TriggerItems();

            var loadMoreItemsTask = dataList.LoadMoreItemsAsync(10).AsTask();

            dataListSource.TriggerItems();

            Assert.AreEqual(true, loadMoreItemsTask.IsCompleted);
            Assert.AreEqual<uint>(5, loadMoreItemsTask.Result.Count);
        }

        [TestMethod]
        public void LoadMoreItemsAsync_ReturnsZero_ForCompletelyFetchedDataList()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new IncrementalLoadingDataList<int>(dataListSource);

            dataList.LoadMoreItemsAsync(10).AsTask();

            dataListSource.TriggerCount();
            dataListSource.TriggerItems();

            dataList.LoadMoreItemsAsync(10).AsTask();

            dataListSource.TriggerItems();

            var loadMoreItemsTask = dataList.LoadMoreItemsAsync(10).AsTask();

            dataListSource.TriggerItems();

            Assert.AreEqual(true, loadMoreItemsTask.IsCompleted);
            Assert.AreEqual<uint>(0, loadMoreItemsTask.Result.Count);
        }

        [TestMethod]
        public void LoadMoreItemsAsync_ReturnsAtLeastMinimumPagingSize_ForAPartialDataList()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new IncrementalLoadingDataList<int>(dataListSource) { MinimumPagingSize = 10 };

            var loadMoreItemsTask = dataList.LoadMoreItemsAsync(5).AsTask();

            dataListSource.TriggerCount();
            dataListSource.TriggerItems();

            Assert.AreEqual(true, loadMoreItemsTask.IsCompleted);
            Assert.AreEqual<uint>(10, loadMoreItemsTask.Result.Count);
        }

        [TestMethod]
        public void LoadMoreItemsAsync_ReturnsLessThanMinimumPagingSize_ForLastPortionOfDataList()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new IncrementalLoadingDataList<int>(dataListSource) { MinimumPagingSize = 10 };

            dataList.LoadMoreItemsAsync(10).AsTask();

            dataListSource.TriggerCount();
            dataListSource.TriggerItems();

            var loadMoreItemsTask = dataList.LoadMoreItemsAsync(2).AsTask();

            dataListSource.TriggerItems();

            Assert.AreEqual(true, loadMoreItemsTask.IsCompleted);
            Assert.AreEqual<uint>(5, loadMoreItemsTask.Result.Count);
        }

        [TestMethod]
        public void LoadMoreItemsAsync_RaisesCollectionChangedEvents()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new IncrementalLoadingDataList<int>(dataListSource);

            dataList.LoadMoreItemsAsync(10).AsTask();

            dataListSource.TriggerCount();

            int collectionChangedCount = 0;

            dataList.CollectionChanged += (object sender, NotifyCollectionChangedEventArgs e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                    collectionChangedCount++;
            };

            dataListSource.TriggerItems();

            Assert.AreEqual(10, collectionChangedCount);
        }

        [TestMethod]
        public void IndexOf_ReturnsItemIndex()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new IncrementalLoadingDataList<int>(dataListSource);

            var loadMoreItemsTask = dataList.LoadMoreItemsAsync(10).AsTask();

            dataListSource.TriggerCount();
            dataListSource.TriggerItems();

            Assert.AreEqual(5, dataList.IndexOf(12));
        }

        [TestMethod]
        public void IndexOf_ReturnsMinusOneIfItemNotInSource()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new IncrementalLoadingDataList<int>(dataListSource);

            var loadMoreItemsTask = dataList.LoadMoreItemsAsync(10).AsTask();

            dataListSource.TriggerCount();
            dataListSource.TriggerItems();

            Assert.AreEqual(-1, dataList.IndexOf(99));
        }

        [TestMethod]
        public void IndexOf_ReturnsMinusOneIfItemOutsideOfVisibleCollection()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new IncrementalLoadingDataList<int>(dataListSource);

            var loadMoreItemsTask = dataList.LoadMoreItemsAsync(10).AsTask();

            dataListSource.TriggerCount();
            dataListSource.TriggerItems();

            Assert.AreEqual(-1, dataList.IndexOf(26));
        }

        // *** Update Method Tests ***

        [TestMethod]
        public void Update_Reset_RaisesCountPropertyChanged()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new IncrementalLoadingDataList<int>(dataListSource);
            var loadMoreItemsTask = dataList.LoadMoreItemsAsync(10).AsTask();
            dataListSource.TriggerCount();
            dataListSource.TriggerItems();

            int propertyChangedCount = 0;
            dataList.PropertyChanged += (sender, e) => { if (e.PropertyName == "Count") { propertyChangedCount++; } };

            ((IUpdatableCollection)dataList).Update(new DataListUpdate(DataListUpdateAction.Reset));

            Assert.AreEqual(1, propertyChangedCount);
        }

        [TestMethod]
        public void Update_Reset_RaisesItemsPropertyChanged()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new IncrementalLoadingDataList<int>(dataListSource);
            var loadMoreItemsTask = dataList.LoadMoreItemsAsync(10).AsTask();
            dataListSource.TriggerCount();
            dataListSource.TriggerItems();

            int propertyChangedCount = 0;
            dataList.PropertyChanged += (sender, e) => { if (e.PropertyName == "Item[]") { propertyChangedCount++; } };

            ((IUpdatableCollection)dataList).Update(new DataListUpdate(DataListUpdateAction.Reset));

            Assert.AreEqual(1, propertyChangedCount);
        }

        [TestMethod]
        public void Update_Reset_RaisesCollectionChanged()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new IncrementalLoadingDataList<int>(dataListSource);
            var loadMoreItemsTask = dataList.LoadMoreItemsAsync(10).AsTask();
            dataListSource.TriggerCount();
            dataListSource.TriggerItems();

            int collectionChangedCount = 0;
            dataList.CollectionChanged += (sender, e) => { if (e.Action == NotifyCollectionChangedAction.Reset) { collectionChangedCount++; } };

            ((IUpdatableCollection)dataList).Update(new DataListUpdate(DataListUpdateAction.Reset));

            Assert.AreEqual(1, collectionChangedCount);
        }

        [TestMethod]
        public void Update_Reset_ResetsVisibleCountToZero()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new IncrementalLoadingDataList<int>(dataListSource);
            dataList.LoadMoreItemsAsync(10).AsTask();
            dataListSource.TriggerCount();
            dataListSource.TriggerItems();

            ((IUpdatableCollection)dataList).Update(new DataListUpdate(DataListUpdateAction.Reset));

            Assert.AreEqual(0, dataList.Count);
        }

        [TestMethod]
        public void Update_Reset_ResetsHasMoreItemsToTrue()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new IncrementalLoadingDataList<int>(dataListSource);

            dataList.LoadMoreItemsAsync(10).AsTask();

            dataListSource.TriggerCount();
            dataListSource.TriggerItems();

            dataList.LoadMoreItemsAsync(10).AsTask();

            dataListSource.TriggerItems();

            ((IUpdatableCollection)dataList).Update(new DataListUpdate(DataListUpdateAction.Reset));

            Assert.AreEqual(true, dataList.HasMoreItems);
        }

        [TestMethod]
        public void Update_Reset_RaisesPropertyChangedForHasMoreItems()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new IncrementalLoadingDataList<int>(dataListSource);

            dataList.LoadMoreItemsAsync(10).AsTask();

            dataListSource.TriggerCount();
            dataListSource.TriggerItems();

            dataList.LoadMoreItemsAsync(10).AsTask();

            dataListSource.TriggerItems();

            int propertyChangedCount = 0;
            dataList.PropertyChanged += (sender, e) => { if (e.PropertyName == "HasMoreItems") { propertyChangedCount++; } };

            ((IUpdatableCollection)dataList).Update(new DataListUpdate(DataListUpdateAction.Reset));

            Assert.AreEqual(1, propertyChangedCount);
        }

        [TestMethod]
        public void Update_Add_RaisesCountPropertyChanged()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new IncrementalLoadingDataList<int>(dataListSource);
            var loadMoreItemsTask = dataList.LoadMoreItemsAsync(10).AsTask();
            dataListSource.TriggerCount();
            dataListSource.TriggerItems();

            int propertyChangedCount = 0;
            dataList.PropertyChanged += (sender, e) => { if (e.PropertyName == "Count") { propertyChangedCount++; } };

            ((IUpdatableCollection)dataList).Update(new DataListUpdate(DataListUpdateAction.Add, 9, 5));

            Assert.AreEqual(1, propertyChangedCount);
        }

        [TestMethod]
        public void Update_Add_RaisesItemsPropertyChanged()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new IncrementalLoadingDataList<int>(dataListSource);
            var loadMoreItemsTask = dataList.LoadMoreItemsAsync(10).AsTask();
            dataListSource.TriggerCount();
            dataListSource.TriggerItems();

            int propertyChangedCount = 0;
            dataList.PropertyChanged += (sender, e) => { if (e.PropertyName == "Item[]") { propertyChangedCount++; } };

            ((IUpdatableCollection)dataList).Update(new DataListUpdate(DataListUpdateAction.Add, 9, 5));

            Assert.AreEqual(1, propertyChangedCount);
        }

        [TestMethod]
        public void Update_Add_RaisesCollectionChanged()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new IncrementalLoadingDataList<int>(dataListSource);
            var loadMoreItemsTask = dataList.LoadMoreItemsAsync(10).AsTask();
            dataListSource.TriggerCount();
            dataListSource.TriggerItems();

            List<int> collectionChangedIndices = new List<int>();
            dataList.CollectionChanged += (sender, e) => { if (e.Action == NotifyCollectionChangedAction.Add) { collectionChangedIndices.Add(e.NewStartingIndex); } };

            ((IUpdatableCollection)dataList).Update(new DataListUpdate(DataListUpdateAction.Add, 9, 5));

            CollectionAssert.AreEqual(new[] { 9, 10, 11, 12, 13 }, collectionChangedIndices);
        }

        [TestMethod]
        public void Update_Add_IncreasesCount()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new IncrementalLoadingDataList<int>(dataListSource);
            var loadMoreItemsTask = dataList.LoadMoreItemsAsync(10).AsTask();
            dataListSource.TriggerCount();
            dataListSource.TriggerItems();

            ((IUpdatableCollection)dataList).Update(new DataListUpdate(DataListUpdateAction.Add, 9, 5));

            Assert.AreEqual(15, dataList.Count);
        }

        [TestMethod]
        public void Update_Add_RaisesCollectionChangedIfOnEdgeOfVisibleCollection()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new IncrementalLoadingDataList<int>(dataListSource);
            var loadMoreItemsTask = dataList.LoadMoreItemsAsync(10).AsTask();
            dataListSource.TriggerCount();
            dataListSource.TriggerItems();

            List<int> collectionChangedIndices = new List<int>();
            dataList.CollectionChanged += (sender, e) => { if (e.Action == NotifyCollectionChangedAction.Add) { collectionChangedIndices.Add(e.NewStartingIndex); } };

            ((IUpdatableCollection)dataList).Update(new DataListUpdate(DataListUpdateAction.Add, 10, 5));

            CollectionAssert.AreEqual(new[] { 10, 11, 12, 13, 14 }, collectionChangedIndices);
        }

        [TestMethod]
        public void Update_Add_IgnoresUpdateIfOutsideOfVisibleCollection()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new IncrementalLoadingDataList<int>(dataListSource);
            var loadMoreItemsTask = dataList.LoadMoreItemsAsync(10).AsTask();
            dataListSource.TriggerCount();
            dataListSource.TriggerItems();

            int propertyChangedCount = 0;
            int collectionChangedCount = 0;
            dataList.PropertyChanged += (sender, e) => { propertyChangedCount++; };
            dataList.CollectionChanged += (sender, e) => { collectionChangedCount++; };

            ((IUpdatableCollection)dataList).Update(new DataListUpdate(DataListUpdateAction.Add, 11, 5));

            Assert.AreEqual(0, propertyChangedCount);
            Assert.AreEqual(0, collectionChangedCount);
        }

        [TestMethod]
        public void Update_Remove_RaisesCountPropertyChanged()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new IncrementalLoadingDataList<int>(dataListSource);
            var loadMoreItemsTask = dataList.LoadMoreItemsAsync(10).AsTask();
            dataListSource.TriggerCount();
            dataListSource.TriggerItems();

            int propertyChangedCount = 0;
            dataList.PropertyChanged += (sender, e) => { if (e.PropertyName == "Count") { propertyChangedCount++; } };

            ((IUpdatableCollection)dataList).Update(new DataListUpdate(DataListUpdateAction.Remove, 2, 5));

            Assert.AreEqual(1, propertyChangedCount);
        }

        [TestMethod]
        public void Update_Remove_RaisesItemsPropertyChanged()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new IncrementalLoadingDataList<int>(dataListSource);
            var loadMoreItemsTask = dataList.LoadMoreItemsAsync(10).AsTask();
            dataListSource.TriggerCount();
            dataListSource.TriggerItems();

            int propertyChangedCount = 0;
            dataList.PropertyChanged += (sender, e) => { if (e.PropertyName == "Item[]") { propertyChangedCount++; } };

            ((IUpdatableCollection)dataList).Update(new DataListUpdate(DataListUpdateAction.Remove, 2, 5));

            Assert.AreEqual(1, propertyChangedCount);
        }

        [TestMethod]
        public void Update_Remove_RaisesCollectionChanged()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new IncrementalLoadingDataList<int>(dataListSource);
            var loadMoreItemsTask = dataList.LoadMoreItemsAsync(10).AsTask();
            dataListSource.TriggerCount();
            dataListSource.TriggerItems();

            List<int> collectionChangedIndices = new List<int>();
            dataList.CollectionChanged += (sender, e) => { if (e.Action == NotifyCollectionChangedAction.Remove) { collectionChangedIndices.Add(e.OldStartingIndex); } };

            ((IUpdatableCollection)dataList).Update(new DataListUpdate(DataListUpdateAction.Remove, 2, 5));

            CollectionAssert.AreEqual(new[] { 6, 5, 4, 3, 2 }, collectionChangedIndices);
        }

        [TestMethod]
        public void Update_Remove_DecreasesCount()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new IncrementalLoadingDataList<int>(dataListSource);
            var loadMoreItemsTask = dataList.LoadMoreItemsAsync(10).AsTask();
            dataListSource.TriggerCount();
            dataListSource.TriggerItems();

            ((IUpdatableCollection)dataList).Update(new DataListUpdate(DataListUpdateAction.Remove, 2, 5));

            Assert.AreEqual(5, dataList.Count);
        }

        [TestMethod]
        public void Update_Remove_RaisesCollectionChangedIfOnlySomeItemsVisible()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new IncrementalLoadingDataList<int>(dataListSource);
            var loadMoreItemsTask = dataList.LoadMoreItemsAsync(10).AsTask();
            dataListSource.TriggerCount();
            dataListSource.TriggerItems();

            List<int> collectionChangedIndices = new List<int>();
            dataList.CollectionChanged += (sender, e) => { if (e.Action == NotifyCollectionChangedAction.Remove) { collectionChangedIndices.Add(e.OldStartingIndex); } };

            ((IUpdatableCollection)dataList).Update(new DataListUpdate(DataListUpdateAction.Remove, 7, 5));

            CollectionAssert.AreEqual(new[] { 9, 8, 7 }, collectionChangedIndices);
        }

        [TestMethod]
        public void Update_Remove_DecreasesCountIfOnlySomeItemsVisible()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new IncrementalLoadingDataList<int>(dataListSource);
            var loadMoreItemsTask = dataList.LoadMoreItemsAsync(10).AsTask();
            dataListSource.TriggerCount();
            dataListSource.TriggerItems();

            ((IUpdatableCollection)dataList).Update(new DataListUpdate(DataListUpdateAction.Remove, 7, 5));

            Assert.AreEqual(7, dataList.Count);
        }

        [TestMethod]
        public void Update_Remove_IgnoresUpdateIfOnEdgeOfVisibleCollection()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new IncrementalLoadingDataList<int>(dataListSource);
            var loadMoreItemsTask = dataList.LoadMoreItemsAsync(10).AsTask();
            dataListSource.TriggerCount();
            dataListSource.TriggerItems();

            int propertyChangedCount = 0;
            int collectionChangedCount = 0;
            dataList.PropertyChanged += (sender, e) => { propertyChangedCount++; };
            dataList.CollectionChanged += (sender, e) => { collectionChangedCount++; };

            ((IUpdatableCollection)dataList).Update(new DataListUpdate(DataListUpdateAction.Remove, 10, 5));

            Assert.AreEqual(0, propertyChangedCount);
            Assert.AreEqual(0, collectionChangedCount);
        }

        [TestMethod]
        public void Update_Remove_IgnoresUpdateIfOutsideOfVisibleCollection()
        {
            var dataListSource = new MockDataListSource();
            var dataList = new IncrementalLoadingDataList<int>(dataListSource);
            var loadMoreItemsTask = dataList.LoadMoreItemsAsync(10).AsTask();
            dataListSource.TriggerCount();
            dataListSource.TriggerItems();

            int propertyChangedCount = 0;
            int collectionChangedCount = 0;
            dataList.PropertyChanged += (sender, e) => { propertyChangedCount++; };
            dataList.CollectionChanged += (sender, e) => { collectionChangedCount++; };

            ((IUpdatableCollection)dataList).Update(new DataListUpdate(DataListUpdateAction.Remove, 11, 5));

            Assert.AreEqual(0, propertyChangedCount);
            Assert.AreEqual(0, collectionChangedCount);
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

            public void TriggerItems(bool resetTrigger = true)
            {
                itemsCompletionSource.SetResult(true);

                if (resetTrigger)
                    itemsCompletionSource = new TaskCompletionSource<bool>();
            }

            // *** IDataListSource<int> Methods ***

            public async Task<int> GetCountAsync()
            {
                await countCompletionSource.Task;
                return 15;
            }

            public async Task<int> GetItemAsync(int index)
            {
                await itemsCompletionSource.Task;
                return (index + 1) * 2;
            }

            public int IndexOf(int item)
            {
                if (item < 0 || item >= 32)
                    return -1;
                else
                    return item / 2 - 1;
            }

            public IDisposable Subscribe(IUpdatableCollection collection)
            {
                return new MockDisposable();
            }
        }

        private class EmptyMockDataListSource : IDataListSource<int>
        {
            // *** Fields ***

            private TaskCompletionSource<bool> countCompletionSource = new TaskCompletionSource<bool>();

            // *** Constructors ***

            public EmptyMockDataListSource()
            {
            }

            // *** Methods ***

            public void TriggerCount()
            {
                countCompletionSource.SetResult(true);
            }

            // *** IDataListSource<int> Methods ***

            public async Task<int> GetCountAsync()
            {
                await countCompletionSource.Task;
                return 0;
            }

            public Task<int> GetItemAsync(int index)
            {
                throw new InvalidOperationException();
            }

            public int IndexOf(int item)
            {
                throw new NotImplementedException();
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

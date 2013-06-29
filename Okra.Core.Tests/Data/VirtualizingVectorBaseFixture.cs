using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Okra.Data;
using Okra.Tests.Helpers;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace Okra.Tests.Data
{
    [TestClass]
    public class VirtualizingVectorBaseFixture
    {
        // *** IList<T> Tests ***

        [TestMethod]
        public void IListT_IsReadOnly_Getter_ReturnsTrue()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();

            Assert.AreEqual(true, ((IList<int>)vector).IsReadOnly);
        }

        [TestMethod]
        public void IListT_Indexer_Getter_ReturnsPlaceholderBeforeItemFetched()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();
            vector.CompleteGetCount(42);

            Assert.AreEqual(0, ((IList<int>)vector)[5]);
        }

        [TestMethod]
        public void IListT_Indexer_Getter_ReturnsItemOnCollectionChangedAfterAsyncItemFetched()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();
            vector.CompleteGetCount(42);
            int firstItemResult = ((IList<int>)vector)[5];

            int? updatedItemResult = null;

            vector.CollectionChanged += (sender, e) =>
                {
                    Assert.AreEqual(NotifyCollectionChangedAction.Replace, e.Action);
                    Assert.AreEqual(5, e.NewStartingIndex);
                    Assert.AreEqual(5, e.OldStartingIndex);
                    CollectionAssert.AreEqual(new int[] { 0 }, e.OldItems);
                    CollectionAssert.AreEqual(new int[] { 15 }, e.NewItems);

                    // NB: Get the item within the collection changed event

                    updatedItemResult = ((IList<int>)vector)[5];
                };

            vector.CompleteGetItem(15, clearCachedValue: true);

            Assert.AreEqual(15, updatedItemResult);
        }

        [TestMethod]
        public void IListT_Indexer_Getter_ReturnsItemDirectlyWithSyncItemFetched()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();
            vector.CompleteGetCount(42);
            vector.CompleteGetItem(15);

            Assert.AreEqual(15, ((IList<int>)vector)[5]);
        }

        [TestMethod]
        public void IListT_Indexer_Getter_ReturnsItemWithNullElements()
        {
            MockNullVirtualizingVector vector = new MockNullVirtualizingVector();

            Assert.AreEqual(null, ((IList<object>)vector)[5]);
        }

        [TestMethod]
        public void IListT_Indexer_Getter_OnlyFetchesOnceWithMultipleCallsWhilstAwaiting()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();
            vector.CompleteGetCount(42);

            int firstItemResult1 = ((IList<int>)vector)[5];
            int firstItemResult2 = ((IList<int>)vector)[5];

            int collectionChangedCount = 0;

            vector.CollectionChanged += (sender, e) =>
            {
                Assert.AreEqual(NotifyCollectionChangedAction.Replace, e.Action);
                Assert.AreEqual(5, e.NewStartingIndex);
                Assert.AreEqual(5, e.OldStartingIndex);
                CollectionAssert.AreEqual(new int[] { 0 }, e.OldItems);
                CollectionAssert.AreEqual(new int[] { 15 }, e.NewItems);

                collectionChangedCount++;
            };

            vector.CompleteGetItem(15, clearCachedValue: true);

            Assert.AreEqual(1, collectionChangedCount);
        }

        [TestMethod]
        public void IListT_Indexer_Setter_ThrowsException_CollectionIsReadOnly()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();

            Assert.ThrowsException<InvalidOperationException>(() => ((IList<int>)vector)[6] = 10);
        }

        [TestMethod]
        public void IListT_Add_ThrowsException_CollectionIsReadOnly()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();

            Assert.ThrowsException<InvalidOperationException>(() => ((IList<int>)vector).Add(42));
        }

        [TestMethod]
        public void IListT_Clear_ThrowsException_CollectionIsReadOnly()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();

            Assert.ThrowsException<InvalidOperationException>(() => ((IList<int>)vector).Clear());
        }

        [TestMethod]
        public void IListT_Contains_ReturnsTrueIfReportedInList()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();

            Assert.AreEqual(true, ((IList<int>)vector).Contains(15));
        }

        [TestMethod]
        public void IListT_Contains_ReturnsFalseIfNotReportedInList()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();

            Assert.AreEqual(false, ((IList<int>)vector).Contains(16));
        }

        [TestMethod]
        public void IListT_CopyTo_CopiesToDestinationArray()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();
            vector.CompleteGetCount(42);
            vector.CompleteGetItem(15);

            int[] values = new int[42];
            ((IList<int>)vector).CopyTo(values, 0);

            CollectionAssert.AreEqual(new int[] { 1, 1, 1, 1, 1, 15, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, values);
        }

        [TestMethod]
        public void IListT_Count_ReturnsZeroBeforeItemFetched()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();

            Assert.AreEqual(0, ((IList<int>)vector).Count);
        }

        [TestMethod]
        public void IListT_Count_CallsFetchCount_ReturnedAsync()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();

            int firstCountResult = ((IList<int>)vector).Count;
            vector.CompleteGetCount(42);

            Assert.AreEqual(42, ((IList<int>)vector).Count);
        }

        [TestMethod]
        public void IListT_Count_CallsFetchCount_ReturnedSync()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();
            vector.CompleteGetCount(42);

            Assert.AreEqual(42, ((IList<int>)vector).Count);
        }

        [TestMethod]
        public void IListT_Count_OnlyCallsFetchCountOnce_ReturnedAsync()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();

            int dummy1 = ((IList<int>)vector).Count;
            int dummy2 = ((IList<int>)vector).Count;
            int dummy3 = ((IList<int>)vector).Count;
            int dummy4 = ((IList<int>)vector).Count;

            vector.CompleteGetCount(42);

            Assert.AreEqual(1, vector.FetchCountCallCount);
        }

        [TestMethod]
        public void IListT_Count_OnlyCallsFetchCountOnce_ReturnedSync()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();
            vector.CompleteGetCount(42);

            int dummy1 = ((IList<int>)vector).Count;
            int dummy2 = ((IList<int>)vector).Count;
            int dummy3 = ((IList<int>)vector).Count;
            int dummy4 = ((IList<int>)vector).Count;

            Assert.AreEqual(1, vector.FetchCountCallCount);
        }

        [TestMethod]
        public void IListT_GetEnumerator_EnumeratesCurrentStateOfCollection()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();
            vector.CompleteGetCount(42);
            vector.CompleteGetItem(15);

            IEnumerable<int> enumerable = (IEnumerable<int>)vector;
            List<int> values = new List<int>();

            foreach (int value in enumerable)
                values.Add(value);

            CollectionAssert.AreEqual(new int[] { 1, 1, 1, 1, 1, 15, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, values);
        }

        [TestMethod]
        public void IListT_IndexOf_ReturnsItemIndexIfReportedInList()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();

            Assert.AreEqual(5, ((IList<int>)vector).IndexOf(15));
        }

        [TestMethod]
        public void IListT_IndexOf_ReturnsMinusOneIfNotReportedInList()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();

            Assert.AreEqual(-1, ((IList<int>)vector).IndexOf(16));
        }

        [TestMethod]
        public void IListT_Insert_ThrowsException_CollectionIsReadOnly()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();

            Assert.ThrowsException<InvalidOperationException>(() => ((IList<int>)vector).Insert(42, 2));
        }

        [TestMethod]
        public void IListT_Remove_ThrowsException_CollectionIsReadOnly()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();

            Assert.ThrowsException<InvalidOperationException>(() => ((IList<int>)vector).Remove(42));
        }

        [TestMethod]
        public void IListT_RemoveAt_ThrowsException_CollectionIsReadOnly()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();

            Assert.ThrowsException<InvalidOperationException>(() => ((IList<int>)vector).RemoveAt(2));
        }

        // *** IList Tests ***

        [TestMethod]
        public void IList_IsFixedSize_Getter_ReturnsFalse()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();

            Assert.AreEqual(false, ((IList)vector).IsFixedSize);
        }

        [TestMethod]
        public void IList_IsSynchronized_Getter_ReturnsFalse()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();

            Assert.AreEqual(false, ((IList)vector).IsSynchronized);
        }

        [TestMethod]
        public void IList_SyncRoot_Getter_ReturnsItself()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();

            Assert.AreEqual(vector, ((IList)vector).SyncRoot);
        }

        [TestMethod]
        public void IList_IsReadOnly_Getter_ReturnsTrue()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();

            Assert.AreEqual(true, ((IList)vector).IsReadOnly);
        }

        [TestMethod]
        public void IList_Indexer_Getter_ReturnsPlaceholderBeforeItemFetched()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();
            vector.CompleteGetCount(42);

            Assert.AreEqual(0, ((IList)vector)[5]);
        }

        [TestMethod]
        public void IList_Indexer_Getter_ReturnsItemOnCollectionChangedAfterAsyncItemFetched()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();
            vector.CompleteGetCount(42);
            object firstItemResult = ((IList)vector)[5];

            object updatedItemResult = null;

            vector.CollectionChanged += (sender, e) =>
            {
                Assert.AreEqual(NotifyCollectionChangedAction.Replace, e.Action);
                Assert.AreEqual(5, e.NewStartingIndex);
                Assert.AreEqual(5, e.OldStartingIndex);
                CollectionAssert.AreEqual(new int[] { 0 }, e.OldItems);
                CollectionAssert.AreEqual(new int[] { 15 }, e.NewItems);

                // NB: Get the item within the collection changed event

                updatedItemResult = ((IList)vector)[5];
            };

            vector.CompleteGetItem(15, clearCachedValue: true);

            Assert.AreEqual(15, updatedItemResult);
        }

        [TestMethod]
        public void IList_Indexer_Getter_ReturnsItemDirectlyWithSyncItemFetched()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();
            vector.CompleteGetCount(42);
            vector.CompleteGetItem(15);

            Assert.AreEqual(15, ((IList)vector)[5]);
        }

        [TestMethod]
        public void IList_Indexer_Getter_ReturnsItemWithNullElements()
        {
            MockNullVirtualizingVector vector = new MockNullVirtualizingVector();

            Assert.AreEqual(null, ((IList)vector)[5]);
        }

        [TestMethod]
        public void IList_Indexer_Setter_ThrowsException_CollectionIsReadOnly()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();

            Assert.ThrowsException<InvalidOperationException>(() => ((IList)vector)[6] = 10);
        }

        [TestMethod]
        public void IList_Add_ThrowsException_CollectionIsReadOnly()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();

            Assert.ThrowsException<InvalidOperationException>(() => ((IList)vector).Add(42));
        }

        [TestMethod]
        public void IList_Clear_ThrowsException_CollectionIsReadOnly()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();

            Assert.ThrowsException<InvalidOperationException>(() => ((IList)vector).Clear());
        }

        [TestMethod]
        public void IList_Contains_ReturnsTrueIfReportedInList()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();

            Assert.AreEqual(true, ((IList)vector).Contains(15));
        }

        [TestMethod]
        public void IList_Contains_ReturnsFalseIfNotReportedInList()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();

            Assert.AreEqual(false, ((IList)vector).Contains(16));
        }

        [TestMethod]
        public void IList_Contains_ReturnsFalseIfNotOfCorrectType()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();

            Assert.AreEqual(false, ((IList)vector).Contains("string"));
        }

        [TestMethod]
        public void IList_CopyTo_CopiesToDestinationArray()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();
            vector.CompleteGetCount(42);
            vector.CompleteGetItem(15);

            int[] values = new int[42];
            ((IList)vector).CopyTo(values, 0);

            CollectionAssert.AreEqual(new int[] { 1, 1, 1, 1, 1, 15, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, values);
        }

        [TestMethod]
        public void IList_Count_ReturnsZeroBeforeItemFetched()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();

            Assert.AreEqual(0, ((IList)vector).Count);
        }

        [TestMethod]
        public void IList_Count_CallsFetchCount_ReturnedAsync()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();

            int firstCountResult = ((IList)vector).Count;
            vector.CompleteGetCount(42);

            Assert.AreEqual(42, ((IList)vector).Count);
        }

        [TestMethod]
        public void IList_Count_CallsFetchCount_ReturnedSync()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();
            vector.CompleteGetCount(42);

            Assert.AreEqual(42, ((IList)vector).Count);
        }

        [TestMethod]
        public void IList_Count_OnlyCallsFetchCountOnce_ReturnedAsync()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();

            int dummy1 = ((IList)vector).Count;
            int dummy2 = ((IList)vector).Count;
            int dummy3 = ((IList)vector).Count;
            int dummy4 = ((IList)vector).Count;

            vector.CompleteGetCount(42);

            Assert.AreEqual(1, vector.FetchCountCallCount);
        }

        [TestMethod]
        public void IList_Count_OnlyCallsFetchCountOnce_ReturnedSync()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();
            vector.CompleteGetCount(42);

            int dummy1 = ((IList)vector).Count;
            int dummy2 = ((IList)vector).Count;
            int dummy3 = ((IList)vector).Count;
            int dummy4 = ((IList)vector).Count;

            Assert.AreEqual(1, vector.FetchCountCallCount);
        }

        [TestMethod]
        public void IList_GetEnumerator_EnumeratesCurrentStateOfCollection()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();
            vector.CompleteGetCount(42);
            vector.CompleteGetItem(15);

            IEnumerable enumerable = (IEnumerable)vector;
            List<int> values = new List<int>();

            foreach (int value in enumerable)
                values.Add(value);

            CollectionAssert.AreEqual(new int[] { 1, 1, 1, 1, 1, 15, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, values);
        }

        [TestMethod]
        public void IList_IndexOf_ReturnsItemIndexIfReportedInList()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();

            Assert.AreEqual(5, ((IList)vector).IndexOf(15));
        }

        [TestMethod]
        public void IList_IndexOf_ReturnsMinusOneIfNotReportedInList()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();

            Assert.AreEqual(-1, ((IList)vector).IndexOf(16));
        }

        [TestMethod]
        public void IList_IndexOf_ReturnsMinusOneIfNotOfCorrectType()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();

            Assert.AreEqual(-1, ((IList)vector).IndexOf("string"));
        }

        [TestMethod]
        public void IList_Insert_ThrowsException_CollectionIsReadOnly()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();

            Assert.ThrowsException<InvalidOperationException>(() => ((IList)vector).Insert(42, 2));
        }

        [TestMethod]
        public void IList_Remove_ThrowsException_CollectionIsReadOnly()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();

            Assert.ThrowsException<InvalidOperationException>(() => ((IList)vector).Remove(42));
        }

        [TestMethod]
        public void IList_RemoveAt_ThrowsException_CollectionIsReadOnly()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();

            Assert.ThrowsException<InvalidOperationException>(() => ((IList)vector).RemoveAt(2));
        }

        // *** Property Tests ***

        [TestMethod]
        public void IsLoading_IsInitiallyFalse()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();

            Assert.AreEqual(false, vector.IsLoading);
        }

        [TestMethod]
        public void IsLoading_TrueAfterFirstCallToCountGetter()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();

            int count = vector.Count;

            Assert.AreEqual(true, vector.IsLoading);
        }

        [TestMethod]
        public void IsLoading_RaisesPropertyChangedToTrue()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();

            int isLoadingChangedCount = 0;

            vector.PropertyChanged += (object sender, PropertyChangedEventArgs e) =>
            {
                if (e.PropertyName == "IsLoading")
                    isLoadingChangedCount++;
            };

            int count = vector.Count;

            Assert.AreEqual(1, isLoadingChangedCount);
        }

        [TestMethod]
        public void IsLoading_FalseAfterCountIsReturned()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();

            int count = vector.Count;
            vector.CompleteGetCount(42);

            Assert.AreEqual(false, vector.IsLoading);
        }

        [TestMethod]
        public void IsLoading_RaisesPropertyChangedToFalse()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();
            int isLoadingChangedCount = 0;

            int count = vector.Count;

            vector.PropertyChanged += (object sender, PropertyChangedEventArgs e) =>
            {
                if (e.PropertyName == "IsLoading")
                    isLoadingChangedCount++;
            };

            vector.CompleteGetCount(42);

            Assert.AreEqual(1, isLoadingChangedCount);
        }

        // *** Update Method Tests ***

        [TestMethod]
        public void Reset_CountReturnsZeroWhilstNewCountIsRetrieved()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();
            int firstCountResult = vector.Count;
            vector.CompleteGetCount(42, true);

            vector.Reset();

            Assert.AreEqual(0, vector.Count);
        }

        [TestMethod]
        public void Reset_CountReturnsNewValueOnceRetrieved()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();
            int firstCountResult = vector.Count;
            vector.CompleteGetCount(42, true);

            vector.Reset();

            int secondCountResult = vector.Count;
            vector.CompleteGetCount(53, true);

            Assert.AreEqual(53, vector.Count);
        }

        [TestMethod]
        public void Reset_FetchesCountTwiceIfPreviousCountRetrieved()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();
            int firstCountResult = vector.Count;
            vector.CompleteGetCount(42, true);

            vector.Reset();

            int secondCountResult = vector.Count;
            vector.CompleteGetCount(53, true);

            Assert.AreEqual(2, vector.FetchCountCallCount);
        }

        [TestMethod]
        public void Reset_CountReturnsNewValueIfFirstFetchDidNotCompleteRetrieved()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();
            int firstCountResult = vector.Count;

            vector.Reset();

            int secondCountResult = vector.Count;
            vector.CompleteGetCount(53, true);

            Assert.AreEqual(53, vector.Count);
        }

        [TestMethod]
        public void Reset_FetchesCountOnceIfFirstFetchDidNotCompleteRetrieved()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();
            int firstCountResult = vector.Count;

            vector.Reset();

            int secondCountResult = vector.Count;
            vector.CompleteGetCount(53, true);

            Assert.AreEqual(1, vector.FetchCountCallCount);
        }

        [TestMethod]
        public void Reset_RaisesCountPropertyChanged()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();
            int firstCountResult = vector.Count;
            vector.CompleteGetCount(42);

            int propertyChangedCount = 0;
            vector.PropertyChanged += (sender, e) => { if (e.PropertyName == "Count") { propertyChangedCount++; } };

            vector.Reset();

            Assert.AreEqual(1, propertyChangedCount);
        }

        [TestMethod]
        public void Reset_RaisesItemsPropertyChanged()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();
            int firstCountResult = vector.Count;
            vector.CompleteGetCount(42);

            int propertyChangedCount = 0;
            vector.PropertyChanged += (sender, e) => { if (e.PropertyName == "Item[]") { propertyChangedCount++; } };

            vector.Reset();

            Assert.AreEqual(1, propertyChangedCount);
        }

        [TestMethod]
        public void Reset_RaisesCollectionChanged()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();
            int firstCountResult = vector.Count;
            vector.CompleteGetCount(42);

            int collectionChangedCount = 0;
            vector.CollectionChanged += (sender, e) => { if (e.Action == NotifyCollectionChangedAction.Reset) { collectionChangedCount++; } };

            vector.Reset();

            Assert.AreEqual(1, collectionChangedCount);
        }

        [TestMethod]
        public void OnItemsAdded_CountPropertyIsIncremented()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();
            int firstCountResult = vector.Count;
            vector.CompleteGetCount(42);

            vector.OnItemsAdded(5, 10);

            Assert.AreEqual(52, vector.Count);
        }

        [TestMethod]
        public void OnItemsAdded_CountIsNotNotRefetched()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();
            int firstCountResult = vector.Count;
            vector.CompleteGetCount(42);

            vector.OnItemsAdded(5, 10);

            int count = vector.Count;

            Assert.AreEqual(1, vector.FetchCountCallCount);
        }

        [TestMethod]
        public void OnItemsAdded_RaisesCountPropertyChanged()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();
            int firstCountResult = vector.Count;
            vector.CompleteGetCount(42);

            int propertyChangedCount = 0;
            vector.PropertyChanged += (sender, e) => { if (e.PropertyName == "Count") { propertyChangedCount++; } };

            vector.OnItemsAdded(5, 10);

            Assert.AreEqual(1, propertyChangedCount);
        }

        [TestMethod]
        public void OnItemsAdded_RaisesItemsPropertyChanged()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();
            int firstCountResult = vector.Count;
            vector.CompleteGetCount(42);

            int propertyChangedCount = 0;
            vector.PropertyChanged += (sender, e) => { if (e.PropertyName == "Item[]") { propertyChangedCount++; } };

            vector.OnItemsAdded(5, 10);

            Assert.AreEqual(1, propertyChangedCount);
        }

        [TestMethod]
        public void OnItemsAdded_RaisesCollectionChanged_ForAddedItems()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();
            int firstCountResult = vector.Count;
            vector.CompleteGetCount(42);

            List<int> collectionChangedIndices = new List<int>();
            vector.CollectionChanged += (sender, e) => { if (e.Action == NotifyCollectionChangedAction.Add) { collectionChangedIndices.Add(e.NewStartingIndex); } };

            vector.OnItemsAdded(5, 10);

            CollectionAssert.AreEqual(new[] { 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }, collectionChangedIndices);
        }

        [TestMethod]
        public void OnItemsAdded_RaisesCollectionChanged_ForAnyFetchingItemsThatHaveMoved()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();
            int firstCountResult = vector.Count;
            vector.CompleteGetCount(42);

            int itemPlaceholder = vector[5];

            List<int> collectionChangedIndices = new List<int>();
            vector.CollectionChanged += (sender, e) => { if (e.Action == NotifyCollectionChangedAction.Replace) { collectionChangedIndices.Add(e.NewStartingIndex); } };

            vector.OnItemsAdded(5, 10);

            CollectionAssert.AreEqual(new[] { 15 }, collectionChangedIndices);
        }

        [TestMethod]
        public void OnItemsAdded_RaisesCollectionChanged_NotForAnyFetchingItemsThatHaveNotMoved()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();
            int firstCountResult = vector.Count;
            vector.CompleteGetCount(42);

            int itemPlaceholder = vector[5];

            List<int> collectionChangedIndices = new List<int>();
            vector.CollectionChanged += (sender, e) => { if (e.Action == NotifyCollectionChangedAction.Replace) { collectionChangedIndices.Add(e.NewStartingIndex); } };

            vector.OnItemsAdded(6, 10);

            CollectionAssert.AreEqual(new int[] { }, collectionChangedIndices);
        }

        [TestMethod]
        public void OnItemsRemoved_CountPropertyIsIncremented()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();
            int firstCountResult = vector.Count;
            vector.CompleteGetCount(42);

            vector.OnItemsRemoved(5, 10);

            Assert.AreEqual(32, vector.Count);
        }

        [TestMethod]
        public void OnItemsRemoved_CountIsNotNotRefetched()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();
            int firstCountResult = vector.Count;
            vector.CompleteGetCount(42);

            vector.OnItemsRemoved(5, 10);

            int count = vector.Count;

            Assert.AreEqual(1, vector.FetchCountCallCount);
        }

        [TestMethod]
        public void OnItemsRemoved_RaisesCountPropertyChanged()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();
            int firstCountResult = vector.Count;
            vector.CompleteGetCount(42);

            int propertyChangedCount = 0;
            vector.PropertyChanged += (sender, e) => { if (e.PropertyName == "Count") { propertyChangedCount++; } };

            vector.OnItemsRemoved(5, 10);

            Assert.AreEqual(1, propertyChangedCount);
        }

        [TestMethod]
        public void OnItemsRemoved_RaisesItemsPropertyChanged()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();
            int firstCountResult = vector.Count;
            vector.CompleteGetCount(42);

            int propertyChangedCount = 0;
            vector.PropertyChanged += (sender, e) => { if (e.PropertyName == "Item[]") { propertyChangedCount++; } };

            vector.OnItemsRemoved(5, 10);

            Assert.AreEqual(1, propertyChangedCount);
        }

        [TestMethod]
        public void OnItemsRemoved_RaisesCollectionChanged_ForRemovedItems()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();
            int firstCountResult = vector.Count;
            vector.CompleteGetCount(42);

            List<int> collectionChangedIndices = new List<int>();
            vector.CollectionChanged += (sender, e) => { if (e.Action == NotifyCollectionChangedAction.Remove) { collectionChangedIndices.Add(e.OldStartingIndex); } };

            vector.OnItemsRemoved(5, 10);

            CollectionAssert.AreEqual(new[] { 14, 13, 12, 11, 10, 9, 8, 7, 6, 5 }, collectionChangedIndices);
        }

        [TestMethod]
        public void OnItemsRemoved_RaisesCollectionChanged_ForAnyFetchingItemsThatHaveMoved()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();
            int firstCountResult = vector.Count;
            vector.CompleteGetCount(42);

            int itemPlaceholder = vector[5];

            List<int> collectionChangedIndices = new List<int>();
            vector.CollectionChanged += (sender, e) => { if (e.Action == NotifyCollectionChangedAction.Replace) { collectionChangedIndices.Add(e.NewStartingIndex); } };

            vector.OnItemsRemoved(1, 3);

            CollectionAssert.AreEqual(new[] { 2 }, collectionChangedIndices);
        }


        [TestMethod]
        public void OnItemsRemoved_RaisesCollectionChanged_NotForAnyFetchingItemsThatHaveBeenRemoved()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();
            int firstCountResult = vector.Count;
            vector.CompleteGetCount(42);

            int itemPlaceholder = vector[5];

            List<int> collectionChangedIndices = new List<int>();
            vector.CollectionChanged += (sender, e) => { if (e.Action == NotifyCollectionChangedAction.Replace) { collectionChangedIndices.Add(e.NewStartingIndex); } };

            vector.OnItemsRemoved(5, 3);

            CollectionAssert.AreEqual(new int[] { }, collectionChangedIndices);
        }

        [TestMethod]
        public void OnItemsRemoved_RaisesCollectionChanged_NotForAnyFetchingItemsThatHaveNotMoved()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();
            int firstCountResult = vector.Count;
            vector.CompleteGetCount(42);

            int itemPlaceholder = vector[5];

            List<int> collectionChangedIndices = new List<int>();
            vector.CollectionChanged += (sender, e) => { if (e.Action == NotifyCollectionChangedAction.Replace) { collectionChangedIndices.Add(e.NewStartingIndex); } };

            vector.OnItemsRemoved(6, 3);

            CollectionAssert.AreEqual(new int[] { }, collectionChangedIndices);
        }

        // *** Behavior Tests ***

        [TestMethod]
        public void ReturnOfCountAsyncFiresPropertyChanged()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();
            int firstCountResult = vector.Count;

            bool propertyChanged = false;
            vector.PropertyChanged += (sender, e) => { if (e.PropertyName == "Count") { propertyChanged = true; } };

            vector.CompleteGetCount(42);

            Assert.AreEqual(true, propertyChanged);
        }

        [TestMethod]
        public void ReturnOfCountAsyncFiresCollectionChanged()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();
            int firstCountResult = vector.Count;

            bool collectionChanged = false;
            vector.CollectionChanged += (sender, e) => { if (e.Action == NotifyCollectionChangedAction.Reset) { collectionChanged = true; } };

            vector.CompleteGetCount(42);

            Assert.AreEqual(true, collectionChanged);
        }

        [TestMethod]
        public void ReturnOfCountSyncDoesNotFirePropertyChanged()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();
            vector.CompleteGetCount(42);

            bool propertyChanged = false;
            vector.PropertyChanged += (sender, e) => { if (e.PropertyName == "Count") { propertyChanged = true; } };

            int count = vector.Count;

            Assert.AreEqual(false, propertyChanged);
        }

        [TestMethod]
        public void ReturnOfCountDoesNotFireEventsIfCountHasNotChanged()
        {
            MockVirtualizingVector vector = new MockVirtualizingVector();
            int firstCountResult = vector.Count;

            bool propertyChanged = false;
            vector.PropertyChanged += (sender, e) => { if (e.PropertyName == "Count") { propertyChanged = true; } };

            bool collectionChanged = false;
            vector.CollectionChanged += (sender, e) => { if (e.Action == NotifyCollectionChangedAction.Reset) { collectionChanged = true; } };

            vector.CompleteGetCount(0);

            Assert.AreEqual(false, propertyChanged);
            Assert.AreEqual(false, collectionChanged);
        }

        // *** Private sub-classes ***

        private class MockVirtualizingVector : VirtualizingVectorBase<int>
        {
            // *** Fields ***

            private TaskCompletionSource<int> getCountCompletion = new TaskCompletionSource<int>();
            private TaskCompletionSource<int> getItemCompletion = new TaskCompletionSource<int>();

            // *** Properties ***

            public int FetchCountCallCount { get; set; }

            // *** Methods ***

            public void CompleteGetCount(int count, bool clearCachedValue = false)
            {
                TaskCompletionSource<int> localGetCountCompletion = getCountCompletion;

                if (clearCachedValue)
                    getCountCompletion = new TaskCompletionSource<int>();

                localGetCountCompletion.SetResult(count);
            }

            public void CompleteGetItem(int item, bool clearCachedValue = false)
            {
                TaskCompletionSource<int> localGetItemCompletion = getItemCompletion;

                if (clearCachedValue)
                    getItemCompletion = new TaskCompletionSource<int>();

                localGetItemCompletion.SetResult(item);
            }

            public new void OnItemsAdded(int index, int count)
            {
                base.OnItemsAdded(index, count);
            }

            public new void OnItemsRemoved(int index, int count)
            {
                base.OnItemsRemoved(index, count);
            }

            public new void Reset()
            {
                base.Reset();
            }

            // *** Overriden Methods ***

            protected override Task<int> GetCountAsync()
            {
                FetchCountCallCount++;

                return getCountCompletion.Task;
            }

            protected override Task<int> GetItemAsync(int index)
            {
                if (index == 5)
                    return getItemCompletion.Task;
                else
                    return Task.FromResult<int>(1);
            }

            protected override int GetIndexOf(int item)
            {
                if (item == 15)
                    return 5;
                else
                    return -1;
            }
        }

        private class MockNullVirtualizingVector : VirtualizingVectorBase<object>
        {
            // *** Overriden Methods ***

            protected override Task<int> GetCountAsync()
            {
                return Task.FromResult(42);
            }

            protected override Task<object> GetItemAsync(int index)
            {
                return Task.FromResult<object>(null);
            }

            protected override int GetIndexOf(object item)
            {
                throw new NotImplementedException();
            }
        }
    }
}

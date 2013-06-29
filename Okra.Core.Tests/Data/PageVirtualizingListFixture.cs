using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Okra.Data;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace Okra.Tests.Data
{
    [TestClass]
    public class PageVirtualizingListFixture
    {
        // *** Constructor Tests ***

        [TestMethod]
        public void Constructor_Void_CollectionIsEmpty()
        {
            PageVirtualizingList<int> list = new PageVirtualizingList<int>();

            Assert.AreEqual(0, list.Count);
        }

        // *** Property Tests ***

        [TestMethod]
        public void Indexer_SettingAndGettingReturnsSameElement_StartOfPage()
        {
            PageVirtualizingList<int> list = new PageVirtualizingList<int>();
            list.UpdateCount(22, 10);
            list[10] = 42;

            Assert.AreEqual(42, list[10]);
        }

        [TestMethod]
        public void Indexer_SettingAndGettingReturnsSameElement_MiddleOfPage()
        {
            PageVirtualizingList<int> list = new PageVirtualizingList<int>();
            list.UpdateCount(22, 10);
            list[15] = 42;

            Assert.AreEqual(42, list[15]);
        }

        [TestMethod]
        public void Indexer_SettingAndGettingReturnsSameElement_EndOfPage()
        {
            PageVirtualizingList<int> list = new PageVirtualizingList<int>();
            list.UpdateCount(22, 10);
            list[19] = 42;

            Assert.AreEqual(42, list[19]);
        }

        [TestMethod]
        public void Indexer_Setter_Exception_IndexIsLessThanZero()
        {
            PageVirtualizingList<int> list = new PageVirtualizingList<int>();
            list.UpdateCount(22, 10);

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => list[-1] = 100);
        }

        [TestMethod]
        public void Indexer_Setter_Exception_IndexIsEqualToCount()
        {
            PageVirtualizingList<int> list = new PageVirtualizingList<int>();
            list.UpdateCount(22, 10);

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => list[22] = 100);
        }

        [TestMethod]
        public void Indexer_Getter_ReturnsPlaceholderBeforeSetElement()
        {
            PageVirtualizingList<int> list = new PageVirtualizingList<int>();
            list.UpdateCount(22, 10);
            list[10] = 42;

            Assert.AreEqual(0, list[9]);
        }

        [TestMethod]
        public void Indexer_Getter_ReturnsPlaceholderAfterSetElement()
        {
            PageVirtualizingList<int> list = new PageVirtualizingList<int>();
            list.UpdateCount(22, 10);
            list[10] = 42;

            Assert.AreEqual(0, list[11]);
        }

        [TestMethod]
        public void Indexer_Getter_ReturnsPlaceholderFarAfterSetElement()
        {
            PageVirtualizingList<int> list = new PageVirtualizingList<int>();
            list.UpdateCount(2000, 100);
            list[10] = 42;

            Assert.AreEqual(0, list[1990]);
        }

        [TestMethod]
        public void Indexer_Getter_Exception_IndexIsLessThanZero()
        {
            PageVirtualizingList<int> list = new PageVirtualizingList<int>();
            list.UpdateCount(22, 10);

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => list[-1]);
        }

        [TestMethod]
        public void Indexer_Getter_Exception_IndexIsEqualToCount()
        {
            PageVirtualizingList<int> list = new PageVirtualizingList<int>();
            list.UpdateCount(22, 10);

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => list[22]);
        }

        [TestMethod]
        public void IsReadOnly_IsFalse()
        {
            PageVirtualizingList<int> list = new PageVirtualizingList<int>();

            Assert.AreEqual(false, list.IsReadOnly);
        }

        [TestMethod]
        public void PageCacheSize_IsInitiallyMaxValue()
        {
            PageVirtualizingList<int> list = new PageVirtualizingList<int>();

            Assert.AreEqual(int.MaxValue, list.PageCacheSize);
        }

        [TestMethod]
        public void PageCacheSize_SetterSetsValue()
        {
            PageVirtualizingList<int> list = new PageVirtualizingList<int>();

            list.PageCacheSize = 5;

            Assert.AreEqual(5, list.PageCacheSize);
        }

        [TestMethod]
        public void PageCacheSize_Exception_ValueIsZero()
        {
            PageVirtualizingList<int> list = new PageVirtualizingList<int>();

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => list.PageCacheSize = 0);
        }

        [TestMethod]
        public void PageCacheSize_Exception_ValueIsLessThanZero()
        {
            PageVirtualizingList<int> list = new PageVirtualizingList<int>();

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => list.PageCacheSize = -1);
        }

        // *** Method Tests ***

        [TestMethod]
        public void Add_AddsNewItemToList()
        {
            PageVirtualizingList<int> list = new PageVirtualizingList<int>();
            list.UpdateCount(22, 10);

            list.Add(123);

            Assert.AreEqual(23, list.Count);
            Assert.AreEqual(123, list[22]);
        }

        [TestMethod]
        public void Clear_ClearsTheList()
        {
            PageVirtualizingList<int> list = new PageVirtualizingList<int>();
            list.UpdateCount(22, 10);

            list.Clear();

            Assert.AreEqual(0, list.Count);
        }

        [TestMethod]
        public void Contains_ReturnsTrueIfItemInList()
        {
            PageVirtualizingList<int> list = new PageVirtualizingList<int>();
            list.UpdateCount(22, 10);
            list[2] = 8;
            list[10] = 42;

            bool result = list.Contains(42);

            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void Contains_ReturnsFalseIfNotInList()
        {
            PageVirtualizingList<int> list = new PageVirtualizingList<int>();
            list.UpdateCount(22, 10);
            list[2] = 8;
            list[10] = 42;

            bool result = list.Contains(15);

            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public void CopyTo_CopiesToDestinationArray()
        {
            PageVirtualizingList<int> list = new PageVirtualizingList<int>();
            list.UpdateCount(22, 3);
            list[2] = 8;
            list[10] = 42;

            int[] destination = new int[27];
            destination[1] = 1;
            destination[5] = 2;
            destination[24] = 3;

            list.CopyTo(destination, 2);

            CollectionAssert.AreEqual(new int[] { 0, 1, 0, 0, 8, 0, 0, 0, 0, 0, 0, 0, 42, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0 }, destination);
        }

        [TestMethod]
        public void IndexOf_ReturnsIndexOfItemInList()
        {
            PageVirtualizingList<int> list = new PageVirtualizingList<int>();
            list.UpdateCount(22, 10);
            list[2] = 8;
            list[10] = 42;

            int result = list.IndexOf(42);

            Assert.AreEqual(10, result);
        }

        [TestMethod]
        public void IndexOf_ReturnsMinusOneIfNotInList()
        {
            PageVirtualizingList<int> list = new PageVirtualizingList<int>();
            list.UpdateCount(22, 10);
            list[2] = 8;
            list[10] = 42;

            int result = list.IndexOf(15);

            Assert.AreEqual(-1, result);
        }

        [TestMethod]
        public void Insert_InsertsItemIntoList()
        {
            PageVirtualizingList<int> list = new PageVirtualizingList<int>();
            list.UpdateCount(20, 6);
            list[2] = 8;
            list[10] = 42;

            list.Insert(5, 123);

            Assert.AreEqual(21, list.Count);
            Assert.AreEqual(8, list[2]);
            Assert.AreEqual(123, list[5]);
            Assert.AreEqual(42, list[11]);
        }

        [TestMethod]
        public void Insert_Exception_IndexIsLessThanZero()
        {
            PageVirtualizingList<int> list = new PageVirtualizingList<int>();
            list.UpdateCount(20, 6);

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => list.Insert(-1, 123));
        }

        [TestMethod]
        public void Insert_Exception_IndexIsGreaterThanCount()
        {
            PageVirtualizingList<int> list = new PageVirtualizingList<int>();
            list.UpdateCount(20, 6);

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => list.Insert(21, 123));
        }

        [TestMethod]
        public void Remove_RemovesItemFromList()
        {
            PageVirtualizingList<int> list = new PageVirtualizingList<int>();
            list.UpdateCount(20, 6);
            list[2] = 8;
            list[5] = 10;
            list[10] = 42;

            list.Remove(10);

            Assert.AreEqual(19, list.Count);
            Assert.AreEqual(8, list[2]);
            Assert.AreEqual(42, list[9]);
        }

        [TestMethod]
        public void Remove_ReturnsTrueForItemInList()
        {
            PageVirtualizingList<int> list = new PageVirtualizingList<int>();
            list.UpdateCount(20, 6);
            list[2] = 8;
            list[5] = 10;
            list[10] = 42;

            bool result = list.Remove(10);

            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void Remove_ReturnsFalseForItemNotInList()
        {
            PageVirtualizingList<int> list = new PageVirtualizingList<int>();
            list.UpdateCount(20, 6);
            list[2] = 8;
            list[5] = 10;
            list[10] = 42;

            bool result = list.Remove(12);

            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public void RemoveAt_RemovesItemFromList()
        {
            PageVirtualizingList<int> list = new PageVirtualizingList<int>();
            list.UpdateCount(20, 6);
            list[2] = 8;
            list[5] = 10;
            list[10] = 42;

            list.RemoveAt(5);

            Assert.AreEqual(19, list.Count);
            Assert.AreEqual(8, list[2]);
            Assert.AreEqual(42, list[9]);
        }

        [TestMethod]
        public void RemoveAt_Exception_IndexIsLessThanZero()
        {
            PageVirtualizingList<int> list = new PageVirtualizingList<int>();
            list.UpdateCount(20, 6);

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => list.RemoveAt(-1));
        }

        [TestMethod]
        public void RemoveAt_Exception_IndexIsEqualToCount()
        {
            PageVirtualizingList<int> list = new PageVirtualizingList<int>();
            list.UpdateCount(20, 6);

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => list.RemoveAt(20));
        }

        [TestMethod]
        public void UpdateCount_UpdatesCountProperty()
        {
            PageVirtualizingList<int> list = new PageVirtualizingList<int>();
            list.UpdateCount(22, 10);

            Assert.AreEqual(22, list.Count);
        }

        [TestMethod]
        public void UpdateCount_UpdatesPageSizeProperty()
        {
            PageVirtualizingList<int> list = new PageVirtualizingList<int>();
            list.UpdateCount(22, 10);

            Assert.AreEqual(10, list.PageSize);
        }

        [TestMethod]
        public void UpdateCount_ChangingPageNumber_PreservesExistingValues()
        {
            PageVirtualizingList<int> list = new PageVirtualizingList<int>();
            list.UpdateCount(22, 10);

            list[15] = 42;

            list.UpdateCount(50, 10);

            Assert.AreEqual(42, list[15]);
        }

        [TestMethod]
        public void UpdateCount_ChangingPageSize_ClearsExistingValues()
        {
            PageVirtualizingList<int> list = new PageVirtualizingList<int>();
            list.UpdateCount(22, 10);

            list[12] = 42;

            list.UpdateCount(22, 5);

            for (int i = 0; i < list.Count; i++)
                Assert.AreEqual(0, list[i]);
        }

        [TestMethod]
        public void UpdateCount_Exception_CountIsLessThanZero()
        {
            PageVirtualizingList<int> list = new PageVirtualizingList<int>();
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => list.UpdateCount(-1, 10));
        }

        [TestMethod]
        public void UpdateCount_Exception_PageSizeIsLessThanOne()
        {
            PageVirtualizingList<int> list = new PageVirtualizingList<int>();
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => list.UpdateCount(22, 0));
        }

        [TestMethod]
        public void GetEnumerator_Generic_EnumeratesArray()
        {
            PageVirtualizingList<int> list = new PageVirtualizingList<int>();
            list.UpdateCount(22, 3);
            list[2] = 8;
            list[10] = 42;

            List<int> result = new List<int>();

            IEnumerator<int> enumerator = ((IEnumerable<int>)list).GetEnumerator();

            while (enumerator.MoveNext())
                result.Add(enumerator.Current);

            CollectionAssert.AreEqual(new int[] { 0, 0, 8, 0, 0, 0, 0, 0, 0, 0, 42, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, result);
        }

        [TestMethod]
        public void GetEnumerator_NonGeneric_EnumeratesArray()
        {
            PageVirtualizingList<int> list = new PageVirtualizingList<int>();
            list.UpdateCount(22, 3);
            list[2] = 8;
            list[10] = 42;

            List<int> result = new List<int>();

            IEnumerator enumerator = ((IEnumerable)list).GetEnumerator();

            while (enumerator.MoveNext())
                result.Add((int)enumerator.Current);

            CollectionAssert.AreEqual(new int[] { 0, 0, 8, 0, 0, 0, 0, 0, 0, 0, 42, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, result);
        }

        // *** Behaviour Tests ***

        [TestMethod]
        public void WithPageCacheSize_SettingMultipleItemsInSinglePage_CachesAllValues()
        {
            PageVirtualizingList<int> list = new PageVirtualizingList<int>() { PageCacheSize = 3 };
            list.UpdateCount(50, 10);

            list[11] = 5;
            list[12] = 10;
            list[13] = 15;
            list[14] = 20;
            list[15] = 25;
            list[16] = 30;
            list[17] = 35;

            Assert.AreEqual(5, list[11]);
            Assert.AreEqual(10, list[12]);
            Assert.AreEqual(15, list[13]);
            Assert.AreEqual(20, list[14]);
            Assert.AreEqual(25, list[15]);
            Assert.AreEqual(30, list[16]);
            Assert.AreEqual(35, list[17]);
        }

        [TestMethod]
        public void WithPageCacheSize_SettingValues_ClearsLeastRecentPages()
        {
            PageVirtualizingList<int> list = new PageVirtualizingList<int>() { PageCacheSize = 3 };
            list.UpdateCount(50, 10);

            list[15] = 5;
            list[25] = 10;
            list[35] = 15;
            list[45] = 20;
            list[10] = 25;

            Assert.AreEqual(25, list[10]);
            Assert.AreEqual(0, list[15]);
            Assert.AreEqual(0, list[25]);
            Assert.AreEqual(15, list[35]);
            Assert.AreEqual(20, list[45]);
        }

        [TestMethod]
        public void WithPageCacheSize_GettingValues_PushesPageUpInRecentlyUsedPagesList()
        {
            PageVirtualizingList<int> list = new PageVirtualizingList<int>() { PageCacheSize = 3 };
            list.UpdateCount(50, 10);

            list[15] = 5;
            list[25] = 10;
            list[35] = 15;
            list[45] = 20;
            int item25 = list[25];
            list[10] = 25;

            Assert.AreEqual(25, list[10]);
            Assert.AreEqual(0, list[15]);
            Assert.AreEqual(10, list[25]);
            Assert.AreEqual(0, list[35]);
            Assert.AreEqual(20, list[45]);
        }
    }
}

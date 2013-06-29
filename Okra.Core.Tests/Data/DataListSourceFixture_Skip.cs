using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Okra.Data;
using System.Threading.Tasks;
using System.Collections;

namespace Okra.Tests.Data
{
    [TestClass]
    public class DataListSourceFixture_Skip
    {
        // *** Constructor Method Tests ***

        [TestMethod]
        public void Skip_Exception_IfSourceIsNull()
        {
            MockDataListSource source = null;

            Assert.ThrowsException<ArgumentNullException>(() => source.Take(20));
        }

        [TestMethod]
        public void Skip_Exception_IfCountIsLessThanZero()
        {
            MockDataListSource source = new MockDataListSource(10);

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source.Take(-1));
        }

        // *** Method Tests ***

        [TestMethod]
        public void GetCount_ReturnsItemsFromListMinusCount()
        {
            MockDataListSource source = new MockDataListSource(10);
            IDataListSource<int> skipSource = source.Skip(4);

            int count = skipSource.GetCountAsync().Result;

            Assert.AreEqual(6, count);
        }

        [TestMethod]
        public void GetCount_ReturnsZeroIfSkipBeyondList()
        {
            MockDataListSource source = new MockDataListSource(10);
            IDataListSource<int> skipSource = source.Skip(15);

            int count = skipSource.GetCountAsync().Result;

            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public void GetItem_ReturnsCorrectItem()
        {
            MockDataListSource source = new MockDataListSource(10);
            IDataListSource<int> skipSource = source.Skip(5);

            int item = skipSource.GetItemAsync(2).Result;

            Assert.AreEqual(8, item);
        }

        [TestMethod]
        public void GetItem_Exception_IfIndexIsLessThanZero()
        {
            MockDataListSource source = new MockDataListSource(10);
            IDataListSource<int> skipSource = source.Skip(4);

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => skipSource.GetItemAsync(-1));
        }

        [TestMethod]
        public void GetItem_Exception_IfIndexIsGreaterThanLength()
        {
            MockDataListSource source = new MockDataListSource(10);
            IDataListSource<int> skipSource = source.Skip(4);

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => skipSource.GetItemAsync(6));
        }

        [TestMethod]
        public void IndexOf_ReturnsCorrectIndexIfInList()
        {
            MockDataListSource source = new MockDataListSource(10);
            IDataListSource<int> skipSource = source.Skip(5);

            int index = skipSource.IndexOf(8);

            Assert.AreEqual(2, index);
        }

        [TestMethod]
        public void IndexOf_ReturnsMinusOneIfOutsideOfWindow()
        {
            MockDataListSource source = new MockDataListSource(10);
            IDataListSource<int> skipSource = source.Skip(5);

            int index = skipSource.IndexOf(5);

            Assert.AreEqual(-1, index);
        }

        [TestMethod]
        public void IndexOf_ReturnsMinusOneIfOutsideOfSourceList()
        {
            MockDataListSource source = new MockDataListSource(10);
            IDataListSource<int> skipSource = source.Skip(5);

            int index = skipSource.IndexOf(20);

            Assert.AreEqual(-1, index);
        }

        [TestMethod]
        public void Subscribe_SubscribesToSource()
        {
            MockDataListSource source = new MockDataListSource(10);
            IDataListSource<int> skipSource = source.Skip(4);

            MockUpdatableCollection updatableCollection = new MockUpdatableCollection();
            IDisposable disposable = skipSource.Subscribe(updatableCollection);

            Assert.AreEqual(1, source.SubscribedUpdatableCollectionCount);
        }

        [TestMethod]
        public void Subscribe_SubscribesToSourceOnlyOnce()
        {
            MockDataListSource source = new MockDataListSource(10);
            IDataListSource<int> skipSource = source.Skip(4);

            MockUpdatableCollection updatableCollection1 = new MockUpdatableCollection();
            IDisposable disposable1 = skipSource.Subscribe(updatableCollection1);
            MockUpdatableCollection updatableCollection2 = new MockUpdatableCollection();
            IDisposable disposable2 = skipSource.Subscribe(updatableCollection2);

            Assert.AreEqual(1, source.SubscribedUpdatableCollectionCount);
        }

        [TestMethod]
        public void Subscribe_NonLastDisposableDoesNotUnsubscribesFromSource()
        {
            MockDataListSource source = new MockDataListSource(10);
            IDataListSource<int> skipSource = source.Skip(4);

            MockUpdatableCollection updatableCollection1 = new MockUpdatableCollection();
            IDisposable disposable1 = skipSource.Subscribe(updatableCollection1);
            MockUpdatableCollection updatableCollection2 = new MockUpdatableCollection();
            IDisposable disposable2 = skipSource.Subscribe(updatableCollection2);
            disposable1.Dispose();

            Assert.AreEqual(1, source.SubscribedUpdatableCollectionCount);
        }

        [TestMethod]
        public void Subscribe_LastDisposableUnsubscribesFromSource()
        {
            MockDataListSource source = new MockDataListSource(10);
            IDataListSource<int> skipSource = source.Skip(4);

            MockUpdatableCollection updatableCollection = new MockUpdatableCollection();
            IDisposable disposable = skipSource.Subscribe(updatableCollection);
            disposable.Dispose();

            Assert.AreEqual(0, source.SubscribedUpdatableCollectionCount);
        }

        // *** Update Tests ***

        [TestMethod]
        public async Task Update_Reset_ForwardsUpdate()
        {
            MockDataListSource source = new MockDataListSource(10);
            IDataListSource<int> skipSource = source.Skip(5);
            int sourceCount = await skipSource.GetCountAsync();
            MockUpdatableCollection updatableCollection = new MockUpdatableCollection();
            skipSource.Subscribe(updatableCollection);

            source.PostUpdate(new DataListUpdate(DataListUpdateAction.Reset));

            CollectionAssert.AreEqual(new[] { new DataListUpdate(DataListUpdateAction.Reset) }, (ICollection)updatableCollection.Updates);
        }

        [TestMethod]
        public void Update_Reset_ReturnsCorrectCountForShortCollections()
        {
            MockDataListSource source = new MockDataListSource(10);
            IDataListSource<int> takeSource = source.Skip(5);

            int firstCount = takeSource.GetCountAsync().Result;

            source.Count = 3;
            source.PostUpdate(new DataListUpdate(DataListUpdateAction.Reset));

            int secondCount = takeSource.GetCountAsync().Result;

            Assert.AreEqual(0, secondCount);
        }

        [TestMethod]
        public void Update_Reset_ReturnsCorrectCountForLongCollections()
        {
            MockDataListSource source = new MockDataListSource(3);
            IDataListSource<int> takeSource = source.Skip(5);

            int firstCount = takeSource.GetCountAsync().Result;

            source.Count = 10;
            source.PostUpdate(new DataListUpdate(DataListUpdateAction.Reset));

            int secondCount = takeSource.GetCountAsync().Result;

            Assert.AreEqual(5, secondCount);
        }

        [TestMethod]
        public async Task Update_Add_ForwardsUpdate_AddInBounds()
        {
            // Index:   0123456789|01234567890123456789
            // Before:  AAAAAAAAAA AAAA
            // After:   AAAAAAAAAA ABBAAA

            MockDataListSource source = new MockDataListSource(14);
            IDataListSource<int> skipSource = source.Skip(10);
            int sourceCount = await skipSource.GetCountAsync();
            MockUpdatableCollection updatableCollection = new MockUpdatableCollection();
            skipSource.Subscribe(updatableCollection);

            source.PostUpdate(new DataListUpdate(DataListUpdateAction.Add, 11, 2));

            CollectionAssert.AreEqual(new[] { new DataListUpdate(DataListUpdateAction.Add, 1, 2) }, (ICollection)updatableCollection.Updates);
        }

        [TestMethod]
        public async Task Update_Add_ForwardsUpdate_AddOverlapsBounds()
        {
            // Index:   0123456789|01234567890123456789
            // Before:  AAAAAAAAAA AAAA
            // After:   AAAAAAAAAB BAAAAA

            MockDataListSource source = new MockDataListSource(14);
            IDataListSource<int> skipSource = source.Skip(10);
            int sourceCount = await skipSource.GetCountAsync();
            MockUpdatableCollection updatableCollection = new MockUpdatableCollection();
            skipSource.Subscribe(updatableCollection);

            source.PostUpdate(new DataListUpdate(DataListUpdateAction.Add, 9, 2));

            CollectionAssert.AreEqual(new[] { new DataListUpdate(DataListUpdateAction.Add, 0, 2) }, (ICollection)updatableCollection.Updates);
        }

        [TestMethod]
        public async Task Update_Add_ForwardsUpdate_AddBeforeBounds()
        {
            // Index:   0123456789|01234567890123456789
            // Before:  AAAAAAAAAA AAAA
            // After:   AABBAAAAAA AAAAAA

            MockDataListSource source = new MockDataListSource(14);
            IDataListSource<int> skipSource = source.Skip(10);
            int sourceCount = await skipSource.GetCountAsync();
            MockUpdatableCollection updatableCollection = new MockUpdatableCollection();
            skipSource.Subscribe(updatableCollection);

            source.PostUpdate(new DataListUpdate(DataListUpdateAction.Add, 2, 2));

            CollectionAssert.AreEqual(new[] { new DataListUpdate(DataListUpdateAction.Add, 0, 2) }, (ICollection)updatableCollection.Updates);
        }

        [TestMethod]
        public async Task Update_Add_IgnoresUpdate_CollectionStillOutOfBounds()
        {
            // Index:   0123456789|01234567890123456789
            // Before:  AAAAA
            // After:   AABBAAA

            MockDataListSource source = new MockDataListSource(5);
            IDataListSource<int> skipSource = source.Skip(10);
            int sourceCount = await skipSource.GetCountAsync();
            MockUpdatableCollection updatableCollection = new MockUpdatableCollection();
            skipSource.Subscribe(updatableCollection);

            source.PostUpdate(new DataListUpdate(DataListUpdateAction.Add, 2, 2));

            CollectionAssert.AreEqual(new DataListUpdate[] { }, (ICollection)updatableCollection.Updates);
        }

        [TestMethod]
        public async Task Update_Add_ForwardsUpdate_ExtendsCollectionIntoOfBounds()
        {
            // Index:   0123456789|01234567890123456789
            // Before:  AAAAAAAAA
            // After:   AABBAAAAAA A

            MockDataListSource source = new MockDataListSource(9);
            IDataListSource<int> skipSource = source.Skip(10);
            int sourceCount = await skipSource.GetCountAsync();
            MockUpdatableCollection updatableCollection = new MockUpdatableCollection();
            skipSource.Subscribe(updatableCollection);

            source.PostUpdate(new DataListUpdate(DataListUpdateAction.Add, 2, 2));

            CollectionAssert.AreEqual(new[] { new DataListUpdate(DataListUpdateAction.Add, 0, 1) }, (ICollection)updatableCollection.Updates);
        }

        [TestMethod]
        public async Task Update_Remove_ForwardsUpdate_RemoveInBounds()
        {
            // Index:   0123456789|01234567890123456789
            // Before:  AAAAAAAAAA AAAA
            // After:   AAAAAAAAAA A  A

            MockDataListSource source = new MockDataListSource(14);
            IDataListSource<int> skipSource = source.Skip(10);
            int sourceCount = await skipSource.GetCountAsync();
            MockUpdatableCollection updatableCollection = new MockUpdatableCollection();
            skipSource.Subscribe(updatableCollection);

            source.PostUpdate(new DataListUpdate(DataListUpdateAction.Remove, 11, 2));

            CollectionAssert.AreEqual(new[] { new DataListUpdate(DataListUpdateAction.Remove, 1, 2) }, (ICollection)updatableCollection.Updates);
        }

        [TestMethod]
        public async Task Update_Remove_ForwardsUpdate_RemoveOverlapsBounds()
        {
            // Index:   0123456789|01234567890123456789
            // Before:  AAAAAAAAAA AAAA
            // After:   AAAAAAAAA   AAA

            MockDataListSource source = new MockDataListSource(14);
            IDataListSource<int> skipSource = source.Skip(10);
            int sourceCount = await skipSource.GetCountAsync();
            MockUpdatableCollection updatableCollection = new MockUpdatableCollection();
            skipSource.Subscribe(updatableCollection);

            source.PostUpdate(new DataListUpdate(DataListUpdateAction.Remove, 9, 2));

            CollectionAssert.AreEqual(new[] { new DataListUpdate(DataListUpdateAction.Remove, 0, 2) }, (ICollection)updatableCollection.Updates);
        }

        [TestMethod]
        public async Task Update_Remove_ForwardsUpdate_RemoveBeforeBounds()
        {
            // Index:   0123456789|01234567890123456789
            // Before:  AAAAAAAAAA AAAA
            // After:   AA  AAAAAA AAAA

            MockDataListSource source = new MockDataListSource(14);
            IDataListSource<int> skipSource = source.Skip(10);
            int sourceCount = await skipSource.GetCountAsync();
            MockUpdatableCollection updatableCollection = new MockUpdatableCollection();
            skipSource.Subscribe(updatableCollection);

            source.PostUpdate(new DataListUpdate(DataListUpdateAction.Remove, 2, 2));

            CollectionAssert.AreEqual(new[] { new DataListUpdate(DataListUpdateAction.Remove, 0, 2) }, (ICollection)updatableCollection.Updates);
        }

        [TestMethod]
        public async Task Update_Remove_IgnoresUpdate_CollectionStartsOutOfBounds()
        {
            // Index:   0123456789|01234567890123456789
            // Before:  AAAAA
            // After:   AA  A

            MockDataListSource source = new MockDataListSource(5);
            IDataListSource<int> skipSource = source.Skip(10);
            int sourceCount = await skipSource.GetCountAsync();
            MockUpdatableCollection updatableCollection = new MockUpdatableCollection();
            skipSource.Subscribe(updatableCollection);

            source.PostUpdate(new DataListUpdate(DataListUpdateAction.Remove, 2, 2));

            CollectionAssert.AreEqual(new DataListUpdate[] { }, (ICollection)updatableCollection.Updates);
        }

        [TestMethod]
        public async Task Update_Remove_ForwardsUpdate_ContractsCollectionOutOfBounds()
        {
            // Index:   0123456789|01234567890123456789
            // Before:  AAAAAAAAAA AA
            // After:   AA    AAAA AA

            MockDataListSource source = new MockDataListSource(12);
            IDataListSource<int> skipSource = source.Skip(10);
            int sourceCount = await skipSource.GetCountAsync();
            MockUpdatableCollection updatableCollection = new MockUpdatableCollection();
            skipSource.Subscribe(updatableCollection);

            source.PostUpdate(new DataListUpdate(DataListUpdateAction.Remove, 2, 4));

            CollectionAssert.AreEqual(new[] { new DataListUpdate(DataListUpdateAction.Remove, 0, 2) }, (ICollection)updatableCollection.Updates);
        }

        // *** Private Sub-Classes ***

        private class MockDataListSource : IDataListSource<int>
        {
            // *** Fields ***

            private List<IUpdatableCollection> updateSubscriptions = new List<IUpdatableCollection>();

            // *** Constructors ***

            public MockDataListSource(int count)
            {
                this.Count = count;
            }

            // *** Properties ***

            public int Count
            {
                get;
                set;
            }

            public int SubscribedUpdatableCollectionCount
            {
                get
                {
                    return updateSubscriptions.Count;
                }
            }

            // *** Methods ***

            public async Task<int> GetCountAsync()
            {
                await Task.Yield();

                return Count;
            }

            public Task<int> GetItemAsync(int index)
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException();

                return GetItemInternalAsync(index);
            }

            public async Task<int> GetItemInternalAsync(int index)
            {
                await Task.Yield();

                return index + 1;
            }

            public int IndexOf(int item)
            {
                if (item >= 1 && item <= Count)
                    return item - 1;
                else
                    return -1;
            }

            public void PostUpdate(DataListUpdate update)
            {
                foreach (IUpdatableCollection subscription in updateSubscriptions)
                    subscription.Update(update);
            }

            public IDisposable Subscribe(IUpdatableCollection collection)
            {
                updateSubscriptions.Add(collection);
                return new DelegateDisposable(() => updateSubscriptions.Remove(collection));
            }

            // *** Private sub-classes ***

            private class DelegateDisposable : IDisposable
            {
                // *** Fields ***

                private readonly Action disposeAction;
                private bool isDisposed;

                // *** Constructors ***

                public DelegateDisposable(Action disposeAction)
                {
                    this.disposeAction = disposeAction;
                }

                // *** Methods ***

                public void Dispose()
                {
                    if (!isDisposed)
                    {
                        disposeAction();
                        isDisposed = true;
                    }
                }
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
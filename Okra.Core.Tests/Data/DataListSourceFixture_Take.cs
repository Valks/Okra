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
    public class DataListSourceFixture_Take
    {
        // *** Constructor Method Tests ***

        [TestMethod]
        public void Take_Exception_IfSourceIsNull()
        {
            MockDataListSource source = null;
            Assert.ThrowsException<ArgumentNullException>(() => source.Take(20));
        }

        [TestMethod]
        public void Take_Exception_IfCountIsLessThanZero()
        {
            MockDataListSource source = new MockDataListSource(10);
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source.Take(-1));
        }

        // *** Method Tests ***

        [TestMethod]
        public void GetCount_ReturnsItemsFromListIfLess()
        {
            MockDataListSource source = new MockDataListSource(10);
            IDataListSource<int> takeSource = source.Take(20);

            int count = takeSource.GetCountAsync().Result;

            Assert.AreEqual(10, count);
        }

        [TestMethod]
        public void GetCount_ReturnsTakeCountIfLess()
        {
            MockDataListSource source = new MockDataListSource(10);
            IDataListSource<int> takeSource = source.Take(5);

            int count = takeSource.GetCountAsync().Result;

            Assert.AreEqual(5, count);
        }

        [TestMethod]
        public void GetItem_ReturnsCorrectItemIfLessThanCount()
        {
            MockDataListSource source = new MockDataListSource(10);
            IDataListSource<int> takeSource = source.Take(5);

            int item = takeSource.GetItemAsync(4).Result;

            Assert.AreEqual(5, item);
        }

        [TestMethod]
        public void GetItem_Exception_IfIndexIsLessThanZero()
        {
            MockDataListSource source = new MockDataListSource(10);
            IDataListSource<int> takeSource = source.Take(4);

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => takeSource.GetItemAsync(-1));
        }

        [TestMethod]
        public void GetItem_Exception_IfIndexIsGreaterThanLength()
        {
            MockDataListSource source = new MockDataListSource(10);
            IDataListSource<int> takeSource = source.Take(4);

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => takeSource.GetItemAsync(4));
        }

        [TestMethod]
        public void IndexOf_ReturnsCorrectIndexIfInList()
        {
            MockDataListSource source = new MockDataListSource(10);
            IDataListSource<int> takeSource = source.Take(5);

            int index = takeSource.IndexOf(5);

            Assert.AreEqual(4, index);
        }

        [TestMethod]
        public void IndexOf_ReturnsMinusOneIfOutsideOfWindow()
        {
            MockDataListSource source = new MockDataListSource(10);
            IDataListSource<int> takeSource = source.Take(4);

            int index = takeSource.IndexOf(5);

            Assert.AreEqual(-1, index);
        }

        [TestMethod]
        public void IndexOf_ReturnsMinusOneIfOutsideOfSourceList()
        {
            MockDataListSource source = new MockDataListSource(10);
            IDataListSource<int> takeSource = source.Take(4);

            int index = takeSource.IndexOf(20);

            Assert.AreEqual(-1, index);
        }

        [TestMethod]
        public void Subscribe_SubscribesToSource()
        {
            MockDataListSource source = new MockDataListSource(10);
            IDataListSource<int> skipSource = source.Take(5);

            MockUpdatableCollection updatableCollection = new MockUpdatableCollection();
            IDisposable disposable = skipSource.Subscribe(updatableCollection);

            Assert.AreEqual(1, source.SubscribedUpdatableCollectionCount);
        }

        [TestMethod]
        public void Subscribe_SubscribesToSourceOnlyOnce()
        {
            MockDataListSource source = new MockDataListSource(10);
            IDataListSource<int> skipSource = source.Take(5);

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
            IDataListSource<int> skipSource = source.Take(5);

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
            IDataListSource<int> skipSource = source.Take(5);

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
            IDataListSource<int> skipSource = source.Take(5);
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
            IDataListSource<int> takeSource = source.Take(5);

            int firstCount = takeSource.GetCountAsync().Result;

            source.Count = 3;
            source.PostUpdate(new DataListUpdate(DataListUpdateAction.Reset));

            int secondCount = takeSource.GetCountAsync().Result;

            Assert.AreEqual(3, secondCount);
        }

        [TestMethod]
        public void Update_Reset_ReturnsCorrectCountForLongCollections()
        {
            MockDataListSource source = new MockDataListSource(3);
            IDataListSource<int> takeSource = source.Take(5);

            int firstCount = takeSource.GetCountAsync().Result;

            source.Count = 10;
            source.PostUpdate(new DataListUpdate(DataListUpdateAction.Reset));

            int secondCount = takeSource.GetCountAsync().Result;

            Assert.AreEqual(5, secondCount);
        }

        [TestMethod]
        public async Task Update_Add_ForwardsUpdate_StillInBounds()
        {
            // Index:   01234567890123456789|0123456789
            // Before:  AAAAAAAAAA
            // After:   AABBBBBAAAAAAAA

            MockDataListSource source = new MockDataListSource(10);
            IDataListSource<int> skipSource = source.Take(20);
            int sourceCount = await skipSource.GetCountAsync();
            MockUpdatableCollection updatableCollection = new MockUpdatableCollection();
            skipSource.Subscribe(updatableCollection);

            source.PostUpdate(new DataListUpdate(DataListUpdateAction.Add, 2, 5));

            CollectionAssert.AreEqual(new[] { new DataListUpdate(DataListUpdateAction.Add, 2, 5) }, (ICollection)updatableCollection.Updates);
        }

        [TestMethod]
        public async Task Update_Add_IgnoresUpdate_UpdateOutsideOfBounds()
        {
            // Index:   0123456789|01234567890123456789
            // Before:  AAAAAAAAAA|AAAAAAAAAA
            // After:   AAAAAAAAAA|AABBBBBAAA

            MockDataListSource source = new MockDataListSource(20);
            IDataListSource<int> skipSource = source.Take(10);
            int sourceCount = await skipSource.GetCountAsync();
            MockUpdatableCollection updatableCollection = new MockUpdatableCollection();
            skipSource.Subscribe(updatableCollection);

            source.PostUpdate(new DataListUpdate(DataListUpdateAction.Add, 12, 5));

            CollectionAssert.AreEqual(new DataListUpdate[] { }, (ICollection)updatableCollection.Updates);
        }

        [TestMethod]
        public async Task Update_Add_ForwardsUpdate_PushesSomeContentOutOfBounds()
        {
            // Index:   01234567890123456789|0123456789
            // Before:  AAAAAAAAAA
            // After:   AABBBBBBBBBBBBBBBAAA|AAAAA

            MockDataListSource source = new MockDataListSource(10);
            IDataListSource<int> skipSource = source.Take(20);
            int sourceCount = await skipSource.GetCountAsync();
            MockUpdatableCollection updatableCollection = new MockUpdatableCollection();
            skipSource.Subscribe(updatableCollection);

            source.PostUpdate(new DataListUpdate(DataListUpdateAction.Add, 2, 15));

            CollectionAssert.AreEqual(new[] { new DataListUpdate(DataListUpdateAction.Add, 2, 15), new DataListUpdate(DataListUpdateAction.Remove, 20, 5) }, (ICollection)updatableCollection.Updates);
        }

        [TestMethod]
        public async Task Update_Add_ForwardsUpdate_PushesAllContentOutOfBounds()
        {
            // Index:   01234567890123456789|0123456789
            // Before:  AAAAAAAAAA
            // After:   AABBBBBBBBBBBBBBBBBB|BBAAAAAAAA

            MockDataListSource source = new MockDataListSource(10);
            IDataListSource<int> skipSource = source.Take(20);
            int sourceCount = await skipSource.GetCountAsync();
            MockUpdatableCollection updatableCollection = new MockUpdatableCollection();
            skipSource.Subscribe(updatableCollection);

            source.PostUpdate(new DataListUpdate(DataListUpdateAction.Add, 2, 20));

            CollectionAssert.AreEqual(new[] { new DataListUpdate(DataListUpdateAction.Add, 2, 18), new DataListUpdate(DataListUpdateAction.Remove, 20, 8) }, (ICollection)updatableCollection.Updates);
        }

        [TestMethod]
        public async Task Update_Add_ForwardsUpdate_PushesSomeClippedContentOutOfBounds()
        {
            // Index:   01234567890123456789|0123456789
            // Before:  AAAAAAAAAAAAAAAAAAAA|AAAAAAAAAA
            // After:   AABBBBBBBBBBBBBBBAAA|AAAAAAAAAA....

            MockDataListSource source = new MockDataListSource(30);
            IDataListSource<int> skipSource = source.Take(20);
            int sourceCount = await skipSource.GetCountAsync();
            MockUpdatableCollection updatableCollection = new MockUpdatableCollection();
            skipSource.Subscribe(updatableCollection);

            source.PostUpdate(new DataListUpdate(DataListUpdateAction.Add, 2, 15));

            CollectionAssert.AreEqual(new[] { new DataListUpdate(DataListUpdateAction.Add, 2, 15), new DataListUpdate(DataListUpdateAction.Remove, 20, 15) }, (ICollection)updatableCollection.Updates);
        }

        [TestMethod]
        public async Task Update_Add_ForwardsUpdate_PushesAllClippedContentOutOfBounds()
        {
            // Index:   0123456789|01234567890123456789
            // Before:  AAAAAAAAAA|AAAAAAAAAA
            // After:   AAAAABBBBB|BBBBBAAAAAAAAAAAAAAA

            MockDataListSource source = new MockDataListSource(20);
            IDataListSource<int> skipSource = source.Take(10);
            int sourceCount = await skipSource.GetCountAsync();
            MockUpdatableCollection updatableCollection = new MockUpdatableCollection();
            skipSource.Subscribe(updatableCollection);

            source.PostUpdate(new DataListUpdate(DataListUpdateAction.Add, 5, 10));

            CollectionAssert.AreEqual(new[] { new DataListUpdate(DataListUpdateAction.Add, 5, 5), new DataListUpdate(DataListUpdateAction.Remove, 10, 5) }, (ICollection)updatableCollection.Updates);
        }

        [TestMethod]
        public async Task Update_Remove_ForwardsUpdate_StillInBounds()
        {
            // Index:   01234567890123456789|0123456789
            // Before:  AAAAAAAAAA
            // After:   AA     AAA

            MockDataListSource source = new MockDataListSource(10);
            IDataListSource<int> skipSource = source.Take(20);
            int sourceCount = await skipSource.GetCountAsync();
            MockUpdatableCollection updatableCollection = new MockUpdatableCollection();
            skipSource.Subscribe(updatableCollection);

            source.PostUpdate(new DataListUpdate(DataListUpdateAction.Remove, 2, 5));

            CollectionAssert.AreEqual(new[] { new DataListUpdate(DataListUpdateAction.Remove, 2, 5) }, (ICollection)updatableCollection.Updates);
        }

        [TestMethod]
        public async Task Update_Remove_IgnoresUpdate_UpdateOutsideOfBounds()
        {
            // Index:   0123456789|01234567890123456789
            // Before:  AAAAAAAAAA|AAAAAAAAAA
            // After:   AAAAAAAAAA|AA     AAA

            MockDataListSource source = new MockDataListSource(20);
            IDataListSource<int> skipSource = source.Take(10);
            int sourceCount = await skipSource.GetCountAsync();
            MockUpdatableCollection updatableCollection = new MockUpdatableCollection();
            skipSource.Subscribe(updatableCollection);

            source.PostUpdate(new DataListUpdate(DataListUpdateAction.Remove, 12, 5));

            CollectionAssert.AreEqual(new DataListUpdate[] { }, (ICollection)updatableCollection.Updates);
        }

        [TestMethod]
        public async Task Update_Remove_ForwardsUpdate_PullsSomeContentFromOutOfBounds()
        {
            // Index:   0123456789|01234567890123456789
            // Before:  AAAAAAAAAA|AAAAAAAAAA
            // After:   AA     AAA AAAAA|AAAAA

            MockDataListSource source = new MockDataListSource(20);
            IDataListSource<int> skipSource = source.Take(10);
            int sourceCount = await skipSource.GetCountAsync();
            MockUpdatableCollection updatableCollection = new MockUpdatableCollection();
            skipSource.Subscribe(updatableCollection);

            source.PostUpdate(new DataListUpdate(DataListUpdateAction.Remove, 2, 5));

            CollectionAssert.AreEqual(new[] { new DataListUpdate(DataListUpdateAction.Remove, 2, 5), new DataListUpdate(DataListUpdateAction.Add, 5, 5) }, (ICollection)updatableCollection.Updates);
        }

        [TestMethod]
        public async Task Update_Remove_ForwardsUpdate_PullsAllContentFromOutOfBounds()
        {
            // Index:   0123456789|01234567890123456789
            // Before:  AAAAAAAAAA|AA
            // After:   AA     AAA AA

            MockDataListSource source = new MockDataListSource(12);
            IDataListSource<int> skipSource = source.Take(10);
            int sourceCount = await skipSource.GetCountAsync();
            MockUpdatableCollection updatableCollection = new MockUpdatableCollection();
            skipSource.Subscribe(updatableCollection);

            source.PostUpdate(new DataListUpdate(DataListUpdateAction.Remove, 2, 5));

            CollectionAssert.AreEqual(new[] { new DataListUpdate(DataListUpdateAction.Remove, 2, 5), new DataListUpdate(DataListUpdateAction.Add, 5, 2) }, (ICollection)updatableCollection.Updates);
        }

        [TestMethod]
        public async Task Update_Remove_ForwardsUpdate_RemoveItemsGoOutOfBoundsWithItemsRemaining()
        {
            // Index:   0123456789|01234567890123456789
            // Before:  AAAAAAAAAA|AAAAAAAAAA
            // After:   AAAAAAAA      AA|AAAAA

            MockDataListSource source = new MockDataListSource(20);
            IDataListSource<int> skipSource = source.Take(10);
            int sourceCount = await skipSource.GetCountAsync();
            MockUpdatableCollection updatableCollection = new MockUpdatableCollection();
            skipSource.Subscribe(updatableCollection);

            source.PostUpdate(new DataListUpdate(DataListUpdateAction.Remove, 8, 5));

            CollectionAssert.AreEqual(new[] { new DataListUpdate(DataListUpdateAction.Remove, 8, 2), new DataListUpdate(DataListUpdateAction.Add, 8, 2) }, (ICollection)updatableCollection.Updates);
        }

        [TestMethod]
        public async Task Update_Remove_ForwardsUpdate_RemoveItemsGoOutOfBoundsToEndOfCollection()
        {
            // Index:   0123456789|01234567890123456789
            // Before:  AAAAAAAAAA|AAAAAAAAAA
            // After:   AAAAAAAA

            MockDataListSource source = new MockDataListSource(20);
            IDataListSource<int> skipSource = source.Take(10);
            int sourceCount = await skipSource.GetCountAsync();
            MockUpdatableCollection updatableCollection = new MockUpdatableCollection();
            skipSource.Subscribe(updatableCollection);

            source.PostUpdate(new DataListUpdate(DataListUpdateAction.Remove, 8, 12));

            CollectionAssert.AreEqual(new[] { new DataListUpdate(DataListUpdateAction.Remove, 8, 2) }, (ICollection)updatableCollection.Updates);
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
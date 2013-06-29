using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Okra.Data;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace Okra.Tests.Data
{
    [TestClass]
    public class DataListUpdateFixture
    {
        [TestMethod]
        public void Constructor_Reset_SetsActionProperty()
        {
            DataListUpdate update = new DataListUpdate(DataListUpdateAction.Reset);

            Assert.AreEqual(DataListUpdateAction.Reset, update.Action);
        }

        [TestMethod]
        public void Constructor_Reset_Exception_IfSuppliesIndexAndCount()
        {
            Assert.ThrowsException<InvalidOperationException>(() => new DataListUpdate(DataListUpdateAction.Reset, 2, 5));
        }

        [TestMethod]
        public void Constructor_Add_SetsActionProperty()
        {
            DataListUpdate update = new DataListUpdate(DataListUpdateAction.Add, 2, 5);

            Assert.AreEqual(DataListUpdateAction.Add, update.Action);
        }

        [TestMethod]
        public void Constructor_Add_SetsIndexProperty()
        {
            DataListUpdate update = new DataListUpdate(DataListUpdateAction.Add, 2, 5);

            Assert.AreEqual(2, update.Index);
        }

        [TestMethod]
        public void Constructor_Add_SetsCountProperty()
        {
            DataListUpdate update = new DataListUpdate(DataListUpdateAction.Add, 2, 5);

            Assert.AreEqual(5, update.Count);
        }

        [TestMethod]
        public void Constructor_Add_Exception_IfDoesNotSupplyIndexAndCount()
        {
            Assert.ThrowsException<InvalidOperationException>(() => new DataListUpdate(DataListUpdateAction.Add));
        }

        [TestMethod]
        public void Constructor_Add_Exception_IfIndexIsLessThanZero()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new DataListUpdate(DataListUpdateAction.Add, -1, 5));
        }

        [TestMethod]
        public void Constructor_Add_Exception_IfCountIsLessThanZero()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new DataListUpdate(DataListUpdateAction.Add, 2, -1));
        }

        [TestMethod]
        public void Constructor_Add_Exception_IfCountIsZero()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new DataListUpdate(DataListUpdateAction.Add, 2, 0));
        }

        [TestMethod]
        public void Constructor_Remove_SetsActionProperty()
        {
            DataListUpdate update = new DataListUpdate(DataListUpdateAction.Remove, 2, 5);

            Assert.AreEqual(DataListUpdateAction.Remove, update.Action);
        }

        [TestMethod]
        public void Constructor_Remove_SetsIndexProperty()
        {
            DataListUpdate update = new DataListUpdate(DataListUpdateAction.Remove, 2, 5);

            Assert.AreEqual(2, update.Index);
        }

        [TestMethod]
        public void Constructor_Remove_SetsCountProperty()
        {
            DataListUpdate update = new DataListUpdate(DataListUpdateAction.Remove, 2, 5);

            Assert.AreEqual(5, update.Count);
        }

        [TestMethod]
        public void Constructor_Remove_Exception_IfDoesNotSupplyIndexAndCount()
        {
            Assert.ThrowsException<InvalidOperationException>(() => new DataListUpdate(DataListUpdateAction.Remove));
        }

        [TestMethod]
        public void Constructor_Remove_Exception_IfIndexIsLessThanZero()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new DataListUpdate(DataListUpdateAction.Remove, -1, 5));
        }

        [TestMethod]
        public void Constructor_Remove_Exception_IfCountIsLessThanZero()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new DataListUpdate(DataListUpdateAction.Remove, 2, -1));
        }

        [TestMethod]
        public void Constructor_Remove_Exception_IfCountIsZero()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new DataListUpdate(DataListUpdateAction.Remove, 2, 0));
        }
    }
}

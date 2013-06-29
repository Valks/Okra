using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Okra.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Okra.Core.Tests.Navigation
{
    [TestClass]
    public class NavigationEntryStateFixture
    {
        // *** Property Tests ***

        [TestMethod]
        public void PageName_IsSetByConstructor()
        {
            byte[] argumentsData = new byte[] { 1, 2, 3 };
            byte[] stateData = new byte[] { 2, 3, 4 };
            NavigationEntryState state = new NavigationEntryState("Page name", argumentsData, stateData);

            Assert.AreEqual("Page name", state.PageName);
        }

        [TestMethod]
        public void ArgumentsData_IsSetByConstructor()
        {
            byte[] argumentsData = new byte[] { 1, 2, 3 };
            byte[] stateData = new byte[] { 2, 3, 4 };
            NavigationEntryState state = new NavigationEntryState("Page name", argumentsData, stateData);

            Assert.AreEqual(argumentsData, state.ArgumentsData);
        }

        [TestMethod]
        public void StateData_IsSetByConstructor()
        {
            byte[] argumentsData = new byte[] { 1, 2, 3 };
            byte[] stateData = new byte[] { 2, 3, 4 };
            NavigationEntryState state = new NavigationEntryState("Page name", argumentsData, stateData);

            Assert.AreEqual(stateData, state.StateData);
        }
    }
}

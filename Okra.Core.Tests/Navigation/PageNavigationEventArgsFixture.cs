using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Okra.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;

namespace Okra.Core.Tests.Navigation
{
    [TestClass]
    public class PageNavigationEventArgsFixture
    {
        // *** Constructor Tests ***

        [TestMethod]
        public void Constructor_SetsPageProperty()
        {
            MockNavigationEntry navigationEntry = new MockNavigationEntry() { PageName = "SamplePage" };
            PageNavigationEventArgs eventArgs = new PageNavigationEventArgs(navigationEntry, NavigationMode.Forward);
            
            Assert.AreEqual(navigationEntry, eventArgs.Page);
        }

        [TestMethod]
        public void Constructor_SetsNavigationMode()
        {
            MockNavigationEntry navigationEntry = new MockNavigationEntry() { PageName = "SamplePage" };
            PageNavigationEventArgs eventArgs = new PageNavigationEventArgs(navigationEntry, NavigationMode.Forward);

            Assert.AreEqual(NavigationMode.Forward, eventArgs.NavigationMode);
        }

        [TestMethod]
        public void Constructor_Exception_PageIsNull()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                {
                    PageNavigationEventArgs eventArgs = new PageNavigationEventArgs(null, NavigationMode.Forward);
                });
        }

        [TestMethod]
        public void Constructor_Exception_InvalidNavigationMode()
        {
            MockNavigationEntry navigationEntry = new MockNavigationEntry() { PageName = "SamplePage" };

            Assert.ThrowsException<ArgumentException>(() =>
            {
                PageNavigationEventArgs eventArgs = new PageNavigationEventArgs(navigationEntry, (NavigationMode)100);
            });
        }

        // *** Private sub-classes ***

        private class MockNavigationEntry : INavigationEntry
        {

            // *** Properties ***

            public string PageName
            {
                get;
                set;
            }

            // *** Methods ***

            public IEnumerable<object> GetElements()
            {
                throw new NotImplementedException();
            }
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Okra.Navigation;
using Okra.Tests.Mocks;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace Okra.Tests.Navigation
{
    [TestClass]
    public class NavigationManagerExFixture
    {
        // *** Static Method Tests ***

        [TestMethod]
        public void NavigateTo_WithType_NavigatesToPageWithFullTypeName()
        {
            MockNavigationManager navigationManager = new MockNavigationManager();

            navigationManager.NavigateTo(typeof(NavigationManagerExFixture));

            CollectionAssert.AreEqual(new[] { new Tuple<string, object>("Okra.Tests.Navigation.NavigationManagerExFixture", null) },
                        (ICollection)navigationManager.NavigatedPages);
        }

        [TestMethod]
        public void NavigateTo_WithType_Exception_IfPageNameIsNull()
        {
            MockNavigationManager navigationManager = new MockNavigationManager();

            Assert.ThrowsException<ArgumentNullException>(() => navigationManager.NavigateTo((Type)null));
        }

        [TestMethod]
        public void NavigateTo_WithTypeAndParameter_NavigatesToPageWithFullTypeName()
        {
            MockNavigationManager navigationManager = new MockNavigationManager();

            navigationManager.NavigateTo(typeof(NavigationManagerExFixture), "Parameter");

            CollectionAssert.AreEqual(new[] { new Tuple<string, object>("Okra.Tests.Navigation.NavigationManagerExFixture", "Parameter") },
                        (ICollection)navigationManager.NavigatedPages);
        }

        [TestMethod]
        public void NavigateTo_WithTypeAndParameter_Exception_IfPageNameIsNull()
        {
            MockNavigationManager navigationManager = new MockNavigationManager();

            Assert.ThrowsException<ArgumentNullException>(() => navigationManager.NavigateTo((Type)null, "Parameter"));
        }
    }
}

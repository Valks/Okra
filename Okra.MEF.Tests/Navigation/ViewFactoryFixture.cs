﻿using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Okra.Navigation;
using System.Composition;
using System.Composition.Hosting.Core;
using Okra.Tests.Mocks;

namespace Okra.MEF.Tests.Navigation
{
    [TestClass]
    public class ViewFactoryFixture
    {
        // *** Method Tests ***

        [TestMethod]
        public void CreateView_CreatesNewPage_WithViewModel()
        {
            IViewFactory viewFactory = CreateViewFactory();
            INavigationContext navigationContext = CreateNavigationContext();

            IViewLifetimeContext lifetimeContext = viewFactory.CreateView("Page 2", navigationContext);
            object view = lifetimeContext.View;

            Assert.IsNotNull(view);
            Assert.IsInstanceOfType(view, typeof(MockPage));
            Assert.AreEqual("Page 2", ((MockPage)view).PageName);
        }

        [TestMethod]
        public void CreateView_CreatesNewPage_WithoutViewModel()
        {
            IViewFactory viewFactory = CreateViewFactory();
            INavigationContext navigationContext = CreateNavigationContext();

            IViewLifetimeContext lifetimeContext = viewFactory.CreateView("Page 3", navigationContext);
            object view = lifetimeContext.View;

            Assert.IsNotNull(view);
            Assert.IsInstanceOfType(view, typeof(MockPage));
            Assert.AreEqual("Page 3", ((MockPage)view).PageName);
        }

        [TestMethod]
        public void CreateView_CreatesNewViewModel_WithViewModel()
        {
            IViewFactory viewFactory = CreateViewFactory();
            INavigationContext navigationContext = CreateNavigationContext();

            IViewLifetimeContext lifetimeContext = viewFactory.CreateView("Page 2", navigationContext);
            object viewModel = lifetimeContext.ViewModel;

            Assert.IsNotNull(viewModel);
            Assert.IsInstanceOfType(viewModel, typeof(MockViewModel<string, string>));
            Assert.AreEqual("ViewModel 2", ((MockViewModel<string, string>)viewModel).Name);
        }

        [TestMethod]
        public void CreateView_ViewModelIsNull_WithoutViewModel()
        {
            IViewFactory viewFactory = CreateViewFactory();
            INavigationContext navigationContext = CreateNavigationContext();

            IViewLifetimeContext lifetimeContext = viewFactory.CreateView("Page 3", navigationContext);
            object viewModel = lifetimeContext.ViewModel;

            Assert.IsNull(viewModel);
        }

        [TestMethod]
        public void CreateView_SetsViewModel_WithViewModel()
        {
            IViewFactory viewFactory = CreateViewFactory();
            INavigationContext navigationContext = CreateNavigationContext();

            IViewLifetimeContext lifetimeContext = viewFactory.CreateView("Page 2", navigationContext);
            MockPage page = lifetimeContext.View as MockPage;
            object viewModel = lifetimeContext.ViewModel;

            Assert.AreEqual(viewModel, page.DataContext);
        }

        [TestMethod]
        public void CreateView_SetsViewModel_ToNullWithoutViewModel()
        {
            IViewFactory viewFactory = CreateViewFactory();
            INavigationContext navigationContext = CreateNavigationContext();

            IViewLifetimeContext lifetimeContext = viewFactory.CreateView("Page 3", navigationContext);
            MockPage page = lifetimeContext.View as MockPage;
            object viewModel = page.DataContext;

            Assert.IsNull(viewModel);
        }

        [TestMethod]
        public void CreateView_InjectsNavigationContextIntoProxy()
        {
            NavigationContextProxy proxy = new NavigationContextProxy();
            IViewFactory viewFactory = CreateViewFactory(proxy);
            INavigationContext navigationContext = CreateNavigationContext();
            
            IViewLifetimeContext lifetimeContext = viewFactory.CreateView("Page 2", navigationContext);

            Assert.AreEqual(navigationContext.GetCurrent(), proxy.GetCurrent());
        }

        [TestMethod]
        public void CreateView_ThrowsException_NoPageWithSpecifiedName()
        {
            IViewFactory viewFactory = CreateViewFactory();
            INavigationContext navigationContext = CreateNavigationContext();

            Assert.ThrowsException<InvalidOperationException>(() => viewFactory.CreateView("Page X", navigationContext));
        }

        [TestMethod]
        public void IsViewDefined_ReturnsTrue_SpecifiedPageExists()
        {
            IViewFactory viewFactory = CreateViewFactory();

            bool viewDefined = viewFactory.IsViewDefined("Page 2");

            Assert.AreEqual(true, viewDefined);
        }

        [TestMethod]
        public void IsViewDefined_ReturnsFalse_NoPageWithSpecifiedName()
        {
            IViewFactory viewFactory = CreateViewFactory();

            bool viewDefined = viewFactory.IsViewDefined("Page X");

            Assert.AreEqual(false, viewDefined);
        }

        // *** Behaviour Tests ***

        [TestMethod]
        public void DisposingViewLifetimeContext_DisposesCurrentPage_WithViewModel()
        {
            IViewFactory viewFactory = CreateViewFactory();
            INavigationContext navigationContext = CreateNavigationContext();

            IViewLifetimeContext lifetimeContext = viewFactory.CreateView("Page 2", navigationContext);
            MockPage page = lifetimeContext.View as MockPage;

            lifetimeContext.Dispose();

            Assert.AreEqual(true, page.IsDisposed);
        }

        [TestMethod]
        public void DisposingViewLifetimeContext_DisposesCurrentPage_WithoutViewModel()
        {
            IViewFactory viewFactory = CreateViewFactory();
            INavigationContext navigationContext = CreateNavigationContext();

            IViewLifetimeContext lifetimeContext = viewFactory.CreateView("Page 3", navigationContext);
            MockPage page = lifetimeContext.View as MockPage;

            lifetimeContext.Dispose();

            Assert.AreEqual(true, page.IsDisposed);
        }

        [TestMethod]
        public void DisposingViewLifetimeContext_DisposesCurrentViewModel()
        {
            IViewFactory viewFactory = CreateViewFactory();
            INavigationContext navigationContext = CreateNavigationContext();

            IViewLifetimeContext lifetimeContext = viewFactory.CreateView("Page 2", navigationContext);
            MockPage page = lifetimeContext.View as MockPage;
            MockViewModel<string, string> viewModel = page.DataContext as MockViewModel<string, string>;

            lifetimeContext.Dispose();

            Assert.AreEqual(true, viewModel.IsDisposed);
        }

        // *** Private Methods ***

        private IViewFactory CreateViewFactory(NavigationContextProxy navigationContextProxy = null)
        {
            if (navigationContextProxy == null)
                navigationContextProxy = new NavigationContextProxy();

            // Create the composition context exporter

            Dictionary<CompositionContract, Func<object>> exportFactories = new Dictionary<CompositionContract, Func<object>>();

            exportFactories[CreatePageContract("Page 1")] = () => new MockPage() { PageName = "Page 1" };
            exportFactories[CreatePageContract("Page 2")] = () => new MockPage() { PageName = "Page 2" };
            exportFactories[CreatePageContract("Page 3")] = () => new MockPage() { PageName = "Page 3" };

            exportFactories[CreateViewModelContract("Page 1")] = () => new MockViewModel<string, string>() { Name = "ViewModel 1" };
            exportFactories[CreateViewModelContract("Page 2")] = () => new MockViewModel<string, string>() { Name = "ViewModel 2" };

            exportFactories[new CompositionContract(typeof(INavigationContext))] = () => navigationContextProxy;

            ExportFactory<CompositionContext> compositionContextFactory = new ExportFactory<CompositionContext>(() => CreateCompositionContext(exportFactories));

            // Create the lazy page exports (for identifying if a view exists)
            // NB: These should never be used to create

            Lazy<object, PageMetadata>[] lazyPageExports = new Lazy<object, PageMetadata>[]
                            {
                                new Lazy<object, PageMetadata>(() => {throw new InvalidOperationException();}, new PageMetadata() { PageName = "Page 1"}),
                                new Lazy<object, PageMetadata>(() => {throw new InvalidOperationException();}, new PageMetadata() { PageName = "Page 2"}),
                                new Lazy<object, PageMetadata>(() => {throw new InvalidOperationException();}, new PageMetadata() { PageName = "Page 3"})
                            };

            // Return a new IViewFactory

            return new TestableViewFactory(compositionContextFactory, lazyPageExports);
        }

        private Tuple<CompositionContext, Action> CreateCompositionContext(Dictionary<CompositionContract, Func<object>> exportFactories)
        {
            MockCompositionContext compositionContext = new MockCompositionContext(exportFactories);

            return new Tuple<CompositionContext, Action>(compositionContext, () => compositionContext.Dispose());
        }

        private CompositionContract CreatePageContract(string pageName)
        {
            Dictionary<string, object> metadata = new Dictionary<string, object>();
            metadata["PageName"] = pageName;
            return new CompositionContract(typeof(object), "OkraPage", metadata);
        }

        private CompositionContract CreateViewModelContract(string viewModelName)
        {
            Dictionary<string, object> metadata = new Dictionary<string, object>();
            metadata["PageName"] = viewModelName;
            return new CompositionContract(typeof(object), "OkraViewModel", metadata);
        }

        private INavigationContext CreateNavigationContext(INavigationBase current = null)
        {
            if (current == null)
                current = new MockNavigationManager();

            return new MockNavigationContext(current);
        }

        // *** Private Sub-classes ***

        private class TestableViewFactory : ViewFactory
        {
            // *** Constructors ***

            public TestableViewFactory(ExportFactory<CompositionContext> compositionContextFactory, Lazy<object, PageMetadata>[] lazyPageExports)
                : base(compositionContextFactory, lazyPageExports)
            {
            }

            // *** Overriden Base Methods ***

            protected override void AttachViewModel(object page, object viewModel)
            {
                if (page is MockPage)
                    ((MockPage)page).DataContext = viewModel;
            }
        }

        private class MockCompositionContext : CompositionContext
        {
            private Dictionary<CompositionContract, Func<object>> exportFactories;
            private IList<object> exports = new List<object>();


            // *** Constructors ***

            public MockCompositionContext(Dictionary<CompositionContract, Func<object>> exportFactories)
            {
                this.exportFactories = exportFactories;
            }

            // *** Overriden Base Methods ***

            public override bool TryGetExport(CompositionContract contract, out object export)
            {
                Func<object> exportFactory;

                if (exportFactories.TryGetValue(contract, out exportFactory))
                {
                    export = exportFactory();
                    exports.Add(export);
                    return true;
                }

                export = null;
                return false;
            }

            // *** Methods ***

            public void Dispose()
            {
                foreach (object export in exports)
                {
                    if (export is IDisposable)
                        ((IDisposable)export).Dispose();
                }
            }
        }

        private class MockNavigationContext : INavigationContext
        {
            // *** Fields ***

            private readonly INavigationBase current;

            // *** Constructors ***

            public MockNavigationContext(INavigationBase current)
            {
                this.current = current;
            }

            // *** Methods ***

            public INavigationBase GetCurrent()
            {
                return current;
            }
        }

        private class MockPage : IDisposable
        {
            // *** Properties ***

            public bool IsDisposed { get; set; }
            public string PageName { get; set; }
            public object DataContext { get; set; }

            // *** Methods ***

            public void Dispose()
            {
                IsDisposed = true;
            }
        }

        private class MockViewModel<TArguments, TState> : IActivatable<TArguments, TState>, IDisposable
        {
            // *** Properties ***

            public TArguments ActivationArguments { get; private set; }
            public TState ActivationState { get; private set; }
            public bool IsActivated { get; private set; }
            public bool IsDisposed { get; set; }
            public string Name { get; set; }
            public TState State { get; set; }

            // *** Methods ***

            public void Activate(TArguments arguments, TState state)
            {
                IsActivated = true;
                ActivationArguments = arguments;
                ActivationState = state;
            }

            public TState SaveState()
            {
                return State;
            }

            public void Dispose()
            {
                IsDisposed = true;
            }
        }
    }
}
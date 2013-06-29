﻿using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Okra.Navigation;
using Windows.ApplicationModel.Activation;
using System.Threading.Tasks;
using System.Composition.Hosting;
using System.Composition;
using System.Collections;
using Windows.ApplicationModel.Core;
using Okra.Services;
using Okra.Search;

namespace Okra.MEF.Tests
{
    [TestClass]
    public class OkraBootstrapperFixture
    {
        // *** Method Tests ***

        [TestMethod]
        public void Initialize_ComposesProperties()
        {
            TestableBootstrapper bootstrapper = new TestableBootstrapper();

            bootstrapper.Initialize();

            Assert.IsInstanceOfType(bootstrapper.ActivationManager, typeof(MockActivationManager));
        }

        [TestMethod]
        public void OnActivated_CallsSetupServices()
        {
            TestableBootstrapper bootstrapper = new TestableBootstrapper();
            bootstrapper.Initialize();

            bootstrapper.SimulateActivate(new MockActivatedEventArgs(ActivationKind.Launch));

            CollectionAssert.Contains((ICollection)bootstrapper.SetupMethodCalls, "SetupServices");
        }

        [TestMethod]
        public void OnActivated_CallsSetupServices_OnlyOnce()
        {
            TestableBootstrapper bootstrapper = new TestableBootstrapper();
            bootstrapper.Initialize();

            bootstrapper.SimulateActivate(new MockActivatedEventArgs(ActivationKind.Launch));
            bootstrapper.SimulateActivate(new MockActivatedEventArgs(ActivationKind.Launch));
            bootstrapper.SimulateActivate(new MockActivatedEventArgs(ActivationKind.Launch));

            Assert.AreEqual(1, bootstrapper.SetupMethodCalls.Count(str => str == "SetupServices"));
        }

        [TestMethod]
        public void OnActivated_Launch_PassesActivationEventToNavigationManager()
        {
            TestableBootstrapper bootstrapper = new TestableBootstrapper();
            bootstrapper.Initialize();

            bootstrapper.SimulateActivate(new MockActivatedEventArgs(ActivationKind.Launch));

            MockActivationManager activationManager = (MockActivationManager)bootstrapper.ActivationManager;
            Assert.AreEqual(1, activationManager.ActivationEventArgs.Count);
            Assert.IsInstanceOfType(activationManager.ActivationEventArgs[0], typeof(MockActivatedEventArgs));
            Assert.AreEqual(ActivationKind.Launch, activationManager.ActivationEventArgs[0].Kind);
        }

        // *** Private Sub-classes ***

        private class TestableBootstrapper : OkraBootstrapper
        {
            // *** Fields ***

            public readonly IList<string> SetupMethodCalls = new List<string>();

            // *** Methods ***

            public void SimulateActivate(IActivatedEventArgs activatedEventArgs)
            {
                base.OnActivated(CoreApplication.MainView, activatedEventArgs);
            }

            // *** Overriden base methods ***

            public override void Initialize()
            {
                base.Initialize(false);
            }

            protected override ContainerConfiguration GetContainerConfiguration()
            {
                return new ContainerConfiguration()
                            .WithPart<MockActivationManager>()
                            .WithPart<MockNavigationManager>()
                            .WithPart<MockSearchManager>()
                            .WithPart<MockLaunchActivationHandler>();
            }

            protected override void SetupServices()
            {
                SetupMethodCalls.Add("SetupServices");
                base.SetupServices();
            }
        }

        [Export(typeof(IActivationManager))]
        [Shared]
        private class MockActivationManager : IActivationManager
        {
            // *** Fields ***

            public readonly IList<IActivatedEventArgs> ActivationEventArgs = new List<IActivatedEventArgs>();

            // *** Events ***

            public event EventHandler<IActivatedEventArgs> Activating;
            public event EventHandler<IActivatedEventArgs> Activated;
            
            // *** Methods ***

            public Task<bool> Activate(IActivatedEventArgs activatedEventArgs)
            {
                ActivationEventArgs.Add(activatedEventArgs);
                return Task.FromResult<bool>(true);
            }

            public void Register(IActivationHandler service)
            {
                throw new NotImplementedException();
            }

            public void Unregister(IActivationHandler service)
            {
                throw new NotImplementedException();
            }
        }

        [Export(typeof(INavigationManager))]
        [Shared]
        private class MockNavigationManager : INavigationManager
        {
            public event EventHandler CanGoBackChanged;
            public event EventHandler<PageNavigationEventArgs> NavigatingFrom;
            public event EventHandler<PageNavigationEventArgs> NavigatedTo;

            public bool CanGoBack
            {
                get { throw new NotImplementedException(); }
            }

            public INavigationEntry CurrentPage
            {
                get { throw new NotImplementedException(); }
            }

            public string HomePageName
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                    throw new NotImplementedException();
                }
            }

            public NavigationStorageType NavigationStorageType
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                    throw new NotImplementedException();
                }
            }

            public bool CanNavigateTo(string pageName)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<object> GetCurrentPageElements()
            {
                throw new NotImplementedException();
            }

            public void GoBack()
            {
                throw new NotImplementedException();
            }

            public void NavigateTo(string pageName)
            {
                throw new NotImplementedException();
            }

            public void NavigateTo(string pageName, object arguments)
            {
                throw new NotImplementedException();
            }

            public Task<bool> RestoreNavigationStack()
            {
                throw new NotImplementedException();
            }
        }

        [Export(typeof(ISearchManager))]
        [Shared]
        private class MockSearchManager : ISearchManager
        {
            public string SearchPageName
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                    throw new NotImplementedException();
                }
            }
        }

        [Export(typeof(ILaunchActivationHandler))]
        [Shared]
        private class MockLaunchActivationHandler : ILaunchActivationHandler
        {
            public Task<bool> Activate(IActivatedEventArgs activatedEventArgs)
            {
                throw new NotImplementedException();
            }
        }

        private class MockActivatedEventArgs : IActivatedEventArgs
        {
            // *** Constructors ***

            public MockActivatedEventArgs(ActivationKind kind)
            {
                this.Kind = kind;
            }

            // *** Properties ***

            public ActivationKind Kind { get; set; }

            public ApplicationExecutionState PreviousExecutionState { get; set; }

            public SplashScreen SplashScreen { get; set; }
        }
    }
}
using Okra.Helpers;
using Okra.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

namespace Okra.Navigation
{
    public class SettingsPaneManager : NavigationBase, ISettingsPaneManager
    {
        // *** Fields ***

        private readonly FlyoutPane settingsFlyout;

        // *** Events ***

        public event EventHandler FlyoutClosed;
        public event EventHandler FlyoutOpened;

        // *** Constructors ***

        public SettingsPaneManager(IViewFactory viewFactory)
            : base(viewFactory)
        {
            FlyoutEdge flyoutEdge = SettingsPane.Edge == SettingsEdgeLocation.Left ? FlyoutEdge.Left : FlyoutEdge.Right;
            settingsFlyout = new FlyoutPane(flyoutEdge, true);

            settingsFlyout.Closed += OnSettingsFlyoutClosed;
            settingsFlyout.Opened += OnSettingsFlyoutOpened;
        }

        // *** Methods ***

        public void ShowSettingsPane()
        {
            SettingsPane.Show();
        }

        // *** Protected Methods ***

        protected void OnSettingsFlyoutClosed(object sender, object e)
        {
            // Raise the FlyoutClosed event

            OnFlyoutClosed();

            // Remove all navigation entries from the stack
            // TODO : Add some way to indicate to VMs that they are closing - IClosingAware?

            NavigationStack.Clear();
        }

        protected void OnSettingsFlyoutOpened(object sender, object e)
        {
            // Raise the FlyoutOpened event

            OnFlyoutOpened();
        }

        protected override void DisplayPage(object page)
        {
            // If the page is null then close the flyout and show the system settings pane

            if (page == null)
            {
                settingsFlyout.Close();
                SettingsPane.Show();
            }

            // Otherwise navigate the flyout to the specified page

            else
            {
                settingsFlyout.Show(page);
            }
        }

        protected virtual void OnFlyoutClosed()
        {
            EventHandler eventHandler = FlyoutClosed;

            if (eventHandler != null)
                eventHandler(this, EventArgs.Empty);
        }

        protected virtual void OnFlyoutOpened()
        {
            EventHandler eventHandler = FlyoutOpened;

            if (eventHandler != null)
                eventHandler(this, EventArgs.Empty);
        }
    }
}

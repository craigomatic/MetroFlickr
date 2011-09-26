using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MetroFlickr.Controllers;
using MetroFlickr.Model;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.UI.ApplicationSettings;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace MetroFlickr
{
    public sealed partial class MainPage
    {
        private NavigationController _NavigationController;

        public MainPage()
        {
            InitializeComponent();                      
        }

        // View state management for switching among Full, Fill, Snapped, and Portrait states

        private DisplayPropertiesEventHandler _displayHandler;
        private TypedEventHandler<ApplicationLayout, ApplicationLayoutChangedEventArgs> _layoutHandler;

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (_displayHandler == null)
            {
                _displayHandler = Page_OrientationChanged;
                _layoutHandler = Page_LayoutChanged;
            }
            DisplayProperties.OrientationChanged += _displayHandler;
            ApplicationLayout.GetForCurrentView().LayoutChanged += _layoutHandler;
            SetCurrentOrientation(this);

            SettingsPane.GetForCurrentView().ApplicationCommands.Add(new SettingsCommand(KnownSettingsCommand.Preferences, new UICommandInvokedHandler(delegate(IUICommand command)
            {
                if (Window.Current.Content != this)
                {
                    //TODO: I get an arguement out of range exception when running this code meaning that once the options have been set they currently cannot be changed :(

                    //Window.Current.Content = this;
                    //Window.Current.Activate();
                    //SettingsView.Margin = ThicknessHelper.FromUniformLength(0);
                    
                }
                else
                {
                    SettingsView.Margin = ThicknessHelper.FromUniformLength(0);
                }
            })));

            object username = null;
            object apiKey = null;

            var settingsViewRequired = !Windows.Storage.ApplicationData.Current.RoamingSettings.Values.TryGetValue("FlickrUsername", out username) || !Windows.Storage.ApplicationData.Current.RoamingSettings.Values.TryGetValue("FlickrApiKey", out apiKey);

            if (string.IsNullOrWhiteSpace((string)username) || string.IsNullOrWhiteSpace((string)apiKey))
            {
                settingsViewRequired = true;
            }

            if (settingsViewRequired)
            {
                SettingsPane.Show();
            }
            else
            {
                _RunApp((string)username, (string)apiKey);
            }
        }

        private void _RunApp(string username, string apiKey)
        {
            _NavigationController = new NavigationController(username, apiKey);
            _NavigationController.SetView("MetroFlickr", ViewType.Home, null, null, null);
        }

        private void LayoutRoot_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerEventArgs args)
        {
            if (SettingsView.Margin.Right == 0)
            {
                SettingsView.Margin = ThicknessHelper.FromLengths(0, 0, -346, 0);
            }

            //verify the settings have been entered and if so, launch the conventional navigation
            var canRun = true;
            object apiKey = null;

            if (!Windows.Storage.ApplicationData.Current.RoamingSettings.Values.TryGetValue("FlickrApiKey", out apiKey) || string.IsNullOrWhiteSpace((string)apiKey))
            {
                canRun = false;
            }

            object username = null;

            if (!Windows.Storage.ApplicationData.Current.RoamingSettings.Values.TryGetValue("FlickrUsername", out username) || string.IsNullOrWhiteSpace((string)username))
            {
                canRun = false;
            }

            if (canRun)
            {
                _RunApp((string)username, (string)apiKey);
            }
            else
            {
                var dialog = new MessageDialog("MetroFlickr requires an API key and a username in order to run. Please enter these in the settings charm under Preferences");
                dialog.ShowAsync();
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            DisplayProperties.OrientationChanged -= _displayHandler;
            ApplicationLayout.GetForCurrentView().LayoutChanged -= _layoutHandler;
        }

        private void Page_LayoutChanged(object sender, ApplicationLayoutChangedEventArgs e)
        {
            SetCurrentOrientation(this);
        }

        private void Page_OrientationChanged(object sender)
        {
            SetCurrentOrientation(this);
        }

        private void SetCurrentOrientation(Control viewStateAwareControl)
        {
            VisualStateManager.GoToState(viewStateAwareControl, this.GetViewState(), false);
        }

        private String GetViewState()
        {
            var orientation = DisplayProperties.CurrentOrientation;
            if (orientation == DisplayOrientations.Portrait ||
                orientation == DisplayOrientations.PortraitFlipped) return "Portrait";
            var layout = ApplicationLayout.Value;
            if (layout == ApplicationLayoutState.Filled) return "Fill";
            if (layout == ApplicationLayoutState.Snapped) return "Snapped";
            return "Full";
        }
    }
}

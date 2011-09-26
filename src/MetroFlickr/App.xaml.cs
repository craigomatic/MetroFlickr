using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using MetroFlickr.Model;
using Windows.UI.ApplicationSettings;
using MetroFlickr.Controllers;
using Windows.UI.Popups;

namespace MetroFlickr
{
    partial class App
    {

        public App()
        {
            InitializeComponent();
        }
        
        protected override void OnFilePickerActivated(FilePickerActivatedEventArgs args)
        {
            var filePickerPage = new FilePickerPage();
            filePickerPage.Activate(args);
        }

        protected override void OnSearchActivated(SearchActivatedEventArgs args)
        {
            var searchResultsPage = new SearchResultsPage();
            searchResultsPage.Activate(args);
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {            
            var page = new MainPage();
            Window.Current.Content = page;
            Window.Current.Activate();
        }
    }
}

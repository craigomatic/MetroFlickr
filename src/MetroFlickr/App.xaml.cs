using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using MetroFlickr.Model;
using Windows.UI.ApplicationSettings;

namespace MetroFlickr
{
    partial class App
    {
        public static readonly string ApiKey = "INSERT_API_KEY";
        public static FlickrDataSource FlickrDataSource;
       
        private static string _Username = "INSERT_USERNAME";

        public App()
        {
            InitializeComponent();
        }

        void Current_DataChanged(Windows.Storage.ApplicationData sender, object args)
        {
            
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

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            //TODO: look in the roaming settings for the API key and username rather than the hardcode
           
            var page = new ImageCollectionPage();

            FlickrDataSource = new FlickrDataSource(_Username, App.ApiKey);
            page.DataContext = App.FlickrDataSource;
            

            Window.Current.Content = page;
            Window.Current.Activate();

            await FlickrDataSource.LoadAsync(page.Dispatcher);
            page.Items = FlickrDataSource.ImageSets;
        }
    }
}

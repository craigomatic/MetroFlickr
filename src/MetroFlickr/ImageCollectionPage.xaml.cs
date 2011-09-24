using System;
using System.Collections.Generic;
using MetroFlickr.Model;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;

namespace MetroFlickr
{
    public sealed partial class ImageCollectionPage
    {
        public ImageCollectionPage()
        {
            InitializeComponent();
        }

        void ItemView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UserControl page = null;
            
            if (this.DataContext is FlickrDataSource)
            {
                //display a page with the images from the selected set
                page = new ImageCollectionPage();

                var imageSet = e.AddedItems[0] as FlickrImageSet;
                page.DataContext = imageSet;
                (page as ImageCollectionPage).BackButton.IsEnabled = true;
                (page as ImageCollectionPage).Items = imageSet.Collection;
            }
            else if (this.DataContext is FlickrImageSet)
            {
                //display the selected image in detail view
                page = new ImageDetailPage();

                var image = e.AddedItems[0] as FlickrImage;
                page.DataContext = image;
                (page as ImageDetailPage).Items = image.ImageSet.Collection;
                (page as ImageDetailPage).Item = image;
            }

            if (page != null)
            {
                Window.Current.Content = page;
                Window.Current.Activate();
            }
        }

        private IEnumerable<Object> _items;
        public IEnumerable<Object> Items
        {
            get
            {
                return this._items;
            }

            set
            {
                this._items = value;
                CollectionViewSource.Source = value;
            }
        }

        // View state management for switching among Full, Fill, Snapped, and Portrait states

        private DisplayPropertiesEventHandler _displayHandler;
        private TypedEventHandler<ApplicationLayout, ApplicationLayoutChangedEventArgs> _layoutHandler;

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            BackButton.Visibility = (this.DataContext is FlickrDataSource) ? Visibility.Collapsed : Visibility.Visible;
           
            if (_displayHandler == null)
            {
                _displayHandler = Page_OrientationChanged;
                _layoutHandler = Page_LayoutChanged;
            }
            DisplayProperties.OrientationChanged += _displayHandler;
            ApplicationLayout.GetForCurrentView().LayoutChanged += _layoutHandler;
            SetCurrentViewState(this);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            DisplayProperties.OrientationChanged -= _displayHandler;
            ApplicationLayout.GetForCurrentView().LayoutChanged -= _layoutHandler;
        }

        private void Page_LayoutChanged(object sender, ApplicationLayoutChangedEventArgs e)
        {
            SetCurrentViewState(this);
        }

        private void Page_OrientationChanged(object sender)
        {
            SetCurrentViewState(this);
        }

        private void SetCurrentViewState(Control viewStateAwareControl)
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

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            //back is only enabled when looking at a specific imageset, so go back home
            var page = new ImageCollectionPage();
            page.DataContext = App.FlickrDataSource;
            page.Items = App.FlickrDataSource.ImageSets;

            Window.Current.Content = page;
            Window.Current.Activate();
        }
    }
}

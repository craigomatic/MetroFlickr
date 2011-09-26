using System;
using System.Collections.Generic;
using MetroFlickr.Controllers;
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
        public NavigationController NavigationController { get; private set; }

        public ImageCollectionPage()
        {
            InitializeComponent();
        }

        public ImageCollectionPage(NavigationController navigationController)
            : this()
        {
            this.NavigationController = navigationController;
        }

        void ItemView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.DataContext is FlickrDataSource)
            {
                //display the selected collection
                var imageSet = e.AddedItems[0] as FlickrImageSet;
                this.NavigationController.SetView(imageSet.Title, ViewType.Collection, imageSet, imageSet.Collection, null);                
            }
            else if (this.DataContext is FlickrImageSet)
            {
                //display the selected image in detail view
                var image = e.AddedItems[0] as FlickrImage;
                this.NavigationController.SetView(image.Title, ViewType.Detail, image, image.ImageSet.Collection, image);
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
            this.NavigationController.SetView("MetroFlickr", ViewType.Home, null, null, null);
        }
    }
}

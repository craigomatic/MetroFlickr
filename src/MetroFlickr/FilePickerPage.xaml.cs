using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MetroFlickr.Model;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Storage.Pickers.Provider;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;

namespace MetroFlickr
{
    public sealed partial class FilePickerPage
    {
        public FilePickerPage()
        {
            InitializeComponent();
            
            JumpViewer.ViewChangeCompleted += new JumpViewerViewChangedEventHandler(JumpViewer_ViewChangeCompleted);
        }

        void JumpViewer_ViewChangeCompleted(object sender, JumpViewerViewChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(e.DestinationItem.Item);
        }

        void ItemView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO: Construct the appropriate destination page and set its context appropriately
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
            if (_displayHandler == null)
            {
                _displayHandler = Page_OrientationChanged;
                _layoutHandler = Page_LayoutChanged;
            }

            VisualStateManager.GoToState(this, "Full", false);

            (JumpViewer.JumpView as ListViewBase).ItemsSource = CollectionViewSource.View.CollectionGroups;
            JumpViewer.IsContentViewActive = false;

            //DisplayProperties.OrientationChanged += _displayHandler;
            //ApplicationLayout.GetForCurrentView().LayoutChanged += _layoutHandler;
            //SetCurrentViewState(this);
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
    }
}

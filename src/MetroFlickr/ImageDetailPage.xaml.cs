using System;
using System.Collections.Generic;
using System.Linq;
using MetroFlickr.Controllers;
using MetroFlickr.Model;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Graphics.Printing;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Printing;

namespace MetroFlickr
{
    public sealed partial class ImageDetailPage
    {
        public NavigationController NavigationController { get; private set; }

        private PropertySet _flipState = new PropertySet();
        private PrintDocument _PrintDocument;

        public ImageDetailPage()
        {
            InitializeComponent();

            _flipState["CanFlipNext"] = false;
            _flipState["CanFlipPrevious"] = false;
            ApplicationBar.DataContext = _flipState;
        }

        public ImageDetailPage(NavigationController navigationController)
            : this()
        {
            this.NavigationController = navigationController;            
        }

        void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationController.SetView("MetroFlickr", ViewType.Home, null, null, null);
        }

        private object _DocumentSource;

        void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            _PrintDocument = new PrintDocument();
            _DocumentSource = _PrintDocument.DocumentSource;
            _PrintDocument.GetPreviewPage += new GetPreviewPageEventHandler(printDocument_GetPreviewPage);
            _PrintDocument.Paginate += new PaginateEventHandler(printDocument_Paginate);
            _PrintDocument.AddPages += new AddPagesEventHandler(printDocument_AddPages);

            var printManager = PrintManager.GetForCurrentView();

            try
            {
                printManager.PrintTaskInitializing += new TypedEventHandler<PrintManager, PrintTaskInitializingEventArgs>(printManager_PrintTaskInitializing);
            }
            catch { }

            PrintManager.ShowPrintUI();
        }

        void printDocument_AddPages(object sender, AddPagesEventArgs e)
        {
            var contentPresenter = new ContentPresenter
            {
                Content = new Image { Source = new BitmapImage(new Uri(_CurrentImage.LargeImageUri)) }
            };
            
            _PrintDocument.AddPage(contentPresenter);
            _PrintDocument.AddPagesComplete();
        }

        void printDocument_Paginate(object sender, PaginateEventArgs e)
        {
            _PrintDocument.SetPreviewPageCount(1);
        }

        void printManager_PrintTaskInitializing(PrintManager sender, PrintTaskInitializingEventArgs args)
        {
            args.Request.InitializePrintTask(_DocumentSource, "MetroFlickr");
        }

        void printDocument_GetPreviewPage(object sender, GetPreviewPageEventArgs e)
        {
            var contentPresenter = new ContentPresenter
            {
                Content = new Image { Source = new BitmapImage(new Uri(_CurrentImage.LargeImageUri)) }
            };

            _PrintDocument.SetPreviewPage(e.PageNumber, contentPresenter);
        }

        void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var associatedImageSet = (this.DataContext as FlickrImage).ImageSet;
            this.NavigationController.SetView(associatedImageSet.Title, ViewType.Collection, associatedImageSet, associatedImageSet.Collection, null);
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
                PageTitle.DataContext = value;
            }
        }

        private FlickrImage _CurrentImage;

        public Object Item
        {
            get
            {
                return FlipView.SelectedItem;
            }

            set
            {
                FlipView.SelectedItem = value;

                if (value is FlickrImage)
                {
                    _CurrentImage = value as FlickrImage;
                }
            }
        }

        // Mirror the flipper controls in the application bar

        void FlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _flipState["CanFlipNext"] = CanFlipNext;
            _flipState["CanFlipPrevious"] = CanFlipPrevious;

            if (e.AddedItems.Count == 1 && e.AddedItems[0] is FlickrImage)
            {
                _CurrentImage = e.AddedItems[0] as FlickrImage;
            }
        }

        bool CanFlipPrevious
        {
            get { return FlipView.SelectedIndex > 0; }
        }

        bool CanFlipNext
        {
            get { return FlipView.SelectedIndex < (FlipView.Items.Count - 1); }
        }

        void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            if (CanFlipPrevious) FlipView.SelectedIndex -= 1;
        }

        void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (CanFlipNext) FlipView.SelectedIndex += 1;
        }

        // View state management for switching among Full, Fill, Snapped, and Portrait states.
        // Complicated by the fact that the page is instantiated multiple times within a FlipView
        // context.

        private DisplayPropertiesEventHandler _displayHandler;
        private TypedEventHandler<ApplicationLayout, ApplicationLayoutChangedEventArgs> _layoutHandler;
        private List<Control> viewStateAwareControls = new List<Control>();

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //Workaround: the initial selection for a FlipView isn't respected when it's made before the page is loaded,
            // so it's necessary to clear its state and try again once the page is loaded.  Clearly this is not intended
            // to be necessary.
            if (sender == this)
            {
                var originallySelectedItem = Item;
                Item = null;
                UpdateLayout();
                Item = originallySelectedItem;
            }

            var control = sender as Control;
            if (viewStateAwareControls.Count == 0)
            {
                if (_displayHandler == null)
                {
                    _displayHandler = Page_OrientationChanged;
                    _layoutHandler = Page_LayoutChanged;
                }
                DisplayProperties.OrientationChanged += _displayHandler;
                ApplicationLayout.GetForCurrentView().LayoutChanged += _layoutHandler;
            }
            viewStateAwareControls.Add(control);
            SetCurrentViewState(control);          
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            viewStateAwareControls.Remove(sender as Control);
            if (viewStateAwareControls.Count == 0)
            {
                DisplayProperties.OrientationChanged -= _displayHandler;
                ApplicationLayout.GetForCurrentView().LayoutChanged -= _layoutHandler;
            }
        }

        private void Page_LayoutChanged(object sender, ApplicationLayoutChangedEventArgs e)
        {
            foreach (var control in viewStateAwareControls)
            {
                SetCurrentViewState(control);
            }
        }

        private void Page_OrientationChanged(object sender)
        {
            foreach (var control in viewStateAwareControls)
            {
                SetCurrentViewState(control);
            }
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

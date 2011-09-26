using System.Collections.Generic;
using MetroFlickr.Model;
using Windows.UI.ApplicationSettings;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace MetroFlickr.Controllers
{
    public class NavigationController //: NotificationObject <- get a compile error when this code is enabled, temporarily disabling for now
    {
        private string _Breadcrumb;

        public string Breadcrumb
        {
            get { return _Breadcrumb; }
            set
            {
                if (_Breadcrumb == value)
                {
                    return;
                }

                _Breadcrumb = value;
                
                //base.OnPropertyChanged("Breadcrumb");
            }
        }

        public ViewType CurrentViewType { get; private set; }

        public FlickrDataSource DataSource { get; private set; }

        public NavigationController(string username, string apiKey)
        {            
            this.DataSource = new FlickrDataSource(username, apiKey);
        }

        public async void SetView(string title, ViewType type, object context, IEnumerable<object> items, object selectedItem)
        {
            switch (type)
            {
                case ViewType.Home:
                    {
                        this.Breadcrumb = title;

                        var page = new ImageCollectionPage(this);
                        page.DataContext = this.DataSource;

                        Window.Current.Content = page;
                        Window.Current.Activate();

                        await this.DataSource.LoadAsync(page.Dispatcher);
                        page.Items = this.DataSource.ImageSets;
                        
                        break;
                    }
                case ViewType.Collection:
                    {
                        this.Breadcrumb = string.Format("{0} -> {1}", "MetroFlickr", title);

                        var page = new ImageCollectionPage(this);
                        page.DataContext = context;
                        page.Items = items;

                        Window.Current.Content = page;
                        Window.Current.Activate();

                        break;
                    }
                case ViewType.Detail:
                    {
                        this.Breadcrumb = string.Format("{0} -> {1}", "MetroFlickr", title);

                        var page = new ImageDetailPage(this);
                        page.DataContext = context;
                        page.Items = items;
                        page.Item = selectedItem;

                        Window.Current.Content = page;
                        Window.Current.Activate();

                        break;
                    }
            }

            this.CurrentViewType = type;

        }
    }
}

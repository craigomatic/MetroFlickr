using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace MetroFlickr.Model
{
    public class FlickrImage : NotificationObject
    {
        public string Title { get; set; }

        public ImageSource Image { get; private set; }

        private string _LargeImageUri;
        private string _SmallImageUri;

        public ImageSource LargeImage
        {
            get
            {
                return new BitmapImage(new Uri(_LargeImageUri));
            }
        }

        public FlickrImageSet ImageSet { get; private set; }

        public FlickrImage(FlickrImageSet imageSet, string smallImageUri, string largeImageUri, string title)
        {
            _SmallImageUri = smallImageUri;
            _LargeImageUri = largeImageUri;

            this.ImageSet = imageSet;
            this.Image = new BitmapImage(new Uri(smallImageUri));
            this.Title = title;
        }

        public override string ToString()
        {
            return string.Format("[Img] {0} - {1}", _SmallImageUri, this.Title);
        }        
    }
}

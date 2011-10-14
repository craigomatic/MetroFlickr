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
        public DateTime Date { get; private set; }

        public string Title { get; set; }

        public ImageSource Image { get; private set; }

        public string ImageUri { get; private set; }

        public string LargeImageUri { get; private set; }

        public ImageSource LargeImage
        {
            get
            {
                return new BitmapImage(new Uri(this.LargeImageUri));
            }
        }

        public FlickrImageSet ImageSet { get; private set; }

        public FlickrImage(FlickrImageSet imageSet, string smallImageUri, string largeImageUri, string title, DateTime date)
        {
            this.ImageUri = smallImageUri;
            this.LargeImageUri = largeImageUri;

            this.ImageSet = imageSet;
            this.Image = new BitmapImage(new Uri(smallImageUri));
            this.Title = title;
            this.Date = date;
        }

        public override string ToString()
        {
            return string.Format("[Img] {0} - {1}", this.ImageUri, this.Title);
        }        
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace MetroFlickr.Model
{
    public class FlickrImageSet : NotificationObject
    {
        public string Title { get; set; }

        public string Subtitle
        {
            get { return this.Title; }
        }

        public DateTime Date { get; set; }

        public string Description { get; set; }

        public ImageSource Image { get; set; }

        public IList<FlickrImage> Collection { get; set; }
        
        public FlickrImageSet(string imageUri, string title, DateTime date, string description)
        {
            this.Collection = new List<FlickrImage>();

            this.Image = new BitmapImage(new Uri(imageUri));
            this.Title = title;
            this.Date = date;
            this.Description = string.IsNullOrWhiteSpace(description) ? title : description;
        }
    }
}

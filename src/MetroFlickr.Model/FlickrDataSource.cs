using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlickrNet;
using Windows.UI.Xaml.Data;

namespace MetroFlickr.Model
{
    public class FlickrDataSource : NotificationObject
    {
        public string Title
        {
            get { return this.Username; }
        }

        public string Username { get; private set; }        
        
        private FlickrNet.Flickr _Flickr;

        public IList<FlickrImageSet> ImageSets { get; set; }

        public FlickrDataSource(string username, string apiKey)
        {
            this.Username = username;
            this.ImageSets = new List<FlickrImageSet>();

            _Flickr = new FlickrNet.Flickr(apiKey);            
        }

        public void Load()
        {
            var flickrUser = _GetUserProfileAsync(this.Username).Result;
            var photoSetCollection = _GetPhotosetCollectionAsync(flickrUser.UserId).Result;

            foreach (var photoSet in photoSetCollection)
            {
                FlickrImageSet imageSet = new FlickrImageSet(photoSet.PhotosetSmallUrl, photoSet.Title, photoSet.DateUpdated, photoSet.Description);
                this.ImageSets.Add(imageSet);
                base.OnPropertyChanged("ImageSets");

                var photosetPhotosCollection = _GetCollectionForSetAsync(photoSet.PhotosetId).Result;

                foreach (var photo in photosetPhotosCollection)
                {
                    var image = new FlickrImage(imageSet, photo.SmallUrl, photo.DoesLargeExist ? photo.LargeUrl : photo.Medium640Url, photo.Title, photo.DateTaken);
                    imageSet.Collection.Add(image);
                }
            }
        }

        public Task<IList<FlickrImageSet>> LoadAsync(Windows.UI.Core.CoreDispatcher dispatcher)
        {
            var task = Task.Run(() =>
            {
                var flickrUser = _GetUserProfileAsync(this.Username).Result;
                var photoSetCollection = _GetPhotosetCollectionAsync(flickrUser.UserId).Result;

                foreach (var photoSet in photoSetCollection)
                {
                    FlickrImageSet imageSet = null;
                    
                    dispatcher.Invoke(Windows.UI.Core.CoreDispatcherPriority.Normal, (x,y) =>
                    {
                        imageSet = new FlickrImageSet(photoSet.PhotosetSmallUrl, photoSet.Title, photoSet.DateUpdated, photoSet.Description);
                        this.ImageSets.Add(imageSet);
                        base.OnPropertyChanged("ImageSets");
                    }, 
                    this, null);

                    var photosetPhotosCollection = _GetCollectionForSetAsync(photoSet.PhotosetId).Result;

                    foreach (var photo in photosetPhotosCollection)
                    {
                        dispatcher.Invoke(Windows.UI.Core.CoreDispatcherPriority.Normal, (x,y) =>
                        {
                            var image = new FlickrImage(imageSet, photo.SmallUrl, photo.DoesLargeExist ? photo.LargeUrl : photo.Medium640Url, photo.Title, photo.DateTaken);
                            imageSet.Collection.Add(image);

                        }, 
                        this, null);
                    }
                }

                return this.ImageSets;
            });

            return task;
        }

        public Task<List<FlickrImage>> SearchAsync(string searchTerm, Windows.UI.Core.CoreDispatcher dispatcher)
        {
            return Task.Run<List<FlickrImage>>(() =>
            {
                var flickrImages = new List<FlickrImage>();

                var photos = _Flickr.PhotosSearch(new PhotoSearchOptions(this.Username, string.Empty, TagMode.None, searchTerm));

                foreach (var photo in photos)
                {
                    dispatcher.Invoke(Windows.UI.Core.CoreDispatcherPriority.Normal, (x, y) =>
                    {
                        flickrImages.Add(new FlickrImage(null, photo.SmallUrl, photo.DoesLargeExist ? photo.LargeUrl : photo.Medium640Url, photo.Title, photo.DateTaken));
                    }, 
                    this, null);
                }
                return flickrImages;
            });
        }

        private Task<Person> _GetUserProfileAsync(string username)
        {
            return Task.Run(() =>
            {
                var profile = _Flickr.PeopleFindByUserName(username);
                return _Flickr.PeopleGetInfo(profile.UserId);
            });
        }

        private Task<PhotosetCollection> _GetPhotosetCollectionAsync(string userId)
        {
            return Task.Run(() =>
            {
                return _Flickr.PhotosetsGetList(userId);
            });
        }

        private Task<PhotosetPhotoCollection> _GetCollectionForSetAsync(string photosetId)
        {
            return Task.Run(() =>
            {
                return _Flickr.PhotosetsGetPhotos(photosetId);            
            });
        }
    }
}

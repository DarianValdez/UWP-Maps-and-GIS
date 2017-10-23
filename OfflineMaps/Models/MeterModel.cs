using Microsoft.WindowsAzure.MobileServices.Sync;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.Devices.Geolocation;
using Windows.Foundation;

namespace OfflineMaps.Models
{
    public class JsonMeterModel
    {   
        public string Id { get; set; }

        /// <summary>
        /// MeterID of the meter.
        /// </summary>
        [JsonProperty(PropertyName = "meterid")]
        public string MeterId { get; set; }

        /// <summary>
        /// Route Number from Database.
        /// </summary>
        [JsonProperty(PropertyName = "routenumber")]
        public string RouteNumber { get; set; }

        [JsonProperty(PropertyName = "latitude")]
        public double Latitude { get; set; }

        [JsonProperty(PropertyName = "longitude")]
        public double Longitude { get; set; }

        /// <summary>
        /// Name of Customer from Database.
        /// </summary>
        [JsonProperty(PropertyName = "customer")]
        public string Customer { get; set; }

        /// <summary>
        /// Physical address of meter.
        /// </summary>
        [JsonProperty(PropertyName = "address")]
        public string Address { get; set; }

        /// <summary>
        /// Account Number from Database.
        /// </summary>
        [JsonProperty(PropertyName = "accountnumber")]
        public string AccountNumber { get; set; }

        /// <summary>
        /// Account Description from Database.
        /// </summary>
        [JsonProperty(PropertyName = "accountdescription")]
        public string AccountDescription { get; set; }

        [JsonProperty(PropertyName = "reading")]
        public string Reading { get; set; }
    }

    public class MeterModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Notifies to it parent if the model has been changed.
        /// </summary>
        /// <param name="PropertyName">Name of the property that was changed.</param>
        private void NotifyPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        public JsonMeterModel MeterInfo { get; set; }

        public Geopoint Location { get; set; }

        private Point _anchorPoint = new Point(0.5, 1.0);
        public Point AnchorPoint
        {
            get { return _anchorPoint; }
        }

        private bool _readUpdated = false;
        public bool ReadUpdated
        {
            get { return _readUpdated; }
            set
            {
                _readUpdated = value;
                NotifyPropertyChanged(nameof(ReadUpdated));
            }
        }
    }

    public class MeterModelCollection : ObservableCollection<MeterModel>
    {
        /// <summary>
        /// Represents a collection of MeterModels and provides notifications for them.
        /// </summary>
        public MeterModelCollection()
        {

        }
    }
}

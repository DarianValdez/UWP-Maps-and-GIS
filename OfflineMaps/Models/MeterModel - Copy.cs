using Microsoft.WindowsAzure.MobileServices.Sync;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.Devices.Geolocation;
using Windows.Foundation;

namespace OfflineMaps.Models
{
    public class MeterModelTest
    {
        /// <summary>
        /// MeterID of the meter.
        /// </summary>
        //[JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "latitude")]
        public double Latitude { get; set; }

        [JsonProperty(PropertyName = "longitude")]
        public double Longitude { get; set; }

        /// <summary>
        /// Route Number from Database.
        /// </summary>
        [JsonProperty(PropertyName = "routenumber")]
        public string RouteNumber { get; set; }

        /// <summary>
        /// Account Description from Database.
        /// </summary>
        [JsonProperty(PropertyName = "accountdescription")]
        public string AccountDescription { get; set; }

        /// <summary>
        /// Physical address of meter.
        /// </summary>
        [JsonProperty(PropertyName = "address")]
        public string Address { get; set; }

        /// <summary>
        /// Name of Customer from Database.
        /// </summary>
        [JsonProperty(PropertyName = "customer")]
        public string Customer { get; set; }

        /// <summary>
        /// Account Number from Database.
        /// </summary>
        [JsonProperty(PropertyName = "accountnumber")]
        public string AccountNumber { get; set; }
    }
}

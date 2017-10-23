using GalaSoft.MvvmLight.Command;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;
using NmeaParser;
using NmeaParser.Nmea.Gps;
using OfflineMaps.Models;
using SerialCommunicationCommands.Common;
using SerialCommunicationCommands.Models.R2;
using SerialCommunicationLibrary.Common;
using SerialCommunicationLibrary.R2.CommandExecution;
using SerialCommunicationLibrary.R2.Response;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Devices.Enumeration;
using Windows.Devices.Geolocation;
using Windows.Devices.SerialCommunication;
using Windows.Storage;
using Windows.Storage.Streams;

namespace OfflineMaps.View_Models
{
    public class OfflineMapsViewModel : ViewModelBase
    {
        //Models
        //DatabaseModel Database = new DatabaseModel();

        //Private objects
        private MobileServiceClient MobileSyncClient;
        private IMobileServiceSyncTable<JsonMeterModel> RouteSyncTable;
        private Queue<JsonMeterModel> MeterSyncQueue = new Queue<JsonMeterModel>();

        private static uint? _geolocatorDefaultAccuracy = 20;
        private Geolocator _geolocator = new Geolocator() { DesiredAccuracyInMeters = _geolocatorDefaultAccuracy, MovementThreshold = 5 };
        private Geoposition _position;
        public SerialDevice R2Device;
        public SerialDevice GPSDevice;
        private SerialPortDevice _serialGPSDevice;
        private Boolean _serialGPSDeviceFound = false;
        private List<string> ReadIds = new List<string>();
        //private string _userFolderToken;
        //private StorageFolder _userFolder;
        //private StorageFile _userFile;

        #region Bindings

        private static string _serviceToken = "Z8JvxTEZ42jbEO6EP8eW~tsH_83MysYFzeeGN_rWL5Q~AhfeEiFHhNnx1xs49tOA468ZABjVrDzN3tyaY51Ac0hfC-u47S_WmuQnwSO2hlHj";
        /// <summary>
        /// [Temp] BingMaps API Service token. 
        /// </summary>
        public string ServiceToken { get { return _serviceToken; } }

        private string _syncStatus = "";
        /// <summary>
        /// Binding for the path of the currently loaded database file.
        /// </summary>
        public string SyncStatus
        {
            get { return _syncStatus; }
            set
            {
                _syncStatus = value;
                NotifyPropertyChanged(nameof(SyncStatus));
            }
        }

        private Geopoint _currentGeopoint = new Geopoint(new BasicGeoposition());
        /// <summary>
        /// Binding for the current position of the GPS device.
        /// </summary>
        public Geopoint CurrentGeopoint
        {
            get { return _currentGeopoint; }
            set
            {
                _currentGeopoint = value;
                //_drivenPositions.Add(_currentGeopoint.Position);
                //RouteDriven = new MapPolyline() { Path = new Geopath(_drivenPositions) };

                NotifyPropertyChanged(nameof(CurrentGeopoint));
            }
        }

        private Boolean _isGeolocatorReady = false;
        /// <summary>
        /// Binding for whether or not the Windows Location service is ready to provide data.
        /// </summary>
        public Boolean IsGeolocatorReady
        {
            get { return _isGeolocatorReady; }
            set
            {
                _isGeolocatorReady = value;
                NotifyPropertyChanged(nameof(IsGeolocatorReady));
            }
        }

        private Boolean _isR2initialized = false;
        /// <summary>
        /// Binding for the initialization status of the R2 device.
        /// </summary>
        public Boolean IsR2Initialized
        {
            get { return _isR2initialized; }
            set
            {
                _isR2initialized = value;
                NotifyPropertyChanged(nameof(IsR2Initialized));
            }
        }

        private Boolean _isR2Listening = false;
        /// <summary>
        /// Binding for the listening status of the R2 device.
        /// </summary>
        public Boolean IsR2Listening
        {
            get { return _isR2Listening; }
            set
            {
                _isR2Listening = value;
                CanStart = IsR2Initialized && !_isR2Listening;
                NotifyPropertyChanged(nameof(IsR2Listening));
            }
        }

        private Boolean _canStart = true;
        /// <summary>
        /// Binding for whether or not the R2 can begin listening.
        /// </summary>
        public Boolean CanStart
        {
            get { return _canStart; }
            set
            {
                _canStart = value;
                CanStop = !_canStart;
                NotifyPropertyChanged(nameof(CanStart));
            }
        }

        private Boolean _canStop = false;
        /// <summary>
        /// Binding for whether or not the R2 can stop listening.
        /// </summary>
        public Boolean CanStop
        {
            get { return _canStop; }
            set
            {
                _canStop = value;
                NotifyPropertyChanged(nameof(CanStop));
            }
        }

        private Boolean _canInitR2 = true;
        /// <summary>
        /// Binding for whether or not the R2 can be initialized.
        /// </summary>
        public Boolean CanInitR2
        {
            get { return _canInitR2; }
            set
            {
                _canInitR2 = value;
                NotifyPropertyChanged(nameof(CanInitR2));
            }
        }

        private Boolean _showRoute = true;
        /// <summary>
        /// Binding for showing the route driven on the map.
        /// </summary>
        public Boolean ShowRoute
        {
            get { return _showRoute; }
            set
            {
                _showRoute = value;
                NotifyPropertyChanged(nameof(ShowRoute));
            }
        }

        private Boolean _useSerialGPS = true;
        /// <summary>
        /// Binding to indicate that the Serial GPS is selected to be used.
        /// </summary>
        public Boolean UseSerialGPS
        {
            get { return _useSerialGPS; }
            set
            {
                _useSerialGPS = value;

                if (_useSerialGPS)
                {
                    _geolocator.PositionChanged -= _geolocator_PositionChanged;
                }
                else if (_serialGPSDevice != null && _serialGPSDevice.IsOpen)
                {
                    CloseAndDisposeSerialGPS();
                    _serialGPSDeviceFound = false;

                    _geolocator.PositionChanged += _geolocator_PositionChanged;
                }

                NotifyPropertyChanged(nameof(UseSerialGPS));
            }
        }

        private MeterModelCollection _meters = new MeterModelCollection();
        /// <summary>
        /// Binding for the collection of MeterModels to be displayed on a map.
        /// </summary>
        public MeterModelCollection Meters
        {
            get { return _meters; }
            set
            {
                _meters = value;
                NotifyPropertyChanged(nameof(Meters));
            }
        }

        private string _r2Packet = "";
        /// <summary>
        /// Binding for the output of formatted R2 data.
        /// </summary>
        public string R2Packet
        {
            get { return _r2Packet; }
            set
            {
                _r2Packet = value;
                NotifyPropertyChanged(nameof(R2Packet));
            }
        }

        private string _nmeaMessage = "";
        /// <summary>
        /// Binding for the last message parsed on the NMEA COM port.
        /// </summary>
        public string NMEAMessage
        {
            get { return _nmeaMessage; }
            set
            {
                _nmeaMessage = value;
                NotifyPropertyChanged(nameof(NMEAMessage));
            }
        }

        private string _geolocatorMessage = "Not Initialized";
        /// <summary>
        /// Binding for the diagnostic message of the geolocator.
        /// </summary>
        public string GeolocatorMessage
        {
            get { return _geolocatorMessage; }
            set
            {
                _geolocatorMessage = value;
                NotifyPropertyChanged(nameof(GeolocatorMessage));
            }
        }

        private int _totalEndpoints = 0;
        /// <summary>
        /// Binding for the total number of loaded endpoints.
        /// </summary>
        public int TotalEndpoints
        {
            get { return _totalEndpoints; }
            set
            {
                _totalEndpoints = value;
                NotifyPropertyChanged(nameof(TotalEndpoints));
            }
        }

        private int _badLocEndpoints = 0;
        /// <summary>
        /// Binding for the total number of endpoints that had bad locations. Not counted in TotalEndpoints.
        /// </summary>
        public int BadLocEndpoints
        {
            get { return _badLocEndpoints; }
            set
            {
                _badLocEndpoints = value;
                NotifyPropertyChanged(nameof(BadLocEndpoints));
            }
        }

        private int _readEndpoints = 0;
        /// <summary>
        /// Binding for the number of endpoints that have been read and updated.
        /// </summary>
        public int ReadEndpoints
        {
            get { return _readEndpoints; }
            set
            {
                _readEndpoints = value;
                NotifyPropertyChanged(nameof(ReadEndpoints));
            }
        }

        private MeterModel _selectedMeter;
        /// <summary>
        /// Binding for the currently selected meter to display its information.
        /// </summary>
        public MeterModel SelectedMeter
        {
            get { return _selectedMeter; }
            set
            {
                _selectedMeter = value;
                NotifyPropertyChanged(nameof(SelectedMeter));
            }
        }

        private uint? _desiredAccuracy = _geolocatorDefaultAccuracy;
        /// <summary>
        /// Binding for the DesiredAccuracyInMeters of the geolocator.
        /// </summary>
        public uint? DesiredAccuracy
        {
            get { return _desiredAccuracy; }
            set
            {
                _desiredAccuracy = value;
                _geolocator.DesiredAccuracyInMeters = _desiredAccuracy;
                NotifyPropertyChanged(nameof(DesiredAccuracy));
            }
        }

        private DeviceInformationCollection _serialDevices;
        /// <summary>
        /// Binding for the list of detected serial devices on COM or Bluetooth ports.
        /// </summary>
        public DeviceInformationCollection SerialDevices
        {
            get { return _serialDevices; }
            set
            {
                _serialDevices = value;
                NotifyPropertyChanged(nameof(SerialDevices));
            }
        }

        private DeviceInformation _selectedDevice;
        /// <summary>
        /// Binding for the currently selected NMEA serial device.
        /// </summary>
        public DeviceInformation SelectedDevice
        {
            get { return _selectedDevice; }
            set
            {
                _selectedDevice = value;
                NotifyPropertyChanged(nameof(SelectedDevice));
            }
        }

        #endregion

        #region Commands

        private ICommand _setGeopositionCommand;
        /// <summary>
        /// Begins Geolocation of the user, and every subsequent exectution recenters the displayed geoposition.
        /// </summary>
        public ICommand SetGeopositionCommand
        {
            get
            {
                if (_setGeopositionCommand == null)
                    _setGeopositionCommand = new RelayCommand(async () => await SetGeoposition());
                return _setGeopositionCommand;
            }
        }
        private async Task SetGeoposition()
        {
            switch (await Geolocator.RequestAccessAsync())
            {
                case GeolocationAccessStatus.Allowed:
                    //Update position and current geopoint
                    _geolocator_PositionChanged(_geolocator, null);
                    break;
                case GeolocationAccessStatus.Unspecified:
                    break;
                case GeolocationAccessStatus.Denied:
                    break;
            }
            //Ensure only one subscription
            _geolocator.PositionChanged -= _geolocator_PositionChanged;
            _geolocator.PositionChanged += _geolocator_PositionChanged;
        }

        private ICommand _initR2Command;
        /// <summary>
        /// Detects and initialized connection to an R2 Device.
        /// </summary>
        public ICommand InitR2Command
        {
            get
            {
                if (_initR2Command == null)
                    _initR2Command = new RelayCommand(async () => await InitR2());
                return _initR2Command;
            }
        }
        private async Task InitR2()
        {
            CanInitR2 = false;
            R2Device = await R2.DetectR2Device();

            if (R2Device != null)
            {
                IsR2Initialized = true;
                //Begin listening if desired
                BeginR2ListenAndSync();
            }
            else
            {
                IsR2Initialized = false;
                CanInitR2 = true;
            }
}

        private ICommand _R2ListenCommand;
        /// <summary>
        /// Begins the Async method of receiving on the R2 port. may be adventagous to start a new thead using TaskFactory and run this loop instead
        /// </summary>
        public ICommand R2ListenCommand
        {
            get
            {
                if (_R2ListenCommand == null)
                    _R2ListenCommand = new RelayCommand(() => BeginR2ListenAndSync());
                return _R2ListenCommand;
            }
        }

        private async void BeginR2ListenAndSync()
        {
            R2Listen();
            await Task.Delay(100);
            //SyncReadQueue();
        }

        /// <summary>
        /// Task for starting the R2 and receiving meters over the COM Port.
        /// </summary>
        /// <returns></returns>
        private async Task R2Listen()
        {
            //Gets any devices associated with the R2 port
            var aqsFilter = SerialDevice.GetDeviceSelector(R2Device.PortName);
            var devices = await DeviceInformation.FindAllAsync(aqsFilter);
            if (devices.Any())
            {
                var deviceId = devices.First().Id;

                //This redundancy ensures the device is supplies a valid datareader object
                using (SerialDevice device = await SerialDevice.FromIdAsync(deviceId))
                {
                    if (device != null)
                    {
                        var readCancellationTokenSource = new CancellationTokenSource();
                        //DataReader dataReaderObject = new DataReader(device.InputStream);

                        //Reading variables
                        R2PacketModel R2Model;
                        int lineCount = 0;
                        StringBuilder OutputBuilder = new StringBuilder();
                        bool packetStart = false, packetEnd = false;
                        string endpointId = "", reading = "", flag = "", rssi = "";
                        DateTime date = new DateTime();
                        TimeSpan time = new TimeSpan();

                        //GetLog tests
                        //R2I8 r = new R2I8();
                        //await r.ExecuteGetLog(device, "7072600", 11520);
                        //var resp = await R2.Listen(device, readCancellationTokenSource, R2PacketHeader.I8LogDetails, R2PacketHeader.I8LogStart, R2PacketHeader.I8LogCompleted, 30, 1);

                        Stream str = device.InputStream.AsStreamForRead(0);
                        IsR2Listening = true;

                        while (IsR2Initialized && IsR2Listening)
                        {
                            byte[] buffer = new byte[1];
                            string response = "";

                            while (!packetStart || !packetEnd)
                            {
                                int readCount = await str.ReadAsync(buffer, 0, 1, readCancellationTokenSource.Token);//.ConfigureAwait(false);

                                string temp = Encoding.UTF8.GetString(buffer);

                                //temp.AddRange(Regex.Split(resp, @"(?<=[$&])"));

                                //string temp = await R2.Listen(device, readCancellationTokenSource, dataReaderObject, 80, true);

                                response += temp;

                                if (temp == "+" || temp == "*")
                                    packetStart = true;
                                else if (temp == "$" || temp == "&")
                                    packetEnd = true;

                                if (!packetStart && packetEnd)
                                    packetEnd = false;
                            }

                            //If a start and end char are found, begin parsing
                            if (packetStart && packetEnd)//foreach (string response in temp)
                            {
                                packetStart = false;
                                packetEnd = false;

                                R2Model = await R2ReadPacketResponseAsync.ParseR2PacketResponse(response);
                                if (R2Model.R2M2WPacketModel != null || R2Model.R2NRDPacketModel != null)
                                {
                                    if (R2Model.R2M2WPacketModel != null)
                                    {
                                        //Set Database Variables
                                        endpointId = R2Model.R2M2WPacketModel.EndpointId;
                                        reading = R2Model.R2M2WPacketModel.Reading;
                                        date = R2Model.R2M2WPacketModel.ReadTime.Date;
                                        time = R2Model.R2M2WPacketModel.ReadTime.TimeOfDay;
                                        flag = R2Model.R2M2WPacketModel.Flag;
                                        rssi = R2Model.R2M2WPacketModel.RSSI;

                                    }
                                    if (R2Model.R2NRDPacketModel != null)
                                    {
                                        //Set Database Variables
                                        endpointId = R2Model.R2NRDPacketModel.EndpointId;
                                        reading = R2Model.R2NRDPacketModel.Reading;
                                        date = R2Model.R2NRDPacketModel.ReadTime.Date;
                                        time = R2Model.R2NRDPacketModel.ReadTime.TimeOfDay;
                                        flag = R2Model.R2NRDPacketModel.Flag;
                                        rssi = R2Model.R2NRDPacketModel.RSSI;
                                    }

                                    //Add to display list
                                    if (lineCount > 21)
                                    {
                                        OutputBuilder.Remove(0, OutputBuilder.ToString().IndexOf('\n') + 1);
                                    }
                                    OutputBuilder.Append("ID: " + endpointId + "\n");
                                    R2Packet = OutputBuilder.ToString();

                                    lineCount++;
                                    //Update Database

                                    if (!(ReadIds.Contains(endpointId)))// && (Meters.Where(meter => meter.MeterInfo.MeterId == endpointId)).Any())
                                    {
                                        MeterModel receivedMeter = Meters.FirstOrDefault(meter => meter.MeterInfo.MeterId == endpointId);

                                        if (receivedMeter != null)
                                        {
                                            int index = Meters.IndexOf(receivedMeter);
                                            Meters[index].MeterInfo.Reading = reading;
                                            Meters[index].ReadUpdated = true;
                                            MeterSyncQueue.Enqueue(Meters[index].MeterInfo);
                                        }
                                        else
                                        {
                                            MeterSyncQueue.Enqueue(new JsonMeterModel() { MeterId = endpointId, Reading = reading });
                                        }

                                        ReadIds.Add(endpointId);
                                        ReadEndpoints++;

                                        //await Task.Delay(10);
                                        ////(@ID, @Read, @Date, @Time, @Codes, @RSSI, @HiUse, @Voltage);";
                                        //Database.UpdateReadCommand.Parameters.Add(new SqliteParameter("Id", endpointId));
                                        //Database.UpdateReadCommand.Parameters.Add(new SqliteParameter("Read", reading));
                                        //Database.UpdateReadCommand.Parameters.Add(new SqliteParameter("Date", date));
                                        //Database.UpdateReadCommand.Parameters.Add(new SqliteParameter("Time", time));
                                        //Database.UpdateReadCommand.Parameters.Add(new SqliteParameter("Codes", flag));
                                        //Database.UpdateReadCommand.Parameters.Add(new SqliteParameter("RSSI", rssi));
                                        ////Database.UpdateReadCommand.Parameters.Add(new SqliteParameter("HiUse", hiUse));
                                        ////Database.UpdateReadCommand.Parameters.Add(new SqliteParameter("Voltage", voltage));

                                        //Database.ExecuteCommand(Database.UpdateReadCommand);

                                        //if (Database.DatabaseFile != null && _userFolder != null)
                                        //{
                                        //    await Database.DatabaseFile.CopyAsync(_userFolder, _userFile.Name, NameCollisionOption.ReplaceExisting);
                                        //}
                                    }
                                    
                                }
                            }
                        }
                        //end thread

                        await PushReadsToCloud();

                        SyncStatus = "Syncronized";
                    }
                }
            }
        }
        /// <summary>
        /// Task for pushing receivied and parsed meters into the local sqlite database.
        /// </summary>
        /// <returns></returns>
        private async Task SyncReadQueue()
        {
            JsonMeterModel MeterToSync;
            while (IsR2Initialized && IsR2Listening)
            {
                SyncStatus = MeterSyncQueue.Count + " Meters in Update Queue";
                if (MeterSyncQueue.Any())
                {
                    try
                    {
                        MeterToSync = MeterSyncQueue.Dequeue();
                        if (Meters.Where(meter => meter.MeterInfo.MeterId == MeterToSync.MeterId).Any())
                        {
                            await RouteSyncTable.UpdateAsync(MeterToSync);
                        }
                        else
                        {
                            await RouteSyncTable.InsertAsync(MeterToSync);
                        }
                    }
                    catch (Exception ex)
                    {
                        SyncStatus = ex.Message;
                    }
                }
            }
        }

        private ICommand _stopR2ListenCommand;
        /// <summary>
        /// Stops listening on the R2 port.
        /// </summary>
        public ICommand StopR2ListenCommand
        {
            get
            {
                if (_stopR2ListenCommand == null)
                    _stopR2ListenCommand = new RelayCommand(() => StopR2Listen());
                return _stopR2ListenCommand;
            }
        }
        private void StopR2Listen()
        {
            IsR2Listening = false;
            //_r2Device.Dispose();
        }

        private ICommand _openCommand;
        /// <summary>
        /// Opens an OpenFolderDialog, then an OpenFileDialog to choose a database file.
        /// </summary>
        public ICommand OpenCommand
        {
            get
            {
                if (_openCommand == null)
                    _openCommand = new RelayCommand(() => Open());
                return _openCommand;
            }
        }
        private async void Open()
        {
            await InitLocalStoreAsync();
            //_userFolder = await ChooseFolder();
            //if (_userFolder != null)
            //{
            //    FileOpenPicker openPicker = new FileOpenPicker()
            //    {
            //        ViewMode = PickerViewMode.List,
            //        SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            //    };
            //    openPicker.FileTypeFilter.Add(".sqlite");
            //    _userFile = await openPicker.PickSingleFileAsync();
            //}

            //if (_userFile != null && _userFolder != null)
            //{
            //    FilePath = _userFile.Path;

            //Clear meters and reset stats
            //Meters.Clear();
            #region Local SQLITE code
            //Database.DatabaseFile = await _userFile.CopyAsync(ApplicationData.Current.LocalFolder, _userFile.Name, NameCollisionOption.ReplaceExisting);
            //Database.Connection.Open();
            //SqliteDataReader reader = Database.SelectMetersCommand.ExecuteReader();
            //if (reader != null)
            //{
            //    while (reader.Read())
            //    {
            //        TotalEndpoints++;
            //        try
            //        {
            //            double _latitude = (double)reader["Latitude"];
            //            double _longitude = (double)reader["Longitude"];

            //            if (_latitude == 0.0 || _longitude == 0.0)
            //                throw new ArgumentOutOfRangeException();
            //            Meters.Add(new MeterModel() {
            //                MeterInfo = new JsonMeterModel()
            //                {
            //                    Latitude = _latitude,
            //                    Longitude = _longitude,
            //                    MeterId = (string)reader["MeterID"],
            //                    RouteNumber = (string)reader["RouteNumber"],
            //                    AccountDescription = (string)reader["AccountDescription"],
            //                    Address = (string)reader["Address"],
            //                    Customer = (string)reader["Customer"],
            //                    AccountNumber = (string)reader["AccountNumber"]
            //                },
            //                Location = new Geopoint(new BasicGeoposition() { Latitude = _latitude, Longitude = _longitude }),
            //                ReadUpdated = false
            //                }
            //            );
            //        }
            //        catch (ArgumentOutOfRangeException aor)
            //        {
            //            //Lookup geocoordinate of an address, works offline

            //            //string address = (string)reader["Address"];
            //            //MapLocationFinderResult result = await MapLocationFinder.FindLocationsAsync(address, referencePoint, 1);
            //            //if (result.Locations.Count == 1)
            //            //{
            //            //    Meters.Add(new MeterModel((string)reader["MeterID"], result.Locations[0].Point.Position.Latitude, result.Locations[0].Point.Position.Longitude, RedImage));
            //            //}
            //            //else
            //            //{
            //            //Meters.Add(new MeterModel((string)reader["MeterID"], 0.0, 0.0, RedImage));
            //            //}
            //            //Logger.WriteLog(aor);
            //            BadLocEndpoints++;
            //        }                 
            //    }
            //}
            //reader.Dispose();
            //Database.Connection.Close();
            #endregion
            //}
        }

        //private ICommand _saveAsCommand;
        ///// <summary>
        ///// Saves database in AppData to disk.
        ///// </summary>
        //public ICommand SaveAsCommand
        //{
        //    get
        //    {
        //        if (_saveAsCommand == null)
        //            _saveAsCommand = new RelayCommand(() => SaveAs());
        //        return _saveAsCommand;
        //    }
        //}
        //private async void SaveAs()
        //{
        //    if (Database.DatabaseFile != null && _userFolder != null)
        //    {
        //         await Database.DatabaseFile.CopyAsync(_userFolder, _userFile.Name, NameCollisionOption.ReplaceExisting);
        //    }
        //}

        //private async Task<StorageFolder> ChooseFolder()
        //{
        //    FolderPicker folderPicker = new FolderPicker()
        //    {
        //        ViewMode = PickerViewMode.List
        //    };
        //    folderPicker.FileTypeFilter.Add(".sqlite");
        //    StorageFolder userFolder = await folderPicker.PickSingleFolderAsync();
        //    if (userFolder != null)
        //        _userFolderToken = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(userFolder);

        //    return userFolder;
        //}

        private ICommand _updateSelectedMeterCommand;
        /// <summary>
        /// Updates the selected meter based on a MeterID.
        /// </summary>
        public ICommand UpdateSelectedMeterCommand
        {
            get
            {
                if (_updateSelectedMeterCommand == null)
                    _updateSelectedMeterCommand = new RelayCommand<string>(param => UpdateSelectedMeter(param));
                return _updateSelectedMeterCommand;
            }
        }
        private void UpdateSelectedMeter(string SelectedMeterID)
        {
            SelectedMeter = Meters.Where(meter => meter.MeterInfo.MeterId == SelectedMeterID).FirstOrDefault();
        }

        #endregion

        public OfflineMapsViewModel()
        {
            Logger.GetLogger().InitiateLogger();
            _geolocator.StatusChanged += _geolocator_StatusChanged;

            Task.Run(async () =>
            {
                //Delays allow Windows to handle serial port closings and flushes

                GetSerialDevices();

                await Task.Delay(1000);
                await FindSerialGPS(SerialDevices);              
            });
        }

        #region Geolocation

        /// <summary>
        /// Event handler for when the Geolocator has updates its ability to provide information.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void _geolocator_StatusChanged(Geolocator sender, StatusChangedEventArgs args)
        { 
            GeolocatorMessage = "Geolocator: " + _geolocator.LocationStatus.ToString();
            if (sender.LocationStatus == PositionStatus.Ready)
            {
                IsGeolocatorReady = true;
            }
            else
                IsGeolocatorReady = false;

        }

        /// <summary>
        /// Event Handler for when the Geolocator has a position update.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void _geolocator_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            try
            {
                GeolocatorMessage = "Position Updating...";
                IsGeolocatorReady = false;
                _position = await _geolocator.GetGeopositionAsync();
                IsGeolocatorReady = true;
                GeolocatorMessage = _geolocator.LocationStatus.ToString();

                CurrentGeopoint = CreateGeopoint(_position.Coordinate.Point.Position.Latitude, _position.Coordinate.Point.Position.Longitude);
            }
            catch (Exception ex)
            {
                GeolocatorMessage = ex.Message;
                IsGeolocatorReady = true;
                //Likely a PipeClosed exception.  GPS Does not have signal.
                //Logger.WriteLog(ex);
            }
        }

        /// <summary>
        /// Gets all serial devices connected to the computer and sets the SerialDevices collection
        /// </summary>
        public async void GetSerialDevices()
        {
            try
            {
                //Gets all COM and Blutooth ports
                var aqs = SerialDevice.GetDeviceSelector();
                SerialDevices = await DeviceInformation.FindAllAsync(aqs);
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// Opens every devcie in the collection and listens for an NMEA message.
        /// </summary>
        /// <param name="Devices">
        /// The collection of devices to be searched.</param>
        /// <returns></returns>
        public async Task FindSerialGPS(DeviceInformationCollection Devices)
        {
            try
            {
                foreach (DeviceInformation deviceInfo in Devices)
                {
                    if (!_serialGPSDeviceFound)
                    {
                        NMEAMessage = "Listening on " + deviceInfo.Name;

                        _serialGPSDevice = await InitGPSAsync(deviceInfo);
                        if (_serialGPSDevice != null)
                        {
                            //Subscribe and open port to check if it is NMEA
                            _serialGPSDevice.MessageReceived += _serialGPSDevice_MessageReceived;
                            await _serialGPSDevice.OpenAsync();

                            //delay 1000ms for safe guarenteed message read.
                            await Task.Delay(1000);

                            if (_serialGPSDeviceFound)
                            {
                                SelectedDevice = deviceInfo;
                            }
                            else
                            {
                                //Close if device is not found
                                CloseAndDisposeSerialGPS();
                            }
                        }
                    }
                }
                NMEAMessage = "No GPS Found";
            }
            catch (Exception ex)
            {
                Logger.WriteLog(ex);
            }
        }

        /// <summary>
        /// Initializes _serialGPSDevice based on the DeviceInformation.
        /// </summary>
        /// <param name="DeviceInfo">
        /// The device to be initialized.
        /// </param>
        /// <returns></returns>
        private async Task<SerialPortDevice> InitGPSAsync(DeviceInformation DeviceInfo)
        {
            if (_serialGPSDevice != null && _serialGPSDevice.IsOpen)
            {
                //Close if a device is currently in use.
                CloseAndDisposeSerialGPS();
            }

            SerialDevice serialPort = await SerialDevice.FromIdAsync(DeviceInfo.Id);
            if (serialPort != null)
            {
                serialPort.BaudRate = 4800;
                serialPort.DataBits = 8;
                serialPort.Parity = SerialParity.None;
                serialPort.StopBits = SerialStopBitCount.One;

                ///IMPORTANT 
                ///Makes sure that the serial port parameters are actually set.
                ///https://github.com/dotMorten/NmeaParser/issues/13
                ///
                var _baud = serialPort.BaudRate;
                var _data = serialPort.DataBits;
                var _pair = serialPort.Parity;
                var _stop = serialPort.StopBits;
                ///IMPORTANT

                return new SerialPortDevice(serialPort);
            }
            return null;
        }

        /// <summary>
        /// Event handler for the SerialGPS, parses the NMEA message and updates CurrentGeopoint.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _serialGPSDevice_MessageReceived(object sender, NmeaMessageReceivedEventArgs e)
        {
            if (!_serialGPSDeviceFound) _serialGPSDeviceFound = true;

            double Latitude = 0.0, Longitude = 0.0;

            NMEAMessage = e.Message.ToString();
            
            //Parse all NMEA messages supported by BU353-s4.
            //Only GGA, RMC, and GLL support location
            switch(e.Message.MessageType)
            {
                case "GPGGA":
                    Gpgga gga = (Gpgga)e.Message;
                    Latitude = gga.Latitude;
                    Longitude = gga.Longitude;
                    
                    break;
                case "GPRMC":
                    Gprmc rmc = (Gprmc)e.Message;
                    Latitude = rmc.Latitude;
                    Longitude = rmc.Longitude;

                    break;
                case "GPGLL":
                    Gpgll gll = (Gpgll)e.Message;
                    Latitude = gll.Latitude;
                    Longitude = gll.Longitude;
                    
                    break;
                case "GPGSA":
                    Gpgsa gsa = (Gpgsa)e.Message;
                    
                    break;
                case "GPGSV":
                    Gpgsv gsv = (Gpgsv)e.Message;
                    break;
            }

            //make sure the delta-p is above a threshold
            if (Latitude != 0.0 && Longitude != 0.0)
            {
                double distance = Haversine(Latitude, Longitude, CurrentGeopoint.Position.Latitude, CurrentGeopoint.Position.Longitude);
                if (distance > 0) //2.5m is minimum accuracy of BU-353 S4 and WAAS/SBAS corrections
                {
                    CurrentGeopoint = CreateGeopoint(Latitude, Longitude);
                }
            }
        }

        /// <summary>
        /// Takes all necessary steps to close _serialGPSDevice.
        /// </summary>
        private async void CloseAndDisposeSerialGPS()
        {
            try
            {
                await _serialGPSDevice.CloseAsync();
                _serialGPSDevice.SerialDevice.Dispose();
                //_serialGPSDevice = null;
            }
            catch (Exception ex)
            {
                //If the SerialDevice.Dispose() throws an error, it is ignored
            }
        }

        /// <summary>
        /// Creates a Geopoint at altitude 0, referenced to the terrain.
        /// </summary>
        /// <param name="Latitude">Latitude</param>
        /// <param name="Longitude">Longitude</param>
        /// <returns>new Geopoint at altitude 0.</returns>
        private Geopoint CreateGeopoint(double Latitude, double Longitude)
        {
            //Altitudereference terrain puts the point on the surface.
            return new Geopoint(new BasicGeoposition() { Latitude = Latitude, Longitude = Longitude, Altitude = 0 }, AltitudeReferenceSystem.Terrain);
        }

        /// <summary>
        /// https://stackoverflow.com/a/41623738
        /// Calulates the Great Circle or surface distance on earth.
        /// </summary>
        private static double Haversine(double lat1, double lon1, double lat2, double lon2)
        {
            const double r = 6371; // meters

            var sdlat = Math.Sin((lat2 - lat1) / 2);
            var sdlon = Math.Sin((lon2 - lon1) / 2);
            var q = sdlat * sdlat + Math.Cos(lat1) * Math.Cos(lat2) * sdlon * sdlon;
            var d = 2 * r * Math.Asin(Math.Sqrt(q));

            return d;
        }
        #endregion

        #region R2

        #endregion

        #region Offline Sync

        private async Task InitLocalStoreAsync()
        {
            MobileSyncClient = new MobileServiceClient("http://qag2mobileapp.azurewebsites.net/");
            RouteSyncTable = MobileSyncClient.GetSyncTable<JsonMeterModel>();
            if (!MobileSyncClient.SyncContext.IsInitialized)
            {
                MobileServiceSQLiteStore store = new MobileServiceSQLiteStore("localstore.sqlite");
                store.DefineTable<JsonMeterModel>();

                await MobileSyncClient.SyncContext.InitializeAsync(store);
            }

            await SyncAsync();
        }

        private async Task SyncAsync()
        {
            try
            {
                await PushReadsToCloud();

                SyncStatus = "Pulling...";
                await RouteSyncTable.PullAsync("PullQuery", RouteSyncTable.CreateQuery());
                SyncStatus = "Syncronized";

                ReadEndpoints = 0;
                TotalEndpoints = 0;
                BadLocEndpoints = 0;
            }
            catch (Exception e)
            {
                SyncStatus = "Error Syncronizing: " + e.Message;
            }
            finally
            {
                IEnumerable<JsonMeterModel> SyncMeters = await RouteSyncTable.ReadAsync();
                foreach (JsonMeterModel model in SyncMeters)
                {
                    Meters.Add(new MeterModel()
                    {
                        MeterInfo = model,
                        Location = new Geopoint(new BasicGeoposition() { Latitude = model.Latitude, Longitude = model.Longitude }),
                        ReadUpdated = false
                    });
                    if (Meters.Count == 20)
                    {
                        break;
                    }
                    //await Task.Delay(1);
                }
            }
        }

        private async Task PushReadsToCloud()
        {
            CancellationTokenSource token = new CancellationTokenSource();

            Task.Run(async () =>
            {
                while (!token.Token.IsCancellationRequested)
                {
                    SyncStatus = "Pushing " + MobileSyncClient.SyncContext.PendingOperations + " Records.";
                    await Task.Delay(1000);
                }
            });

            await MobileSyncClient.SyncContext.PushAsync();
            token.Cancel();
        }
        #endregion
    }
}

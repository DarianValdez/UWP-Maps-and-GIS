using GalaSoft.MvvmLight.Command;
using SerialCommunicationLibrary.R2.CommandExecution;
using System;
using System.Linq;
using System.Windows.Input;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;

namespace OfflineMaps.View_Models
{
    class R2ConfigViewModel : ViewModelBase
    {
        #region Bindings

        private SerialDevice _r2Device;
        public SerialDevice R2Device
        {
            get { return _r2Device; }
            set
            {
                _r2Device = value;
                NotifyPropertyChanged(nameof(R2Device));
            }
        }

        private double _frequency = 0.0;
        public double Frequency
        {
            get { return _frequency; }
            set
            {
                _frequency = value;
                NotifyPropertyChanged(nameof(Frequency));
            }
        }

        private double _workingFrequency = 0.0;
        public double WorkingFrequency
        {
            get { return _workingFrequency; }
            set
            {
                _workingFrequency = value;
                NotifyPropertyChanged(nameof(WorkingFrequency));
            }
        }

        private string _firmwareVer = "";
        public string FirmwareVer
        {
            get { return _firmwareVer; }
            set
            {
                _firmwareVer = value;
                NotifyPropertyChanged(nameof(FirmwareVer));
            }
        }

        private string _serialNum = "";
        public string SerialNum
        {
            get { return _serialNum; }
            set
            {
                _serialNum = value;
                NotifyPropertyChanged(nameof(SerialNum));
            }
        }

        private Boolean _rxSensitivity;
        /// <summary>
        /// True = On
        /// False = Off
        /// </summary>
        public Boolean RxSensitivity
        {
            get { return _rxSensitivity; }
            set
            {
                _rxSensitivity = value;
                NotifyPropertyChanged(nameof(RxSensitivity));
            }
        }

        private Boolean _rxBandwidth;
        /// <summary>
        /// True = High Bandwidth
        /// False = Low Bandwidth
        /// </summary>
        public Boolean RxBandwidth
        {
            get { return _rxBandwidth; }
            set
            {
                _rxBandwidth = value;
                NotifyPropertyChanged(nameof(RxBandwidth));
            }
        }

        private Boolean _r2Optimization;
        /// <summary>
        /// True = Distance
        /// False = Data Rate
        /// </summary>
        public Boolean R2Optimization
        {
            get { return _r2Optimization; }
            set
            {
                _r2Optimization = value;
                NotifyPropertyChanged(nameof(R2Optimization));
            }
        }

        #endregion

        #region Commands
        //private async void GetCurrentPort(Func<SerialDevice, double, double> myMethodName)
        //{
        //    //return null;
        //}

        private ICommand _getFreqCommand;
        /// <summary>
        /// Gets the currently connected R2 frequency.
        /// </summary>
        public ICommand GetFreqCommand
        {
            get
            {
                if (_getFreqCommand == null)
                    _getFreqCommand = new RelayCommand(() => GetFreq());
                return _getFreqCommand;
            }
        }
        private async void GetFreq()
        {
            var aqsFilter = SerialDevice.GetDeviceSelector(R2Device.PortName);
            var devices = await DeviceInformation.FindAllAsync(aqsFilter);
            if (devices.Any())
            {
                var deviceId = devices.First().Id;
                using (var serialPort = await SerialDevice.FromIdAsync(deviceId))
                {
                    if (serialPort != null)
                    {
                        Frequency = await R2.GetFrequency(serialPort);
                    }
                }
            }
        }

        private ICommand _setFreqCommand;
        /// <summary>
        /// Sets the currently connected R2 frequency.
        /// </summary>
        public ICommand SetFreqCommand
        {
            get
            {
                if (_setFreqCommand == null)
                    _setFreqCommand = new RelayCommand(() => SetFreq());
                return _setFreqCommand;
            }
        }
        private async void SetFreq()
        {
            var aqsFilter = SerialDevice.GetDeviceSelector(R2Device.PortName);
            var devices = await DeviceInformation.FindAllAsync(aqsFilter);
            if (devices.Any())
            {
                var deviceId = devices.First().Id;
                using (var serialPort = await SerialDevice.FromIdAsync(deviceId))
                {
                    if (serialPort != null)
                    {
                        await R2.SetFrequency(serialPort, Frequency);
                    }
                }
            }
        }

        private ICommand _getWorkingFreqCommand;
        /// <summary>
        /// Gets the currently connected R2 WorkingFrequency.
        /// </summary>
        public ICommand GetWorkingFreqCommand
        {
            get
            {
                if (_getWorkingFreqCommand == null)
                    _getWorkingFreqCommand = new RelayCommand(() => GetWorkingFreq());
                return _getWorkingFreqCommand;
            }
        }
        private async void GetWorkingFreq()
        {
            var aqsFilter = SerialDevice.GetDeviceSelector(R2Device.PortName);
            var devices = await DeviceInformation.FindAllAsync(aqsFilter);
            if (devices.Any())
            {
                var deviceId = devices.First().Id;
                using (var serialPort = await SerialDevice.FromIdAsync(deviceId))
                {
                    if (serialPort != null)
                    {
                        //WorkingFrequency = await R2.GetWorkingFrequency(serialPort);
                    }
                }
            }
        }

        private ICommand _setWorkingFreqCommand;
        /// <summary>
        /// Sets the currently connected R2 WorkingFrequency.
        /// </summary>
        public ICommand SetWorkingFreqCommand
        {
            get
            {
                if (_setWorkingFreqCommand == null)
                    _setWorkingFreqCommand = new RelayCommand(() => SetWorkingFreq());
                return _setWorkingFreqCommand;
            }
        }
        private async void SetWorkingFreq()
        {
            var aqsFilter = SerialDevice.GetDeviceSelector(R2Device.PortName);
            var devices = await DeviceInformation.FindAllAsync(aqsFilter);
            if (devices.Any())
            {
                var deviceId = devices.First().Id;
                using (var serialPort = await SerialDevice.FromIdAsync(deviceId))
                {
                    if (serialPort != null)
                    {
                        //await R2.SetWorkingFrequency(serialPort, WorkingFrequency);
                    }
                }
            }
        }

        private ICommand _getFirmwareCommand;
        /// <summary>
        /// Gets the currently connected R2 Firmwareuency.
        /// </summary>
        public ICommand GetFirmwareCommand
        {
            get
            {
                if (_getFirmwareCommand == null)
                    _getFirmwareCommand = new RelayCommand(() => GetFirmware());
                return _getFirmwareCommand;
            }
        }
        private async void GetFirmware()
        {
            var aqsFilter = SerialDevice.GetDeviceSelector(R2Device.PortName);
            var devices = await DeviceInformation.FindAllAsync(aqsFilter);
            if (devices.Any())
            {
                var deviceId = devices.First().Id;
                using (var serialPort = await SerialDevice.FromIdAsync(deviceId))
                {
                    if (serialPort != null)
                    {
                        FirmwareVer = await R2.GetFirmware(serialPort);
                    }
                }
            }
        }

        private ICommand _getSerialNumCommand;
        /// <summary>
        /// Gets the currently connected R2 SerialNumuency.
        /// </summary>
        public ICommand GetSerialNumCommand
        {
            get
            {
                if (_getSerialNumCommand == null)
                    _getSerialNumCommand = new RelayCommand(() => GetSerialNum());
                return _getSerialNumCommand;
            }
        }
        private async void GetSerialNum()
        {
            var aqsFilter = SerialDevice.GetDeviceSelector(R2Device.PortName);
            var devices = await DeviceInformation.FindAllAsync(aqsFilter);
            if (devices.Any())
            {
                var deviceId = devices.First().Id;
                using (var serialPort = await SerialDevice.FromIdAsync(deviceId))
                {
                    if (serialPort != null)
                    {
                        //SerialNum = await R2.GetSerialNumber(serialPort);
                    }
                }
            }
        }

        private ICommand _getRxSensCommand;
        /// <summary>
        /// Gets the currently connected R2 RxSensuency.
        /// </summary>
        public ICommand GetRxSensCommand
        {
            get
            {
                if (_getRxSensCommand == null)
                    _getRxSensCommand = new RelayCommand(() => GetRxSens());
                return _getRxSensCommand;
            }
        }
        private async void GetRxSens()
        {
            var aqsFilter = SerialDevice.GetDeviceSelector(R2Device.PortName);
            var devices = await DeviceInformation.FindAllAsync(aqsFilter);
            if (devices.Any())
            {
                var deviceId = devices.First().Id;
                using (var serialPort = await SerialDevice.FromIdAsync(deviceId))
                {
                    if (serialPort != null)
                    {
                        //RxSensuency = await R2.GetRxSensuency(serialPort);
                    }
                }
            }
        }

        private ICommand _setRxSensCommand;
        /// <summary>
        /// Sets the currently connected R2 RxSensuency.
        /// </summary>
        public ICommand SetRxSensCommand
        {
            get
            {
                if (_setRxSensCommand == null)
                    _setRxSensCommand = new RelayCommand(() => SetRxSens());
                return _setRxSensCommand;
            }
        }
        private async void SetRxSens()
        {
            var aqsFilter = SerialDevice.GetDeviceSelector(R2Device.PortName);
            var devices = await DeviceInformation.FindAllAsync(aqsFilter);
            if (devices.Any())
            {
                var deviceId = devices.First().Id;
                using (var serialPort = await SerialDevice.FromIdAsync(deviceId))
                {
                    if (serialPort != null)
                    {
                        //await R2.SetRxSensuency(serialPort, RxSensuency);
                    }
                }
            }
        }

        private ICommand _getRxBandwidthCommand;
        /// <summary>
        /// Gets the currently connected R2 RxBandwidthuency.
        /// </summary>
        public ICommand GetRxBandwidthCommand
        {
            get
            {
                if (_getRxBandwidthCommand == null)
                    _getRxBandwidthCommand = new RelayCommand(() => GetRxBandwidth());
                return _getRxBandwidthCommand;
            }
        }
        private async void GetRxBandwidth()
        {
            var aqsFilter = SerialDevice.GetDeviceSelector(R2Device.PortName);
            var devices = await DeviceInformation.FindAllAsync(aqsFilter);
            if (devices.Any())
            {
                var deviceId = devices.First().Id;
                using (var serialPort = await SerialDevice.FromIdAsync(deviceId))
                {
                    if (serialPort != null)
                    {
                        //RxBandwidthuency = await R2.GetRxBandwidthuency(serialPort);
                    }
                }
            }
        }

        private ICommand _setRxBandwidthCommand;
        /// <summary>
        /// Sets the currently connected R2 RxBandwidthuency.
        /// </summary>
        public ICommand SetRxBandwidthCommand
        {
            get
            {
                if (_setRxBandwidthCommand == null)
                    _setRxBandwidthCommand = new RelayCommand(() => SetRxBandwidth());
                return _setRxBandwidthCommand;
            }
        }
        private async void SetRxBandwidth()
        {
            var aqsFilter = SerialDevice.GetDeviceSelector(R2Device.PortName);
            var devices = await DeviceInformation.FindAllAsync(aqsFilter);
            if (devices.Any())
            {
                var deviceId = devices.First().Id;
                using (var serialPort = await SerialDevice.FromIdAsync(deviceId))
                {
                    if (serialPort != null)
                    {
                        //await R2.SetRxBandwidthuency(serialPort, RxBandwidthuency);
                    }
                }
            }
        }

        private ICommand _getR2OptimizationCommand;
        /// <summary>
        /// Gets the currently connected R2 R2Optimizationuency.
        /// </summary>
        public ICommand GetR2OptimizationCommand
        {
            get
            {
                if (_getR2OptimizationCommand == null)
                    _getR2OptimizationCommand = new RelayCommand(() => GetR2Optimization());
                return _getR2OptimizationCommand;
            }
        }
        private async void GetR2Optimization()
        {
            var aqsFilter = SerialDevice.GetDeviceSelector(R2Device.PortName);
            var devices = await DeviceInformation.FindAllAsync(aqsFilter);
            if (devices.Any())
            {
                var deviceId = devices.First().Id;
                using (var serialPort = await SerialDevice.FromIdAsync(deviceId))
                {
                    if (serialPort != null)
                    {
                        string n = await R2.GetR2Optimization(serialPort);
                    }
                }
            }
        }

        private ICommand _setR2OptimizationCommand;
        /// <summary>
        /// Sets the currently connected R2 R2Optimizationuency.
        /// </summary>
        public ICommand SetR2OptimizationCommand
        {
            get
            {
                if (_setR2OptimizationCommand == null)
                    _setR2OptimizationCommand = new RelayCommand(() => SetR2Optimization());
                return _setR2OptimizationCommand;
            }
        }
        private async void SetR2Optimization()
        {
            var aqsFilter = SerialDevice.GetDeviceSelector(R2Device.PortName);
            var devices = await DeviceInformation.FindAllAsync(aqsFilter);
            if (devices.Any())
            {
                var deviceId = devices.First().Id;
                using (var serialPort = await SerialDevice.FromIdAsync(deviceId))
                {
                    if (serialPort != null)
                    {
                        //await R2.SetR2Optimizationuency(serialPort, R2Optimizationuency);
                    }
                }
            }
        }
        #endregion

        public R2ConfigViewModel()
        { }
    }
}

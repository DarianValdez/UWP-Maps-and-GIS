using Extensions.AnimatedHub;
using OfflineMaps.View_Models;
using OfflineMaps.Views;
using System;
using System.Collections.Generic;
using Windows.Devices.Enumeration;
using Windows.Devices.Geolocation;
using Windows.Devices.SerialCommunication;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Media;

namespace OfflineMaps
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private List<BasicGeoposition> _drivenPositions = new List<BasicGeoposition>();

        public MainPage()
        {
            this.InitializeComponent();
            ((OfflineMapsViewModel)this.DataContext).PropertyChanged += MainPage_PropertyChanged;
        }

        private async void MainPage_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(OfflineMapsViewModel.CurrentGeopoint))
            {
                _drivenPositions.Add(((OfflineMapsViewModel)sender).CurrentGeopoint.Position);
                if (((OfflineMapsViewModel)sender).ShowRoute)
                {
                    Map.MapElements.Clear();
                    Map.MapElements.Add(new MapPolyline() { Path = new Geopath(_drivenPositions), StrokeThickness = 4 });
                }
                await Map.TrySetViewAsync(((OfflineMapsViewModel)sender).CurrentGeopoint, Map.ZoomLevel);
            }
            else if (e.PropertyName == nameof(OfflineMapsViewModel.ShowRoute))
            {
                Map.MapElements.Clear();
                if (((OfflineMapsViewModel)sender).ShowRoute)
                {
                    Map.MapElements.Add(new MapPolyline() { Path = new Geopath(_drivenPositions), StrokeThickness = 4});
                }
            }
            
        }

        public void ScrollToSectionAnimated(Hub hub, HubSection section)
        {
            GeneralTransform transform = section.TransformToVisual(hub);
            Point point = transform.TransformPoint(new Point(0, 0));
            ScrollViewer viewer = Hub.GetFirstDescendantOfType<ScrollViewer>();
            viewer.ChangeView(point.X, null, null, false);
        }

        private void BtnMainMenu_Click(object sender, RoutedEventArgs e)
        {
            foreach (HubSection section in Hub.SectionsInView)
            {
                Frame frame = section.GetFirstDescendantOfType<Frame>();
                if (frame != null)
                    frame.Content = null;
            }
            ScrollToSectionAnimated(Hub, HubMain);
            BtnMainMenu.Visibility = Visibility.Collapsed;
        }

        private void BtnRoute_Click(object sender, RoutedEventArgs e)
        {
            ScrollToSectionAnimated(Hub, HubRoute);
            BtnMainMenu.Visibility = Visibility.Visible;
        }

        private void BtnR2Config_Click(object sender, RoutedEventArgs e)
        {
            if(((OfflineMapsViewModel)this.DataContext).R2Device != null)
            {
                Frame frame = HubR2Config.GetFirstDescendantOfType<Frame>();
                frame.Navigate(typeof(R2ConfigView), ((OfflineMapsViewModel)this.DataContext).R2Device);
                HubR2Config.Header = "R2 Configuration";

                ScrollToSectionAnimated(Hub, HubR2Config);
                BtnMainMenu.Visibility = Visibility.Visible;
            }
        }

        private void BtnGPSConfig_Click(object sender, RoutedEventArgs e)
        {
            ((OfflineMapsViewModel)this.DataContext).GetSerialDevices();
            HubGPSConfig.GetFirstDescendantOfType<ComboBox>().SelectedItem = ((OfflineMapsViewModel)this.DataContext).SelectedDevice;
            ScrollToSectionAnimated(Hub, HubGPSConfig);
            BtnMainMenu.Visibility = Visibility.Visible;
        }

        private void BtnDatabase_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void BoxDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
             if (((ComboBox)sender).SelectedItem != null)
            {
                if (((OfflineMapsViewModel)this.DataContext).UseSerialGPS)
                {
                    DeviceInformationCollection SerialDevices = null;
                    using (SerialDevice device = await SerialDevice.FromIdAsync(((DeviceInformation)((ComboBox)sender).SelectedItem).Id))
                    {
                        if (device != null)
                        {
                            var aqs = SerialDevice.GetDeviceSelector(device.PortName);
                            SerialDevices = await DeviceInformation.FindAllAsync(aqs);
                        }
                    }

                    if(SerialDevices != null)
                        await ((OfflineMapsViewModel)this.DataContext).FindSerialGPS(SerialDevices);
                }
            }
        }

        private void BtnNMEA_Checked(object sender, RoutedEventArgs e)
        {
            if (HubGPSConfig.GetFirstDescendantOfType<ComboBox>().SelectedItem != null)
            {
                BoxDevices_SelectionChanged(HubGPSConfig.GetFirstDescendantOfType<ComboBox>(), null);
            }
        }
    }
}

using OfflineMaps.Models;
using System;
using System.Collections.ObjectModel;
using System.Text;
using Windows.Devices.Geolocation;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace OfflineMaps.View_Models
{
    public class GeopositionToString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            BasicGeoposition _pos = (BasicGeoposition)value;
            string param = (string)parameter;
            return _pos.Latitude.ToString(param) + ", " + _pos.Longitude.ToString(param);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return System.Convert.ToDouble((string)value);
        }
    }

    public class BooleanNotConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return !((Boolean)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return !((Boolean)value);
        }
    }

    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Boolean VisibleState = System.Convert.ToBoolean(parameter);
            return (Visibility)System.Convert.ToInt32(VisibleState && !(Boolean)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return !System.Convert.ToBoolean((int)value);
        }
    }

    public class DoubleToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return System.Convert.ToString((double)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return System.Convert.ToDouble((string)value);
        }
    }

    public class BooleanToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if((bool)value)
                return new SolidColorBrush(Colors.LimeGreen);
            else
                return new SolidColorBrush(Colors.Red);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class BooleanToUriConverter : IValueConverter
    {
        /// <summary>
        /// Red Pin URI
        /// </summary>
        public static Uri RedImage = new Uri("ms-appx:///Assets/pushpin_Red.png");
        /// <summary>
        /// Green Pin URI
        /// </summary>
        public static Uri GreenImage = new Uri("ms-appx:///Assets/pushpin_Green.png");

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((Boolean)value == true)
                return GreenImage;
            else
                return RedImage;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if ((Uri)value == GreenImage)
                return true;
            else
                return false;
        }
    }

    public class JsonMeterToMapItemConverter : IValueConverter
    {
        //<Maps:MapItemsControl x:Name="Meters" ItemsSource="{Binding Meters}">
        //            <Maps:MapItemsControl.ItemTemplate>
        //                <DataTemplate>
        //                    <StackPanel Maps:MapControl.NormalizedAnchorPoint="{Binding AnchorPoint}">
        //                        <Button x:Name="MapItemButton" Background="Transparent" Padding="0,-3,-3,0" Content="{Binding MeterInfo.MeterID}" Command="{Binding DataContext.UpdateSelectedMeterCommand, ElementName=Map, Mode=OneWay}" CommandParameter="{Binding MeterInfo.MeterID}">
        //                        </Button>

        //                        <Image Maps:MapControl.Location="{Binding Location}">
        //                            <Image.Source>
        //                                <BitmapImage UriSource = "{Binding ReadUpdated}" />
        //                            </ Image.Source >
        //                        </ Image >
        //                    </ StackPanel >
        //                </ DataTemplate >
        //            </ Maps:MapItemsControl.ItemTemplate>
        //        </Maps:MapItemsControl>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            MeterModelCollection Meters = (MeterModelCollection)value;
            Uri imgUri = new Uri("ms-appx:///Assets/pushpin_Red.png");
            MapItemsControl MapControls = new MapItemsControl();
            
            foreach (MeterModel Meter in Meters)
            {
                if (Meter.ReadUpdated)
                    imgUri = new Uri("ms-appx:///Assets/pushpin_Green.png");

                MapIcon icon = new MapIcon()
                {
                    Image = RandomAccessStreamReference.CreateFromUri(imgUri),
                    NormalizedAnchorPoint = new Windows.Foundation.Point(0.5, 1.0),
                    Location = new Geopoint(new BasicGeoposition() { Latitude = Meter.MeterInfo.Latitude, Longitude = Meter.MeterInfo.Longitude }),
                    Title = Meter.MeterInfo.MeterId
                };

                MapControls.Items.Add(icon);
            }
            return MapControls.Items;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

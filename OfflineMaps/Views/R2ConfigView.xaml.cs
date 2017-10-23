using OfflineMaps.View_Models;
using Windows.Devices.SerialCommunication;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace OfflineMaps.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class R2ConfigView : Page
    {
        public R2ConfigView()
        {
            this.InitializeComponent();
            this.DataContext = new R2ConfigViewModel();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ((R2ConfigViewModel)this.DataContext).R2Device = (SerialDevice)e.Parameter;
        }
    }
}

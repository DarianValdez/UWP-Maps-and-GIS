using System;
using System.ComponentModel;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace OfflineMaps.View_Models
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        private CoreDispatcher _dispatcher = CoreApplication.GetCurrentView().CoreWindow.Dispatcher;
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notifies to the View that the property has changed. Invokes the change on the current view Dispatcher.
        /// </summary>
        /// <param name="PropertyName">Name of the property that was changed.</param>
        public async void NotifyPropertyChanged(string PropertyName)
        {
            await _dispatcher.TryRunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
                });
        }
    }
}

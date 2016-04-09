using Prism.Windows.Mvvm;
using System.Collections.Generic;
using Prism.Windows.Navigation;
using System.Windows.Input;
using Prism.Commands;
using Microsoft.Band;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Band.Sensors;
using Windows.ApplicationModel.Core;
using System;
using System.Runtime.CompilerServices;
using HelloBand.BandTiles;
using Microsoft.Band.Tiles;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Microsoft.Band.Notifications;
using Microsoft.Band.Tiles.Pages;

namespace HelloBand.ViewModels
{
    class MainPageViewModel : ViewModelBase, IMainPageViewModel
    {
        private readonly IBandClientManager bandClientManager;
        private IBandClient bandClient;

        //TODO: For your own applications, generate new, unique GUIDs.
        private readonly Guid tileGuid = Guid.Parse("F21F3CB6-8A45-4472-8B00-E5764AD0B022");
        private readonly Guid garagePageGuid = Guid.Parse("6D88EBE1-7773-4E82-B310-7B7907C55BD7");
        private readonly Guid garageControlPageGuid = Guid.Parse("451B5BB2-D9CE-4E36-AAE5-1DE4DC680493");
        private readonly Guid barcodePageGuid = Guid.Parse("E9DD36FD-F993-4C08-A42A-EB15DEED60D2");
        private BandTile tile;

        public MainPageViewModel(IBandClientManager bandClientManager)
        {
            this.bandClientManager = bandClientManager;

            // Set up ICommand targets for events from the view
            this.OnConnect = DelegateCommand.FromAsyncHandler(OnConnectExecute);
            this.OnSubscribeToAccelerometer = DelegateCommand.FromAsyncHandler(OnSubscribeToAccelerometerExecute, () => IsConnected);
            this.OnSubscribeToHeartRate = DelegateCommand.FromAsyncHandler(OnSubscribeToHearthRateExecute, () => IsConnected);
            this.OnDoorbellRing = DelegateCommand.FromAsyncHandler(OnDoorbellRingExecute, () => IsConnected);
            this.OnGarageOpen = DelegateCommand.FromAsyncHandler(OnGarageOpenExecute, () => IsConnected);
            this.OnGarageClose = DelegateCommand.FromAsyncHandler(OnGarageCloseExecute, () => IsConnected);
            this.OnSendBarcode = DelegateCommand.FromAsyncHandler(OnSendBarcodeExecute, () => IsConnected);
        }


        #region IMainPageViewModel
        public ICommand OnConnect { get; protected set; }
        public ICommand OnSubscribeToAccelerometer { get; protected set; }
        public ICommand OnSubscribeToHeartRate { get; protected set; }
        public ICommand OnDoorbellRing { get; protected set; }
        public ICommand OnGarageOpen { get; protected set; }
        public ICommand OnGarageClose { get; protected set; }
        public ICommand OnSendBarcode { get; protected set; }


        private string statusMessage;
        public string StatusMessage
        {
            get { return statusMessage; }
            protected set { SetProperty(ref statusMessage, value); }
        }


        private bool isConnected = false;
        public bool IsConnected
        {
            get { return isConnected; }
            protected set { SetProperty(ref isConnected, value); }
        }

        double accelerationX;
        public double AccelerationX
        {
            get { return accelerationX; }
            set { SetProperty(ref accelerationX, value); }
        }

        double accelerationY;
        public double AccelerationY
        {
            get { return accelerationY; }
            set { SetProperty(ref accelerationY, value); }
        }

        double accelerationZ;
        public double AccelerationZ
        {
            get { return accelerationZ; }
            set { SetProperty(ref accelerationZ, value); }
        }


        private int heartRate;
        public int HeartRate
        {
            get { return heartRate; }
            set { SetProperty(ref heartRate, value); }
        }

        private string barcodeText;
        public string BarcodeText
        {
            get { return barcodeText; }
            set { SetProperty(ref barcodeText, value); }
        }

        private string firmwareVersion;
        public string FirmwareVersion
        {
            get { return firmwareVersion; }
            protected set { SetProperty(ref firmwareVersion, value); }
        }

        private string hardwareVersion;
        public string HardwareVersion
        {
            get { return hardwareVersion; }
            protected set { SetProperty(ref hardwareVersion, value); }
        }
        #endregion

        private async Task OnConnectExecute()
        {
            await Task.CompletedTask;
        }

        private async Task OnSubscribeToAccelerometerExecute()
        {
            await Task.CompletedTask;
        }

        private async Task OnSubscribeToHearthRateExecute()
        {
            await Task.CompletedTask;
        }

        private async Task OnDoorbellRingExecute()
        {
            await Task.CompletedTask;
        }

        private async Task OnGarageOpenExecute()
        {
            await Task.CompletedTask;
        }

        private async Task OnGarageCloseExecute()
        {
            await Task.CompletedTask;
        }

        private async Task OnSendBarcodeExecute()
        {
            await Task.CompletedTask;
        }

        #region Helper methods
        private async Task<BandIcon> LoadIcon(string uri)
        {
            StorageFile imageFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri(uri));

            using (IRandomAccessStream fileStream = await imageFile.OpenAsync(FileAccessMode.Read))
            {
                var bitmap = new WriteableBitmap(1, 1);
                await bitmap.SetSourceAsync(fileStream);
                return bitmap.ToBandIcon();
            }
        }

        //NOTE: This version of SetProperty is used to handle cross threading.
        protected override bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            storage = value;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => OnPropertyChanged(propertyName));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            return true;
        }
        #endregion
    }
}

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
        private readonly GarageDoorStatusLayout statusLayout = new GarageDoorStatusLayout();
        private readonly GarageControlsLayout controlsLayout = new GarageControlsLayout();
        private readonly BarcodeLayout barCodeLayout = new BarcodeLayout();

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
            if (IsConnected) return;

            try
            {
                bandClient = await ConnectToFirstBand();
                IsConnected = this.bandClient != null;
                if (IsConnected)
                {
                    // Raise CanExecuteChanged event for all dependent controls.
                    ((DelegateCommand) OnSubscribeToAccelerometer).RaiseCanExecuteChanged();
                    ((DelegateCommand) OnSubscribeToHeartRate).RaiseCanExecuteChanged();
                    ((DelegateCommand) OnDoorbellRing).RaiseCanExecuteChanged();
                    ((DelegateCommand) OnGarageOpen).RaiseCanExecuteChanged();
                    ((DelegateCommand) OnGarageClose).RaiseCanExecuteChanged();
                    ((DelegateCommand) OnSendBarcode).RaiseCanExecuteChanged();

                    StatusMessage = "Connection successful.";

                    await AddTileToBand();
                    AddTileEventHandlers();
                }
            }
            catch (BandException bex)
            {
                Debug.WriteLine(bex.Message);
                StatusMessage = "Connection failed.";
                //TODO: Log error, etc.
            }
        }

        private async Task AddTileToBand()
        {
            //NOTE: During debugging this makes it easier to update things.
            await bandClient.TileManager.RemoveTileAsync(tileGuid);

            // Add tile to the Band
            // The name and icons are required
            tile = new BandTile(tileGuid)
            {
                Name = "Home Monitor",
                SmallIcon = await LoadIconAsync(uri: "ms-appx:///Assets/SmallIcon.png"),
                TileIcon = await LoadIconAsync(uri: "ms-appx:///Assets/LargeIcon.png")
            };

            tile.PageLayouts.Add(statusLayout.Layout);
            tile.PageLayouts.Add(controlsLayout.Layout);
            tile.PageLayouts.Add(barCodeLayout.Layout);

            bool succeeded = await bandClient.TileManager.AddTileAsync(tile);
            if (!succeeded)
                StatusMessage = "Failed to add UI";

            // This will cause the page to display with the Open/Close buttons
            var pageData = new PageData(garageControlPageGuid, 1, controlsLayout.Data.All);
            await bandClient.TileManager.SetPagesAsync(tileGuid, pageData);
        }

        private void AddTileEventHandlers()
        {
            bandClient.TileManager.TileOpened += TileManager_TileOpened;
            bandClient.TileManager.TileClosed += TileManager_TileClosed;

            bandClient.TileManager.TileButtonPressed += TileManager_TileButtonPressed;

            bandClient.TileManager.StartReadingsAsync();
        }

        private void TileManager_TileOpened(object sender, BandTileEventArgs<IBandTileOpenedEvent> e)
        {
            StatusMessage = $"Tile {e.TileEvent.TileId} opened at {e.TileEvent.Timestamp}";
        }

        private void TileManager_TileClosed(object sender, BandTileEventArgs<IBandTileClosedEvent> e)
        {
            StatusMessage = $"Tile {e.TileEvent.TileId} closed at {e.TileEvent.Timestamp}";
        }

        private void TileManager_TileButtonPressed(object sender, BandTileEventArgs<IBandTileButtonPressedEvent> e)
        {
            StatusMessage = $"Button {e.TileEvent.ElementId} on page {e.TileEvent.PageId} pressed at {e.TileEvent.Timestamp}";
        }

        public async Task<IBandClient> ConnectToFirstBand()
        {
            // Get a list of the available bands in the form of IBandInfo[] then take the first one from the list.
            // We could also present the list to the user to choose a band.
            IBandInfo[] bands = await bandClientManager.GetBandsAsync();
            if (bands == null || bands.Length == 0)
                return null;

            IBandInfo band = bands[0];

            return await bandClientManager.ConnectAsync(band);
        }


        private bool subscribedToAccelerometer = false;

        private async Task OnSubscribeToAccelerometerExecute()
        {
            if (subscribedToAccelerometer) // If already subscribed then unsubscribe.
            {
                await bandClient.SensorManager.Accelerometer.StopReadingsAsync();
                bandClient.SensorManager.Accelerometer.ReadingChanged -= Accelerometer_ReadingChanged;
                subscribedToAccelerometer = false;
            }
            else // otherwise subscribe.
            {
                if (await GetUserConsentAsync(bandClient.SensorManager.Accelerometer))
                {
                    bandClient.SensorManager.Accelerometer.ReadingChanged += Accelerometer_ReadingChanged;

                    subscribedToAccelerometer = await bandClient.SensorManager.Accelerometer.StartReadingsAsync();

                    if (!subscribedToAccelerometer)
                    {
                        StatusMessage = "Unable to start the accelerometer";
                    }
                }
                else
                {
                    StatusMessage = "Consent must be granted to access the accelerometer.";
                }
            }
        }

        private void Accelerometer_ReadingChanged(object sender, BandSensorReadingEventArgs<IBandAccelerometerReading> e)
        {            
            AccelerationX = e.SensorReading.AccelerationX;
            AccelerationY = e.SensorReading.AccelerationY;
            AccelerationZ = e.SensorReading.AccelerationZ;
        }

        bool subscribedToHeartRate = false;
        private async Task OnSubscribeToHearthRateExecute()
        {
            if (subscribedToHeartRate) // If already subscribed then unsubscribe.
            {
                await bandClient.SensorManager.HeartRate.StopReadingsAsync();
                bandClient.SensorManager.HeartRate.ReadingChanged -= HeartRate_ReadingChanged;
                subscribedToHeartRate = false;
            }
            else // otherwise subscribe.
            {
                if (await GetUserConsentAsync(bandClient.SensorManager.HeartRate))
                {

                    bandClient.SensorManager.HeartRate.ReadingChanged += HeartRate_ReadingChanged;

                    subscribedToHeartRate = await bandClient.SensorManager.HeartRate.StartReadingsAsync();

                    if (!subscribedToHeartRate)
                    {
                        StatusMessage = "Unable to start the heart rate sensor.";
                    }
                }
                else
                {
                    StatusMessage = "Consent must be granted to access the heart rate sensor.";
                }
            }
        }

        private void HeartRate_ReadingChanged(object sender, BandSensorReadingEventArgs<IBandHeartRateReading> e)
        {
            HeartRate = e.SensorReading.HeartRate;
        }

        private async Task OnDoorbellRingExecute()
        {
            try
            {
                // Send a message to the tile with a dialog
                await bandClient.NotificationManager.SendMessageAsync(tileGuid,
                    "Doorbell", "Ring",
                    DateTimeOffset.Now, MessageFlags.ShowDialog);
            }
            catch (BandException bex)
            {
                StatusMessage = "Notification failed. Make sure Band is within range.";
            }
        }

        private async Task OnGarageOpenExecute()
        {
            try
            {
                statusLayout.OpenTimeData.Text = $"Open: {DateTime.Now:MM/dd - hh:mm tt}";

                //NOTE: With the newer Band firmware there is a way to send partial updates as well.
                var pageData = new PageData(garagePageGuid, 0, statusLayout.Data.All);
                await bandClient.TileManager.SetPagesAsync(tileGuid, pageData);

                // Send a dialog to alert the user of new data
                await bandClient.NotificationManager.ShowDialogAsync(tileGuid, "Home Monitor", "Garage door opened");
                await bandClient.NotificationManager.VibrateAsync(VibrationType.ThreeToneHigh);
            }
            catch (BandException bex)
            {
                Debug.WriteLine(bex.Message);
                StatusMessage = "Notification failed. Make sure Band is within range.";
            }
        }

        private async Task OnGarageCloseExecute()
        {
            try
            {
                statusLayout.CloseTimeData.Text = $"Closed: {DateTime.Now:MM/dd - hh:mm tt}";

                //NOTE: With the newer Band firmware there is a way to send partial updates as well.
                var pageData = new PageData(garagePageGuid, 0, statusLayout.Data.All);
                await bandClient.TileManager.SetPagesAsync(tileGuid, pageData);

                // Send a dialog to alert the user of new data
                await bandClient.NotificationManager.ShowDialogAsync(tileGuid, "Home Monitor", "Garage door closed");
            }
            catch (BandException bex)
            {
                Debug.WriteLine(bex.Message);
                StatusMessage = "Notification failed. Make sure Band is within range.";
            }
        }

        private async Task OnSendBarcodeExecute()
        {
            try
            {
                barCodeLayout.ScanCodeData.Barcode = BarcodeText;

                var pageData = new PageData(barcodePageGuid, 2, barCodeLayout.Data.All);
                await bandClient.TileManager.SetPagesAsync(tileGuid, pageData);
            }
            catch (BandException bex)
            {
                Debug.WriteLine(bex.Message);
                StatusMessage = "Failed to send barcode. Make sure Band is within range.";
            }
        }


        //NOTE: This is only for demo purposes. The generic version below is better.
        private async Task<bool> GetHeartRateConsent()
        {
            bool granted = false;

            switch (bandClient.SensorManager.HeartRate.GetCurrentUserConsent())
            {
                case UserConsent.NotSpecified:
                    granted = await bandClient.SensorManager.HeartRate.RequestUserConsentAsync();
                    break;
                case UserConsent.Granted:
                    granted = true;
                    break;
                case UserConsent.Declined:
                    granted = false;
                    break;
            }

            return granted;
        }

        #region Helper methods
        private async Task<bool> GetUserConsentAsync<T>(IBandSensor<T> sensor) where T : IBandSensorReading
        {
            bool granted = false;

            switch (sensor.GetCurrentUserConsent())
            {
                case UserConsent.NotSpecified:
                    granted = await sensor.RequestUserConsentAsync();
                    break;
                case UserConsent.Granted:
                    granted = true;
                    break;
                case UserConsent.Declined:
                    granted = false;
                    break;
            }

            return granted;
        }


        private async Task<BandIcon> LoadIconAsync(string uri)
        {
            StorageFile imageFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri(uri));

            using (IRandomAccessStream fileStream = await imageFile.OpenAsync(FileAccessMode.Read))
            {
                var bitmap = new WriteableBitmap(1, 1);
                await bitmap.SetSourceAsync(fileStream);
                return bitmap.ToBandIcon();
            }
        }

        //HACK: To handle cross threading.
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

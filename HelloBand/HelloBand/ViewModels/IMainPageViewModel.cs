using Prism.Commands;
using System.Windows.Input;

namespace HelloBand.ViewModels
{
    interface IMainPageViewModel
    {
        // Commands
        ICommand OnConnect { get; }
        ICommand OnSubscribeToHeartRate { get; }
        ICommand OnSubscribeToAccelerometer { get; }
        ICommand OnDoorbellRing { get; }
        ICommand OnGarageOpen { get; }
        ICommand OnGarageClose { get; }
        ICommand OnSendBarcode { get; }

        // Properties
        string StatusMessage { get; }
        bool IsConnected { get; }
        int HeartRate { get; }
        double AccelerationX { get; }
        double AccelerationY { get; }
        double AccelerationZ { get; }
        string BarcodeText { get; }
        string FirmwareVersion { get; }
        string HardwareVersion { get; }
    }
}
using Prism.Unity.Windows;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Prism.Mvvm;
using HelloBand.ViewModels;
using Microsoft.Band;

namespace HelloBand
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : PrismUnityApplication
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }
        protected override Task OnInitializeAsync(IActivatedEventArgs args)
        {

            // Register services
            //NOTE: By using dependency injection in this case, we can test using a Band emulator such as the one at: https://github.com/BandOnTheRun/fake-band
            Container.RegisterInstance<IBandClientManager>(BandClientManager.Instance);

            // Register view models
            Container.RegisterType<IMainPageViewModel, MainPageViewModel>();


            return base.OnInitializeAsync(args);
        }

        protected override Task OnLaunchApplicationAsync(LaunchActivatedEventArgs args)
        {
            NavigationService.Navigate(pageToken: "Main", parameter: null);
            return Task.FromResult(result: true);
        }

    }
}

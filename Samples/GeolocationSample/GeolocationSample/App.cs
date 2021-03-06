﻿
using CrossPlatformLibrary.Bootstrapping;

using Xamarin.Forms;

namespace GeolocationSample
{
    public class App : Application
    {
        private readonly Bootstrapper bootstrapper;

        public App()
        {
            this.bootstrapper = new Bootstrapper();
            this.bootstrapper.Startup();

            this.MainPage = new NavigationPage(new MainPage());
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
            this.bootstrapper.Sleep();
        }

        protected override void OnResume()
        {
            this.bootstrapper.Resume();
        }
    }
}
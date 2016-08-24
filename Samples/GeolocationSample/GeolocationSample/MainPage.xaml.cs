using System;
using System.Text;

using CrossPlatformLibrary.Geolocation;
using CrossPlatformLibrary.IoC;

using Xamarin.Forms;

namespace GeolocationSample
{
    public partial class MainPage : ContentPage
    {
        private readonly ILocationService locationService;
        private readonly ILocationServiceSettings locationServiceSettings;

        public MainPage()
        {
            this.InitializeComponent();

            this.locationService = SimpleIoc.Default.GetInstance<ILocationService>();
            this.locationServiceSettings = SimpleIoc.Default.GetInstance<ILocationServiceSettings>();

            this.GetGPSButton.IsEnabled = this.locationService.IsGeolocationEnabled;

            var stringBuilder= new StringBuilder();
            stringBuilder.AppendLine($"IsGeolocationAvailable={this.locationService.IsGeolocationAvailable}");
            stringBuilder.AppendLine($"IsGeolocationEnabled={this.locationService.IsGeolocationEnabled}");
            this.LocationOutput.Text = stringBuilder.ToString();
        }

        private async void OnButtonGetGPSLocationClicked(object sender, EventArgs e)
        {
            if (this.locationService.IsGeolocationEnabled)
            {
                try
                {
                    Position position = await this.locationService.GetPositionAsync(timeoutMilliseconds: 10000);

                    var sb = new StringBuilder();
                    sb.AppendLine($"Lat={position.Latitude}");
                    sb.AppendLine($"Lon={position.Longitude}");
                    sb.AppendLine($"Date={position.Timestamp}");

                    this.LocationOutput.Text = sb.ToString();
                }
                catch (Exception ex)
                {
                    this.LocationOutput.Text = ex.Message;
                }
            }
            else
            {
                this.locationServiceSettings.Show();
            }
        }
    }
}
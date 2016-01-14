using System;
using System.Text;
using CrossPlatformLibrary.Geolocation;
using CrossPlatformLibrary.IoC;

using Xamarin.Forms;

namespace GeolocationSample
{
    class DemoPage : ContentPage
    {
        public DemoPage()
        {
            ILocationService locationService = SimpleIoc.Default.GetInstance<ILocationService>();

            this.Title=  "CrossPlatformLibrary.Geolocation Demo";
            var locationLabel = new Label { Text = "Press button to get GPS location" };

            var getPositionButton = new Button
            {
                Text = "Get GPS location"
            };
            getPositionButton.Clicked += async (sender, args) =>
            {
                if (locationService.IsGeolocationEnabled)
                {
                    try
                    {
                        Position position = await locationService.GetPositionAsync(timeoutMilliseconds: 10000);

                        var sb = new StringBuilder();
                        sb.AppendLine(string.Format("Lat={0}", position.Latitude));
                        sb.AppendLine(string.Format("Lon={0}", position.Longitude));
                        sb.AppendLine(string.Format("Date={0}", position.Timestamp));

                        locationLabel.Text = sb.ToString();
                    }
                    catch (Exception ex)
                    {
                        locationLabel.Text = ex.Message;
                    }
                }
                else
                {
                    ILocationServiceSettings locationServiceSettings = SimpleIoc.Default.GetInstance<ILocationServiceSettings>();
                    locationServiceSettings.Show();
                }

            };

            var stackPanel = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                Children = { getPositionButton, locationLabel }
            };

            this.Content = stackPanel;
        }
    }
}

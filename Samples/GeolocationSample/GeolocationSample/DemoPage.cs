using CrossPlatformLibrary.Geolocation;
using CrossPlatformLibrary.IoC;

using Xamarin.Forms;

namespace GeolocationSample
{
    class DemoPage : ContentPage
    {
        public DemoPage()
        {
            var locationLabel = new Label { Text = "Press button to get GPS location" };

            var getPositionButton = new Button
            {
                Text = "Get GPS location"
            };
            getPositionButton.Clicked += async (sender, args) =>
            {
                ILocationService locationService = SimpleIoc.Default.GetInstance<ILocationService>();
                if (!locationService.IsGeolocationEnabled)
                {
                    var position = await locationService.GetPositionAsync();
                    locationLabel.Text = position.ToString();
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

using Microsoft.Phone.Controls;

namespace GeolocationSample.WinPhone
{
    public partial class MainPage : Xamarin.Forms.Platform.WinPhone.FormsApplicationPage
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.SupportedOrientations = SupportedPageOrientation.PortraitOrLandscape;

            Xamarin.Forms.Forms.Init();
            this.LoadApplication(new GeolocationSample.App());
        }
    }
}

using Android.App;
using Android.Content;
using Android.Provider;

namespace CrossPlatformLibrary.Geolocation
{
    public class LocationServiceSettings : ILocationServiceSettings
    {
        private readonly Context context;

        public LocationServiceSettings()
        {
            this.context = Application.Context;
        }

        public void Show()
        {
            var intent = new Intent(Settings.ActionLocationSourceSettings);
            intent.AddFlags(ActivityFlags.NewTask);
            this.context.StartActivity(intent);
        }
    }
}
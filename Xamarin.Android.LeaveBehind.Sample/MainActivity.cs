using Android.App;
using Android.OS;
using Xamarin.Android.LeaveBehind.Library;

namespace Xamarin.Android.LeaveBehind.Sample
{
    [Activity(Label = "Xamarin.Android.LeaveBehind.Sample", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {
        LeaveBehindLayout leaveBehindLayout;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_main);
        }
    }
}


using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Xamarin.Forms;
using MyDiary.Services;
using Plugin.SecureStorage;

namespace MyDiary.Droid
{
    [Activity(Label = "MyDiary", MainLauncher = true, Theme="@style/android:Theme.Holo.Light",
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Remove the icon to make more room.
            ActionBar.SetIcon(Android.Resource.Color.Transparent);

            Microsoft.WindowsAzure.MobileServices.CurrentPlatform.Init();

            // Lab2: set storage password
            SecureStorageImplementation.StoragePassword = Build.Id;

            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App());
        }
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Foundation;
using ObjCRuntime;
using UIKit;
using Xamarin.Forms.Platform.iOS;

namespace MyDiary.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            Microsoft.WindowsAzure.MobileServices.CurrentPlatform.Init();

            UITableViewCellAppearance.SetAppearanceBackgroundView (new UIView { BackgroundColor = App.TintColor.ToUIColor () });
            UINavigationBar.Appearance.SetTitleTextAttributes (new UITextAttributes {
                TextColor = App.TintTextColor.ToUIColor()
            });

            global::Xamarin.Forms.Forms.Init();
            LoadApplication(new App());

            return base.FinishedLaunching(app, options);
        }
    }

    class UITableViewCellAppearance
    {
        [DllImport ("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
        static extern void void_objc_msgSend_UIView (IntPtr receiver, IntPtr selector, IntPtr arg1);

        public static void SetAppearanceBackgroundView (UIView backgroundView)
        {
            void_objc_msgSend_UIView (UITableViewCell.Appearance.Handle, Selector.GetHandle ("setSelectedBackgroundView:"), backgroundView.Handle);
        }
    }

}

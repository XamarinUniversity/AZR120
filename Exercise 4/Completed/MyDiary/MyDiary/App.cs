using MyDiary.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XamarinUniversity.Services;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace MyDiary
{
    public class App : Application
    {
        public static Color TintColor = Color.FromHex ("#03A9F4");
        public static Color TintTextColor = Color.White;

        public App()
        {
            // Register dependencies.
            var serviceLocator = XamUInfrastructure.Init();
            serviceLocator.Register<IDiaryService, AzureDiaryService>();

            // Setup the main page.
            MainPage = serviceLocator.Get<MainPage>();
        }
    }
}

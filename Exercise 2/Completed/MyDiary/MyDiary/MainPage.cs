using System.Diagnostics;
using MyDiary.Services;
using MyDiary.ViewModels;
using Xamarin.Forms;
using XamarinUniversity.Interfaces;
using XamarinUniversity.Services;

namespace MyDiary
{
    /// <summary>
    /// MainPage used in the application. Uses a MasterDetailPage to display diary entries and details.
    /// </summary>
    public class MainPage : MasterDetailPage
    {
        public MainPage(IDependencyService serviceLocator)
        {
            var mainViewModel = serviceLocator.Get<MainViewModel>();
            Master = new DiaryListPage { BindingContext = mainViewModel };

            var diaryPage = new DiaryEntryPage { BindingContext = mainViewModel };
            Detail = new NavigationPage (diaryPage) { 
                BarBackgroundColor = App.TintColor,
                BarTextColor = App.TintTextColor,
            };
            Detail.SetBinding (TitleProperty, new Binding (nameof (diaryPage.Title), source: diaryPage));

            if (Device.Idiom != TargetIdiom.Desktop)
            {
                var navService = serviceLocator.Get<INavigationService>() as FormsNavigationPageService;
                Debug.Assert(navService != null);
                navService.RegisterAction(AppPage.Master, () => IsPresented = true);
                navService.RegisterAction(AppPage.Detail, () => IsPresented = false);
            }
            else if (Device.Idiom == TargetIdiom.Tablet)
            {
                MasterBehavior = MasterBehavior.SplitOnLandscape;
            }
            else if (Device.Idiom == TargetIdiom.Phone)
            {
                MasterBehavior = MasterBehavior.Popover;
            }

        }
    }
}

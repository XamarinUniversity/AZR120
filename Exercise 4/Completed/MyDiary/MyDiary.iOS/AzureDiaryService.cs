using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using UIKit;

namespace MyDiary.Services
{
	partial class AzureDiaryService
	{
		private Task PlatformLoginAsync(MobileServiceAuthenticationProvider provider)
		{
			return azureClient.LoginAsync(UIApplication.SharedApplication.KeyWindow.RootViewController, provider);
		}
	}
}
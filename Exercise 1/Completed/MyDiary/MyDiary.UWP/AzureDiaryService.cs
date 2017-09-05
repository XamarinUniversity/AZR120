using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;

namespace MyDiary.Services
{
	partial class AzureDiaryService
	{
		private Task PlatformLoginAsync(MobileServiceAuthenticationProvider provider)
		{
			return azureClient.LoginAsync(provider);
		}
	}
}
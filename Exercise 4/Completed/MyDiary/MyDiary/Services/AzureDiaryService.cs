using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;
using MyDiary.Models;
using Newtonsoft.Json.Linq;
using Plugin.Connectivity;
using Plugin.SecureStorage;

namespace MyDiary.Services
{
    /// <summary>
    ///     Wrapper around our Azure diary service client. This is essentially the same code
    ///     built for the service in AZR115 - check out that class if you need some help with any
    ///     of the code.
    /// </summary>
    public partial class AzureDiaryService : IDiaryService
    {
        private const string AzureServiceUrl = "https://mysecurepersonaldiary.azurewebsites.net/";
		const string UserIdKey = ":UserId";
		const string TokenKey = ":Token";

        /// <summary>
        ///     Constructor
        /// </summary>
        public AzureDiaryService()
        {
            azureClient = new MobileServiceClient(AzureServiceUrl);
        }

		/// <summary>
		/// Authenticate using our social provider so Azure knows who we are.
		/// </summary>
		/// <returns>The async.</returns>
		async Task LoginAsync()
		{
            // See if we have a token cached off.
            if (CrossSecureStorage.Current.HasKey(UserIdKey)
                && CrossSecureStorage.Current.HasKey(TokenKey))
            {
                string userId = CrossSecureStorage.Current.GetValue(UserIdKey);
                string token = CrossSecureStorage.Current.GetValue(TokenKey);

                // Lab3: check for an expired token
                if (!IsTokenExpired(token))
                {
                    azureClient.CurrentUser = new MobileServiceUser(userId)
                    {
                        MobileServiceAuthenticationToken = token
                    };
                    return;
                }

                // Expired; refresh it.
                try
				{
                    // Only works with Google, MSA and Azure.
                    await azureClient.RefreshUserAsync();
                }
				catch
				{
					// Failed - clear local user cache.
					await azureClient.LogoutAsync();
				}
			}

            // Do a login.
            // Lab3: use MSA since it supports refresh tokens
            if (azureClient.CurrentUser == null)
                await PlatformLoginAsync(MobileServiceAuthenticationProvider.MicrosoftAccount);

            // Save off the credentials.
            var user = azureClient.CurrentUser;
            if (user != null)
            {
                CrossSecureStorage.Current.SetValue(UserIdKey, user.UserId);
                CrossSecureStorage.Current.SetValue(TokenKey, user.MobileServiceAuthenticationToken);
            }
        }

        /// <summary>
        ///     Initialize the service and retrieve the offline synch. table.
        /// </summary>
        /// <returns></returns>
        private async Task InitializeAsync()
        {
            if (diaryTable != null)
                return;

            var store = new MobileServiceSQLiteStore(localDatabaseFile);
            store.DefineTable<DiaryEntry>();

            await azureClient.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());
            diaryTable = azureClient.GetSyncTable<DiaryEntry>();

			// Lab1: added call to purge any offline records.
			await diaryTable.PurgeAsync(true);
        }

        /// <summary>
        ///     Add a new entry to the Azure DB.
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public async Task AddEntryAsync(DiaryEntry entry)
        {
            Debug.Assert(entry.Id == null);

            await InitializeAsync();
            await diaryTable.InsertAsync(entry);
            await SynchronizeAsync();
        }

        /// <summary>
        ///     Retrieve all the records from the Azure EasyTable.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<DiaryEntry>> GetEntriesAsync()
        {
            await InitializeAsync();
            await SynchronizeAsync();

            return await diaryTable.ToEnumerableAsync();
        }

        /// <summary>
        ///     Update an existing entry in the Azure DB.
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public async Task UpdateEntryAsync(DiaryEntry entry)
        {
            Debug.Assert(entry.Id != null);

            await InitializeAsync();
            await diaryTable.UpdateAsync(entry);
            await SynchronizeAsync();
        }

        /// <summary>
        ///     Delete an entry in the Azure DB.
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public async Task DeleteEntryAsync(DiaryEntry entry)
        {
            await InitializeAsync();
            await diaryTable.DeleteAsync(entry);
            await SynchronizeAsync();
        }

		// Lab3: add logoff support from Azure client
		/// <summary>
		/// Log off
		/// </summary>
		/// <returns></returns>
		public async Task LogoffAsync()
		{
			if (azureClient.CurrentUser == null)
				return;

			// Throw away token cache on server
			using (var client = new HttpClient())
			{
				client.DefaultRequestHeaders.Add("X-ZUMO-AUTH",
					azureClient.CurrentUser.MobileServiceAuthenticationToken);
				await client.GetAsync(azureClient.MobileAppUri + "/.auth/logout");
			}

			// Throw away token in client library
			await azureClient.LogoutAsync();
			Debug.Assert(azureClient.CurrentUser == null);

			// Delete the local cache
			CrossSecureStorage.Current.DeleteKey(UserIdKey);
			CrossSecureStorage.Current.DeleteKey(TokenKey);

			// Finally, purge the local data.
			await diaryTable.PurgeAsync(true);
		}

        /// <summary>
        ///     Method to synchronize our local DB store with the Azure remote DB.
        /// </summary>
        /// <returns></returns>
        private async Task SynchronizeAsync()
        {
            if (!CrossConnectivity.Current.IsConnected)
                return;

            try
            {
                await azureClient.SyncContext.PushAsync();
                await diaryTable.PullAsync($"all{nameof(DiaryEntry)}", diaryTable.CreateQuery());
            }
            catch (MobileServicePushFailedException ex)
            {
				if (ex.PushResult.Status == MobileServicePushStatus.CancelledByAuthenticationError)
				{
					await LoginAsync();
					await SynchronizeAsync();
					return;
				}

                if (ex.PushResult != null)
                    foreach (var result in ex.PushResult.Errors)
                        await ResolveErrorAsync(result);
            }
			catch (MobileServiceInvalidOperationException ex)
			{
				if (ex.Response.StatusCode == HttpStatusCode.Unauthorized)
				{
					await LoginAsync();
					await SynchronizeAsync();
					return;
				}

				throw;
			}
        }

        /// <summary>
        /// Check to see whether a JWT token has expired.
        /// See original code from https://github.com/jwt-dotnet/jwt/blob/master/src/JWT/JWT.cs
        /// </summary>
        /// <param name="token">Encoded JWT token</param>
        /// <returns>True/False</returns>
        private bool IsTokenExpired(string token)
        {
            // No token == expired.
            if (string.IsNullOrEmpty(token))
                return true;

            // Split the string apart; we want the JSON payload.
            string[] parts = token.Split('.');
            if (parts.Length != 3)
                throw new ArgumentException("Token must consist from 3 delimited by dot parts.");

            string jwt = parts[1]
                .Replace('-', '+')  // 62nd char of encoding
                .Replace('_', '/'); // 63rd char of encoding
            switch (jwt.Length % 4) // Pad with trailing '='s
            {
                case 0: break; // No pad chars in this case
                case 2: jwt += "=="; break; // Two pad chars
                case 3: jwt += "="; break;  // One pad char
                default:
                    throw new ArgumentException("Token is not a valid Base64 string.");
            }

            // Convert to a JSON string (std. Base64 decode)
            string json = Encoding.UTF8.GetString(Convert.FromBase64String(jwt));

            // Get the expiration date from the JSON object.
            var jsonObj = JObject.Parse(json);
            var exp = Convert.ToDouble(jsonObj["exp"].ToString());

            // JWT expiration is an offset from 1/1/1970 UTC
            var expire = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(exp);
            return expire < DateTime.UtcNow;
        }

        /// <summary>
        ///     This method is used to resolve any conflicts that occur. This can happen
        ///     if two clients update the same record and then try to push their changes.
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task ResolveErrorAsync(MobileServiceTableOperationError result)
        {
            // Ignore if we can't see both sides.
            if ((result.Result == null) || (result.Item == null))
                return;

            var serverItem = result.Result.ToObject<DiaryEntry>();
            var localItem = result.Item.ToObject<DiaryEntry>();

            if ((serverItem.Title == localItem.Title)
                && (serverItem.Text == localItem.Text))
            {
                // Items are the same, so ignore the conflict
                await result.CancelAndDiscardItemAsync();
            }
            else
            {
                // Always take the client
                localItem.AzureVersion = serverItem.AzureVersion;
                await result.UpdateOperationAsync(JObject.FromObject(localItem));
            }
        }

        #region Private Data

        private const string localDatabaseFile = "diary.db";
        private readonly MobileServiceClient azureClient;
        private IMobileServiceSyncTable<DiaryEntry> diaryTable;

        #endregion
    }
}
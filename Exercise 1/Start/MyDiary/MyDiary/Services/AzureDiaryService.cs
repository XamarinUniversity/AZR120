using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;
using MyDiary.Models;
using Newtonsoft.Json.Linq;
using Plugin.Connectivity;

namespace MyDiary.Services
{
    /// <summary>
    ///     Wrapper around our Azure diary service client. This is essentially the same code
    ///     built for the service in AZR115 - check out that class if you need some help with any
    ///     of the code.
    /// </summary>
    public class AzureDiaryService : IDiaryService
    {
        private const string AzureServiceUrl = "https://mysecurepersonaldiary.azurewebsites.net/";

        /// <summary>
        ///     Constructor
        /// </summary>
        public AzureDiaryService()
        {
            azureClient = new MobileServiceClient(AzureServiceUrl);
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
                if (ex.PushResult != null)
                    foreach (var result in ex.PushResult.Errors)
                        await ResolveErrorAsync(result);
            }
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
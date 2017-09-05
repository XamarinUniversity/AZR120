using System.Collections.Generic;
using System.Threading.Tasks;
using MyDiary.Models;

namespace MyDiary.Services
{
    /// <summary>
    /// Abstraction for our diary service.
    /// </summary>
    public interface IDiaryService
    {
        /// <summary>
        ///     Add a new entry to the Azure DB.
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        Task AddEntryAsync(DiaryEntry entry);

        /// <summary>
        ///     Retrieve all the records from the Azure EasyTable.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<DiaryEntry>> GetEntriesAsync();

        /// <summary>
        ///     Update an existing entry in the Azure DB.
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        Task UpdateEntryAsync(DiaryEntry entry);

        /// <summary>
        ///     Delete an entry in the Azure DB.
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        Task DeleteEntryAsync(DiaryEntry entry);
    }
}
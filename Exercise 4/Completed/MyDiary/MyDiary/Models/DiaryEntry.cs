using Microsoft.WindowsAzure.MobileServices;
using System;
using Newtonsoft.Json;

namespace MyDiary.Models
{
    /// <summary>
    /// A single diary entry in our Azure table.
    /// </summary>
	[JsonObject(Title = "diaryentry3")]
    public class DiaryEntry
    {
        /// <summary>
        /// Unique identifier for this entry, supplied by Azure.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Diary entry text
        /// </summary>
        public string Text { get; set; }

		// Lab4: added UserId field
        /// <summary>
        /// UserId that created this record
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// When this entry was initially created, set by Azure.
        /// </summary>
        [CreatedAt]
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// Last update time, updated by Azure.
        /// </summary>
        [UpdatedAt]
        public DateTimeOffset UpdatedAt { get; set; }

        /// <summary>
        /// Current version of this record (added by Azure)
        /// </summary>
        [Version]
        public string AzureVersion { get; set; }

   }
}

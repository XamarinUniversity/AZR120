using Microsoft.Azure.Mobile.Server;

namespace MyDiaryServer.DataObjects
{
    public class DiaryEntry : EntityData
    {
        public string Title { get; set; }
        public string Text { get; set; }
        public string UserId { get; set; }
    }
}
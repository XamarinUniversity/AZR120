using MyDiary.Models;
using XamarinUniversity.Infrastructure;

namespace MyDiary.ViewModels
{
    /// <summary>
    /// This ViewModel wraps a single diary entry model.
    /// </summary>
    public class DiaryEntryViewModel : SimpleViewModel
    {
        private readonly DiaryEntry diaryEntry;
        private bool canSave;

        /// <summary>
        /// Default constructor to generate a new DiaryEntry.
        /// </summary>
        public DiaryEntryViewModel()
            : this(new DiaryEntry())
        {
        }
        
        /// <summary>
        /// Constructor to wrap an existing DiaryEntry.
        /// </summary>
        /// <param name="diaryEntry"></param>
        public DiaryEntryViewModel(DiaryEntry diaryEntry)
        {
            this.diaryEntry = diaryEntry;
            CanSave = false; // Don't allow saves initially.
        }

        /// <summary>
        /// Provides access to the underlying model object so we can
        /// retrieve it.
        /// </summary>
        public DiaryEntry Model { get { return diaryEntry; } }

        /// <summary>
        /// True if this DiaryEntry is new and needs to be added to the DB.
        /// </summary>
        public bool IsNew { get { return diaryEntry.Id == null; } }

        /// <summary>
        /// Title in shortened form, used as title for page.
        /// </summary>
        public string ShortTitle
        {
            get {
                return diaryEntry.Title == null 
                    ? "New Entry" 
                    : diaryEntry.Title.Length <= 20 ? diaryEntry.Title : diaryEntry.Title.Substring(0, 20) + "...";
            }
        }


        /// <summary>
        /// The full title for the entry.
        /// </summary>
        public string Title
        {
            get { return diaryEntry.Title; }
            set {
                if (diaryEntry.Title != value) {
                    diaryEntry.Title = value;
                    RaisePropertyChanged(); // this property
                    RaisePropertyChanged(() => ShortTitle);
                    CheckValid();
                }
            }
        }

        /// <summary>
        /// Text for the diary entry.
        /// </summary>
        public string Text
        {
            get { return diaryEntry.Text; }
            set
            {
                if (diaryEntry.Text != value)
                {
                    diaryEntry.Text = value;
                    RaisePropertyChanged();
                    CheckValid();
                }
            }
        }

        /// <summary>
        /// Changes the state of the DiaryEntry - whether we can SAVE the changes.
        /// </summary>
        void CheckValid()
        {
            CanSave = !string.IsNullOrEmpty(Title) && !string.IsNullOrEmpty(Text);
        }

        /// <summary>
        /// Valid flag which determines whether we can save this DiaryEntry.
        /// </summary>
        public bool CanSave
        {
            get { return canSave; }
            set { SetPropertyValue(ref canSave, value); }
        }
    }
}
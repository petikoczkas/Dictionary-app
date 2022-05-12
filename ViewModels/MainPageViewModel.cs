using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Template10.Mvvm;
using Dictionary.Services;
using Windows.UI.Xaml.Navigation;
using System.ComponentModel;
using Dictionary.Models;
using System.Globalization;

namespace Dictionary.ViewModels
{
    /// <summary>
    /// This is the ViewModel for the app.
    /// </summary>
    class MainPageViewModel : ViewModelBase, INotifyPropertyChanged
    {
        /// <value>
        /// Property <c>Translated</c> represents the translated data from api calls.
        /// </value>
        public Translated Translated { get; set; }
        = new Translated();

        /// <value>
        /// Property <c>Languages</c> is a collection for the language pairs.
        /// </value>
        public ObservableCollection<Language> Languages { get; set; } =
            new ObservableCollection<Language>();

        /// <value>
        /// Property <c>From</c> is a collection for the languages which you can translate from.
        /// </value>
        public ObservableCollection<string> From { get; set; } =
            new ObservableCollection<string>();

        private string _fromSelected;
        /// <value>
        /// Property<c>FromSelected</c> represents the language selected for translation from the From collection.
        /// </value>
        public string FromSelected
        {
            get { return _fromSelected; }
            /// <remarks>In this set method the collection of languages which you can translate to is filled with languages.</remarks>
            set
            {
                if (_fromSelected != value && value != null)
                {
                    To.Clear();
                    _fromSelected = value;
                    foreach (var l in Languages)
                    {
                        string[] list = l.LanguageName.Split('-');
                        ///<remarks>Converts the short name of the language to the normal form.</remarks>
                        CultureInfo c = new CultureInfo(list[0]);
                        if (c.EnglishName == _fromSelected)
                            To.Add(new CultureInfo(list[1]).EnglishName);
                    }
                }
                TranslateCommand.RaiseCanExecuteChanged();

            }
        }

        /// <value>
        /// Property<c>To</c> is a collection for the languages which you can translate to.
        /// </value>
        public ObservableCollection<string> To { get; set; }
        = new ObservableCollection<string>();

        private string _toSelected;
        /// <value>
        /// Property<c>ToSelected</c> represents the language selected for translation from the To collection.
        /// </value>
        public string ToSelected
        {
            get { return _toSelected; }
            set
            {
                Set(ref _toSelected, value);
                TranslateCommand.RaiseCanExecuteChanged();
            }
        }

        private string _textFrom;
        /// <value>
        /// Property<c>TextTo</c> represents the text to be translated.
        /// </value>
        public string TextFrom
        {
            get { return _textFrom; }
            set
            {
                Set(ref _textFrom, value);
                OnPropertyChanged("TextFrom");
                TranslateCommand.RaiseCanExecuteChanged();
            }
        }

        /// <value>
        /// Property<c>TextTo</c> is a collection of the translated texts.
        /// </value>
        public ObservableCollection<string> TextTo { get; set; }


        /// <value>
        /// Property<c>SynLanguages</c> is a collection of languages belonging to synonyms.
        /// </value>
        public ObservableCollection<string> SynLanguages { get; set; } =
        new ObservableCollection<string>();

        private string _languageSelected;
        /// <value>
        /// Property<c>LanguageSelected</c> represents the language chosen for the synonym.
        /// </value>
        public string LanguageSelected 
        {
            get { return _languageSelected; }
            set
            {
                Set(ref _languageSelected, value);
                SynonymCommand.RaiseCanExecuteChanged();
            }
        }

        private string _synTextFrom;
        /// <value>
        /// Property<c>SynTextFrom</c> represents the text that needs its synonyms.
        /// </value>
        public string SynTextFrom
        {
            get { return _synTextFrom; }
            set
            {
                Set(ref _synTextFrom, value);
                OnPropertyChanged("SynTextFrom");
                SynonymCommand.RaiseCanExecuteChanged();
            }
        }

        /// <value>
        /// Property<c>SynTextTo</c> is a collection of the synonyms.
        /// </value>
        public ObservableCollection<string> SynTextTo { get; set; }

        /// <summary>
        /// The help with this command we can not press the "Translate" button when one or more data is missing for the api call.
        /// </summary>
        public DelegateCommand TranslateCommand { get; }
        /// <summary>
        /// The help with this command we can not press the "Get synonyms" button when one or more data is missing for the api call.
        /// </summary>
        public DelegateCommand SynonymCommand { get; }


        /// <summary>
        /// Fills the Languages collection with data, then sets the From and the SynLanguages collections.
        /// </summary>
        public override async Task OnNavigatedToAsync(
        object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            var service = new DictionaryService();
            var languages = await service.GetLanguagesAsync();
            foreach (var item in languages)
            {
                Languages.Add(new Language(item));
                var temp = item.Split('-');
                CultureInfo c = new CultureInfo(temp[0]);
                if(!From.Contains(c.EnglishName))
                    From.Add(c.EnglishName);
                if (temp[0].Equals(temp[1]))
                    SynLanguages.Add(c.EnglishName);
            }
            
            await base.OnNavigatedToAsync(parameter, mode, state);
            
        }


        /// <summary>
        /// Inicializes collections and sets the return values fro the Commands.
        /// </summary>
        public MainPageViewModel()
        {
            TranslateCommand = new DelegateCommand(TranslatedAsync, () =>
            {
                if (FromSelected == null || ToSelected == null || TextFrom == null || TextFrom == "") return false;
                else return true;
            });

            SynonymCommand = new DelegateCommand(SynAsync, () =>
            {
                if (LanguageSelected == null || SynTextFrom == null || SynTextFrom == "") return false;
                else return true;
            });

            TextTo = new ObservableCollection<string>();
            SynTextTo = new ObservableCollection<string>();
        }


        /// <summary>
        /// This method collects the translations from the api call to the TextTo property.
        /// </summary>
        public async void TranslatedAsync()
        {
            TextTo.Clear();
            string from = null, to= null;
            foreach(var l in Languages)
            {
                CultureInfo c = new CultureInfo(l.LanguageName.Split('-')[0]);
                if (c.EnglishName == FromSelected) from = l.LanguageName.Split('-')[0];
                if (c.EnglishName == ToSelected) to = l.LanguageName.Split('-')[0];
            }
            string lang = from + '-' + to;

            var service = new DictionaryService();
            Translated = await service.GetTranslationsAsync(lang, TextFrom);
            if (Translated.def.Length == 0) TextTo.Add("Translate not found");
            else
            {
                foreach (var d in Translated.def)
                {
                    foreach(var t in d.tr) TextTo.Add(t.text.ToString());
                }
            }
        }

        /// <summary>
        /// This method collects the synonyms from the api call to the SynTextTo property.
        /// </summary>
        public async void SynAsync()
        {
            SynTextTo.Clear();
            string l = null;
            foreach(var i in Languages)
            {
                CultureInfo c = new CultureInfo(i.LanguageName.Split('-')[0]);
                if (c.EnglishName == LanguageSelected) l = i.LanguageName.Split('-')[0];
            }
            string lang = l + '-' + l;
            var service = new DictionaryService();
            Translated = await service.GetTranslationsAsync(lang, SynTextFrom);
            if (Translated.def.Length == 0) SynTextTo.Add("Synonym not found");
            else
            {
                foreach (var d in Translated.def)
                {
                    foreach (var t in d.tr)
                    {
                        SynTextTo.Add(t.text.ToString());
                        if (t.syn != null)
                        {
                            foreach (var s in t.syn) SynTextTo.Add(s.text.ToString());
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 
        /// </summary>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}

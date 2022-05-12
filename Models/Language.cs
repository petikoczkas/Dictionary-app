
namespace Dictionary.Models
{
    /// <summary>
    /// Class <c>Language</c> represents a model for the languages from web calls.
    /// </summary>
    public class Language
    {
        public string LanguageName { get; set; }

        public Language(string name)
        {
            LanguageName = name;
        }
    }
}

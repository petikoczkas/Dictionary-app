using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Web.Http;
using Newtonsoft.Json;
using Dictionary.Models;

namespace Dictionary.Services
{
    /// <summary>
    /// Class <c>DictionaryService</c> handles the api calls
    /// </summary>
    class DictionaryService
    {
        /// <summary>
        /// Executes a GET request, then deserialize the result from the JSON.
        /// </summary>
        /// <param name="uri">the url where the data is located</param>
        private async Task<T> GetAsync<T>(Uri uri)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(uri);
                var json = await response.Content.ReadAsStringAsync();
                T result = JsonConvert.DeserializeObject<T>(json);
                return result;
            }
        }

        private readonly Uri urlLang = new Uri("https://dictionary.yandex.net/api/v1/dicservice.json/getLangs?key=");
        private readonly string key = "dict.1.1.20220423T165948Z.83851796b49fb925.896dad70fb09af36963a3622c51e497c56c73a22";

        /// <summary>
        /// Fetches all language pairs.
        /// </summary>
        public async Task<List<string>> GetLanguagesAsync()
        {
            return await GetAsync<List<string>>(new Uri(urlLang + key));
        }

        private readonly Uri urlTranslate = new Uri("https://dictionary.yandex.net/api/v1/dicservice.json/lookup?key=");


        /// <summary>
        /// Fetches the translated data.
        /// </summary>
        /// <param name="transLang">the selected language pair</param>
        /// <param name="text">the text that you want to translate</param>
        /// <returns></returns>
        public async Task<Translated> GetTranslationsAsync(string transLang, string text)
        {
            return await GetAsync<Translated>(new Uri(urlTranslate + key + "&lang=" + transLang + "&text=" + text));
        }


    }
}

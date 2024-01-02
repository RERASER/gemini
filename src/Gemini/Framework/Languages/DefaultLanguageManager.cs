using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gemini.Framework.Services;
using Gemini.Properties;
using ResourceManager = System.Resources.ResourceManager;

namespace Gemini.Framework.Languages
{
    [Export(typeof(ILanguageManager))]
    internal class DefaultLanguageManager : ILanguageManager
    {
        private List<string> cachedAvaliableLanguages;

        public IEnumerable<string> GetAvaliableLanguageNames()
        {
            if (cachedAvaliableLanguages is null)
            {
                cachedAvaliableLanguages = new List<string>();
                var rm = new ResourceManager(typeof(Resources));

                CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
                foreach (CultureInfo culture in cultures)
                {
                    ResourceSet rs = rm.GetResourceSet(culture, true, false);
                    if (rs != null)
                        cachedAvaliableLanguages.Add(culture.Name);
                }
            }

            return cachedAvaliableLanguages;
        }

        public string GetCurrentLanguage()
        {
            return Settings.Default.LanguageCode;
        }

        public void SetLanguage(string languageName)
        {
            var culture = string.IsNullOrWhiteSpace(languageName) ? CultureInfo.DefaultThreadCurrentCulture : CultureInfo.GetCultureInfo(languageName);
            var uiCulture = string.IsNullOrWhiteSpace(languageName) ? CultureInfo.DefaultThreadCurrentUICulture : CultureInfo.GetCultureInfo(languageName);

            Thread.CurrentThread.CurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = uiCulture;

            Settings.Default.LanguageCode = languageName;
            Settings.Default.Save();
        }
    }
}

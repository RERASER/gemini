using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Caliburn.Micro;
using Gemini.Framework.Services;
using Gemini.Modules.Shell.Views;
using Gemini.Properties;
using Xceed.Wpf.Toolkit.Core.Utilities;
using ResourceManager = System.Resources.ResourceManager;

namespace Gemini.Framework.Languages
{
    [Export(typeof(ILanguageManager))]
    [PartCreationPolicy(CreationPolicy.Shared)]
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

        private readonly Dictionary<int, TranslationSource> cachedSources = new();
        private bool isUpdating;

        public INotifyPropertyChanged GetTranslationSource(Func<string, CultureInfo, string> callback)
        {
            var key = callback.GetHashCode();
            if (!cachedSources.TryGetValue(key, out var source))
                cachedSources[key] = source = new TranslationSource((key) => callback(key, Thread.CurrentThread.CurrentUICulture));
            return source;
        }

        public void SetLanguage(string languageName)
        {
            var culture = string.IsNullOrWhiteSpace(languageName) ? CultureInfo.DefaultThreadCurrentCulture : CultureInfo.GetCultureInfo(languageName);
            var uiCulture = string.IsNullOrWhiteSpace(languageName) ? CultureInfo.DefaultThreadCurrentUICulture : CultureInfo.GetCultureInfo(languageName);

            Thread.CurrentThread.CurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = uiCulture;

            Settings.Default.LanguageCode = languageName;
            Settings.Default.Save();

            foreach (var source in cachedSources.Values)
                source.Refresh();

            /*
            var visuals = new Queue<Visual>();
            foreach (var window in Application.Current.Windows.OfType<Visual>())
                visuals.Enqueue(window);

            void UpdateBindingTargets(DependencyObject obj)
            {
                var localValues = obj.GetLocalValueEnumerator();
                while (localValues.MoveNext())
                {
                    var entry = localValues.Current;

                    if (BindingOperations.IsDataBound(obj, entry.Property))
                    {
                        var bindingExpr = BindingOperations.GetBindingExpression(obj, entry.Property);
                        bindingExpr.UpdateSource();
                    }
                }
            }

            while (visuals.TryDequeue(out var visual))
            {
                UpdateBindingTargets(visual);

                var childCount = VisualTreeHelper.GetChildrenCount(visual);
                for (int i = 0; i < childCount; i++)
                {
                    var child = (Visual)VisualTreeHelper.GetChild(visual, i);
                    visuals.Enqueue(child);
                }
            }
            */
        }
    }
}

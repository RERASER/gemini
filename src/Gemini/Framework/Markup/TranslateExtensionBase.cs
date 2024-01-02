using System;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;
using Caliburn.Micro;
using Gemini.Framework.Languages;

namespace Gemini.Framework.Markup
{
    public class TranslateExtensionBase : Binding
    {
        public TranslateExtensionBase(string member, Func<string, CultureInfo, string> callback)
            : base(member)
        {
            Mode = BindingMode.OneWay;
            Source = IoC.Get<ILanguageManager>().GetTranslationSource(callback);
        }
    }
}

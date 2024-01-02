using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Gemini.Framework.Services;
using Gemini.Properties;

namespace Gemini.Framework.Languages
{
    public class TranslationSource : PropertyChangedBase
    {
        public TranslationSource(Func<string, string> getStringFunc)
        {
            this.getStringFunc = getStringFunc;
        }

        private readonly Func<string, string> getStringFunc;

        public string this[string key]
        {
            get { return getStringFunc(key); }
        }
    }
}

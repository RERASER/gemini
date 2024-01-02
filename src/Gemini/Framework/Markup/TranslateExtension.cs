using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gemini.Properties;

namespace Gemini.Framework.Markup
{
    public class TranslateExtension : TranslateExtensionBase
    {
        public TranslateExtension(string member) : base(member, Resources.ResourceManager.GetString)
        {
        }
    }
}

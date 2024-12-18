using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Resources;
using Caliburn.Micro;
using Gemini.Framework.Languages;
using Gemini.Framework.Services;
using Gemini.Framework.Themes;
using Gemini.Modules.Settings;
using Gemini.Properties;
using ResourceManager = System.Resources.ResourceManager;

namespace Gemini.Modules.MainMenu.ViewModels
{
    [Export(typeof(ISettingsEditor))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class MainMenuSettingsViewModel : PropertyChangedBase, ISettingsEditor
    {
        private readonly IThemeManager _themeManager;
        private readonly ILanguageManager _languageManager;
        private readonly List<string> _availableLanguages = new ();

        private ITheme _selectedTheme;
        private string _selectedLanguage;
        private bool _autoHideMainMenu;

        [ImportingConstructor]
        public MainMenuSettingsViewModel(IThemeManager themeManager, ILanguageManager languageManager)
        {
            _themeManager = themeManager;
            _languageManager = languageManager;
            SelectedTheme = themeManager.CurrentTheme;
            AutoHideMainMenu = Properties.Settings.Default.AutoHideMainMenu;

            SelectedLanguage = _languageManager.GetCurrentLanguage();
            _availableLanguages.AddRange(_languageManager.GetAvaliableLanguageNames());
        }


        public IEnumerable<ITheme> Themes
        {
            get { return _themeManager.Themes; }
        }

        public ITheme SelectedTheme
        {
            get { return _selectedTheme; }
            set
            {
                if (value.Equals(_selectedTheme))
                    return;
                _selectedTheme = value;
                NotifyOfPropertyChange(() => SelectedTheme);
            }
        }

        public IEnumerable<string> Languages
        {
            get { return _availableLanguages; }
        }

        public string SelectedLanguage
        {
            get { return _selectedLanguage; }
            set
            {
                if (value.Equals(_selectedLanguage))
                    return;
                _selectedLanguage = value;
                NotifyOfPropertyChange(() => SelectedLanguage);
            }
        }

        public bool AutoHideMainMenu
        {
            get { return _autoHideMainMenu; }
            set
            {
                if (value.Equals(_autoHideMainMenu))
                    return;
                _autoHideMainMenu = value;
                NotifyOfPropertyChange(() => AutoHideMainMenu);
            }
        }

        public string SettingsPageName
        {
            get { return Properties.Resources.SettingsPageGeneral; }
        }

        public string SettingsPagePath
        {
            get { return Properties.Resources.SettingsPathEnvironment; }
        }

        public void ApplyChanges()
        {
            Properties.Settings.Default.ThemeName = SelectedTheme.GetType().Name;
            Properties.Settings.Default.AutoHideMainMenu = AutoHideMainMenu;
            Properties.Settings.Default.Save();
            _themeManager.SetCurrentTheme(Properties.Settings.Default.ThemeName);
            _languageManager.SetLanguage(SelectedLanguage);
        }
    }
}

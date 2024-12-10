/*
 * Original source code from the Wide framework:
 * https://github.com/chandramouleswaran/Wide
 * 
 * Used in Gemini with kind permission of the author.
 *
 * Original licence follows:
 *
 * Copyright (c) 2013 Chandramouleswaran Ravichandran
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using Gemini.Framework.Services;
using Gemini.Framework.Win32;

namespace Gemini.Framework.Themes
{
    [Export(typeof(IThemeManager))]
    public class ThemeManager : IThemeManager
    {
        public event EventHandler CurrentThemeChanged;
        public event EventHandler<IThemeManager.SystemTheme> CurrentSystemThemeChanged;

        private readonly SettingsPropertyChangedEventManager<Properties.Settings> _settingsEventManager =
            new SettingsPropertyChangedEventManager<Properties.Settings>(Properties.Settings.Default);

        private ResourceDictionary _applicationResourceDictionary;

        public List<ITheme> Themes
        {
            get; private set;
        }

        public ITheme CurrentTheme { get; private set; }

        public IThemeManager.SystemTheme CurrentSystemTheme { get; private set; }

        private bool isSystemTheme;

        [ImportingConstructor]
        public ThemeManager([ImportMany] ITheme[] themes)
        {
            Themes = new List<ITheme>(themes);
            //_settingsEventManager.AddListener(s => s.ThemeName, value => SetCurrentTheme(value));
            CurrentSystemTheme = ReadSystemTheme();
            Thread thread = new(SystemThemeListenThread);
            thread.IsBackground = true;
            thread.Start();
        }

        private void SetSystemTheme(object sender, IThemeManager.SystemTheme theme)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                SetCurrentTheme("SystemTheme");
            });
        }

        public bool SetCurrentTheme(string name)
        {
            var realname = name;
            if (name == "SystemTheme")
            {
                if (!isSystemTheme)
                {
                    CurrentSystemThemeChanged += SetSystemTheme;
                }
                switch (CurrentSystemTheme)
                {
                    case IThemeManager.SystemTheme.Light:
                        name = "LightTheme";
                        break;
                    case IThemeManager.SystemTheme.Dark:
                        name = "DarkTheme";
                        break;
                    default:
                        return false;
                }
            }
            else
            {
                if (isSystemTheme)
                {
                    CurrentSystemThemeChanged -= SetSystemTheme;
                }
            }
            var theme = Themes.FirstOrDefault(x => x.GetType().Name == name);
            var realtheme = Themes.FirstOrDefault(x => x.GetType().Name == realname);
            if (theme == null)
                return false;

            var mainWindow = Application.Current.MainWindow;
            if (mainWindow == null)
                return false;

            CurrentTheme = realtheme;

            if (_applicationResourceDictionary == null)
            {
                _applicationResourceDictionary = new ResourceDictionary();
                Application.Current.Resources.MergedDictionaries.Add(_applicationResourceDictionary);
            }
            _applicationResourceDictionary.BeginInit();
            _applicationResourceDictionary.MergedDictionaries.Clear();

            var windowResourceDictionary = mainWindow.Resources.MergedDictionaries[0];
            windowResourceDictionary.BeginInit();
            windowResourceDictionary.MergedDictionaries.Clear();

            foreach (var uri in theme.ApplicationResources)
                _applicationResourceDictionary.MergedDictionaries.Add(new ResourceDictionary
                {
                    Source = uri
                });

            foreach (var uri in theme.MainWindowResources)
                windowResourceDictionary.MergedDictionaries.Add(new ResourceDictionary
                {
                    Source = uri
                });

            _applicationResourceDictionary.EndInit();
            windowResourceDictionary.EndInit();

            RaiseCurrentThemeChanged(EventArgs.Empty);

            return true;
        }

        private void RaiseCurrentThemeChanged(EventArgs args)
        {
            var handler = CurrentThemeChanged;
            if (handler != null)
                handler(this, args);
        }

        public static IThemeManager.SystemTheme ReadSystemTheme()
        {
            IntPtr subKeyName = Marshal.StringToCoTaskMemAnsi("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize");
            IntPtr hKeyRef = Marshal.AllocCoTaskMem(8);
            int hr = NativeMethods.RegOpenKeyExA(NativeMethods.HKEY_CURRENT_USER, subKeyName, 0, NativeMethods.KEY_READ, hKeyRef);
            Marshal.FreeCoTaskMem(subKeyName);
            if (hr != 0)
            {
                Marshal.FreeCoTaskMem(hKeyRef);
                return IThemeManager.SystemTheme.Light;
            }
            IntPtr hKey = Marshal.ReadIntPtr(hKeyRef);
            Marshal.FreeCoTaskMem(hKeyRef);
            subKeyName = Marshal.StringToCoTaskMemAnsi("AppsUseLightTheme");
            IntPtr dwSizeRef = Marshal.AllocCoTaskMem(4);
            IntPtr valueRef = Marshal.AllocCoTaskMem(4);
            Marshal.WriteInt32(dwSizeRef, 4);
            NativeMethods.RegQueryValueExA(hKey, subKeyName, IntPtr.Zero, IntPtr.Zero, valueRef, dwSizeRef);
            Marshal.FreeCoTaskMem(subKeyName);
            Marshal.FreeCoTaskMem(dwSizeRef);
            int value = Marshal.ReadInt32(valueRef);
            Marshal.FreeCoTaskMem(valueRef);
            NativeMethods.RegCloseKey(hKey);
            return value > 0 ? IThemeManager.SystemTheme.Light : IThemeManager.SystemTheme.Dark;
        }

        private void SystemThemeListenThread()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            Application.Current.Dispatcher.Invoke(() => { Application.Current.Exit += (o, e) => { cts.Cancel(); }; });
            IntPtr subKeyName = Marshal.StringToCoTaskMemAnsi("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize");
            IntPtr hKeyRef = Marshal.AllocCoTaskMem(8);
            int hr = NativeMethods.RegOpenKeyExA(NativeMethods.HKEY_CURRENT_USER, subKeyName, 0, NativeMethods.KEY_READ, hKeyRef);
            Marshal.FreeCoTaskMem(subKeyName);
            if (hr != 0)
            {
                Marshal.FreeCoTaskMem(hKeyRef);
                return;
            }
            IntPtr hKey = Marshal.ReadIntPtr(hKeyRef);
            Marshal.FreeCoTaskMem(hKeyRef);
            subKeyName = Marshal.StringToCoTaskMemAnsi("AppsUseLightTheme");
            IntPtr dwSizeRef = Marshal.AllocCoTaskMem(4);
            IntPtr valueRef = Marshal.AllocCoTaskMem(4);
            Marshal.WriteInt32(dwSizeRef, 4);
            while (true)
            {
                NativeMethods.RegNotifyChangeKeyValue(hKey, 1, 0x00000004, IntPtr.Zero, 0);
                NativeMethods.RegQueryValueExA(hKey, subKeyName, IntPtr.Zero, IntPtr.Zero, valueRef, dwSizeRef);
                int value = Marshal.ReadInt32(valueRef);
                IThemeManager.SystemTheme result = value > 0 ? IThemeManager.SystemTheme.Light : IThemeManager.SystemTheme.Dark;
                CurrentSystemTheme = result;
                if (cts.Token.IsCancellationRequested)
                {
                    break;
                }
                CurrentSystemThemeChanged?.Invoke(this, result);
            }
            Marshal.FreeCoTaskMem(subKeyName);
            Marshal.FreeCoTaskMem(dwSizeRef);
            Marshal.FreeCoTaskMem(valueRef);
            NativeMethods.RegCloseKey(hKey);
        }
    }
}

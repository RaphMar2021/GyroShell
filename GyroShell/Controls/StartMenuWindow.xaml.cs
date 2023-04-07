// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Windowing;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.InteropServices;
using Windows.Foundation;
using Windows.UI;
using Windows.Graphics;
using Windows.Foundation.Collections;
using WinRT;
using GyroShell.Helpers;
using Microsoft.UI.Composition.SystemBackdrops;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace GyroShell.Controls
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class StartMenuWindow : Window
    {
        public StartMenuWindow()
        {
            this.InitializeComponent();
            var presenter = GetAppWindowAndPresenter();
            presenter.IsMaximizable = false;
            presenter.IsMinimizable = false;
            presenter.IsAlwaysOnTop = true;
            presenter.IsResizable = false;
            presenter.SetBorderAndTitleBar(false, false);

            SetBackdrop();

            var window = this;
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

            appWindow.Resize(new SizeInt32(700, 800));
        }

        private OverlappedPresenter GetAppWindowAndPresenter()
        {
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WindowId myWndId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            var _apw = AppWindow.GetFromWindowId(myWndId);
            return _apw.Presenter as OverlappedPresenter;
        }

        #region backdrop stuff

        private bool finalOpt;
        private byte finalA;
        private byte finalR;
        private byte finalG;
        private byte finalB;
        private float finalLO;
        private float finalTO;
        WindowsSystemDispatcherQueueHelper m_wsdqHelper;
        MicaController micaController;
        DesktopAcrylicController acrylicController;
        SystemBackdropConfiguration m_configurationSource;

        private void SetBackdrop()
        {
            bool? option = App.localSettings.Values["isCustomTransparency"] as bool?;
            byte? alpha = App.localSettings.Values["aTint"] as byte?;
            byte? red = App.localSettings.Values["rTint"] as byte?;
            byte? green = App.localSettings.Values["gTint"] as byte?;
            byte? blue = App.localSettings.Values["bTint"] as byte?;
            float? luminOpacity = App.localSettings.Values["luminOpacity"] as float?;
            float? tintOpacity = App.localSettings.Values["tintOpacity"] as float?;
            finalOpt = option != null ? (bool)option : false;
            finalA = alpha != null ? (byte)alpha : (byte)0;
            finalR = red != null ? (byte)red : (byte)0;
            finalG = green != null ? (byte)green : (byte)0;
            finalB = blue != null ? (byte)blue : (byte)0;
            finalLO = luminOpacity != null ? (float)luminOpacity : (float)0.2f;
            finalTO = tintOpacity != null ? (float)tintOpacity : (float)0.3f;

            int? transparencyType = App.localSettings.Values["transparencyType"] as int?;
            switch (transparencyType)
            {
                case 0:
                default:
                    if (Helpers.OSVersion.IsWin11())
                    {
                        TrySetMicaBackdrop(MicaKind.BaseAlt);
                    }
                    else
                    {
                        TrySetAcrylicBackdrop();
                    }
                    break;
                case 1:
                    if (Helpers.OSVersion.IsWin11())
                    {
                        TrySetMicaBackdrop(MicaKind.Base);
                    }
                    else
                    {
                        TrySetAcrylicBackdrop();
                    }
                    break;
                case 2:
                    TrySetAcrylicBackdrop();
                    break;
            }
        }

        bool TrySetMicaBackdrop(MicaKind micaKind)
        {
            if (MicaController.IsSupported())
            {
                m_wsdqHelper = new WindowsSystemDispatcherQueueHelper();
                m_wsdqHelper.EnsureWindowsSystemDispatcherQueueController();
                m_configurationSource = new Microsoft.UI.Composition.SystemBackdrops.SystemBackdropConfiguration();
                this.Activated += Window_Activated;
                this.Closed += Window_Closed;
                ((FrameworkElement)this.Content).ActualThemeChanged += Window_ThemeChanged;
                m_configurationSource.IsInputActive = true;
                SetConfigurationSourceTheme();
                micaController = new Microsoft.UI.Composition.SystemBackdrops.MicaController();
                micaController.Kind = micaKind;
                if (finalOpt == true)
                {
                    micaController.TintColor = Color.FromArgb(finalA, finalR, finalG, finalB);
                    micaController.TintOpacity = finalTO;
                }
                micaController.AddSystemBackdropTarget(this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
                micaController.SetSystemBackdropConfiguration(m_configurationSource);
                return true;
            }
            TrySetAcrylicBackdrop();
            return false;
        }
        bool TrySetAcrylicBackdrop()
        {
            if (DesktopAcrylicController.IsSupported())
            {
                m_wsdqHelper = new WindowsSystemDispatcherQueueHelper();
                m_wsdqHelper.EnsureWindowsSystemDispatcherQueueController();
                m_configurationSource = new SystemBackdropConfiguration();
                this.Activated += Window_Activated;
                this.Closed += Window_Closed;
                m_configurationSource.IsInputActive = true;
                SetConfigurationSourceTheme();
                acrylicController = new DesktopAcrylicController();
                acrylicController.TintColor = Color.FromArgb(finalA, finalR, finalG, finalB);
                acrylicController.TintOpacity = finalTO;
                acrylicController.LuminosityOpacity = finalLO;
                ((FrameworkElement)this.Content).ActualThemeChanged += Window_ThemeChanged;
                acrylicController.AddSystemBackdropTarget(this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
                acrylicController.SetSystemBackdropConfiguration(m_configurationSource);
                return true;
            }
            return false;
        }

        private void Window_Activated(object sender, Microsoft.UI.Xaml.WindowActivatedEventArgs args)
        {
            m_configurationSource.IsInputActive = true;
        }

        private void Window_Closed(object sender, WindowEventArgs args)
        {
            if (micaController != null)
            {
                micaController.Dispose();
                micaController = null;
            }
            if (acrylicController != null)
            {
                acrylicController.Dispose();
                acrylicController = null;
            }
            this.Activated -= Window_Activated;
            m_configurationSource = null;
        }

        private void Window_ThemeChanged(FrameworkElement sender, object args)
        {
            if (m_configurationSource != null)
            {
                SetConfigurationSourceTheme();
            }
        }

        private void SetConfigurationSourceTheme()
        {
            switch (((FrameworkElement)this.Content).ActualTheme)
            {
                case ElementTheme.Dark: m_configurationSource.Theme = SystemBackdropTheme.Dark; if (acrylicController != null) { acrylicController.TintColor = Color.FromArgb(255, 0, 0, 0); } break;
                case ElementTheme.Light: m_configurationSource.Theme = SystemBackdropTheme.Light; if (acrylicController != null) { acrylicController.TintColor = Color.FromArgb(255, 255, 255, 255); } break;
                case ElementTheme.Default: m_configurationSource.Theme = SystemBackdropTheme.Default; if (acrylicController != null) { acrylicController.TintColor = Color.FromArgb(255, 0, 0, 0); } break;
            }
        }
        #endregion

    }
}

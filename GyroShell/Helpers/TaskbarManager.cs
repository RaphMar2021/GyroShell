﻿using System;
using static GyroShell.Helpers.Win32Interop;
using System.Threading.Tasks;
using GyroShell.Controls;
using WinRT.Interop;

namespace GyroShell.Helpers
{
    public class TaskbarManager
    {
        static StartMenuWindow sm;
        static bool smVisible = false;
        static NativeRect WorkingArea;
        public static void Init()
        {
            sm = new();
            m_hStartMenu = WindowNative.GetWindowHandle(sm);
            sm.Activate();
            ShowWindow(m_hStartMenu, SW_HIDE);


            m_hTaskBar = FindWindow("Shell_TrayWnd", null);
            m_hMultiTaskBar = FindWindow("Shell_SecondaryTrayWnd", null);

            if (m_hStartMenu == IntPtr.Zero)
            {
                m_hStartMenu = FindWindow("Button", null);
            }
        }

        public static void SetHeight(int left, int right, int top, int bottom)
        {
            int screenWidth = GetSystemMetrics(SM_CXSCREEN);
            int screenHeight = GetSystemMetrics(SM_CYSCREEN);

            NativeRect workArea = new NativeRect();

            workArea.Top = top;
            workArea.Left = left;
            workArea.Right = screenWidth - right;
            workArea.Bottom = screenHeight - bottom;

            //Probably will need rework when using more than 1 monitor
            SystemParametersInfoA(SPI_SETWORKAREA, 0, ref workArea, SPIF_UPDATEINIFILE);
            WorkingArea = workArea;


            var window = sm;
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

            appWindow.Move(new(10, workArea.Bottom - appWindow.Size.Height - 10));
        }

        public static void ShowTaskbar()
        {
            SetVisibility(true);
        }

        public static void HideTaskbar()
        {
            SetVisibility(false);
        }

        private static void SetVisibility(bool isVisible)
        {
            int nCmd = isVisible ? SW_SHOW : SW_HIDE;

            ShowWindow(m_hTaskBar, nCmd);
            ShowWindow(m_hStartMenu, nCmd);
            ShowWindow(m_hMultiTaskBar, nCmd);
        }

        public static async Task ToggleStart()
        {
            //SendMessage(m_hTaskBar, /*WM_SYSCOMMAND*/ 0x0112, (IntPtr) /*SC_TASKLIST*/ 0xF130, (IntPtr)0);

            if (smVisible) { smVisible = false; ShowWindow(m_hStartMenu, SW_HIDE); } else { smVisible = true; ShowWindow(m_hStartMenu, SW_SHOW); }
        }

        private const int WM_COMMAND = 0x0111;
        private const int ID_TRAY_SHOW_OVERFLOW = 0x028a;
        private const int ID_TRAY_HIDE_OVERFLOW = 0x028b;
        public static async Task ShowSysTray()
        {
            SendMessage(m_hTaskBar, WM_COMMAND, (IntPtr)ID_TRAY_SHOW_OVERFLOW, IntPtr.Zero);
        }

        public static async Task ToggleSysControl()
        {
            ShellExecute(IntPtr.Zero, "open", "ms-actioncenter:controlcenter/&suppressAnimations=false&showFooter=true&allowPageNavigation=true" /* CNTRLCTR, bool, bool, bool */, null, null, 1);
        }

        public static async Task ToggleActionCenter()
        {
            ShellExecute(IntPtr.Zero, "open", "ms-actioncenter:" /* ACTION CENTER */, null, null, 1);
        }

        private static IntPtr m_hTaskBar;
        private static IntPtr m_hMultiTaskBar;
        private static IntPtr m_hStartMenu;

    }
}

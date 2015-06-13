using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using ManagedWinapi.Windows;

namespace Wox.Plugin.Windows
{
	public static class WindowsProvider
	{
		public static Dictionary<string, IntPtr> GetWindows()
		{
			EnumWindows(new WindowEnumCallback(AddWnd), 0);

			return Windowss;
		}

		public static Dictionary<string, IntPtr> GetWindows3()
		{
			var windows = new Dictionary<string, IntPtr>();

			var apps = Process.GetProcesses();

			foreach (var app in apps)
			{
				windows.Add(app.MainWindowTitle, app.MainWindowHandle);

				var childWindows = new WindowHandleInfo(app.MainWindowHandle).GetAllChildHandles();
			}

			return windows;
		}

		public static List<SystemWindow> GetWindowsManaged()
		{
			Predicate<SystemWindow> filter = window => !string.IsNullOrEmpty(window.Title) && window.Visible && window.VisibilityFlag;
			var windows = SystemWindow.FilterToplevelWindows(filter).ToList();
			var toplevelCount = windows.Count;

			for (var i = 0; i < toplevelCount; i++)
			{
				windows.AddRange(windows[i].FilterDescendantWindows(true, filter));
			}

			return windows;
		}

		public delegate bool WindowEnumCallback(int hwnd, int lparam);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool EnumWindows(WindowEnumCallback lpEnumFunc, int lParam);

		[DllImport("user32.dll")]
		public static extern void GetWindowText(int h, StringBuilder s, int nMaxCount);

		[DllImport("user32.dll")]
		public static extern bool IsWindowVisible(int h);

		private static readonly Dictionary<string, IntPtr> Windowss = new Dictionary<string, IntPtr>();
		private static bool AddWnd(int hwnd, int lparam)
		{
			if (IsWindowVisible(hwnd))
			{
				StringBuilder sb = new StringBuilder(255);
				GetWindowText(hwnd, sb, sb.Capacity);

				var windowTitle = sb.ToString();

				if (!Windowss.ContainsKey(windowTitle))
				{
					Windowss.Add(windowTitle, (IntPtr)hwnd);
					Trace.WriteLine(windowTitle);
				}
			}
			return true;
		}
	}
}

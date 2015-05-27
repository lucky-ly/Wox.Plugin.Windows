using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Wox.Plugin.Windows
{
	// http://stackoverflow.com/questions/1363167/how-can-i-get-the-child-windows-of-a-window-given-its-hwnd
	public class WindowHandleInfo
	{
		private delegate bool EnumWindowProc(IntPtr hwnd, IntPtr lParam);

		[DllImport("user32")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool EnumChildWindows(IntPtr window, EnumWindowProc callback, IntPtr lParam);

		private IntPtr _MainHandle;

		public WindowHandleInfo(IntPtr handle)
		{
			this._MainHandle = handle;
		}

		public List<IntPtr> GetAllChildHandles()
		{
			List<IntPtr> childHandles = new List<IntPtr>();

			GCHandle gcChildhandlesList = GCHandle.Alloc(childHandles);
			IntPtr pointerChildHandlesList = GCHandle.ToIntPtr(gcChildhandlesList);

			try
			{
				EnumWindowProc childProc = new EnumWindowProc(this.EnumWindow);
				EnumChildWindows(this._MainHandle, childProc, pointerChildHandlesList);
			}
			finally
			{
				gcChildhandlesList.Free();
			}

			return childHandles;
		}

		private bool EnumWindow(IntPtr hWnd, IntPtr lParam)
		{
			GCHandle gcChildhandlesList = GCHandle.FromIntPtr(lParam);

			if (gcChildhandlesList == null || gcChildhandlesList.Target == null)
			{
				return false;
			}

			List<IntPtr> childHandles = gcChildhandlesList.Target as List<IntPtr>;
			childHandles.Add(hWnd);

			return true;
		}
	}
}
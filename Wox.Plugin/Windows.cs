using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using ManagedWinapi.Windows;

namespace Wox.Plugin.Windows
{
	public class Windows : IPlugin
	{
		private PluginInitContext _context;

		public List<Result> Query(Query query)
		{
			this._context.API.StartLoadingBar();
			var results = new List<Result>();

			var search = string.Join(" ", query.ActionParameters.ToArray()).ToLower();
			/*
			var processes = Process.GetProcesses()
				.Where(p => !string.IsNullOrEmpty(p.MainWindowTitle))
				.Select(p =>
					{
						var score = 100;
						var matchTitle = MatchString(p.MainWindowTitle, search);
						var matchExecutable = MatchString(p.ProcessName, search);
						var scoreOff = Math.Max(matchExecutable.Length, matchTitle.Length);
						if (!p.MainWindowTitle.Contains(search) && !p.ProcessName.Contains(search))
							score = score - scoreOff;

						if (matchTitle.Success || matchExecutable.Success)
							return new Result
							{
								Title = p.MainWindowTitle,
								SubTitle = p.ProcessName,
								Action = c =>
								{
									SetForegroundWindow(p.MainWindowHandle);
									return true;
								},
								IcoPath = GetPath(p),
								Score = score
							};

						return null;
					})
				.Where(x => x != null);
			*/

			var windows = this.GetWindowsManaged().Select(w => new Result
			{
				Title = w.Title,
				SubTitle = w.Process != null ? w.Process.ProcessName : "Process is null",
				IcoPath = GetPath(w.Process),
				Action = c =>
				{
					w.TopMost = true;
					return true;
				},
				ContextMenu =
				{
					new Result {Title = "Make Top", Action = c => {w.TopMost = true; return true;}},
					new Result {Title = "Highlight", Action = c => {w.Highlight(); return true;}},
					new Result {Title = "Close", Action = c => {w.SendClose(); return true;}},
					new Result {Title = "Refresh", Action = c => {w.Refresh(); return true;}},
				}
			});
			this._context.API.StopLoadingBar();
			//results.AddRange(processes);
			results.AddRange(windows);
			return results;
		}

		private static string GetPath(Process p)
		{
			var path = "";
			try
			{
				path = p.Modules != null && p.Modules.Count > 0 ? p.Modules[0].FileName : "";
			}
			catch
			{ }

			return path.ToLower();
		}

		public void Init(PluginInitContext context)
		{
			this._context = context;
		}

		private static Match MatchString(string candidate, string search)
		{
			var regexp = string.Join(".*?", search.ToCharArray().Select(x => x.ToString()).ToArray());
		
			if (string.IsNullOrEmpty(regexp)) regexp = ".*";
			
			return Regex.Match(candidate.ToLower(), regexp);
		}

		private List<SystemWindow> GetWindowsManaged()
		{
			var windows = SystemWindow.AllToplevelWindows.ToList();
			var toplevelCount = windows.Count;

			for (var i = 0; i < toplevelCount; i++)
			{
				windows.AddRange(windows[i].AllChildWindows);
			}

			return windows;
		}
		
		[DllImport("USER32.dll")]
		public static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32.dll", EntryPoint = "FindWindowEx")]
		public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

	}
}

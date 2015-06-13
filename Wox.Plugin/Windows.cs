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
//			var rawindows = WindowsProvider.GetWindowsManaged();
			var rawindows = WindowsProvider.GetWindows();

			var results = new List<Result>();

			results.AddRange(rawindows.Select(window =>
			{
				uint procid;
				GetWindowThreadProcessId(window.Value, out procid);

				var process = Process.GetProcessById((int)procid);

				var score = 100;
				var matchTitle = MatchString(window.Key, search);
				var matchExecutable = MatchString(process.ProcessName, search);
				var scoreOff = Math.Max(matchExecutable.Length, matchTitle.Length);
				if (!window.Key.Contains(search) && !process.ProcessName.Contains(search))
					score = score - scoreOff;

				if (matchTitle.Success || matchExecutable.Success)
					return new Result
					{
						Title = window.Key,
						SubTitle = process.ProcessName,
						IcoPath = GetPath(process),
						Score = score,
						Action = c =>
						{
							SetForegroundWindow(window.Value);
							return true;
						},
						//					ContextMenu = new List<Result>
						//					{
						//						new Result {Title = "Make Top", Action = c =>{SetForegroundWindow(window.Value);return true;}},
						//						new Result{Title = "Highlight", Action = c =>{window.Highlight();return true;}},
						//						new Result{Title = "Close", Action = c =>{window.SendClose();return true;}},
						//						new Result{Title = "Refresh", Action = c =>{window.Refresh();return true;}},
						//					}
					};
				else
					return null;
			}).Where(x=>x!=null));
			
			//results.AddRange(processes);
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

		
		
		[DllImport("USER32.dll")]
		public static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32.dll", SetLastError = true)]
		static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

//		[DllImport("user32.dll", EntryPoint = "FindWindowEx")]
//		public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
	}
}

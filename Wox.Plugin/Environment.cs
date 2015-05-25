using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Wox.Plugin.Environment
{
	public class Environment : IPlugin
	{
		private PluginInitContext _context;

		public List<Result> Query(Query query)
		{
			this._context.API.StartLoadingBar();
			var results = new List<Result>();

			var search = string.Join(" ", query.ActionParameters.ToArray()).ToLower();
			var maxLev = (int)Math.Round(search.Length*0.2);

			var processes = Process.GetProcesses()
				.Where(p => !string.IsNullOrEmpty(p.MainWindowTitle))
				.Where(p => IsProcessPass(p,search,maxLev))
				.Select(p => new Result
			{
				Title = p.MainWindowTitle,
				SubTitle = p.ProcessName,
				Action = c =>
				{
					SetForegroundWindow(p.MainWindowHandle);
					return true;
				},
				IcoPath = GetPath(p)
			});

			this._context.API.StopLoadingBar();
			results.AddRange(processes);
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

		private static bool IsProcessPass(Process candidate, string search, int maxLev)
		{
			if (string.IsNullOrEmpty(search)) return true;

			var pass = false;
			var regexp = string.Join(".*?", search.ToCharArray().Select(x=>x.ToString()).ToArray());
			var mtc = Regex.Match(candidate.MainWindowTitle, regexp);

//			pass = pass || candidate.MainWindowTitle.ToLower().Contains(search);
//			pass = pass || Levenshtein.Compute(candidate.MainWindowTitle.ToLower(), search) <= maxLev;
			pass = pass || mtc.Success;
			return pass;
		}

		[DllImport("USER32.dll")]
		public static extern bool SetForegroundWindow(IntPtr hWnd);
	}
}

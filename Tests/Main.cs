using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wox.Plugin;
using Wox.Plugin.Windows;

namespace Tests
{
	[TestClass]
	public class Main
	{
		class ApiMock : IPublicAPI
		{
			public void PushResults(Query query, PluginMetadata plugin, List<Result> results)
			{
				throw new NotImplementedException();
			}

			public bool ShellRun(string cmd, bool runAsAdministrator = false)
			{
				throw new NotImplementedException();
			}

			public void ChangeQuery(string query, bool requery = false)
			{
				throw new NotImplementedException();
			}

			public void CloseApp()
			{
				throw new NotImplementedException();
			}

			public void HideApp()
			{
				throw new NotImplementedException();
			}

			public void ShowApp()
			{
				throw new NotImplementedException();
			}

			public void ShowMsg(string title, string subTitle, string iconPath)
			{
				throw new NotImplementedException();
			}

			public void OpenSettingDialog()
			{
				throw new NotImplementedException();
			}

			public void StartLoadingBar()
			{
				Trace.WriteLine("StartLoadingBar call");
			}

			public void StopLoadingBar()
			{
				Trace.WriteLine("StopLoadingBar call");
			}

			public void InstallPlugin(string path)
			{
				throw new NotImplementedException();
			}

			public void ReloadPlugins()
			{
				throw new NotImplementedException();
			}

			public List<PluginPair> GetAllPlugins()
			{
				throw new NotImplementedException();
			}

			public event WoxKeyDownEventHandler BackKeyDownEvent;
		}

		class DbgPlugin : IPlugin
		{
			private readonly IPlugin _plugin;
			private readonly Stopwatch _t = new Stopwatch();

			public DbgPlugin(IPlugin plugin)
			{
				this._plugin = plugin;
			}
			public List<Result> Query(Query query)
			{
				_t.Reset();
				Trace.WriteLine("Query called: "+query.RawQuery);
				_t.Start();
				var result = this._plugin.Query(query);
				_t.Stop();
				Trace.WriteLine("Query result: "+result.Count+" items in "+_t.ElapsedMilliseconds+"ms");
				return result;
			}

			public void Init(PluginInitContext context)
			{
				_t.Reset();
				Trace.WriteLine("Init called");
				_t.Start();
				this._plugin.Init(context);
				_t.Stop();
				Trace.WriteLine("Init ok in "+_t.ElapsedMilliseconds+"ms");
			}
		}

		[TestMethod]
		public void GetAllProcesses()
		{
			var plugin = new DbgPlugin(new Windows());
			plugin.Init(new PluginInitContext(){API = new ApiMock()});

			var results = plugin.Query(new Query(""));

			Assert.IsNotNull(results);
		}

		[TestMethod]
		public void GetWindowsManagedTest()
		{
			var windows = WindowsProvider.GetWindowsManaged();

			Assert.IsNotNull(windows);
		}

		[TestMethod]
		public void GetWindowsTest()
		{
			var windows = WindowsProvider.GetWindows();

			Assert.IsNotNull(windows);
		}
	}
}

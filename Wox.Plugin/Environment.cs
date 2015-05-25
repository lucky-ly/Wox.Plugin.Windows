using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wox.Plugin.Environment
{
	public class Environment : IPlugin
	{
		public List<Result> Query(Query query)
		{
			throw new NotImplementedException();
		}

		public void Init(PluginInitContext context)
		{
			this._context = context;
		}
	}
}

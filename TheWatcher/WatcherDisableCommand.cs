using Smod2.Commands;
using Smod2;
using Smod2.API;
using System.IO;

namespace TheWatcher
{
	class WatcherDisableCommand : ICommandHandler
	{
		private TheWatcher plugin;

		public WatcherDisableCommand(TheWatcher plugin)
		{
			this.plugin = plugin;
		}

		public string GetCommandDescription()
		{
			return "Disables TheWatcher";
		}

		public string GetUsage()
		{
			return "WATCHERDISABLE";
		}

		public string[] OnCall(ICommandSender sender, string[] args)
		{
			plugin.Info(sender + " ran the " + GetUsage() + " command!");
			this.plugin.pluginManager.DisablePlugin(this.plugin);
			return new string[] { "TheWatcher Disabled" };
		}
	}
}

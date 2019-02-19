using Smod2;
using Smod2.Attributes;
using Smod2.EventHandlers;
using Smod2.Events;
using Smod2.Config;
using System.Collections.Generic;

namespace TheWatcher
{
	[PluginDetails(
		author = "Spooky",
		name = "TheWatcher",
		description = "",
		id = "xyz.wizardlywonders.TheWatcher",
		version = "1.1.4",
		SmodMajor = 3,
		SmodMinor = 3,
		SmodRevision = 0
	)]
	public class TheWatcher : Plugin
    {
		public List<string> ActiveWatchers = new List<string>();

		public override void OnDisable()
		{
			this.Info("TheWatcher has been disabled.");
		}

		public override void OnEnable()
		{
			this.Info("TheWatcher has loaded successfully.");
		}

		public override void Register()
		{
			// Register config
			this.AddConfig(new ConfigSetting("watcher_enable", true, SettingType.BOOL, true, "Whether TheWatcher should be enabled on server start."));

			// Register events
			this.AddEventHandlers(new MiscEventHandler(this), Priority.Highest);

			// Register commands
			this.AddCommand("watcherdisable", new WatcherDisableCommand(this));
			this.AddCommand("watcher", new MakeWatcherCommand(this));
		}
	}
}

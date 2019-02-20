using Smod2.Commands;
using Smod2;
using Smod2.API;
using System.IO;
using System;
using UnityEngine;

namespace TheWatcher
{
	class MakeWatcherCommand : ICommandHandler
	{
		private TheWatcher plugin;

		public MakeWatcherCommand(TheWatcher plugin)
		{
			this.plugin = plugin;
		}

		public string GetCommandDescription()
		{
			return "Toggles a player's role as Watcher.";
		}

		public string GetUsage()
		{
			return "WATCHER <playerID>";
		}

		public string[] OnCall(ICommandSender sender, string[] args)
		{
			// -- Fetch target
			if (args.Length == 1)
			{
				// -- Find target player by args[0]
				if (short.TryParse(args[0], out short pID))
				{
					foreach (Player p in this.plugin.Server.GetPlayers())
					{
						// -- Toggle player as Watcher
						if (p.PlayerId == pID)
						{
							if (this.plugin.ActiveWatchers.Contains(p.SteamId))
							{
								p.ChangeRole(Role.SPECTATOR);

								this.plugin.ActiveWatchers.Remove(p.SteamId);

								return new string[] { args[0] + " is no longer a Watcher. " };
							}
							else
							{
								p.ChangeRole(Role.TUTORIAL, true, true, true);
								p.GiveItem(ItemType.FLASHLIGHT);
								p.GiveItem(ItemType.COM15);

								this.plugin.ActiveWatchers.Add(p.SteamId);

								return new string[] { args[0] + " set as Watcher successfully." };
							}
						}
					}

					return new string[] { "Nobody with that player ID exists." };
				}
				else
				{
					return new string[] { "Could not parse player ID." };
				}
			}
			else
			{
				return new string[] { "Invalid number of arguments." };
			}
		}
	}
}

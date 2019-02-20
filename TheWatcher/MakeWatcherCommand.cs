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
								// -- Set as tutorial role and spawn in d-class chambers
								p.ChangeRole(Role.TUTORIAL, true, true, true);

								// -- Give appropriate items
								p.GiveItem(ItemType.FLASHLIGHT);
								p.GiveItem(ItemType.COM15);

								// -- Modify gun to have supressor
								GameObject player = (GameObject)p.GetGameObject();
								Inventory inv = player.GetComponent<Inventory>();
								Inventory.SyncItemInfo gun = inv.items[1];
								gun.modBarrel = 1;
								gun.modOther = 0;
								inv.items[1] = gun;

								// -- Give informational personal broadcast
								p.PersonalBroadcast(10, "You are a <color=#55ff55>Watcher</color>! You traded your ability to interact with the game in return for immortality.", false);

								// -- Add to global watcher list
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

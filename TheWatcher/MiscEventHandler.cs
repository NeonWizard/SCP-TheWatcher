using Smod2;
using Smod2.API;
using Smod2.Events;
using Smod2.EventHandlers;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace TheWatcher
{
	class MiscEventHandler :
		IEventHandlerWaitingForPlayers, IEventHandlerSetConfig, IEventHandlerPlayerPickupItem,
		IEventHandlerDoorAccess, IEventHandlerElevatorUse, IEventHandlerPlayerHurt, IEventHandlerWarheadStartCountdown,
		IEventHandlerWarheadStopCountdown, IEventHandlerCheckEscape, IEventHandlerPocketDimensionEnter,
		IEventHandlerRoundEnd, IEventHandlerSetRole
	{
		private readonly TheWatcher plugin;

		public MiscEventHandler(TheWatcher plugin) => this.plugin = plugin;

		public void OnWaitingForPlayers(WaitingForPlayersEvent ev)
		{
			if (!this.plugin.GetConfigBool("watcher_enable")) this.plugin.pluginManager.DisablePlugin(plugin);
		}

		public void OnSetConfig(SetConfigEvent ev)
		{
			// -- Ensure the Tutorial role can't trigger 096
			if (ev.Key == "scp096_ignored_role")
			{
				List<int> value = new List<int>((int[])ev.Value);
				value.Add(14);
				ev.Value = value.ToArray();
			}
		}

		public void OnPlayerPickupItem(PlayerPickupItemEvent ev)
		{
			// -- Block item pickup
			if (this.plugin.ActiveWatchers.Contains(ev.Player.SteamId))
			{
				ev.Item.Drop();
				ev.Allow = false;
			}
		}

		public void OnDoorAccess(PlayerDoorAccessEvent ev)
		{
			// -- Block door access
			if (this.plugin.ActiveWatchers.Contains(ev.Player.SteamId))
			{
				GameObject player = ((GameObject)ev.Player.GetGameObject());
				Vector3 destination = player.transform.position + player.transform.forward * 2.8f;
				ev.Player.Teleport(new Vector(destination.x, destination.y, destination.z));

				ev.Allow = false;
			}
		}

		public void OnElevatorUse(PlayerElevatorUseEvent ev)
		{
			// -- Block elevator access
			if (this.plugin.ActiveWatchers.Contains(ev.Player.SteamId))
			{
				ev.AllowUse = false;
			}
		}

		public void OnPlayerHurt(PlayerHurtEvent ev)
		{
			// -- Block (most) damage
			if (this.plugin.ActiveWatchers.Contains(ev.Player.SteamId))
			{
				// Unavoidable damage types
				if (ev.DamageType == DamageType.LURE || ev.DamageType == DamageType.WALL || ev.DamageType == DamageType.NUKE)
				{
					ev.Damage = ev.Player.GetHealth() + 100;
				}
				// Null any other damage
				else
				{
					if (ev.Player.GetHealth() < 1000)
					{
						ev.Player.AddHealth(1000);
					}
					ev.Damage = 0;
				}
			}
		}

		public void OnStartCountdown(WarheadStartEvent ev)
		{
			// -- Block nuke activation
			if (this.plugin.ActiveWatchers.Contains(ev.Activator?.SteamId))
			{
				ev.Cancel = true;
			}
		}

		public void OnStopCountdown(WarheadStopEvent ev)
		{
			// -- Block nuke deactivation
			if (this.plugin.ActiveWatchers.Contains(ev.Activator?.SteamId))
			{
				ev.Cancel = true;
			}
		}

		public void OnCheckEscape(PlayerCheckEscapeEvent ev)
		{
			// -- Block player escape
			if (this.plugin.ActiveWatchers.Contains(ev.Player.SteamId))
			{
				ev.AllowEscape = false;
			}
		}

		public void OnPocketDimensionEnter(PlayerPocketDimensionEnterEvent ev)
		{
			// -- Block teleportation to the pocket dimension
			if (this.plugin.ActiveWatchers.Contains(ev.Player.SteamId))
			{
				ev.TargetPosition = ev.LastPosition;
			}
		}

		public void OnRoundEnd(RoundEndEvent ev)
		{
			// -- Reset all Watchers back to normal players
			foreach (Player p in this.plugin.Server.GetPlayers())
			{
				if (this.plugin.ActiveWatchers.Contains(p.SteamId))
				{
					p.SetHealth(100);
				}
			}
			this.plugin.ActiveWatchers.Clear();
		}

		public void OnSetRole(PlayerSetRoleEvent ev)
		{
			// -- Remove from ActiveWatcher list
			this.plugin.ActiveWatchers.Remove(ev.Player.SteamId);
		}

		//public void OnUpdate(UpdateEvent ev)
		//{
		//	// -- Block player from entering nuke room
		//	DateTime timeOnEvent = DateTime.Now;
		//	if (DateTime.Now >= timeOnEvent)
		//	{
		//		if (this.plugin.ActiveWatchers.Count == 0) return;

		//		timeOnEvent = DateTime.Now.AddSeconds(4.0);
		//		foreach (Player p in PluginManager.Manager.Server.GetPlayers())
		//		{
		//			if (this.plugin.ActiveWatchers.Contains(p.SteamId))
		//			{
		//				foreach (var elevator in this.plugin.Server.Map.GetElevators())
		//				{
		//					if (elevator.ElevatorType == ElevatorType.WarheadRoom)
		//					{
		//						List<Vector> nukeElevator = elevator.GetPositions();

		//						if (Vector.Distance(p.GetPosition(), nukeElevator[1]) < 8)
		//						{
		//							p.Teleport(new Vector(nukeElevator[0].x, nukeElevator[0].y + 1, nukeElevator[0].z));
		//						}
		//					}
		//				}
		//			}
		//		}
		//	}
		//}
	}
}
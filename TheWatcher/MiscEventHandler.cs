using Smod2;
using Smod2.API;
using ServerMod2.API;
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
		IEventHandlerWaitingForPlayers, IEventHandlerSetConfig, IEventHandlerPlayerPickupItem, IEventHandlerPlayerDropItem,
		IEventHandlerDoorAccess, IEventHandlerElevatorUse, IEventHandlerPlayerHurt, IEventHandlerWarheadStartCountdown,
		IEventHandlerWarheadStopCountdown, IEventHandlerCheckEscape, IEventHandlerPocketDimensionEnter,
		IEventHandlerRoundEnd, IEventHandlerSetRole, IEventHandlerGeneratorEjectTablet, IEventHandlerShoot
	{
		private readonly TheWatcher plugin;

		private int lastElevatorEvent = 0;

		public MiscEventHandler(TheWatcher plugin) => this.plugin = plugin;

		public void OnWaitingForPlayers(WaitingForPlayersEvent ev)
		{
			if (!this.plugin.GetConfigBool("watcher_enable")) this.plugin.pluginManager.DisablePlugin(plugin);

			PlayerManager.localPlayer.GetComponent<CharacterClassManager>().klasy[14].runSpeed = 200f;
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

		public void OnPlayerDropItem(PlayerDropItemEvent ev)
		{
			// -- Block item drop
			if (this.plugin.ActiveWatchers.Contains(ev.Player.SteamId))
			{
				ev.Allow = false;
			}
		}

		public void OnDoorAccess(PlayerDoorAccessEvent ev)
		{
			if (this.plugin.ActiveWatchers.Contains(ev.Player.SteamId))
			{
				// -- Teleport through door
				GameObject player = ((GameObject)ev.Player.GetGameObject());
				Vector3 destination = player.transform.position + player.transform.forward * 2.8f;
				ev.Player.Teleport(new Vector(destination.x, destination.y, destination.z));

				// -- Block door access
				ev.Allow = false;
			}
		}

		public void OnElevatorUse(PlayerElevatorUseEvent ev)
		{
			// -- Teleport to other elevator
			if (this.plugin.ActiveWatchers.Contains(ev.Player.SteamId))
			{
				// -- Block elevator access
				ev.AllowUse = false;

				// -- Elevator event gets called twice sometimes :^)
				int now = Time.frameCount;
				if (now - lastElevatorEvent < 5) return;
				lastElevatorEvent = now;

				Vector3 pos = new Vector3(ev.Player.GetPosition().x, ev.Player.GetPosition().y, ev.Player.GetPosition().z);
				// -- Search through base game lifts for the one being used
				foreach (Lift lift in UnityEngine.Object.FindObjectsOfType<Lift>())
				{
					SmodElevator smodElevator = new SmodElevator(lift);

					if (smodElevator.ElevatorType == ev.Elevator.ElevatorType)
					{
						// -- Once found, find an exit point away from the player and teleport him
						foreach (Lift.Elevator e in lift.elevators)
						{
							if (Vector3.Distance(e.target.transform.position, pos) > 50)
							{
								Vector3 destination = e.target.transform.position + Quaternion.Euler(0, 90, 0) * e.target.transform.forward * 5.5f;
								ev.Player.Teleport(new Vector(destination.x, destination.y, destination.z));

								return;
							}
						}

						return;
					}
				}
			}
		}

		public void OnPlayerHurt(PlayerHurtEvent ev)
		{
			// -- Block (most) damage
			if (this.plugin.ActiveWatchers.Contains(ev.Player.SteamId))
			{
				// Until 173_ignore gets fixed, allow peanut to kill the watcher to balance things out
				if (ev.DamageType == DamageType.SCP_173)
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

			// -- Nullify teleport gun damage
			if (this.plugin.ActiveWatchers.Contains(ev.Attacker.SteamId))
			{
				ev.Damage = 0;
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

		public void OnGeneratorEjectTablet(PlayerGeneratorEjectTabletEvent ev)
		{
			if (this.plugin.ActiveWatchers.Contains(ev.Player.SteamId))
			{
				ev.Allow = false;
			}
		}

		public void OnShoot(PlayerShootEvent ev)
		{
			// -- Warping pistol
			if (this.plugin.ActiveWatchers.Contains(ev.Player.SteamId))
			{
				GameObject player = (GameObject)ev.Player.GetGameObject();
				WeaponManager playerWM = player.GetComponent<WeaponManager>();
				Ray ray = new Ray(playerWM.camera.transform.position + playerWM.camera.transform.forward, playerWM.camera.transform.forward);
				if (Physics.Raycast(ray, out RaycastHit raycastHit, 150f))
				{
					Vector3 destination = raycastHit.point + raycastHit.normal * 1f;
					ev.Player.Teleport(new Vector(destination.x, destination.y, destination.z));
				}

				ev.Player.SetAmmo(AmmoType.DROPPED_9, 20);
			}
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

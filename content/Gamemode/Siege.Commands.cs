﻿using Keg.Extensions;
using TC2.Base.Components;

namespace TC2.Siege
{
	public static partial class Siege
	{
#if SERVER
		[ChatCommand.Region("kobold", "", creative: true)]
		public static void KoboldCommand(ref ChatCommand.Context context, byte? faction_id = null)
		{
			ref var region = ref context.GetRegion();
			ref var player = ref context.GetPlayer();

			region.SpawnPrefab("kobold.male", player.control.mouse.position, faction_id: faction_id ?? player.faction_id).ContinueWith((ent) =>
			{
				SetKoboldLoadout(ent);
			});
		}

		[ChatCommand.Region("hoob", "", creative: true)]
		public static void HoobCommand(ref ChatCommand.Context context, byte? faction_id = null)
		{
			ref var region = ref context.GetRegion();
			ref var player = ref context.GetPlayer();

			App.WriteLine(faction_id);

			region.SpawnPrefab("hoob", player.control.mouse.position, faction_id: faction_id ?? player.faction_id).ContinueWith((ent) =>
			{
				SetHoobLoadout(ent);
			});
		}

		[ChatCommand.Region("nextmap", "", admin: true)]
		public static void NextMapCommand(ref ChatCommand.Context context, string map)
		{
			ref var region = ref context.GetRegion();
			if (!region.IsNull())
			{
				var map_handle = new Map.Handle(map);
				//App.WriteLine($"nextmapping {map}; {map_handle}; {map_handle.id}");

				//if (map_handle.id != 0)
				{
					Siege.ChangeMap(ref region, map_handle);
				}
			}
		}

		[ChatCommand.Region("difficulty", "", admin: true)]
		public static void DifficultyCommand(ref ChatCommand.Context context, float difficulty)
		{
			ref var region = ref context.GetRegion();
			if (!region.IsNull())
			{
				ref var g_siege_state = ref region.GetSingletonComponent<Siege.Gamemode.State>();
				if (!g_siege_state.IsNull())
				{
					var difficulty_old = g_siege_state.difficulty;
					g_siege_state.difficulty = difficulty;

					region.SyncGlobal(ref g_siege_state);

					if (context.GetConnection().IsNotNull())
					{
						Server.SendChatMessage($"Set difficulty from {difficulty_old:0.00} to {g_siege_state.difficulty:0.00}.", channel: Chat.Channel.System, target_player_id: context.GetConnection().GetPlayerID());
					}

					//else
					//{
					//	Server.SendChatMessage($"Current difficulty: {g_siege_state.difficulty:0.00}.", channel: Chat.Channel.System, target_player_id: context.GetConnection().GetPlayerID());
					//}
				}
			}
		}

		[ChatCommand.Region("nextwave", "", admin: true)]
		public static void NextWaveCommand(ref ChatCommand.Context context)
		{
			ref var region = ref context.GetRegion();
			if (!region.IsNull())
			{
				ref var g_siege_state = ref region.GetSingletonComponent<Siege.Gamemode.State>();
				if (!g_siege_state.IsNull())
				{
					if (g_siege_state.status == Gamemode.Status.Preparing)
					{
						g_siege_state.status = Gamemode.Status.Running;
					}
					else
					{
						g_siege_state.t_next_wave = g_siege_state.t_match_elapsed;
						Server.SendChatMessage($"Forced next wave.", channel: Chat.Channel.System);
					}

					region.SyncGlobal(ref g_siege_state);
				}
			}
		}

		[ChatCommand.Region("setwave", "", admin: true)]
		public static void SetWaveCommand(ref ChatCommand.Context context, int wave)
		{
			ref var region = ref context.GetRegion();
			if (!region.IsNull())
			{
				ref var g_siege_state = ref region.GetSingletonComponent<Siege.Gamemode.State>();
				if (!g_siege_state.IsNull())
				{
					if (g_siege_state.status == Gamemode.Status.Preparing)
					{
						g_siege_state.status = Gamemode.Status.Running;
					}
					
					g_siege_state.wave_current = (ushort)(wave - 1);
					g_siege_state.t_next_wave = g_siege_state.t_match_elapsed;
					Server.SendChatMessage($"Set wave to {wave}.", channel: Chat.Channel.System);
					
					region.SyncGlobal(ref g_siege_state);
				}
			}
		}


		[ChatCommand.Region("pause", "", admin: true)]
		public static void PauseCommand(ref ChatCommand.Context context, bool? value = null)
		{
			ref var region = ref context.GetRegion();
			if (!region.IsNull())
			{
				ref var g_siege_state = ref region.GetSingletonComponent<Siege.Gamemode.State>();
				if (!g_siege_state.IsNull())
				{
					var sync = false;
					sync |= g_siege_state.flags.TrySetFlag(Siege.Gamemode.Flags.Paused, value ?? !g_siege_state.flags.HasAll(Siege.Gamemode.Flags.Paused));
					Server.SendChatMessage(g_siege_state.flags.HasAll(Siege.Gamemode.Flags.Paused) ? "Paused Siege." : "Unpaused Siege.", channel: Chat.Channel.System);

					if (sync)
					{
						region.SyncGlobal(ref g_siege_state);
					}
				}
			}
		}

		public static void ChangeMap(ref Region.Data region, Map.Handle map)
		{
			//ref var region = ref world.GetAnyRegion();
			if (!region.IsNull())
			{
				var region_id_old = region.GetID();

				ref var world = ref Server.GetWorld();
				if (world.TryGetFirstAvailableRegionID(out var region_id_new))
				{
					region.Wait().ContinueWith(() =>
					{
						Net.SetActiveRegionForAllPlayers(0);

						ref var world = ref Server.GetWorld();
						world.UnloadRegion(region_id_old).ContinueWith(() =>
						{
							ref var world = ref Server.GetWorld();

							ref var region_new = ref world.ImportRegion(region_id_new, map);
							if (!region_new.IsNull())
							{
								world.SetContinueRegionID(region_id_new);

								region_new.Wait().ContinueWith(() =>
								{
									Net.SetActiveRegionForAllPlayers(region_id_new);
								});
							}
						});
					});
				}
			}
		}
#endif
	}
}

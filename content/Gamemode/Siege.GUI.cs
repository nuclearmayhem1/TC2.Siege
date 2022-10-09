﻿using Keg.Extensions;
using TC2.Base.Components;

namespace TC2.Siege
{
	public static partial class Siege
	{
#if CLIENT
		public partial struct SiegeDefenderGUI: IGUICommand
		{
			//public Entity ent_siege;
			public Siege.Gamemode siege;
			public Siege.Bounty.Global g_bounty;

			public void Draw()
			{
				var window_pos = (GUI.CanvasSize * new Vector2(0.50f, 0.00f)) + new Vector2(0, 64 + 4);
				using (var window = GUI.Window.Standalone("Siege", position: window_pos, size: new Vector2(400, 0), pivot: new Vector2(0.50f, 0.00f), padding: new(4)))
				{
					this.StoreCurrentWindowTypeID();
					if (window.show)
					{
						//GUI.DrawWindowBackground();

						ref var region = ref Client.GetRegion();
						ref var world = ref Client.GetWorld();
						ref var game_info = ref Client.GetGameInfo();

						var time_left = MathF.Max(this.siege.t_next_wave - this.siege.match_time, 0.00f);

						using (GUI.Group.New(size: new Vector2(GUI.GetRemainingWidth() - 120, GUI.GetRemainingHeight())))
						{
							if (this.siege.status == Gamemode.Status.Running)
							{
								GUI.Title($"Next wave: {(time_left):0} s", size: 22, color: time_left > 10.00f ? GUI.font_color_title : GUI.font_color_yellow);
								GUI.Title($"Next reward: {(MathF.Max(this.g_bounty.t_next_update - this.siege.match_time, 0.00f)):0} s", size: 22);
								GUI.Title($"Difficulty: {this.siege.difficulty:0.0}", size: 22);
								//Shop.DrawProducts(ref region, default, default, default, this.g_bounty.rewards.AsSpan(), 1);
							}
							else
							{
								GUI.TitleCentered($"{this.siege.status}", size: 32, pivot: new(0.00f, 0.50f), color: GUI.font_color_yellow);
							}
						}

						GUI.SameLine();

						using (GUI.Group.New(size: new Vector2(GUI.GetRemainingWidth(), GUI.GetRemainingHeight())))
						{
							GUI.TitleCentered($"Wave: {this.siege.wave_current}", size: 32, pivot: new(1.00f, 0.50f));
						}
					}
				}
			}
		}

		[ISystem.EarlyGUI(ISystem.Mode.Single)]
		public static void OnGUIDefender(Entity entity, [Source.Owned] in Player.Data player, [Source.Global] in Siege.Gamemode siege, [Source.Global] in Siege.Bounty.Global g_bounty)
		{
			if (player.IsLocal() && player.faction_id == siege.faction_defenders)
			{
				var gui = new SiegeDefenderGUI()
				{
					siege = siege,
					g_bounty = g_bounty
				};
				gui.Submit();
			}
		}
#endif

#if CLIENT
		public partial struct SiegeAttackerGUI: IGUICommand
		{
			public Entity ent_siege;
			public Siege.Gamemode siege;

			public void Draw()
			{
				var window_pos = (GUI.CanvasSize * new Vector2(0.50f, 0.00f)) + new Vector2(100, 48);
				using (var window = GUI.Window.Standalone("Siege2", position: window_pos, size: new Vector2(700, 400), pivot: new Vector2(0.50f, 0.00f)))
				{
					this.StoreCurrentWindowTypeID();
					if (window.show)
					{
						ref var region = ref Client.GetRegion();
						ref var world = ref Client.GetWorld();
						ref var game_info = ref Client.GetGameInfo();

						GUI.Title($"{this.siege.faction_defenders.id}");
					}
				}
			}
		}

		[ISystem.EarlyGUI(ISystem.Mode.Single)]
		public static void OnGUIAttacker(Entity entity, [Source.Owned] in Player.Data player, [Source.Global] in Siege.Gamemode siege)
		{
			if (player.IsLocal() && player.faction_id == siege.faction_attackers)
			{
				var gui = new SiegeAttackerGUI()
				{
					siege = siege
				};
				gui.Submit();
			}
		}
#endif
	}
}

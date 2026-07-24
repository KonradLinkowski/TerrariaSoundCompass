using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SoundCompass.Common.Models;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SoundCompass.Common.Systems
{
	public class SoundTrackerSystem : ModSystem
	{
		private const float MaxDistanceTiles = 100f;
		private const float MaxDistancePixels = MaxDistanceTiles * 16f;
		private const int MaxEmitters = 96;
		private const bool EnableEmitterLogging = false;
		private const uint EmitterLogCooldownTicks = 120;

		private static readonly Dictionary<int, uint> _nextLogTickByEmitter = new();

		private static readonly List<SoundEmitter> _activeEmitters = new();

		public static IReadOnlyList<SoundEmitter> ActiveEmitters => _activeEmitters;
		public static float MaxDistanceForCompass => MaxDistancePixels;

		public override void PostUpdateEverything()
		{
			if (_nextLogTickByEmitter.Count > 4096)
			{
				_nextLogTickByEmitter.Clear();
			}

			_activeEmitters.Clear();
			if (!SoundCompassStateSystem.IsOverlayActive)
			{
				_nextLogTickByEmitter.Clear();
				return;
			}

			Player player = Main.LocalPlayer;
			if (!player.active || player.dead)
			{
				return;
			}

			CollectNpcEmitters(player);
			CollectProjectileEmitters(player);
			CollectItemEmitters(player);

			_activeEmitters.Sort(static (a, b) => b.Distance.CompareTo(a.Distance));
			if (_activeEmitters.Count > MaxEmitters)
			{
				_activeEmitters.RemoveRange(MaxEmitters, _activeEmitters.Count - MaxEmitters);
			}
		}

		private static void CollectNpcEmitters(Player player)
		{
			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];

				float distance = Vector2.Distance(player.Center, npc.Center);
				if (distance > MaxDistancePixels)
				{
					continue;
				}

				TrackEmitter(SoundEmitterKind.NPC, npc.type, npc.whoAmI, npc.Center, distance);
			}
		}

		private static void CollectProjectileEmitters(Player player)
		{
			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile projectile = Main.projectile[i];
				if (!projectile.active || projectile.hide || projectile.type <= ProjectileID.None)
				{
					continue;
				}

				// Heuristic: only include projectiles likely to produce gameplay sounds.
				if (projectile.damage <= 0)
				{
					continue;
				}

				if (!projectile.hostile && !projectile.friendly)
				{
					continue;
				}

				if (projectile.width <= 1 || projectile.height <= 1)
				{
					continue;
				}

				float speedSq = projectile.velocity.LengthSquared();
				if (speedSq < 0.04f)
				{
					continue;
				}

				float distance = Vector2.Distance(player.Center, projectile.Center);
				if (distance > MaxDistancePixels)
				{
					continue;
				}

				TrackEmitter(SoundEmitterKind.Projectile, projectile.type, projectile.whoAmI, projectile.Center, distance);
			}
		}

		private static void CollectItemEmitters(Player player)
		{
			if (player.itemAnimation <= 0 || player.HeldItem == null || player.HeldItem.IsAir)
			{
				return;
			}

			int itemType = player.HeldItem.type;
			if (itemType <= 0)
			{
				return;
			}

			float distance = 24f;
			TrackEmitter(SoundEmitterKind.Item, itemType, player.whoAmI, player.Center, distance);
		}

		private static void TrackEmitter(SoundEmitterKind kind, int type, int identity, Vector2 worldPosition, float distance)
		{
			_activeEmitters.Add(new SoundEmitter(kind, type, identity, worldPosition, distance));

			if (!EnableEmitterLogging)
			{
				return;
			}

			uint now = Main.GameUpdateCount;
			int emitterKey = HashCode.Combine((int)kind, type, identity);
			if (_nextLogTickByEmitter.TryGetValue(emitterKey, out uint nextLogTick) && now < nextLogTick)
			{
				return;
			}

			_nextLogTickByEmitter[emitterKey] = now + EmitterLogCooldownTicks;

			Vector2 tilePosition = worldPosition / 16f;
			ModContent.GetInstance<global::SoundCompass.SoundCompass>().Logger.Info(
				$"[SoundCompass] Tracked sound emitter kind={kind} type={type} id={identity} world=({worldPosition.X:0.0}, {worldPosition.Y:0.0}) tiles=({tilePosition.X:0.00}, {tilePosition.Y:0.00}) distancePx={distance:0.0}");
		}
	}
}
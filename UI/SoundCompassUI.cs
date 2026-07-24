using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SoundCompass.Common.Configs;
using SoundCompass.Common.Models;
using SoundCompass.Common.Systems;
using Terraria;
using Terraria.GameContent;
using Terraria.UI.Chat;

namespace SoundCompass.UI
{
	public class SoundCompassUI
	{
		private const bool DebugOverlay = true;
		private const float RingThickness = 2f;
		private const float MarkerLength = 10f;
		private const float MarkerThickness = 2f;
		private const float DotSize = 5f;
		private const float IconMaxSize = 20f;
		private const float MinAlpha = 0.2f;
		private const int MaxDebugLines = 16;

		public void Draw(SpriteBatch spriteBatch)
		{
			Player player = Main.LocalPlayer;
			if (!player.active || player.dead)
			{
				return;
			}

			Vector2 center = new(Main.screenWidth * 0.5f, Main.screenHeight * 0.5f);
			CompassLayoutType layout = SoundCompassStateSystem.CompassLayout;
			float circleRadius = SoundCompassStateSystem.CircleRadius;

			if (DebugOverlay)
			{
				DrawDebugOverlay(spriteBatch);
			}

			foreach (SoundEmitter emitter in SoundTrackerSystem.ActiveEmitters)
			{
				Vector2 toEmitter = emitter.WorldPosition - player.Center;
				if (toEmitter.LengthSquared() < 0.0001f)
				{
					continue;
				}

				if (SoundCompassStateSystem.OutsideFocusAreaOnly && !IsOutsideFocusArea(emitter, layout, circleRadius, player))
				{
					continue;
				}

				Vector2 direction = Vector2.Normalize(toEmitter);
				Vector2 iconPosition = layout == CompassLayoutType.CenterRing
					? center + direction * circleRadius
					: ProjectToScreenBorder(center, direction);
				float alpha = CalculateAlpha(emitter.Distance);
				DrawEmitterMarker(spriteBatch, emitter, iconPosition, direction, alpha);
			}
		}

		private static bool IsOutsideFocusArea(SoundEmitter emitter, CompassLayoutType layout, float circleRadius, Player player)
		{
			// Convert world-space delta to UI-space pixels so focus checks match drawn marker spaces.
			Vector2 toEmitterWorld = emitter.WorldPosition - player.Center;
			Vector2 zoom = Main.GameViewMatrix.Zoom;
			float safeUiScale = Main.UIScale <= 0f ? 1f : Main.UIScale;
			Vector2 toEmitterUi = new(
				toEmitterWorld.X * zoom.X / safeUiScale,
				toEmitterWorld.Y * zoom.Y / safeUiScale);

			if (layout == CompassLayoutType.CenterRing)
			{
				float distance = toEmitterUi.Length();
				return distance > circleRadius;
			}

			float halfWidth = Math.Max(8f, Main.screenWidth * 0.5f - SoundCompassStateSystem.BorderInset);
			float halfHeight = Math.Max(8f, Main.screenHeight * 0.5f - SoundCompassStateSystem.BorderInset);

			bool isInsideFocusRect = Math.Abs(toEmitterUi.X) <= halfWidth
				&& Math.Abs(toEmitterUi.Y) <= halfHeight;

			return !isInsideFocusRect;
		}

		private static Vector2 ProjectToScreenBorder(Vector2 center, Vector2 direction)
		{
			float halfWidth = Math.Max(8f, Main.screenWidth * 0.5f - SoundCompassStateSystem.BorderInset);
			float halfHeight = Math.Max(8f, Main.screenHeight * 0.5f - SoundCompassStateSystem.BorderInset);

			float dx = Math.Abs(direction.X) > 0.0001f ? halfWidth / Math.Abs(direction.X) : float.MaxValue;
			float dy = Math.Abs(direction.Y) > 0.0001f ? halfHeight / Math.Abs(direction.Y) : float.MaxValue;
			float scale = Math.Min(dx, dy);

			return center + direction * scale;
		}

		private static float CalculateAlpha(float distance)
		{
			float t = MathHelper.Clamp(distance / SoundTrackerSystem.MaxDistanceForCompass, 0f, 1f);
			return MathHelper.Lerp(1f, MinAlpha, t);
		}

		private static void DrawEmitterMarker(SpriteBatch spriteBatch, SoundEmitter emitter, Vector2 iconPosition, Vector2 direction, float alpha)
		{
			if (TryGetEmitterSprite(emitter, out Texture2D texture, out Rectangle sourceFrame))
			{
				Vector2 sourceSize = new(sourceFrame.Width, sourceFrame.Height);
				float maxDimension = Math.Max(sourceSize.X, sourceSize.Y);
				float scale = maxDimension > 0f ? IconMaxSize / maxDimension : 1f;

				Vector2 origin = new(sourceFrame.Width * 0.5f, sourceFrame.Height * 0.5f);
				Color drawColor = Color.White * alpha;

				// Small shadow keeps icons readable on bright backgrounds.
				spriteBatch.Draw(texture, iconPosition + Vector2.One, sourceFrame, Color.Black * (alpha * 0.5f), 0f, origin, scale, SpriteEffects.None, 0f);
				spriteBatch.Draw(texture, iconPosition, sourceFrame, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
				return;
			}

			DrawFallbackMarker(spriteBatch, emitter, iconPosition, direction, alpha);
		}

		private static bool TryGetEmitterSprite(SoundEmitter emitter, out Texture2D texture, out Rectangle sourceFrame)
		{
			texture = null;
			sourceFrame = default;

			switch (emitter.Kind)
			{
				case SoundEmitterKind.NPC:
					if ((uint)emitter.Identity >= Main.maxNPCs)
					{
						return false;
					}

					NPC npc = Main.npc[emitter.Identity];
					if (!npc.active || npc.type != emitter.Type)
					{
						return false;
					}

					texture = TextureAssets.Npc[npc.type].Value;
					sourceFrame = npc.frame.Width > 0 && npc.frame.Height > 0 ? npc.frame : texture.Frame();
					return true;

				case SoundEmitterKind.Projectile:
					if ((uint)emitter.Identity >= Main.maxProjectiles)
					{
						return false;
					}

					Projectile projectile = Main.projectile[emitter.Identity];
					if (!projectile.active || projectile.type != emitter.Type)
					{
						return false;
					}

					texture = TextureAssets.Projectile[projectile.type].Value;
					int frames = Main.projFrames[projectile.type];
					if (frames <= 1)
					{
						sourceFrame = texture.Frame();
						return true;
					}

					int frameHeight = texture.Height / frames;
					int frameY = Math.Clamp(projectile.frame, 0, frames - 1) * frameHeight;
					sourceFrame = new Rectangle(0, frameY, texture.Width, frameHeight);
					return true;

				case SoundEmitterKind.Item:
					if (emitter.Type <= 0)
					{
						return false;
					}

					texture = TextureAssets.Item[emitter.Type].Value;
					if (Main.itemAnimations[emitter.Type] != null)
					{
						sourceFrame = Main.itemAnimations[emitter.Type].GetFrame(texture);
					}
					else
					{
						sourceFrame = texture.Frame();
					}

					return true;
			}

			return false;
		}

		private static void DrawFallbackMarker(SpriteBatch spriteBatch, SoundEmitter emitter, Vector2 iconPosition, Vector2 direction, float alpha)
		{
			Color baseColor = emitter.Kind switch
			{
				SoundEmitterKind.NPC => new Color(255, 184, 96),
				SoundEmitterKind.Projectile => new Color(120, 220, 255),
				SoundEmitterKind.Item => new Color(160, 255, 160),
				_ => Color.White
			};

			Texture2D pixel = TextureAssets.MagicPixel.Value;
			Rectangle pixelSource = new(0, 0, 1, 1);
			Color color = baseColor * alpha;

			float rotation = (float)Math.Atan2(direction.Y, direction.X);
			Vector2 lineScale = new(MarkerLength, MarkerThickness);
			Vector2 lineCenter = iconPosition - direction * (MarkerLength * 0.5f);
			spriteBatch.Draw(pixel, lineCenter, pixelSource, color, rotation, Vector2.One * 0.5f, lineScale, SpriteEffects.None, 0f);

			spriteBatch.Draw(pixel, iconPosition, pixelSource, color, 0f, Vector2.One * 0.5f, new Vector2(DotSize, DotSize), SpriteEffects.None, 0f);
		}

		private static void DrawDebugOverlay(SpriteBatch spriteBatch)
		{
			StringBuilder sb = new();
			sb.Append($"SoundCompass UI: emitters={SoundTrackerSystem.ActiveEmitters.Count}, layout={SoundCompassStateSystem.CompassLayout}");

			int lines = Math.Min(MaxDebugLines, SoundTrackerSystem.ActiveEmitters.Count);
			for (int i = 0; i < lines; i++)
			{
				SoundEmitter emitter = SoundTrackerSystem.ActiveEmitters[i];
				sb.Append('\n');
				sb.Append(FormatEmitterDebugLine(emitter));
			}

			if (SoundTrackerSystem.ActiveEmitters.Count > lines)
			{
				sb.Append($"\n... +{SoundTrackerSystem.ActiveEmitters.Count - lines} more");
			}

			string text = sb.ToString();
			ChatManager.DrawColorCodedStringWithShadow(
				spriteBatch,
				FontAssets.MouseText.Value,
				text,
				new Vector2(24f, 24f),
				new Color(120, 240, 255),
				0f,
				Vector2.Zero,
				Vector2.One);
		}

		private static string FormatEmitterDebugLine(SoundEmitter emitter)
		{
			float tiles = emitter.Distance / 16f;

			return emitter.Kind switch
			{
				SoundEmitterKind.NPC => $"[n:{emitter.Type}] {Lang.GetNPCNameValue(emitter.Type)} - {tiles:0.0} tiles",
				SoundEmitterKind.Projectile => $"[c/78DCFF:Proj] {Lang.GetProjectileName(emitter.Type).Value} - {tiles:0.0} tiles",
				SoundEmitterKind.Item => $"[i:{emitter.Type}] {Lang.GetItemNameValue(emitter.Type)} - {tiles:0.0} tiles",
				_ => $"Unknown type={emitter.Type} - {tiles:0.0} tiles"
			};
		}
	}
}
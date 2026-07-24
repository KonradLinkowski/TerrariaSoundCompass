using SoundCompass.Common.Configs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace SoundCompass.Common.Systems
{
	public class SoundCompassStateSystem : ModSystem
	{
		private static bool _overlayToggledOn = true;
		private static CompassLayoutType? _layoutOverride;

		public static bool IsOverlayActive
		{
			get
			{
				SoundCompassClientConfig config = ModContent.GetInstance<SoundCompassClientConfig>();
				return config.OverlayEnabled && _overlayToggledOn;
			}
		}

		public static CompassLayoutType CompassLayout
		{
			get
			{
				SoundCompassClientConfig config = ModContent.GetInstance<SoundCompassClientConfig>();
				return _layoutOverride ?? config.CompassLayout;
			}
		}

		public static bool OutsideFocusAreaOnly => ModContent.GetInstance<SoundCompassClientConfig>().OutsideFocusAreaOnly;

		public static float CircleRadius
		{
			get
			{
				SoundCompassClientConfig config = ModContent.GetInstance<SoundCompassClientConfig>();
				return MathHelper.Clamp(config.CircleRadius, 40f, 260f);
			}
		}

    public static float BorderInset
		{
			get
			{
				SoundCompassClientConfig config = ModContent.GetInstance<SoundCompassClientConfig>();
				return MathHelper.Clamp(config.BorderInset, 4f, 256f);
			}
		}

		public override void OnWorldLoad()
		{
			_overlayToggledOn = true;
			_layoutOverride = null;
		}

		public override void PreUpdatePlayers()
		{
			if (Main.gameMenu)
			{
				return;
			}

			if (SoundCompass.ToggleOverlayKeybind?.JustPressed == true)
			{
				_overlayToggledOn = !_overlayToggledOn;
				string stateText = IsOverlayActive ? "enabled" : "disabled";
				Main.NewText($"[SoundCompass] Overlay {stateText}", new Microsoft.Xna.Framework.Color(120, 220, 255));
			}

			if (SoundCompass.SwitchCompassTypeKeybind?.JustPressed == true)
			{
				CompassLayoutType newLayout = CompassLayout == CompassLayoutType.CenterRing
					? CompassLayoutType.ScreenBorder
					: CompassLayoutType.CenterRing;

				_layoutOverride = newLayout;
				string layoutText = newLayout == CompassLayoutType.CenterRing ? "Circle" : "Screen Border";
				Main.NewText($"[SoundCompass] Compass type: {layoutText}", new Microsoft.Xna.Framework.Color(120, 220, 255));
			}
		}
	}
}

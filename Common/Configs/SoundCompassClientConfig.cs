using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace SoundCompass.Common.Configs
{
	public enum CompassLayoutType
	{
		CenterRing = 0,
		ScreenBorder = 1
	}

	public class SoundCompassClientConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		[DefaultValue(true)]
		public bool OverlayEnabled { get; set; } = true;

		[DefaultValue(CompassLayoutType.CenterRing)]
		public CompassLayoutType CompassLayout { get; set; } = CompassLayoutType.CenterRing;

		[DefaultValue(true)]
		public bool OutsideFocusAreaOnly { get; set; } = true;

		[Range(40f, 260f)]
		[Increment(2f)]
		[DefaultValue(84f)]
		public float CircleRadius { get; set; } = 84f;

    [Range(0f, 256f)]
		[Increment(2f)]
		[DefaultValue(4f)]
		public float BorderInset { get; set; } = 4f;
	}
}

using Terraria.ModLoader;

namespace SoundCompass
{
	public class SoundCompass : Mod
	{
		public static ModKeybind ToggleOverlayKeybind { get; private set; }
		public static ModKeybind SwitchCompassTypeKeybind { get; private set; }

		public override void Load()
		{
			ToggleOverlayKeybind = KeybindLoader.RegisterKeybind(this, "Toggle Overlay", "P");
			SwitchCompassTypeKeybind = KeybindLoader.RegisterKeybind(this, "Switch Compass Type", "O");
		}

		public override void Unload()
		{
			SwitchCompassTypeKeybind = null;
			ToggleOverlayKeybind = null;
		}
	}
}

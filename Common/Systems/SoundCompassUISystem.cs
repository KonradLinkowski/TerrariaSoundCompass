using System.Collections.Generic;
using SoundCompass.UI;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace SoundCompass.Common.Systems
{
	public class SoundCompassUISystem : ModSystem
	{
		private SoundCompassUI _compassUI;
		private bool _announcedUiReady;

		public override void Load()
		{
			_compassUI = new SoundCompassUI();
		}

		public override void Unload()
		{
			_compassUI = null;
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			int mouseTextLayer = layers.FindIndex(static layer => layer.Name.Equals("Vanilla: Mouse Text"));
			if (mouseTextLayer < 0)
			{
				mouseTextLayer = layers.FindIndex(static layer => layer.Name.Contains("Mouse", System.StringComparison.OrdinalIgnoreCase));
			}

			if (mouseTextLayer < 0)
			{
				mouseTextLayer = layers.Count;
			}

			layers.Insert(mouseTextLayer, new LegacyGameInterfaceLayer(
				"SoundCompass: Compass",
				delegate
				{
					if (Main.gameMenu || _compassUI == null || !SoundCompassStateSystem.IsOverlayActive)
					{
						return true;
					}

					if (!_announcedUiReady && Main.netMode != Terraria.ID.NetmodeID.Server)
					{
						Main.NewText("[SoundCompass] UI layer active", new Microsoft.Xna.Framework.Color(120, 220, 255));
						_announcedUiReady = true;
					}

					_compassUI.Draw(Main.spriteBatch);
					return true;
				},
				InterfaceScaleType.UI));
		}
	}
}
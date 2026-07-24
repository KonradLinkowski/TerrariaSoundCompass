using Microsoft.Xna.Framework;

namespace SoundCompass.Common.Models
{
	public enum SoundEmitterKind
	{
		NPC,
		Projectile,
		Item
	}

	public readonly struct SoundEmitter
	{
		public SoundEmitter(SoundEmitterKind kind, int type, int identity, Vector2 worldPosition, float distance)
		{
			Kind = kind;
			Type = type;
			Identity = identity;
			WorldPosition = worldPosition;
			Distance = distance;
		}

		public SoundEmitterKind Kind { get; }
		public int Type { get; }
		public int Identity { get; }
		public Vector2 WorldPosition { get; }
		public float Distance { get; }
	}
}
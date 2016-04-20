using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace GeometryWars
{
    static class TextureLoader
	{
		public static Texture2D Player { get; private set; }
		public static Texture2D Seeker { get; private set; }
		public static Texture2D Wanderer { get; private set; }
		public static Texture2D Bullet { get; private set; }
		public static Texture2D Pointer { get; private set; }
		public static Texture2D BlackHole { get; private set; }

		public static Texture2D LineParticle { get; private set; }
		public static Texture2D Glow { get; private set; }
		public static Texture2D Pixel { get; private set; }		// a single white pixel

		public static SpriteFont Font { get; private set; }

		public static void Load(ContentManager content)
		{
			Player = content.Load<Texture2D>("Textures/Player");
			Seeker = content.Load<Texture2D>("Textures/Seeker");
			Wanderer = content.Load<Texture2D>("Textures/Wanderer");
			Bullet = content.Load<Texture2D>("Textures/Bullet");
			Pointer = content.Load<Texture2D>("Textures/Pointer");
			BlackHole = content.Load<Texture2D>("Textures/Black Hole");

			LineParticle = content.Load<Texture2D>("Textures/Laser");
			Glow = content.Load<Texture2D>("Textures/Glow");

			Pixel = new Texture2D(Player.GraphicsDevice, 1, 1);
			Pixel.SetData(new[] { Color.White });

			Font = content.Load<SpriteFont>("Font");
		}
	}
}


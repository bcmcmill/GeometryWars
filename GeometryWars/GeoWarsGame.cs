using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using GeometryWars.Effects.Particle;
using GeometryWars.Entities.World.Grid;
using GeometryWars.Effects.Bloom;
using GeometryWars.Entities;
using GeometryWars.Entities.Player;
using GeometryWars.Effects;
using GeometryWars.Utilities;
using GeometryWars.Entities.Enemies;

namespace GeometryWars
{
    public class GeoWarsGame : Game
    {
		public static GeoWarsGame Instance { get; private set; }
		public static Viewport Viewport { get { return Instance.GraphicsDevice.Viewport; } }
		public static Vector2 ScreenSize { get { return new Vector2(Viewport.Width, Viewport.Height); } }
		public static GameTime GameTime { get; private set; }
		public static ParticleManager ParticleManager { get; private set; }
		public static Grid Grid { get; private set; }

		public GraphicsDeviceManager Graphics;
		SpriteBatch _spriteBatch;
		BloomComponent _bloom;

		bool _paused = false;
		bool _useBloom = true;

		public GeoWarsGame()
		{
			Instance = this;
			Graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";

			Graphics.PreferredBackBufferWidth = 1024;
			Graphics.PreferredBackBufferHeight = 768;

			_bloom = new BloomComponent(this);
			Components.Add(_bloom);
			_bloom.Settings = new BloomSettings(null, 0.25f, 4, 2, 1, 1.5f, 1);
		}

		protected override void Initialize()
		{
			base.Initialize();

			ParticleManager = new ParticleManager(1024 * 20, ParticleState.UpdateParticle);

			const int maxGridPoints = 1600;
			var gridSpacing = new Vector2((float)Math.Sqrt(Viewport.Width * Viewport.Height / maxGridPoints));
			Grid = new Grid(Viewport.Bounds, gridSpacing);

			EntityManager.Add(PlayerShip.Instance);

			MediaPlayer.IsRepeating = true;
			MediaPlayer.Play(Sound.Music);
		}

		protected override void LoadContent()
		{
			_spriteBatch = new SpriteBatch(GraphicsDevice);
			TextureLoader.Load(Content);
			Sound.Load(Content);
		}

		protected override void Update(GameTime gameTime)
		{
			GameTime = gameTime;
			Input.Update();

			// Allows the game to exit
			if (Input.WasButtonPressed(Buttons.Back) || Input.WasKeyPressed(Keys.Escape))
                Exit();

			if (Input.WasKeyPressed(Keys.P))
				_paused = !_paused;
			if (Input.WasKeyPressed(Keys.B))
				_useBloom = !_useBloom;

			if (!_paused)
			{
				PlayerStatus.Update();
				EntityManager.Update();
				EnemySpawner.Update();
				ParticleManager.Update();
				Grid.Update();
			}

			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			_bloom.BeginDraw();
			if (!_useBloom)
				base.Draw(gameTime);

			GraphicsDevice.Clear(Color.TransparentBlack);

			_spriteBatch.Begin(SpriteSortMode.Texture, BlendState.Additive);
			EntityManager.Draw(_spriteBatch);
			_spriteBatch.End();

			_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
			Grid.Draw(_spriteBatch);
			ParticleManager.Draw(_spriteBatch);
			_spriteBatch.End();

			if (_useBloom)
				base.Draw(gameTime);

			// Draw the user interface without bloom
			_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);

			_spriteBatch.DrawString(TextureLoader.Font, "Lives: " + PlayerStatus.Lives, new Vector2(5), Color.White);
			DrawRightAlignedString("Score: " + PlayerStatus.Score, 5);
			DrawRightAlignedString("Multiplier: " + PlayerStatus.Multiplier, 35);
			// draw the custom mouse cursor
			_spriteBatch.Draw(TextureLoader.Pointer, Input.MousePosition, Color.White);

			if (PlayerStatus.IsGameOver)
			{
				var text = "Game Over\n" +
					"Your Score: " + PlayerStatus.Score + "\n" +
					"High Score: " + PlayerStatus.HighScore;

				var textSize = TextureLoader.Font.MeasureString(text);
				_spriteBatch.DrawString(TextureLoader.Font, text, ScreenSize / 2 - textSize / 2, Color.White);
			}

			_spriteBatch.End();
		}

		private void DrawRightAlignedString(string text, float y)
		{
			var textWidth = TextureLoader.Font.MeasureString(text).X;
			_spriteBatch.DrawString(TextureLoader.Font, text, new Vector2(ScreenSize.X - textWidth - 5, y), Color.White);
		}
	}
}


using System;
using GeometryWars.Effects;
using GeometryWars.Effects.Bloom;
using GeometryWars.Effects.Particle;
using GeometryWars.Entities;
using GeometryWars.Entities.Enemies;
using GeometryWars.Entities.Player;
using GeometryWars.Entities.World.Grid;
using GeometryWars.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace GeometryWars
{
    public class GeoWarsGame : Game
    {
        private readonly BloomComponent _bloom;

        private bool _paused;
        private SpriteBatch _spriteBatch;
        private bool _useBloom = true;
        private readonly FrameCounter _frameCounter = new FrameCounter();

        public GraphicsDeviceManager Graphics;

        public GeoWarsGame()
        {
            Instance = this;
            Graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            Graphics.PreferredBackBufferWidth = 1920;
            Graphics.PreferredBackBufferHeight = 1080;
            Graphics.IsFullScreen = true;
            _bloom = new BloomComponent(this);
            Components.Add(_bloom);
            _bloom.Settings = new BloomSettings(null, 0.25f, 4, 3, 1, 1.8f, 1.5f);
        }

        public static GeoWarsGame Instance { get; private set; }
        public static Viewport Viewport => Instance.GraphicsDevice.Viewport;
        public static Vector2 ScreenSize => new Vector2(Viewport.Width, Viewport.Height);
        public static GameTime GameTime { get; private set; }
        public static ParticleManager ParticleManager { get; private set; }
        public static Grid Grid { get; private set; }

        protected override void Initialize()
        {
            base.Initialize();

            ParticleManager = new ParticleManager(1024*25, ParticleState.UpdateParticle);

            const int maxGridPoints = 2200;
            var gridSpacing = new Vector2((float) Math.Sqrt(Viewport.Width * Viewport.Height / maxGridPoints));
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

            // Draw the user interface without bloom effect
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);

            _spriteBatch.DrawString(TextureLoader.Font, "Lives: " + PlayerStatus.Lives, new Vector2(5), Color.White);
            DrawRightAlignedString("Score: " + PlayerStatus.Score, 5);
            DrawRightAlignedString("Multiplier: " + PlayerStatus.Multiplier, 35);

            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            _frameCounter.Update(deltaTime);

            var fps = $"FPS: {_frameCounter.AverageFramesPerSecond}";
            var textSize = TextureLoader.Font.MeasureString(fps);
            _spriteBatch.DrawString(TextureLoader.Font, fps, new Vector2(ScreenSize.X / 2 - textSize.X / 2, 5), Color.White);

            if (PlayerStatus.IsGameOver)
            {
                var text = "   Game Over\n" +
                           "Your Score: " + PlayerStatus.Score + "\n" +
                           "High Score: " + PlayerStatus.HighScore;

                 textSize = TextureLoader.Font.MeasureString(text);
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
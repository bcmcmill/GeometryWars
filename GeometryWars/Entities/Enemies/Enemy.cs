using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GeometryWars.Effects;
using GeometryWars.Effects.Particle;
using GeometryWars.Entities.Player;
using GeometryWars.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GeometryWars.Entities.Enemies
{
    internal class Enemy : Entity
    {
        public static Random Rand = new Random();

        private readonly List<IEnumerator<int>> _behaviors = new List<IEnumerator<int>>();
        private int _timeUntilStart = 60;

        public Enemy(Texture2D image, Vector2 position)
        {
            Image = image;
            Position = position;
            Radius = image.Width/2f;
            Color = Color.Transparent;
            PointValue = 1;
        }

        public bool IsActive => _timeUntilStart <= 0;
        public int PointValue { get; private set; }

        public static Enemy CreateSeeker(Vector2 position)
        {
            var enemy = new Enemy(TextureLoader.Seeker, position);
            enemy.AddBehavior(enemy.SeekPlayer(0.9f));
            enemy.PointValue = 2;

            return enemy;
        }

        public static Enemy CreateWanderer(Vector2 position)
        {
            var enemy = new Enemy(TextureLoader.Wanderer, position);
            enemy.AddBehavior(enemy.WanderRandomly());

            return enemy;
        }

        public override void Update()
        {
            if (_timeUntilStart <= 0)
                ApplyBehaviors();
            else
            {
                _timeUntilStart--;
                Color = Color.White*(1 - _timeUntilStart/60f);
            }

            Position += Velocity;
            Position = Vector2.Clamp(Position, Size/2, GeoWarsGame.ScreenSize - Size/2);

            Velocity *= 0.8f;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_timeUntilStart > 0)
            {
                // Draw an expanding, fading-out version of the sprite as part of the spawn-in effect.
                var factor = _timeUntilStart/60f; // decreases from 1 to 0 as the enemy spawns in
                spriteBatch.Draw(Image, Position, null, Color.White*factor, Orientation, Size/2f, 2 - factor, 0, 0);
            }

            base.Draw(spriteBatch);
        }

        private void AddBehavior(IEnumerable<int> behavior)
        {
            _behaviors.Add(behavior.GetEnumerator());
        }

        private void ApplyBehaviors()
        {
            Parallel.For(0, _behaviors.Count,
                i =>
                {
                    if (!_behaviors[i].MoveNext())
                        _behaviors.RemoveAt(i);
                });
        }

        public void HandleCollision(Enemy other)
        {
            var d = Position - other.Position;
            Velocity += 10*d/(d.LengthSquared() + 1);
        }

        public void WasShot()
        {
            IsExpired = true;
            PlayerStatus.AddPoints(PointValue);
            PlayerStatus.IncreaseMultiplier();

            var hue1 = Rand.NextFloat(0, 6);
            var hue2 = (hue1 + Rand.NextFloat(0, 2))%6f;
            var color1 = ColorUtil.HsvToColor(hue1, 0.5f, 1);
            var color2 = ColorUtil.HsvToColor(hue2, 0.5f, 1);

            Parallel.For(0, 120,
                i =>
                {
                    var speed = 18f*(1f - 1/Rand.NextFloat(1, 10));
                    var state = new ParticleState
                    {
                        Velocity = Rand.NextVector2(speed, speed),
                        Type = ParticleType.Enemy,
                        LengthMultiplier = 1
                    };

                    var color = Color.Lerp(color1, color2, Rand.NextFloat(0, 1));
                    GeoWarsGame.ParticleManager.CreateParticle(TextureLoader.LineParticle, Position, color, 190, 1.5f,
                        state);
                }); // End Parallel For

            Sound.Explosion.Play(0.5f, Rand.NextFloat(-0.2f, 0.2f), 0);
        }

        private IEnumerable<int> SeekPlayer(float acceleration)
        {
            while (true)
            {
                if (!PlayerShip.Instance.IsDead)
                    Velocity += (PlayerShip.Instance.Position - Position).ScaleTo(acceleration);

                if (Velocity != Vector2.Zero)
                    Orientation = Velocity.ToAngle();

                yield return 0;
            }
        }

        private IEnumerable<int> WanderRandomly()
        {
            var direction = Rand.NextFloat(0, MathHelper.TwoPi);

            while (true)
            {
                direction += Rand.NextFloat(-0.1f, 0.1f);
                direction = MathHelper.WrapAngle(direction);

                for (var i = 0; i < 6; i++)
                {
                    Velocity += MathUtil.FromPolar(direction, 0.4f);
                    Orientation -= 0.05f;

                    var bounds = GeoWarsGame.Viewport.Bounds;
                    bounds.Inflate(-Image.Width/2 - 1, -Image.Height/2 - 1);

                    // if the enemy is outside the bounds, make it move away from the edge
                    if (!bounds.Contains(Position.ToPoint()))
                        direction = (GeoWarsGame.ScreenSize/2 - Position).ToAngle() +
                                    Rand.NextFloat(-MathHelper.PiOver2, MathHelper.PiOver2);

                    yield return 0;
                }
            }
        }
    }
}
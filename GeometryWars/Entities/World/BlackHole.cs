using System;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GeometryWars.Effects.Particle;
using GeometryWars.Utilities;
using GeometryWars.Entities.Enemies;

namespace GeometryWars.Entities.World
{
    class BlackHole : Entity
	{
		private static Random _rand = new Random();

		private int _hitPoints = 10;
		private float _sprayAngle = 0;

		public BlackHole(Vector2 position)
		{
			Image = TextureLoader.BlackHole;
			Position = position;
			Radius = Image.Width / 2f;
		}

		public override void Update()
		{
			var entities = EntityManager.GetNearbyEntities(Position, 250);

            // Parallel For to add effects of BlackHole to Bullets and Particles -- Example of Task Parallelism
            Parallel.For(0, entities.Count, 
                i => {
                        if (entities[i] is Enemy && !(entities[i] as Enemy).IsActive)
                           return;

                        // bullets are repelled by black holes and everything else is attracted
                        if (entities[i] is Player.Bullet)
                           entities[i].Velocity += (entities[i].Position - Position).ScaleTo(0.3f);
                        else
                        {
                           var dPos = Position - entities[i].Position;
                           var length = dPos.Length();

                           entities[i].Velocity += dPos.ScaleTo(MathHelper.Lerp(2, 0, length / 250f));
                        }
                     }); // end parallel for

            // The black holes spray some orbiting particles. The spray toggles on and off every quarter second.
            if ((GeoWarsGame.GameTime.TotalGameTime.Milliseconds / 250) % 2 == 0)
			{
				var sprayVel = MathUtil.FromPolar(_sprayAngle, _rand.NextFloat(12, 15));
				var color = ColorUtil.HsvToColor(5, 0.5f, 0.8f);	// light purple
				var pos = Position + 2f * new Vector2(sprayVel.Y, -sprayVel.X) + _rand.NextVector2(4, 8);
				var state = new ParticleState() 
				{ 
					Velocity = sprayVel, 
					LengthMultiplier = 1, 
					Type = ParticleType.Enemy 
				};

				GeoWarsGame.ParticleManager.CreateParticle(TextureLoader.LineParticle, pos, color, 190, 1.5f, state);
			}

			// rotate the spray direction
			_sprayAngle -= MathHelper.TwoPi / 50f;

			GeoWarsGame.Grid.ApplyImplosiveForce((float)Math.Sin(_sprayAngle / 2) * 10 + 20, Position, 200);
		}

		public void WasShot()
		{
            // Apply damage
			_hitPoints--;
            
            // Has it been destroyed?
			if (_hitPoints <= 0)
			{
				IsExpired = true;
                Player.PlayerStatus.AddPoints(5);
                Player.PlayerStatus.IncreaseMultiplier();
			}


			var hue = (float)((3 * GeoWarsGame.GameTime.TotalGameTime.TotalSeconds) % 6);
			var color = ColorUtil.HsvToColor(hue, 0.25f, 1);
			const int numParticles = 150;
			var startOffset = _rand.NextFloat(0, MathHelper.TwoPi / numParticles);

            // Parallel For to add effects Particles when they come in contact with the BlackHole -- Example of Task Parallelism
            Parallel.For(0, numParticles,
                index => {
                            var sprayVel = MathUtil.FromPolar(MathHelper.TwoPi * index / numParticles + startOffset, _rand.NextFloat(8, 16));
                            var pos = Position + 2f * sprayVel;
                            var state = new ParticleState()
                            {
                                Velocity = sprayVel,
                                LengthMultiplier = 1,
                                Type = ParticleType.IgnoreGravity
                            };

                            GeoWarsGame.ParticleManager.CreateParticle(TextureLoader.LineParticle, pos, color, 90, 1.5f, state);
                         }); // End Parallel For

            Effects.Sound.Explosion.Play(0.5f, _rand.NextFloat(-0.2f, 0.2f), 0);
		}

		public void Kill()
		{
			_hitPoints = 0;
			WasShot();
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			// Pulsate the size of the BlackHole
			var scale = 1 + 0.1f * (float)Math.Sin(10 * GeoWarsGame.GameTime.TotalGameTime.TotalSeconds);
			spriteBatch.Draw(Image, Position, null, Color, Orientation, Size / 2f, scale, 0, 0);
		}
	}
}
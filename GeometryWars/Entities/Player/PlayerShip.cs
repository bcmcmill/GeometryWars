using System;
using System.Threading.Tasks;
using GeometryWars.Effects;
using GeometryWars.Effects.Particle;
using GeometryWars.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GeometryWars.Entities.Player
{
    internal class PlayerShip : Entity
	{
		private static PlayerShip _instance;
		public static PlayerShip Instance => _instance ?? (_instance = new PlayerShip());

        private const int CooldownFrames = 6;
        private int _cooldowmRemaining;

        private int _framesUntilRespawn;
		public bool IsDead => _framesUntilRespawn > 0;

        private static readonly Random Rand = new Random();

		private PlayerShip()
		{
			Image = TextureLoader.Player;
			Position = GeoWarsGame.ScreenSize / 2;
			Radius = 10;
		}

		public override void Update()
		{
			if (IsDead)
			{
			    if (--_framesUntilRespawn != 0) return;

                if (PlayerStatus.Lives == 0)
			    {
			        PlayerStatus.Reset();
			        Position = GeoWarsGame.ScreenSize / 2;
			    }

                GeoWarsGame.Grid.ApplyDirectedForce(new Vector3(0, 0, 5000), new Vector3(Position, 0), 50);
			    return;
			}

			var aim = Input.GetAimDirection();
			if (aim.LengthSquared() > 0 && _cooldowmRemaining <= 0)
			{
				_cooldowmRemaining = CooldownFrames;
				var aimAngle = aim.ToAngle();
				var aimQuat = Quaternion.CreateFromYawPitchRoll(0, 0, aimAngle);

				var randomSpread = Rand.NextFloat(-0.04f, 0.04f) + Rand.NextFloat(-0.04f, 0.04f);
				var vel = MathUtil.FromPolar(aimAngle + randomSpread, 11f);

				var offset = Vector2.Transform(new Vector2(35, -8), aimQuat);
				EntityManager.Add(new Bullet(Position + offset, vel));

				offset = Vector2.Transform(new Vector2(35, 8), aimQuat);
				EntityManager.Add(new Bullet(Position + offset, vel));

				Sound.Shot.Play(0.2f, Rand.NextFloat(-0.2f, 0.2f), 0);
			}

			if (_cooldowmRemaining > 0)
				_cooldowmRemaining--;

			const float speed = 8;
			Velocity += speed * Input.GetMovementDirection();
			Position += Velocity;
			Position = Vector2.Clamp(Position, Size / 2, GeoWarsGame.ScreenSize - Size / 2);

			if (Velocity.LengthSquared() > 0)
				Orientation = Velocity.ToAngle();

			MakeExhaustFire();
			Velocity = Vector2.Zero;
		}

		private void MakeExhaustFire()
		{
		    if (!(Velocity.LengthSquared() > 0.1f)) return;

		    // set up some variables
		    Orientation = Velocity.ToAngle();
		    var rot = Quaternion.CreateFromYawPitchRoll(0f, 0f, Orientation);

		    var t = GeoWarsGame.GameTime.TotalGameTime.TotalSeconds;
		    // The primary velocity of the particles is 3 pixels/frame in the direction opposite to which the ship is travelling.
		    var baseVel = Velocity.ScaleTo(-3); 
		    // Calculate the sideways velocity for the two side streams. The direction is perpendicular to the ship's velocity and the
		    // magnitude varies sinusoidally.
		    var perpVel = new Vector2(baseVel.Y, -baseVel.X) * (0.6f * (float)Math.Sin(t * 10));
		    var sideColor = new Color(200, 38, 9);	// deep red
		    var midColor = new Color(255, 187, 30);	// orange-yellow
		    var pos = Position + Vector2.Transform(new Vector2(-25, 0), rot);	// position of the ship's exhaust pipe.
		    const float alpha = 0.7f;

		    // middle particle stream
		    var velMid = baseVel + Rand.NextVector2(0, 1);
		    GeoWarsGame.ParticleManager.CreateParticle(TextureLoader.LineParticle, pos, Color.White * alpha, 60f, new Vector2(0.5f, 1),
		        new ParticleState(velMid, ParticleType.Enemy));
		    GeoWarsGame.ParticleManager.CreateParticle(TextureLoader.Glow, pos, midColor * alpha, 60f, new Vector2(0.5f, 1),
		        new ParticleState(velMid, ParticleType.Enemy));

		    // side particle streams
		    var vel1 = baseVel + perpVel + Rand.NextVector2(0, 0.3f);
		    var vel2 = baseVel - perpVel + Rand.NextVector2(0, 0.3f);
		    GeoWarsGame.ParticleManager.CreateParticle(TextureLoader.LineParticle, pos, Color.White * alpha, 60f, new Vector2(0.5f, 1),
		        new ParticleState(vel1, ParticleType.Enemy));
		    GeoWarsGame.ParticleManager.CreateParticle(TextureLoader.LineParticle, pos, Color.White * alpha, 60f, new Vector2(0.5f, 1),
		        new ParticleState(vel2, ParticleType.Enemy));

		    GeoWarsGame.ParticleManager.CreateParticle(TextureLoader.Glow, pos, sideColor * alpha, 60f, new Vector2(0.5f, 1),
		        new ParticleState(vel1, ParticleType.Enemy));
		    GeoWarsGame.ParticleManager.CreateParticle(TextureLoader.Glow, pos, sideColor * alpha, 60f, new Vector2(0.5f, 1),
		        new ParticleState(vel2, ParticleType.Enemy));
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (!IsDead)
				base.Draw(spriteBatch);
		}

		public void Kill()
		{
			PlayerStatus.RemoveLife();
			_framesUntilRespawn = PlayerStatus.IsGameOver ? 300 : 120;

			var explosionColor = new Color(0.8f, 0.8f, 0.4f); // yellow

            Parallel.For(0, 1200, 
                i => {
                        var speed = 18f * (1f - 1 / Rand.NextFloat(1f, 10f));
                        var color = Color.Lerp(Color.White, explosionColor, Rand.NextFloat(0, 1));
                        var state = new ParticleState
                        {
                            Velocity = Rand.NextVector2(speed, speed),
                            Type = ParticleType.None,
                            LengthMultiplier = 1
                        };

                        GeoWarsGame.ParticleManager.CreateParticle(TextureLoader.LineParticle, Position, color, 190, 1.5f, state);
                     });
		}
	}
}
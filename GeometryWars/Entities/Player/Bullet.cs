﻿using System;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace GeometryWars
{
    class Bullet : Entity
	{
		private static Random rand = new Random();

		public Bullet(Vector2 position, Vector2 velocity)
		{
			image = TextureLoader.Bullet;
			Position = position;
			Velocity = velocity;
			Orientation = Velocity.ToAngle();
			Radius = 8;
		}

		public override void Update()
		{
			if (Velocity.LengthSquared() > 0)
				Orientation = Velocity.ToAngle();

			Position += Velocity;
			GeoWarsGame.Grid.ApplyExplosiveForce(0.5f * Velocity.Length(), Position, 80);

			// delete bullets that go off-screen
			if (!GeoWarsGame.Viewport.Bounds.Contains(Position.ToPoint()))
			{
				IsExpired = true;

                Parallel.For(0, 30, 
                    i => {
                            GeoWarsGame.ParticleManager.CreateParticle(TextureLoader.LineParticle, Position, Color.LightBlue, 50, 1,
                            new ParticleState() { Velocity = rand.NextVector2(0, 9), Type = ParticleType.Bullet, LengthMultiplier = 1 });
                         });
			}
		}
	}
}
using System;
using GeometryWars.Entities;
using GeometryWars.Utilities;
using Microsoft.Xna.Framework;

namespace GeometryWars.Effects.Particle
{
    public enum ParticleType
    {
        None,
        Enemy,
        Bullet,
        IgnoreGravity
    }

    public struct ParticleState
    {
        public Vector2 Velocity;
        public ParticleType Type;
        public float LengthMultiplier;

        private static readonly Random Rand = new Random();

        public ParticleState(Vector2 velocity, ParticleType type, float lengthMultiplier = 1f)
        {
            Velocity = velocity;
            Type = type;
            LengthMultiplier = lengthMultiplier;
        }

        public static ParticleState GetRandom(float minVel, float maxVel)
        {
            var state = new ParticleState
            {
                Velocity = Rand.NextVector2(minVel, maxVel),
                Type = ParticleType.None,
                LengthMultiplier = 1
            };

            return state;
        }

        public static void UpdateParticle(Particle particle)
        {
            var vel = particle.State.Velocity;
            var speed = vel.Length();

            // using Vector2.Add() should be slightly faster than doing "x.Position += vel;" because the Vector2s
            // are passed by reference and don't need to be copied. Since we may have to update a very large 
            // number of particles, this method is a good candidate for optimizations.
            Vector2.Add(ref particle.Position, ref vel, out particle.Position);

            // fade the particle if its PercentLife or speed is low.
            var alpha = Math.Min(1, Math.Min(particle.PercentLife*2, speed*1f));
            alpha *= alpha;

            particle.Tint.A = (byte) (255*alpha);

            // the length of bullet particles will be less dependent on their speed than other particles
            if (particle.State.Type == ParticleType.Bullet)
                particle.Scale.X = particle.State.LengthMultiplier*Math.Min(Math.Min(1f, 0.1f*speed + 0.1f), alpha);
            else
                particle.Scale.X = particle.State.LengthMultiplier*Math.Min(Math.Min(1f, 0.2f*speed + 0.1f), alpha);

            particle.Orientation = vel.ToAngle();

            var pos = particle.Position;
            var width = (int) GeoWarsGame.ScreenSize.X;
            var height = (int) GeoWarsGame.ScreenSize.Y;

            // collide with the edges of the screen
            if (pos.X < 0)
                vel.X = Math.Abs(vel.X);
            else if (pos.X > width)
                vel.X = -Math.Abs(vel.X);
            if (pos.Y < 0)
                vel.Y = Math.Abs(vel.Y);
            else if (pos.Y > height)
                vel.Y = -Math.Abs(vel.Y);

            if (particle.State.Type != ParticleType.IgnoreGravity)
            {
                for (var i = 0; i < EntityManager.BlackHoleCount; i++)
                {
                    var dPos = EntityManager.BlackHoles[i].Position - pos;
                    var distance = dPos.Length();
                    var n = dPos/distance;
                    vel += 10000*n/(distance*distance + 10000);

                    // add tangential acceleration for nearby particles
                    if (distance < 400)
                        vel += 45*new Vector2(n.Y, -n.X)/(distance + 100);
                }
            }

            if (Math.Abs(vel.X) + Math.Abs(vel.Y) < 0.00000000001f) // normalize floats
                vel = Vector2.Zero;
            else if (particle.State.Type == ParticleType.Enemy)
                vel *= 0.94f;
            else
                vel *= 0.96f + Math.Abs(pos.X)%0.04f;
                    // rand.Next() isn't thread-safe, so use the position for pseudo-randomness

            particle.State.Velocity = vel;
        }
    }
}
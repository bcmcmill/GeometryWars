using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GeometryWars.Effects.Particle
{
    public class Particle
    {
        public float Duration;
        public float Orientation;
        public float PercentLife = 1.0f;
        public Vector2 Position;

        public Vector2 Scale = Vector2.One;
        public ParticleState State;
        public Texture2D Texture;

        public Color Tint;
    }
}
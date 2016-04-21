using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GeometryWars.Effects.Particle
{
    public class Particle
    {
        public Texture2D Texture;
        public Vector2 Position;
        public float Orientation;

        public Vector2 Scale = Vector2.One;

        public Color Tint;
        public float Duration;
        public float PercentLife = 1.0f;
        public ParticleState State;
    }
}

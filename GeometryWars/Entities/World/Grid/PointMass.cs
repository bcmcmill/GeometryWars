using Microsoft.Xna.Framework;

namespace GeometryWars.Entities.World.Grid
{
    internal class PointMass
    {
        private Vector3 _acceleration;
        private float _damping = 0.98f;
        public float InverseMass;
        public Vector3 Position;
        public Vector3 Velocity;

        public PointMass(Vector3 position, float invMass)
        {
            Position = position;
            InverseMass = invMass;
        }

        public void ApplyForce(Vector3 force)
        {
            _acceleration += force*InverseMass;
        }

        public void IncreaseDamping(float factor)
        {
            _damping *= factor;
        }

        public void Update()
        {
            Velocity += _acceleration;
            Position += Velocity;
            _acceleration = Vector3.Zero;
            if (Velocity.LengthSquared() < 0.001f*0.001f)
                Velocity = Vector3.Zero;

            Velocity *= _damping;
            _damping = 0.98f;
        }
    }
}
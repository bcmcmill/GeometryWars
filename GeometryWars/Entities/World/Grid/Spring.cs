using Microsoft.Xna.Framework;

namespace GeometryWars.Entities.World.Grid
{
    internal struct Spring
    {
        public PointMass StartPoint;
        public PointMass EndPoint;
        public float TargetLength;
        public float Stiffness;
        public float Damping;

        public Spring(PointMass startPoint, PointMass endPoint, float stiffness, float damping)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
            Stiffness = stiffness;
            Damping = damping;
            TargetLength = Vector3.Distance(startPoint.Position, endPoint.Position) * 0.95f;
        }

        public void Update()
        {
            var x = StartPoint.Position - EndPoint.Position;

            var length = x.Length();
            // these springs can only pull, not push
            if (length <= TargetLength)
                return;

            x = x / length * (length - TargetLength);
            var dv = EndPoint.Velocity - StartPoint.Velocity;
            var force = Stiffness * x - dv * Damping;

            StartPoint.ApplyForce(-force);
            EndPoint.ApplyForce(force);
        }
    }
}
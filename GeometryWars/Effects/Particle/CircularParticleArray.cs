namespace GeometryWars.Effects.Particle
{
    public class CircularParticleArray
    {
        private Particle[] list;
        private int start;

        public int Start
        {
            get { return start; }
            set { start = value % list.Length; }
        }

        public int Count { get; set; }
        public int Capacity { get { return list.Length; } }
        

        public CircularParticleArray() { } 

        public CircularParticleArray(int capacity)
        {
            list = new Particle[capacity];
        }

        public Particle this[int i]
        {
            get { return list[(start + i) % list.Length]; }
            set { list[(start + i) % list.Length] = value; }
        }
    }
}

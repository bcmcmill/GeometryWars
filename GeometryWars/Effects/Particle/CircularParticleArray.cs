namespace GeometryWars
{
    // Represents a circular array with an arbitrary starting point. It's useful for efficiently overwriting
    // the oldest particles when the array gets full. Simply overwrite particleList[0] and advance Start.
    public class CircularParticleArray
    {
        private int start;
        public int Start
        {
            get { return start; }
            set { start = value % list.Length; }
        }

        public int Count { get; set; }
        public int Capacity { get { return list.Length; } }
        private Particle[] list;

        public CircularParticleArray() { }  // for serialization

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

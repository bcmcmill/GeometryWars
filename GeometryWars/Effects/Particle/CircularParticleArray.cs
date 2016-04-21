namespace GeometryWars.Effects.Particle
{
    public class CircularParticleArray
    {
        private readonly Particle[] _list;
        private int _start;


        public CircularParticleArray()
        {
        }

        public CircularParticleArray(int capacity)
        {
            _list = new Particle[capacity];
        }

        public int Start
        {
            get { return _start; }
            set { _start = value%_list.Length; }
        }

        public int Count { get; set; }
        public int Capacity => _list.Length;

        public Particle this[int i]
        {
            get { return _list[(_start + i)%_list.Length]; }
            set { _list[(_start + i)%_list.Length] = value; }
        }
    }
}
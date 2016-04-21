using System;
using System.Linq;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace GeometryWars.Effects
{
    internal static class Sound
    {
        private static readonly Random Rand = new Random();

        private static SoundEffect[] _explosions;

        private static SoundEffect[] _shots;

        private static SoundEffect[] _spawns;
        public static Song Music { get; private set; }
        // return a random explosion sound
        public static SoundEffect Explosion => _explosions[Rand.Next(_explosions.Length)];
        public static SoundEffect Shot => _shots[Rand.Next(_shots.Length)];
        public static SoundEffect Spawn => _spawns[Rand.Next(_spawns.Length)];

        public static void Load(ContentManager content)
        {
            Music = content.Load<Song>("Sound/Music");

            // These linq expressions are just a fancy way loading all sounds of each category into an array.
            _explosions =
                Enumerable.Range(1, 8).Select(x => content.Load<SoundEffect>("Sound/explosion-0" + x)).ToArray();
            _shots = Enumerable.Range(1, 4).Select(x => content.Load<SoundEffect>("Sound/shoot-0" + x)).ToArray();
            _spawns = Enumerable.Range(1, 8).Select(x => content.Load<SoundEffect>("Sound/spawn-0" + x)).ToArray();
        }
    }
}
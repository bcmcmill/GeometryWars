using System;
using GeometryWars.Entities.Player;
using GeometryWars.Entities.World;
using Microsoft.Xna.Framework;

namespace GeometryWars.Entities.Enemies
{
    internal static class EnemySpawner
    {
        private static readonly Random Rand = new Random();
        private static float _inverseSpawnChance = 90;
        private const float InverseBlackHoleChance = 600;

        public static void Update()
        {
            if (!PlayerShip.Instance.IsDead && EntityManager.Count < 200)
            {
                if (Rand.Next((int) _inverseSpawnChance) == 0)
                    EntityManager.Add(Enemy.CreateSeeker(GetSpawnPosition()));

                if (Rand.Next((int) _inverseSpawnChance) == 0)
                    EntityManager.Add(Enemy.CreateWanderer(GetSpawnPosition()));

                if (EntityManager.BlackHoleCount < 2 && Rand.Next((int) InverseBlackHoleChance) == 0)
                    EntityManager.Add(new BlackHole(GetSpawnPosition()));
            }

            // slowly increase the spawn rate as time progresses
            if (_inverseSpawnChance > 30)
                _inverseSpawnChance -= 0.005f;
        }

        private static Vector2 GetSpawnPosition()
        {
            Vector2 pos;

            do
            {
                pos = new Vector2(Rand.Next((int) GeoWarsGame.ScreenSize.X), Rand.Next((int) GeoWarsGame.ScreenSize.Y));
            } while (Vector2.DistanceSquared(pos, PlayerShip.Instance.Position) < 250*250);

            return pos;
        }

        public static void Reset()
        {
            _inverseSpawnChance = 90;
        }
    }
}
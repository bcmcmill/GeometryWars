using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GeometryWars.Entities.Enemies;
using GeometryWars.Entities.Player;
using GeometryWars.Entities.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GeometryWars.Entities
{
    internal static class EntityManager
    {
        public static List<Entity> Entities = new List<Entity>();
        public static List<Enemy> Enemies = new List<Enemy>();
        public static List<Bullet> Bullets = new List<Bullet>();
        public static List<BlackHole> BlackHoles = new List<BlackHole>();

        private static bool _isUpdating;
        private static readonly List<Entity> AddedEntities = new List<Entity>();

        public static int Count => Entities.Count;
        public static int BlackHoleCount => BlackHoles.Count;

        public static void Add(Entity entity)
        {
            if (!_isUpdating)
                AddEntity(entity);
            else
                AddedEntities.Add(entity);
        }

        private static void AddEntity(Entity entity)
        {
            Entities.Add(entity);
            if (entity is Bullet)
                Bullets.Add((Bullet) entity);
            else if (entity is Enemy)
                Enemies.Add((Enemy) entity);
            else if (entity is BlackHole)
                BlackHoles.Add((BlackHole) entity);
        }

        public static void Update()
        {
            _isUpdating = true;
            HandleCollisions();

            Parallel.ForEach(Entities, entity => entity.Update());

            _isUpdating = false;

            Parallel.ForEach(AddedEntities, AddEntity);

            AddedEntities.Clear();

            Entities = Entities.AsParallel().Where(x => !x.IsExpired).ToList();
            Bullets = Bullets.AsParallel().Where(x => !x.IsExpired).ToList();
            Enemies = Enemies.AsParallel().Where(x => !x.IsExpired).ToList();
            BlackHoles = BlackHoles.Where(x => !x.IsExpired).ToList();
        }

        private static void HandleCollisions()
        {
            // handle collisions between enemies
            Parallel.For(0, Enemies.Count,
                i =>
                {
                    for (var j = i + 1; j < Enemies.Count; j++)
                    {
                        if (IsColliding(Enemies[i], Enemies[j]))
                        {
                            Enemies[i].HandleCollision(Enemies[j]);
                            Enemies[j].HandleCollision(Enemies[i]);
                        }
                    } // end parallel for
                }); // end parallel for

            // handle collisions between bullets and enemies
            Parallel.ForEach(Enemies,
                enemy =>
                {
                    Parallel.ForEach(Bullets,
                        bullet =>
                        {
                            if (IsColliding(enemy, bullet))
                            {
                                enemy.WasShot();
                                bullet.IsExpired = true;
                            }
                        }); // end parallel foreach
                }); // end parallel foreach

            // handle collisions between the player and enemies

            Parallel.ForEach(Enemies, (enemy, loopState) =>
            {
                if (enemy.IsActive && IsColliding(PlayerShip.Instance, enemy))
                {
                    KillPlayer();
                    loopState.Stop();
                }
            });

            // handle collisions with black holes
            Parallel.For(0, BlackHoles.Count,
                (i, loopState) =>
                {
                    foreach (var enemy in Enemies)
                        if (enemy.IsActive && IsColliding(BlackHoles[i], enemy))
                            enemy.WasShot();

                    foreach (var bullet in Bullets)
                    {
                        if (!IsColliding(BlackHoles[i], bullet)) continue;
                        bullet.IsExpired = true;
                        BlackHoles[i].WasShot();
                    }

                    if (IsColliding(PlayerShip.Instance, BlackHoles[i]))
                    {
                        KillPlayer();
                        loopState.Stop();
                    }
                }); // end parallel for
        }

        private static void KillPlayer()
        {
            PlayerShip.Instance.Kill();
            Enemies.ForEach(enemy => enemy.WasShot());
            BlackHoles.ForEach(blackHole => blackHole.Kill());
            EnemySpawner.Reset();
        }

        private static bool IsColliding(Entity a, Entity b)
        {
            var radius = a.Radius + b.Radius;
            return !a.IsExpired && !b.IsExpired && Vector2.DistanceSquared(a.Position, b.Position) < radius*radius;
        }

        public static List<Entity> GetNearbyEntities(Vector2 position, float radius)
        {
            return Entities.Where(x => Vector2.DistanceSquared(position, x.Position) < radius*radius).ToList();
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            foreach (var entity in Entities)
            {
                entity.Draw(spriteBatch);
            }
        }
    }
}
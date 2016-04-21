using System.Threading.Tasks;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GeometryWars.Entities.World;
using GeometryWars.Entities.Player;
using GeometryWars.Entities.Enemies;
using System.Collections.Generic;

namespace GeometryWars.Entities
{
    static class EntityManager
	{
		static List<Entity> entities = new List<Entity>();
		static List<Enemy> enemies = new List<Enemy>();
		static List<Bullet> bullets = new List<Bullet>();
		static List<BlackHole> blackHoles = new List<BlackHole>();

		public static IEnumerable<BlackHole> BlackHoles { get { return blackHoles; } }

		static bool isUpdating;
		static List<Entity> addedEntities = new List<Entity>();

		public static int Count { get { return entities.Count; } }
		public static int BlackHoleCount { get { return blackHoles.Count; } }

		public static void Add(Entity entity)
		{
			if (!isUpdating)
				AddEntity(entity);
			else
				addedEntities.Add(entity);
		}

		private static void AddEntity(Entity entity)
		{
			entities.Add(entity);
			if (entity is Bullet)
				bullets.Add(entity as Bullet);
			else if (entity is Enemy)
				enemies.Add(entity as Enemy);
			else if (entity is BlackHole)
				blackHoles.Add(entity as BlackHole);
		}

		public static void Update()
		{
			isUpdating = true;
			HandleCollisions();

			Parallel.ForEach (entities, entity => entity.Update());

			isUpdating = false;

            Parallel.ForEach(addedEntities, entity => AddEntity(entity));

			addedEntities.Clear();

            entities = entities.AsParallel<Entity>().Where(x => !x.IsExpired).ToList();
            bullets = bullets.AsParallel<Bullet>().Where(x => !x.IsExpired).ToList();
            enemies = enemies.AsParallel<Enemy>().Where(x => !x.IsExpired).ToList();
            blackHoles = blackHoles.Where(x => !x.IsExpired).ToList();
        }

		static void HandleCollisions()
		{
            // handle collisions between enemies
            Parallel.For(0, enemies.Count,
                i => {
                        for(int j = i + 1; j < enemies.Count; j++)
                        {
                            if (IsColliding(enemies[i], enemies[j]))
                            {
                                enemies[i].HandleCollision(enemies[j]);
                                enemies[j].HandleCollision(enemies[i]);
                            }
                        } // end parallel for
                     }); // end parallel for

            // handle collisions between bullets and enemies
            Parallel.ForEach<Enemy>(enemies,
                enemy => {
                    Parallel.ForEach<Bullet>(bullets,
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

            Parallel.ForEach(enemies, (enemy, loopState) => {
                if(enemy.IsActive && IsColliding(PlayerShip.Instance, enemy))
                {
                    KillPlayer();
                    loopState.Stop();
                    return;
                }           
            });

            // handle collisions with black holes
            Parallel.For(0, blackHoles.Count, 
                (i, loopState) => {
                    for (int j = 0; j < enemies.Count; j++)

                        if (enemies[j].IsActive && IsColliding(blackHoles[i], enemies[j]))
                            enemies[j].WasShot();

                    for (int j = 0; j < bullets.Count; j++)
                    {
                        if (IsColliding(blackHoles[i], bullets[j]))
                        {
                            bullets[j].IsExpired = true;
                            blackHoles[i].WasShot();
                        }
                    }

                    if (IsColliding(PlayerShip.Instance, blackHoles[i]))
                        {
                            KillPlayer();
                            loopState.Stop();
                            return;
                        }
                     }); // end parallel for
        }

		private static void KillPlayer()
		{
			PlayerShip.Instance.Kill();
            Parallel.ForEach(enemies, enemy => enemy.WasShot());
            blackHoles.ForEach(x => x.Kill());
            EnemySpawner.Reset();
		}

		private static bool IsColliding(Entity a, Entity b)
		{
			float radius = a.Radius + b.Radius;
			return !a.IsExpired && !b.IsExpired && Vector2.DistanceSquared(a.Position, b.Position) < radius * radius;
		}

		public static IEnumerable<Entity> GetNearbyEntities(Vector2 position, float radius)
		{
			return entities.AsParallel<Entity>().Where(x => Vector2.DistanceSquared(position, x.Position) < radius * radius);
		}

		public static void Draw(SpriteBatch spriteBatch)
		{
            Parallel.ForEach(entities, entity => entity.Draw(spriteBatch));
		}
	}
}
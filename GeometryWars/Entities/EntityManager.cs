using System.Threading.Tasks;
using System.Collections;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GeometryWars.Entities.World;
using GeometryWars.Entities.Player;
using GeometryWars.Entities.Enemies;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GeometryWars.Entities
{
    static class EntityManager
	{
		static List<Entity> entities = new List<Entity>();
          static List<Entity> addedEntities = new List<Entity>();
          public static int Count => entities.Count;

         static List<Enemy> enemies = new List<Enemy>();
		static List<Bullet> bullets = new List<Bullet>();

          static List<BlackHole> blackHoles = new List<BlackHole>();
		public static IEnumerable<BlackHole> BlackHoles => blackHoles;
          public static int BlackHoleCount => blackHoles.Count;

          static bool isUpdating;

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
				bullets.Add((Bullet) entity);
			else if (entity is Enemy)
				enemies.Add((Enemy) entity);
			else if (entity is BlackHole)
				blackHoles.Add((BlackHole) entity);
		}

		public static void Update()
		{
			isUpdating = true;
			HandleCollisions();

			Parallel.ForEach(entities, entity => entity.Update());

			isUpdating = false;

            Parallel.ForEach(addedEntities, AddEntity);

			addedEntities.Clear();

            entities = entities.AsParallel().Where(x => !x.IsExpired).ToList();
            bullets = bullets.AsParallel().Where(x => !x.IsExpired).ToList();
            enemies = enemies.AsParallel().Where(x => !x.IsExpired).ToList();
            blackHoles = blackHoles.AsParallel().Where(x => !x.IsExpired).ToList();
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
                        } // End Parallel For
                     }); // End Parallel For

            // handle collisions between bullets and enemies
            Parallel.For(0, enemies.Count, i => {
                        for (int j = 0; j < bullets.Count; j++)
                        {
                            if (IsColliding(enemies[i], bullets[j]))
                            {
                                enemies[i].WasShot();
                                bullets[j].IsExpired = true;
                            }
                        } // end for
                     }); // end parallel for

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
                        Parallel.For(0, enemies.Count, 
                            j => {
                                    if (enemies[j].IsActive && IsColliding(blackHoles[i], enemies[j]))
                                        enemies[j].WasShot();
                                 }); // end parallel for

                        Parallel.For(0, bullets.Count,
                            j => {
                                    if (IsColliding(blackHoles[i], bullets[j]))
                                    {
                                        bullets[j].IsExpired = true;
                                        blackHoles[i].WasShot();
                                    }
                                 }); // end parallel for

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
            Parallel.ForEach(blackHoles, blackHole => blackHole.Kill());
			EnemySpawner.Reset();
		}

		private static bool IsColliding(Entity a, Entity b)
		{
			float radius = a.Radius + b.Radius;
			return !a.IsExpired && !b.IsExpired && Vector2.DistanceSquared(a.Position, b.Position) < radius * radius;
		}

		public static IEnumerable<Entity> GetNearbyEntities(Vector2 position, float radius)
		{
			return entities.Where(x => Vector2.DistanceSquared(position, x.Position) < radius * radius);
		}

		public static void Draw(SpriteBatch spriteBatch)
		{
            Parallel.ForEach(entities, entity => entity.Draw(spriteBatch));
		}
	}
}
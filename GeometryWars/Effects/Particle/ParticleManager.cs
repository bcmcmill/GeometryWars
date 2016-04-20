using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GeometryWars
{
    public class ParticleManager
	{
		// This delegate will be called for each particle.
		private Action<Particle> updateParticle;
		private CircularParticleArray particleList;

		public ParticleManager(int capacity, Action<Particle> updateParticle)
		{
			this.updateParticle = updateParticle;
			particleList = new CircularParticleArray(capacity);

			// Populate the list with empty particle objects, for reuse.
			for (int i = 0; i < capacity; i++)
				particleList[i] = new Particle();
		}

		public void Update()
		{
			int removalCount = 0;
			for (int i = 0; i < particleList.Count; i++)
			{
				var particle = particleList[i];

				updateParticle(particle);

				particle.PercentLife -= 1f / particle.Duration;

				// sift deleted particles to the end of the list
				Swap(particleList, i - removalCount, i);

				// if the alpha < 0, delete this particle
				if (particle.PercentLife < 0)
					removalCount++;
			}
			particleList.Count -= removalCount;
		}

		private static void Swap(CircularParticleArray list, int index1, int index2)
		{
			var temp = list[index1];
			list[index1] = list[index2];
			list[index2] = temp;
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			for (int i = 0; i < particleList.Count; i++)
			{
				var particle = particleList[i];

				Vector2 origin = new Vector2(particle.Texture.Width / 2, particle.Texture.Height / 2);
				spriteBatch.Draw(particle.Texture, particle.Position, null, particle.Tint, particle.Orientation, origin, particle.Scale, 0, 0);
			}
		}

		public void CreateParticle(Texture2D texture, Vector2 position, Color tint, float duration, float scale, ParticleState state, float theta = 0)
		{
			CreateParticle(texture, position, tint, duration, new Vector2(scale), state, theta);
		}

		public void CreateParticle(Texture2D texture, Vector2 position, Color tint, float duration, Vector2 scale, ParticleState state, float theta = 0)
		{
			Particle particle;
			if (particleList.Count == particleList.Capacity)
			{
				// if the list is full, overwrite the oldest particle, and rotate the circular list
				particle = particleList[0];
				particleList.Start++;
			}
			else
			{
				particle = particleList[particleList.Count];
				particleList.Count++;
			}

			// Create the particle
			particle.Texture = texture;
			particle.Position = position;
			particle.Tint = tint;

			particle.Duration = duration;
			particle.PercentLife = 1f;
			particle.Scale = scale;
			particle.Orientation = theta;
			particle.State = state;
		}

		public void Clear()
		{
			particleList.Count = 0;
		}

		public int ParticleCount
		{
			get { return particleList.Count; }
		}
	}
}
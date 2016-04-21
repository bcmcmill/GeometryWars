using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GeometryWars.Effects.Particle
{
    public class ParticleManager
    {
        private readonly CircularParticleArray _particleList;
        // This delegate will be called for each particle.
        private readonly Action<Particle> _updateParticle;

        public ParticleManager(int capacity, Action<Particle> updateParticle)
        {
            _updateParticle = updateParticle;
            _particleList = new CircularParticleArray(capacity);

            // Populate the list with empty particle objects, for reuse.
            for (var i = 0; i < capacity; i++)
                _particleList[i] = new Particle();
        }

        public int ParticleCount => _particleList.Count;

        public void Update()
        {
            var removalCount = 0;

            for (var i = 0; i < _particleList.Count; i++)
            {
                var particle = _particleList[i];

                _updateParticle(particle);

                particle.PercentLife -= 1f/particle.Duration;

                // sift deleted particles to the end of the list
                Swap(_particleList, i - removalCount, i);

                // if the alpha < 0, delete this particle
                if (particle.PercentLife < 0)
                    removalCount++;
            }

            _particleList.Count -= removalCount;
        }

        private static void Swap(CircularParticleArray list, int index1, int index2)
        {
            var temp = list[index1];
            list[index1] = list[index2];
            list[index2] = temp;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (var i = 0; i < _particleList.Count; i++)
            {
                var particle = _particleList[i];

                var origin = new Vector2(particle.Texture.Width/2, particle.Texture.Height/2);
                spriteBatch.Draw(particle.Texture, particle.Position, null, particle.Tint, particle.Orientation, origin,
                    particle.Scale, 0, 0);
            }
        }

        public void CreateParticle(Texture2D texture, Vector2 position, Color tint, float duration, float scale,
            ParticleState state, float theta = 0)
        {
            CreateParticle(texture, position, tint, duration, new Vector2(scale), state, theta);
        }

        public void CreateParticle(Texture2D texture, Vector2 position, Color tint, float duration, Vector2 scale,
            ParticleState state, float theta = 0)
        {
            Particle particle;
            if (_particleList.Count == _particleList.Capacity)
            {
                // if the list is full, overwrite the oldest particle, and rotate the circular list
                particle = _particleList[0];
                _particleList.Start++;
            }
            else
            {
                particle = _particleList[_particleList.Count];
                _particleList.Count++;
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
            _particleList.Count = 0;
        }
    }
}
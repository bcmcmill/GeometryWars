using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GeometryWars.Entities
{
    internal abstract class Entity
    {
        // The tint of the image. This will also allow us to change the transparency.
        protected Color Color = Color.White;
        protected Texture2D Image;
        public bool IsExpired; // true if the entity was destroyed and should be deleted.
        public float Orientation;

        public Vector2 Position, Velocity;
        public float Radius = 20; // used for circular collision detection

        public Vector2 Size => Image == null ? Vector2.Zero : new Vector2(Image.Width, Image.Height);

        public abstract void Update();

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Image, Position, null, Color, Orientation, Size/2f, 1f, 0, 0);
        }
    }
}
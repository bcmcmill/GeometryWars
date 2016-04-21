﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GeometryWars.Utilities
{
    internal static class Extensions
    {
        public static void DrawLine(this SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color,
            float thickness = 2f)
        {
            var delta = end - start;
            spriteBatch.Draw(TextureLoader.Pixel, start, null, color, delta.ToAngle(), new Vector2(0, 0.5f),
                new Vector2(delta.Length(), thickness), SpriteEffects.None, 0f);
        }

        public static float ToAngle(this Vector2 vector)
        {
            return (float) Math.Atan2(vector.Y, vector.X);
        }

        public static Vector2 ScaleTo(this Vector2 vector, float length)
        {
            return vector*(length/vector.Length());
        }

        public static Point ToPoint(this Vector2 vector)
        {
            return new Point((int) vector.X, (int) vector.Y);
        }

        public static float NextFloat(this Random rand, float minValue, float maxValue)
        {
            return (float) rand.NextDouble()*(maxValue - minValue) + minValue;
        }

        public static Vector2 NextVector2(this Random rand, float minLength, float maxLength)
        {
            var theta = rand.NextDouble()*2*Math.PI;
            var length = rand.NextFloat(minLength, maxLength);
            return new Vector2(length*(float) Math.Cos(theta), length*(float) Math.Sin(theta));
        }
    }
}
using System;
using Microsoft.Xna.Framework;

namespace GeometryWars.Utilities
{
    internal static class ColorUtil
    {
        private const float Tolerance = 0.000000001f;

        public static Vector3 ColorToHsv(Color color)
        {
            var c = color.ToVector3();
            var v = Math.Max(c.X, Math.Max(c.Y, c.Z));
            var chroma = v - Math.Min(c.X, Math.Min(c.Y, c.Z));

            if (Math.Abs(chroma) < Tolerance)
                return new Vector3(0, 0, v);

            var s = chroma/v;

            if (c.X >= c.Y && c.Y >= c.Z)
            {
                var h = (c.Y - c.Z)/chroma;
                if (h < 0)
                    h += 6;
                return new Vector3(h, s, v);
            }
            if (c.Y >= c.Z && c.Y >= c.X)
                return new Vector3((c.Z - c.X)/chroma + 2, s, v);
            return new Vector3((c.X - c.Y)/chroma + 4, s, v);
        }

        public static Color HsvToColor(Vector3 hsv)
        {
            return HsvToColor(hsv.X, hsv.Y, hsv.Z);
        }

        public static Color HsvToColor(float h, float s, float v)
        {
            if (Math.Abs(h) < Tolerance && Math.Abs(s) < Tolerance)
                return new Color(v, v, v);

            var c = s*v;
            var x = c*(1 - Math.Abs(h%2 - 1));
            var m = v - c;

            if (h < 1) return new Color(c + m, x + m, m);
            if (h < 2) return new Color(x + m, c + m, m);
            if (h < 3) return new Color(m, c + m, x + m);
            if (h < 4) return new Color(m, x + m, c + m);
            if (h < 5) return new Color(x + m, m, c + m);
            return new Color(c + m, m, x + m);
        }
    }
}
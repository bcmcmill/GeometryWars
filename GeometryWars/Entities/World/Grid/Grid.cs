﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GeometryWars.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GeometryWars.Entities.World.Grid
{
    public class Grid
    {
        private readonly PointMass[,] _points;
        private readonly Spring[] _springs;
        private Vector2 _screenSize;

        public Grid(Rectangle size, Vector2 spacing)
        {
            var springList = new List<Spring>();

            var numColumns = (int) (size.Width/spacing.X) + 1;
            var numRows = (int) (size.Height/spacing.Y) + 1;
            _points = new PointMass[numColumns, numRows];

            // these fixed points will be used to anchor the grid to fixed positions on the screen
            var fixedPoints = new PointMass[numColumns, numRows];


            Parallel.For(size.Top, numRows, i =>
            {
                Parallel.For(size.Left, numColumns, j =>
                {
                    _points[j, i] = new PointMass(new Vector3(j*spacing.X, i*spacing.Y, 0), 1);
                    fixedPoints[j, i] = new PointMass(new Vector3(j*spacing.X, i*spacing.Y, 0), 0);
                });
            });

            // link the point masses with springs

            var _lock = new object();

            Parallel.For(0, numRows, y =>
            {
                var tempList = new List<Spring>();

                for (var x = 0; x < numColumns; x++)
                {
                    if (x == 0 || y == 0 || x == numColumns - 1 || y == numRows - 1)
                        tempList.Add(new Spring(fixedPoints[x, y], _points[x, y], 0.1f, 0.1f));
                    else if (x % 3 == 0 && y % 3 == 0)
                        tempList.Add(new Spring(fixedPoints[x, y], _points[x, y], 0.002f, 0.02f));

                    const float stiffness = 0.28f;
                    const float damping = 0.06f;

                    if (x > 0)
                        tempList.Add(new Spring(_points[x - 1, y], _points[x, y], stiffness, damping));
                    if (y > 0)
                        tempList.Add(new Spring(_points[x, y - 1], _points[x, y], stiffness, damping));
                }

                lock (_lock) { 
                    springList.AddRange(tempList);
                }
            });

            //for (var y = 0; y < numRows; y++)
            //{
            //    for (var x = 0; x < numColumns; x++)
            //    {
            //        if (x == 0 || y == 0 || x == numColumns - 1 || y == numRows - 1)
            //            springList.Add(new Spring(fixedPoints[x, y], _points[x, y], 0.1f, 0.1f));
            //        else if (x%3 == 0 && y%3 == 0)
            //            springList.Add(new Spring(fixedPoints[x, y], _points[x, y], 0.002f, 0.02f));

            //        const float stiffness = 0.28f;
            //        const float damping = 0.06f;

            //        if (x > 0)
            //            springList.Add(new Spring(_points[x - 1, y], _points[x, y], stiffness, damping));
            //        if (y > 0)
            //            springList.Add(new Spring(_points[x, y - 1], _points[x, y], stiffness, damping));
            //    }
            //}

            _springs = springList.ToArray();
        }

        public void ApplyDirectedForce(Vector2 force, Vector2 position, float radius)
        {
            ApplyDirectedForce(new Vector3(force, 0), new Vector3(position, 0), radius);
        }

        public void ApplyDirectedForce(Vector3 force, Vector3 position, float radius)
        {
            Parallel.ForEach(_points.Cast<PointMass>(), mass =>
            {
                if (Vector3.DistanceSquared(position, mass.Position) < radius*radius)
                    mass.ApplyForce(10*force/(10 + Vector3.Distance(position, mass.Position)));
            });
        }
        
        public void ApplyImplosiveForce(float force, Vector2 position, float radius)
        {
            ApplyImplosiveForce(force, new Vector3(position, 0), radius);
        }

        public void ApplyImplosiveForce(float force, Vector3 position, float radius)
        {
            Parallel.ForEach(_points.Cast<PointMass>(), mass =>
            {
                var dist2 = Vector3.DistanceSquared(position, mass.Position);
                if (!(dist2 < radius*radius)) return;
                mass.ApplyForce(10*force*(position - mass.Position)/(100 + dist2));
                mass.IncreaseDamping(0.6f);
            });
        }

        public void ApplyExplosiveForce(float force, Vector2 position, float radius)
        {
            ApplyExplosiveForce(force, new Vector3(position, 0), radius);
        }

        public void ApplyExplosiveForce(float force, Vector3 position, float radius)
        {
            Parallel.ForEach(_points.Cast<PointMass>(), mass =>
            {
                var dist2 = Vector3.DistanceSquared(position, mass.Position);
                if (!(dist2 < radius*radius)) return;
                mass.ApplyForce(100 * force * (mass.Position - position) / (10000 + dist2));
                mass.IncreaseDamping(0.6f);
            });
        }

        public void Update()
        {
            Parallel.ForEach(_springs, spring => spring.Update());
            Parallel.ForEach(_points.Cast<PointMass>(), mass => mass.Update());
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _screenSize = GeoWarsGame.ScreenSize;

            var width = _points.GetLength(0);
            var height = _points.GetLength(1);
            var color = new Color(23, 97, 181, 85);

            for (var y = 1; y < height; y++)
            {
                for (var x = 1; x < width; x++)
                {
                    Vector2 left = new Vector2(), up = new Vector2();
                    var p = ToVec2(_points[x, y].Position);
                    if (x > 1)
                    {
                        left = ToVec2(_points[x - 1, y].Position);
                        var thickness = y % 3 == 1 ? 3f : 1f;

                        // use Catmull-Rom interpolation to help smooth bends in the grid
                        var clampedX = Math.Min(x + 1, width - 1);
                        var mid = Vector2.CatmullRom(ToVec2(_points[x - 2, y].Position), left, p,
                            ToVec2(_points[clampedX, y].Position), 0.5f);

                        // If the grid is very straight here, draw a single straight line. Otherwise, draw lines to our
                        // new interpolated midpoint
                        if (Vector2.DistanceSquared(mid, (left + p)/2) > 1)
                        {
                            spriteBatch.DrawLine(left, mid, color, thickness);
                            spriteBatch.DrawLine(mid, p, color, thickness);
                        }
                        else
                            spriteBatch.DrawLine(left, p, color, thickness);
                    }
                    if (y > 1)
                    {
                        up = ToVec2(_points[x, y - 1].Position);
                        var thickness = x % 3 == 1 ? 3f : 1f;
                        var clampedY = Math.Min(y + 1, height - 1);
                        var mid = Vector2.CatmullRom(ToVec2(_points[x, y - 2].Position), up, p,
                            ToVec2(_points[x, clampedY].Position), 0.5f);

                        if (Vector2.DistanceSquared(mid, (up + p)/2) > 1)
                        {
                            spriteBatch.DrawLine(up, mid, color, thickness);
                            spriteBatch.DrawLine(mid, p, color, thickness);
                        }
                        else
                            spriteBatch.DrawLine(up, p, color, thickness);
                    }

                    // Add interpolated lines halfway between our point masses. This makes the grid look
                    // denser without the cost of simulating more springs and point masses.
                    if (x <= 1 || y <= 1)
                        continue;

                    var upLeft = ToVec2(_points[x - 1, y - 1].Position);
                    spriteBatch.DrawLine(0.5f*(upLeft + up), 0.5f * (left + p), color, 1f); // vertical line
                    spriteBatch.DrawLine(0.5f*(upLeft + left), 0.5f * (up + p), color, 1f); // horizontal line
                }
            }
        }

        public Vector2 ToVec2(Vector3 v)
        {
            // do a perspective projection
            var factor = (v.Z + 2000) / 2000;
            return (new Vector2(v.X, v.Y) - _screenSize/2f)*factor + _screenSize/2;
        }
    }
}
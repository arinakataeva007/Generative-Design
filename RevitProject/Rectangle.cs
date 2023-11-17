using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;

namespace RevitProject
{
    public class Rectangle
    {
        public readonly XYZ minXminY;
        public readonly XYZ maxXminY;
        public readonly XYZ maxXmaxY;
        public readonly XYZ minXmaxY;

        public double WidthMeter { get { return (maxXmaxY.X - minXminY.X) * 0.3048; } }
        public double HeightMeter { get { return (minXmaxY.Y - minXminY.Y) * 0.3048; } }

        public Rectangle(XYZ minXminY, XYZ maxXmaxY)
        {
            this.minXminY = minXminY;
            this.maxXmaxY = maxXmaxY;

            maxXminY = new XYZ(maxXmaxY.X, minXminY.Y, minXminY.Z);
            minXmaxY = new XYZ(minXminY.X, maxXmaxY.Y, minXminY.Z);
        }

        public Rectangle(XYZ minXminY, XYZ maxXminY, XYZ maxXmaxY, XYZ minXmaxY)
        {
            this.minXminY = minXminY;
            this.maxXminY = maxXminY;
            this.maxXmaxY = maxXmaxY;
            this.minXmaxY = minXmaxY;
        }

        public Rectangle(XYZ[] points)
        {
            minXminY = points[0];
            maxXminY = points[1];
            maxXmaxY = points[2];
            minXmaxY = points[3];
        }

        public bool IntersectsWith(Rectangle other)
        {
            return !(minXminY.X > other.maxXmaxY.X || other.minXminY.X > maxXmaxY.X || 
                minXminY.Y > other.maxXmaxY.Y || other.minXminY.Y > maxXmaxY.Y);
        }

        public bool ContainsRectangle(Rectangle other)
        {
            return (minXminY.X <= other.minXminY.X && minXminY.Y <= other.minXminY.Y &&
                other.maxXmaxY.X <= maxXmaxY.X && other.maxXmaxY.Y <= maxXmaxY.Y);
        }

        public Rectangle GetIntersectionRectangle(Rectangle other)
        {
            if (IntersectsWith(other))
            {
                var minX = Math.Max(minXminY.X, other.minXminY.X);
                var minY = Math.Max(minXminY.Y, other.minXminY.Y);
                var maxX = Math.Min(maxXmaxY.X, other.maxXmaxY.X);
                var maxY = Math.Min(maxXmaxY.Y, other.maxXmaxY.Y);
                var pointZ = minXminY.Z;

                return new Rectangle(new XYZ(minX, minY, pointZ), new XYZ(maxX, minY, pointZ), 
                    new XYZ(maxX, maxY, pointZ), new XYZ(minX, maxY, pointZ));
            }

            return null;
        }

        public override string ToString()
        {
            return $"MinX_MinY - {minXminY}\n" +
                $"MaxX_MinY{maxXminY}\n" +
                $"MaxX_MaxY - {maxXmaxY}\n" +
                $"MinX_MaxY{minXmaxY}";
        }
    }
}

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
        public readonly double widthFeet;
        public readonly double heightFeet;


        public double WidthMeter { get { return widthFeet * 0.3048; } }
        public double HeightMeter { get { return heightFeet * 0.3048; } }
        public double SquareMeter { get { return WidthMeter * HeightMeter; } }

        public Rectangle(XYZ minXminY, XYZ maxXmaxY)
        {
            this.minXminY = minXminY;
            this.maxXmaxY = maxXmaxY;

            maxXminY = new XYZ(maxXmaxY.X, minXminY.Y, minXminY.Z);
            minXmaxY = new XYZ(minXminY.X, maxXmaxY.Y, minXminY.Z);
            widthFeet = maxXmaxY.Y - minXminY.Y;
            heightFeet = maxXmaxY.X - minXminY.X;
        }

        public Rectangle(XYZ minXminY, XYZ maxXminY, XYZ maxXmaxY, XYZ minXmaxY)
        {
            this.minXminY = minXminY;
            this.maxXminY = maxXminY;
            this.maxXmaxY = maxXmaxY;
            this.minXmaxY = minXmaxY;
            widthFeet = maxXmaxY.Y - minXminY.Y;
            heightFeet = maxXmaxY.X - minXminY.X;
        }

        public Rectangle(XYZ[] points)
        {
            minXminY = points[0];
            maxXminY = points[1];
            maxXmaxY = points[2];
            minXmaxY = points[3];
            widthFeet = maxXmaxY.Y - minXminY.Y;
            heightFeet = maxXmaxY.X - minXminY.X;
        }

        public Rectangle(XYZ minXminY, double widthFeet, double heightFeet)
        {
            this.minXminY = minXminY;
            this.widthFeet = widthFeet;
            this.heightFeet = heightFeet;

            maxXminY = new XYZ(minXminY.X + widthFeet, minXminY.Y, minXminY.Z);
            maxXmaxY = new XYZ(minXminY.X + widthFeet, minXminY.Y + heightFeet, minXminY.Z);
            minXmaxY = new XYZ(minXminY.X, minXminY.Y + heightFeet, minXminY.Z);
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

        public bool ContainsPoint(XYZ point)
        {
            return (minXminY.X <= point.X && point.X <= maxXminY.X) && (minXminY.Y <= point.Y && point.Y <= maxXmaxY.Y);
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

        public double GetMinDistanceSideToPoint(XYZ point)
        {
            if (ContainsPoint(point)) return 0;

            var sides = new (XYZ, XYZ)[4]
            {
                (minXminY, maxXminY),
                (maxXminY, maxXmaxY),
                (minXmaxY, maxXmaxY),
                (minXminY, minXmaxY)
            };

            var minDistance = double.MaxValue;

            if (minXminY.X <= point.X && point.X <= maxXmaxY.X)
                return Math.Min(Math.Abs(point.Y - minXminY.Y), Math.Abs(point.Y - maxXmaxY.Y));
            else if (minXminY.Y <= point.Y && point.Y <= maxXmaxY.Y)
                return Math.Min(Math.Abs(point.X - minXminY.X), Math.Abs(point.X - maxXmaxY.X));
            else
            {
                var minDistanceX = Math.Min(Math.Abs(point.X - minXminY.X), Math.Abs(point.X - maxXmaxY.X));
                var minDistanceY = Math.Min(Math.Abs(point.Y - minXminY.Y), Math.Abs(point.Y - maxXmaxY.Y));
                return Math.Min(minDistanceX, minDistanceY);
            }

            //foreach (var side in sides)
            //{
            //    var value = ((point.X - side.Item1.X) * (side.Item2.X - side.Item1.X) + 
            //        (point.Y - side.Item1.Y) * (side.Item2.Y - side.Item1.Y)) / 
            //        (Math.Pow(side.Item2.X - side.Item1.Y, 2) + Math.Pow(side.Item2.Y - side.Item1.Y, 2));
            //    if (value < 0)
            //        value = 0;
            //    if (value > 1)
            //        value = 1;
            //    var distance = Math.Sqrt(Math.Pow(side.Item1.X - point.X + (side.Item2.X - side.Item1.X) * value, 2) + 
            //        Math.Pow(side.Item1.Y - point.Y + (side.Item2.Y - side.Item1.Y) * value, 2));
            //    minDistance = Math.Min(minDistance, distance);
            //}

            //return minDistance;
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

using Autodesk.Revit.DB;
using System;
using System.Linq;

namespace RevitProject
{
    public class Rectangle2D : IGeometricShape2D
    {
        private readonly XYZ minXminY;
        private readonly XYZ maxXminY;
        private readonly XYZ maxXmaxY;
        private readonly XYZ minXmaxY;

        public XYZ[] ExtremePoints 
        { 
            get
            {
                return new XYZ[]
                {
                    minXminY, 
                    maxXminY, 
                    maxXmaxY, 
                    minXmaxY
                };
            } 
        }
        public Side2D[] Sides 
        { 
            get
            {
                return new Side2D[4]
                {
                    new Side2D(minXminY, minXmaxY),
                    new Side2D(minXmaxY, maxXmaxY),
                    new Side2D(maxXminY, maxXmaxY),
                    new Side2D(minXminY, maxXminY),
                };
            } 
        }

        public XYZ MinXminY { get => minXminY; }
        public XYZ MaxXminY { get => maxXminY; }
        public XYZ MaxXmaxY { get => maxXmaxY; }
        public XYZ MinXmaxY { get => minXmaxY; }

        public Side2D Width { get => Sides[1]; }
        public Side2D Height { get => Sides[0]; }

        public double SquareMeter { get => Width.LengthOnMeter * Height.LengthOnMeter; }
        public double SquareFeet { get => Width.LengthOnFeet * Height.LengthOnFeet; }

        public Rectangle2D(XYZ minXminY, XYZ maxXmaxY)
        {
            this.minXminY = minXminY;
            this.maxXmaxY = maxXmaxY;

            maxXminY = new XYZ(maxXmaxY.X, minXminY.Y, minXminY.Z);
            minXmaxY = new XYZ(minXminY.X, maxXmaxY.Y, minXminY.Z);
        }

        public Rectangle2D(XYZ minXminY, XYZ maxXminY, XYZ maxXmaxY, XYZ minXmaxY)
        {
            this.minXminY = minXminY;
            this.maxXminY = maxXminY;
            this.maxXmaxY = maxXmaxY;
            this.minXmaxY = minXmaxY;
        }

        public Rectangle2D(XYZ[] points)
        {
            minXminY = points[0];
            maxXminY = points[1];
            maxXmaxY = points[2];
            minXmaxY = points[3];
        }

        public Rectangle2D(XYZ minXminY, double widthFeet, double heightFeet)
        {
            this.minXminY = minXminY;
            maxXminY = new XYZ(minXminY.X + widthFeet, minXminY.Y, minXminY.Z);
            maxXmaxY = new XYZ(minXminY.X + widthFeet, minXminY.Y + heightFeet, minXminY.Z);
            minXmaxY = new XYZ(minXminY.X, minXminY.Y + heightFeet, minXminY.Z);
        }

        public bool IntersectsWith(Rectangle2D other)
        {
            return !(MinXminY.X > other.MaxXmaxY.X || other.MinXminY.X > MaxXmaxY.X || 
                MinXminY.Y > other.MaxXmaxY.Y || other.MinXminY.Y > MaxXmaxY.Y);
        }

        public bool Contains(XYZ point)
        {
            return (MinXminY.X <= point.X && point.X <= MaxXminY.X) && (MinXminY.Y <= point.Y && point.Y <= MaxXmaxY.Y);
        }

        public bool Contains(Rectangle2D other)
        {
            return (MinXminY.X <= other.MinXminY.X && MinXminY.Y <= other.MinXminY.Y &&
                other.MaxXmaxY.X <= MaxXmaxY.X && other.MaxXmaxY.Y <= MaxXmaxY.Y);
        }

        public Rectangle2D GetIntersectionRectangle(Rectangle2D other)
        {
            if (IntersectsWith(other))
            {
                var minX = Math.Max(MinXminY.X, other.MinXminY.X);
                var minY = Math.Max(MinXminY.Y, other.MinXminY.Y);
                var maxX = Math.Min(MaxXmaxY.X, other.MaxXmaxY.X);
                var maxY = Math.Min(MaxXmaxY.Y, other.MaxXmaxY.Y);
                var pointZ = MinXminY.Z;

                return new Rectangle2D(new XYZ(minX, minY, pointZ), new XYZ(maxX, minY, pointZ), 
                    new XYZ(maxX, maxY, pointZ), new XYZ(minX, maxY, pointZ));
            }

            return null;
        }

        //public double GetMinDistanceSideToPoint(XYZ point)
        //{
        //    if (ContainsPoint(point)) return 0;

        //    var sides = new (XYZ, XYZ)[4]
        //    {
        //        (MinXminY, MaxXminY),
        //        (MaxXminY, MaxXmaxY),
        //        (MinXmaxY, MaxXmaxY),
        //        (MinXminY, MinXmaxY)
        //    };

        //    if (MinXminY.X <= point.X && point.X <= MaxXmaxY.X)
        //        return Math.Min(Math.Abs(point.Y - MinXminY.Y), Math.Abs(point.Y - MaxXmaxY.Y));
        //    else if (MinXminY.Y <= point.Y && point.Y <= MaxXmaxY.Y)
        //        return Math.Min(Math.Abs(point.X - MinXminY.X), Math.Abs(point.X - MaxXmaxY.X));
        //    else
        //    {
        //        var minDistanceX = Math.Min(Math.Abs(point.X - MinXminY.X), Math.Abs(point.X - MaxXmaxY.X));
        //        var minDistanceY = Math.Min(Math.Abs(point.Y - MinXminY.Y), Math.Abs(point.Y - MaxXmaxY.Y));
        //        return Math.Min(minDistanceX, minDistanceY);
        //    }

        //    //foreach (var side in sides)
        //    //{
        //    //    var value = ((point.X - side.Item1.X) * (side.Item2.X - side.Item1.X) + 
        //    //        (point.Y - side.Item1.Y) * (side.Item2.Y - side.Item1.Y)) / 
        //    //        (Math.Pow(side.Item2.X - side.Item1.Y, 2) + Math.Pow(side.Item2.Y - side.Item1.Y, 2));
        //    //    if (value < 0)
        //    //        value = 0;
        //    //    if (value > 1)
        //    //        value = 1;
        //    //    var distance = Math.Sqrt(Math.Pow(side.Item1.X - point.X + (side.Item2.X - side.Item1.X) * value, 2) + 
        //    //        Math.Pow(side.Item1.Y - point.Y + (side.Item2.Y - side.Item1.Y) * value, 2));
        //    //    minDistance = Math.Min(minDistance, distance);
        //    //}

        //    //return minDistance;
        //}

        public override string ToString()
        {
            return $"MinX_MinY - {MinXminY}\n" +
                $"MaxX_MinY{MaxXminY}\n" +
                $"MaxX_MaxY - {MaxXmaxY}\n" +
                $"MinX_MaxY{MinXmaxY}";
        }
    }
}

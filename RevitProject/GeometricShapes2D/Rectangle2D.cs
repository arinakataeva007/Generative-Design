using Autodesk.Revit.DB;
using System;

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

        public override string ToString()
        {
            return $"MinX_MinY - {MinXminY}\n" +
                $"MaxX_MinY{MaxXminY}\n" +
                $"MaxX_MaxY - {MaxXmaxY}\n" +
                $"MinX_MaxY{MinXmaxY}";
        }
    }
}

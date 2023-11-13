using Autodesk.Revit.DB;

namespace RevitProject
{
    public class Rectangle
    {
        public readonly XYZ minXminY;
        public readonly XYZ maxXminY;
        public readonly XYZ maxXmaxY;
        public readonly XYZ minXmaxY;

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
                maxXmaxY.Y > other.minXminY.Y || other.maxXmaxY.Y > minXminY.Y);
        }

        public bool ContainsRectangle(Rectangle other)
        {
            return (minXminY.X <= other.minXminY.X && minXminY.Y <= other.minXminY.Y &&
                other.maxXmaxY.X <= maxXmaxY.X && other.maxXmaxY.Y <= maxXmaxY.Y);
        }

        public override string ToString()
        {
            return $"MIN - {minXminY}\n{maxXminY}\nMAX - {maxXmaxY}\n{minXmaxY}";
        }
    }
}

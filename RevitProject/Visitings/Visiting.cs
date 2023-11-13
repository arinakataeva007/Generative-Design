using Autodesk.Revit.DB;
using System.Drawing;

namespace RevitProject
{
    public abstract class Visiting
    {
        public abstract string Name {  get; }

        protected abstract double MinWidthMeter { get; }
        protected abstract double MinHeightMeter { get; }
        protected abstract double MinSquareMeter { get; }

        public XYZ minPoint; // костыль (public)

        protected double widthMeter;
        public double WidthMeter
        {
            get
            {
                if (widthMeter == 0)
                    return squareMeter / heightMeter;
                return widthMeter;
            }
            set
            {
                if (widthMeter == MinWidthMeter)
                    widthMeter = value;
            }
        }
        public double WidthFeet { get { return widthMeter / 0.3048; } }


        protected double heightMeter;
        public double HeightMeter
        {
            get
            {
                if (heightMeter == 0)
                    return squareMeter / widthMeter;
                return heightMeter;
            }
            set
            {
                if (heightMeter == MinHeightMeter)
                    heightMeter = value;
            }
        }

        public double HeightFeet { get { return heightMeter / 0.3048; } }

        protected double squareMeter;
        public double SquareMeter
        {
            get
            {
                if (squareMeter == 0)
                    return widthMeter * heightMeter;
                return squareMeter;
            }
            set
            {
                if (squareMeter == MinSquareMeter)
                    squareMeter = value;
            }
        }

        public Rectangle Rectangle
        {
            get
            {
                if (GetExtremePoints().Length == 4)
                    return new Rectangle(GetExtremePoints());

                return null;
            }
        }

        public Visiting()
        {
            widthMeter = MinWidthMeter;
            squareMeter = MinSquareMeter;
            heightMeter = squareMeter / widthMeter;
        }

        public Visiting(XYZ minPoint)
        {
            this.minPoint = minPoint;
            widthMeter = MinWidthMeter;
            squareMeter = MinSquareMeter;
            heightMeter = squareMeter / widthMeter;
        }

        public Visiting(XYZ minPoint, double widthMeter = 0.0, double heightMeter = 0.0, double squareMeter = 0.0)
        {
            this.minPoint = minPoint;

            if (widthMeter == 0)
                widthMeter = squareMeter / heightMeter;
            if (heightMeter == 0)
                heightMeter = squareMeter / widthMeter;
            if (squareMeter == 0)
                squareMeter = widthMeter * heightMeter;

            this.widthMeter = widthMeter;
            this.heightMeter = heightMeter;
            this.squareMeter = squareMeter;
        }

        public XYZ[] GetExtremePoints()
        {
            var extremePoints = new XYZ[4];
            var min = minPoint;
            var max = min + new XYZ(widthMeter / 0.3048, heightMeter / 0.3048, 3 / 0.3048);

            extremePoints[0] = new XYZ(min.X, min.Y, min.Z);
            extremePoints[1] = new XYZ(max.X, min.Y, min.Z);
            extremePoints[2] = new XYZ(max.X, max.Y, min.Z);
            extremePoints[3] = new XYZ(min.X, max.Y, min.Z);

            return extremePoints;
        }

        public void RotatePerpendicular()
        {
            (widthMeter, heightMeter) = (heightMeter, widthMeter);
        }
    }
}

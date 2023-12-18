using Autodesk.Revit.DB;
using System;

namespace RevitProject
{
    public class Side2D
    {
        public readonly XYZ pointMin;
        public readonly XYZ pointMax;

        public double LengthOnFeet { get => GetLengthOnFeet(); }
        public double LengthOnMeter { get => Math.Round(LengthOnFeet * 0.3048, 3); }

        public Side2D(XYZ pointMin, XYZ pointMax)
        {
            this.pointMin = pointMin;
            this.pointMax = pointMax;
        }

        private double GetLengthOnFeet()
        {
            return Math.Sqrt(Math.Pow(pointMax.X - pointMin.X, 2) + Math.Pow(pointMax.Y - pointMin.Y, 2));
        }

        public bool Contains(XYZ point)
        {
            //return (point.X - pointMin.X) * (pointMax.X - pointMin.X) == (point.Y - pointMin.Y) * (pointMax.Y - pointMin.Y) ||
            //    (point.X - pointMax.X) * (pointMin.X - pointMax.X) == (point.Y - pointMax.Y) * (pointMin.Y - pointMax.Y);

            return Math.Min(pointMin.X, pointMax.X) <= point.X && point.X <= Math.Max(pointMin.X, pointMax.X) &&
                Math.Min(pointMin.Y, pointMax.Y) <= point.Y && point.Y <= Math.Max(pointMin.Y, pointMax.Y);
        }

        public double DistanceToPoint(XYZ point)
        {
            var lenToPointMin = Math.Sqrt(Math.Pow(point.X - pointMin.X, 2) + Math.Pow(point.Y - pointMin.Y, 2));
            var lenToPointMax = Math.Sqrt(Math.Pow(point.X - pointMax.X, 2) + Math.Pow(point.Y - pointMax.Y, 2));
            var halfMeter = (LengthOnFeet + lenToPointMax + lenToPointMin) / 2;
            var square = Math.Sqrt(halfMeter * (halfMeter - LengthOnFeet) * (halfMeter - lenToPointMin) * (halfMeter - lenToPointMax));
            var scalarToPointMin = (point.X - pointMin.X) * (pointMax.X - pointMin.X) + (point.Y - pointMin.Y) * (pointMax.Y - pointMin.Y);
            var scalarToPointMax = (point.X - pointMax.X) * (pointMin.X - pointMax.X) + (point.Y - pointMax.Y) * (pointMin.Y - pointMax.Y);

            if (scalarToPointMax > 0 && scalarToPointMin > 0)
                return square * 2 / LengthOnFeet;
            else if (scalarToPointMin <= 0 || scalarToPointMax <= 0)
                return Math.Min(lenToPointMin, lenToPointMax);
            else
                return 0;
        }

        public override string ToString()
        {
            return $"PointMin - {pointMin}\nPointMax - {pointMax}";
        }
    }
}

using Autodesk.Revit.DB;
using System.Windows.Media;

namespace RevitProject
{
    public abstract class Room
    {
        public abstract string Name { get; }
        protected abstract double MinWidthMeter { get; }
        protected abstract double MinHeightMeter { get; }
        protected abstract double MinSquareMeter { get; }

        protected XYZ minPoint;

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

        protected Rectangle rectangle;

        public Rectangle Rectangle
        {
            get
            {
                if (rectangle != null)
                    return rectangle;
                if (GetExtremePoints().Length == 4)
                    return new Rectangle(GetExtremePoints());

                return null;
            }
        }

        public Room()
        {
            widthMeter = MinWidthMeter;
            squareMeter = MinSquareMeter;
            heightMeter = squareMeter / widthMeter;
        }

        public Room(XYZ minPoint)
        {
            this.minPoint = minPoint;
            widthMeter = MinWidthMeter;
            squareMeter = MinSquareMeter;
            heightMeter = squareMeter / widthMeter;
        }

        public Room(XYZ minPoint, double widthMeter = 0.0, double heightMeter = 0.0, double squareMeter = 0.0)
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

        public Room(Rectangle rectangle)
        {
            this.rectangle = rectangle;

            widthMeter = rectangle.WidthMeter;
            heightMeter = rectangle.HeightMeter;
            squareMeter = rectangle.SquareMeter;
        }

        public XYZ[] GetExtremePoints()
        {
            var extremePoints = new XYZ[4];

            if (minPoint != null)
            {
                var min = minPoint;
                var max = min + new XYZ(widthMeter / 0.3048, heightMeter / 0.3048, 3 / 0.3048);

                extremePoints[0] = new XYZ(min.X, min.Y, min.Z);
                extremePoints[1] = new XYZ(max.X, min.Y, min.Z);
                extremePoints[2] = new XYZ(max.X, max.Y, min.Z);
                extremePoints[3] = new XYZ(min.X, max.Y, min.Z);
            }
            if (rectangle != null)
            {
                extremePoints[0] = rectangle.minXminY;
                extremePoints[1] = rectangle.maxXminY;
                extremePoints[2] = rectangle.maxXmaxY;
                extremePoints[3] = rectangle.minXmaxY;
            }

            return extremePoints;
        }

        public void RotatePerpendicular()
        {
            (widthMeter, heightMeter) = (heightMeter, widthMeter);
        }

        public static Room CreateNewRoom(Room room, Rectangle rectangle)
        {
            switch (room.GetType().Name)
            {
                case nameof(Kitchen):
                    return new Kitchen(rectangle);
                case nameof(Hallway): 
                    return new Hallway(rectangle);
                case nameof(Bathroom): 
                    return new Bathroom(rectangle);
                case nameof(LivingRoom): 
                    return new LivingRoom(rectangle);

            }

            return null;
        }

        public static Room CreateNewRoom(Room room, XYZ pointMin, XYZ pointMax)
        {
            var rectangle = new Rectangle(pointMin, pointMax);

            switch (room.GetType().Name)
            {
                case nameof(Kitchen):
                    return new Kitchen(rectangle);
                case nameof(Hallway):
                    return new Hallway(rectangle);
                case nameof(Bathroom):
                    return new Bathroom(rectangle);
                case nameof(LivingRoom):
                    return new LivingRoom(rectangle);
            }

            return null;
        }

        public static Room CreateNewRoom(Room room, XYZ position,
            double widthMeter, double heightMeter, double squareMeter)
        {
            switch (room.GetType().Name)
            {
                case nameof(Kitchen):
                    return new Kitchen(position, widthMeter, heightMeter, squareMeter);
                case nameof(Hallway):
                    return new Hallway(position, widthMeter, heightMeter, squareMeter);
                case nameof(Bathroom):
                    return new Bathroom(position, widthMeter, heightMeter, squareMeter);
                case nameof(LivingRoom):
                    return new LivingRoom(position, widthMeter, heightMeter, squareMeter);
            }

            return null;
        }
    }
}

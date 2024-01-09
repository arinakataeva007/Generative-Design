using Autodesk.Revit.DB;

namespace RevitProject
{
    public abstract class Room
    {
        public abstract string Name { get; }
        protected abstract double MinWidthMeter { get; }
        protected abstract double MinHeightMeter { get; }
        protected abstract double MinSquareMeter { get; }
        public abstract bool canNearWindow { get; }

        protected XYZ minPoint;

        protected double widthMeter;
        public double WidthMeter => widthMeter;
        public double WidthFeet => widthMeter / 0.3048;

        protected double heightMeter;
        public double HeightMeter => heightMeter;
        public double HeightFeet => heightMeter / 0.3048;

        protected double squareMeter;
        public double SquareMeter => squareMeter;

        protected Rectangle2D rectangle;
        public Rectangle2D Rectangle => rectangle;

        public Room()
        {
            widthMeter = MinWidthMeter;
            heightMeter = MinHeightMeter;
            squareMeter = widthMeter * heightMeter;
        }

        public Room(XYZ minPoint)
        {
            this.minPoint = minPoint;
            widthMeter = MinWidthMeter;
            squareMeter = MinSquareMeter;
            heightMeter = squareMeter / widthMeter;
            rectangle = new Rectangle2D(minPoint, WidthFeet, HeightFeet);
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
            rectangle = new Rectangle2D(minPoint, WidthFeet, HeightFeet);
        }

        public Room(Rectangle2D rectangle)
        {
            this.rectangle = rectangle;

            minPoint = rectangle.MinXminY;
            widthMeter = rectangle.Width.LengthOnMeter;
            heightMeter = rectangle.Height.LengthOnMeter;
            squareMeter = rectangle.SquareMeter;
        }

        public void RotatePerpendicular()
        {
            (widthMeter, heightMeter) = (heightMeter, widthMeter);
        }

        public bool CanReduceWidthBy(double value) => WidthMeter - value * 0.3048 >= MinWidthMeter;

        public bool CanReduceHeightBy(double value) => HeightMeter - value * 0.3048 >= MinHeightMeter;

        public abstract Room CreateNew(Rectangle2D newRectangle);

        public abstract Room CreateNew(XYZ pointMin, XYZ pointMax);

        public abstract bool IsCorectPositionRelativeWalls(Side2D wallWithDoor, Side2D wallWithWindow);
    }
}

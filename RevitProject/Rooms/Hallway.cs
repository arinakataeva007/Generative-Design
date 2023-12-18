using Autodesk.Revit.DB;

namespace RevitProject
{
    public class Hallway : Room
    {
        public override string Name => "Hallway";

        protected override double MinWidthMeter => 1.1;

        protected override double MinHeightMeter => 1.1;

        protected override double MinSquareMeter => MinWidthMeter * MinHeightMeter;

        public override bool canNearWindow => false;

        public Hallway() : base() { }

        public Hallway(XYZ minPoint) : base(minPoint) { }

        public Hallway(XYZ minPoint, double widthMeter = 0, double heightMeter = 0, double squareMeter = 0) : 
            base(minPoint, widthMeter, heightMeter, squareMeter) { }

        public Hallway(Rectangle2D rectangle) : base(rectangle) { }

        public override Room CreateNew(Rectangle2D newRectangle)
        {
            return new Hallway(newRectangle);
        }

        public override Room CreateNew(XYZ pointMin, XYZ pointMax)
        {
            return new Hallway(new Rectangle2D(pointMin, pointMax));
        }

        public override bool IsCorectPositionRelativeWalls(Side2D wallWithDoor, Side2D wallWithWindow)
        {
            for (var i = 0; i < rectangle.ExtremePoints.Length; i++)
            {
                if (wallWithDoor.Contains(rectangle.ExtremePoints[i]))
                    return true;
            }

            return false;
        }
    }
}

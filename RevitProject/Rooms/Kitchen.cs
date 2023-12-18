using Autodesk.Revit.DB;

namespace RevitProject
{
    public class Kitchen : Room
    {
        public override string Name => "Kitchen";

        protected override double MinWidthMeter => 2.8;

        protected override double MinHeightMeter => 2.8;

        protected override double MinSquareMeter => MinWidthMeter * MinHeightMeter;

        public override bool canNearWindow => true;

        public Kitchen() : base() { }

        public Kitchen(XYZ minPoint) : base(minPoint) { }

        public Kitchen(XYZ minPoint, double widthMeter = 0, double heightMeter = 0, double squareMeter = 0) :
            base(minPoint, widthMeter, heightMeter, squareMeter) { }

        public Kitchen(Rectangle2D rectangle) : base(rectangle) { }

        public override Room CreateNew(Rectangle2D newRectangle)
        {
            return new Kitchen(newRectangle);
        }

        public override Room CreateNew(XYZ pointMin, XYZ pointMax)
        {
            return new Kitchen(new Rectangle2D(pointMin, pointMax));
        }

        public override bool IsCorectPositionRelativeWalls(Side2D wallWithDoor, Side2D wallWithWindow)
        {
            return true;
        }
    }
}

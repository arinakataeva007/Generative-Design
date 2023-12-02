using Autodesk.Revit.DB;

namespace RevitProject
{
    public class Kitchen : Room
    {
        public override string Name => "Kitchen";

        protected override double MinWidthMeter => 2.8;

        protected override double MinHeightMeter => MinSquareMeter / MinWidthMeter;

        protected override double MinSquareMeter => 10;

        public Kitchen() : base() { }

        public Kitchen(XYZ minPoint) : base(minPoint) { }

        public Kitchen(XYZ minPoint, double widthMeter = 0, double heightMeter = 0, double squareMeter = 0) :
            base(minPoint, widthMeter, heightMeter, squareMeter) { }

        public Kitchen(Rectangle rectangle) : base(rectangle) { }
    }
}

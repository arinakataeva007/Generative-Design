using Autodesk.Revit.DB;

namespace RevitProject
{
    internal class Bathroom : Visiting
    {
        public override string Name => "Bathroom";

        protected override double MinWidthMeter => 1.65;

        protected override double MinHeightMeter => 1.65;

        protected override double MinSquareMeter => 4;

        public Bathroom() : base() { }

        public Bathroom(XYZ minPoint) : base(minPoint) { }

        public Bathroom(XYZ minPoint, double widthMeter = 0, double heightMeter = 0, double squareMeter = 0) :
            base(minPoint, widthMeter, heightMeter, squareMeter)
        {
        }

    }
}

using Autodesk.Revit.DB;

namespace RevitProject
{
    public class Hallway : Visiting
    {
        public override string Name => "Hallway";

        protected override double MinWidthMeter => 1.1;

        protected override double MinHeightMeter => 1.1;

        protected override double MinSquareMeter => 1.1;

        public Hallway() : base() { }

        public Hallway(XYZ minPoint) : base(minPoint) { }

        public Hallway(XYZ minPoint, double widthMeter = 0, double heightMeter = 0, double squareMeter = 0) : 
            base(minPoint, widthMeter, heightMeter, squareMeter)
        {
        }

        
    }
}

using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitProject.Visitings
{
    internal class LivingRoom : Visiting
    {
        public override string Name => "Living Room";

        protected override double MinWidthMeter => 3;

        protected override double MinHeightMeter => 3;

        protected override double MinSquareMeter => 12;

        public LivingRoom() : base() { }

        public LivingRoom(XYZ minPoint) : base(minPoint) { }

        public LivingRoom(XYZ minPoint, double widthMeter = 0, double heightMeter = 0, double squareMeter = 0) : 
            base(minPoint, widthMeter, heightMeter, squareMeter)
        {
        }
    }
}

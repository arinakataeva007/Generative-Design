using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitProject
{
    public class Visiting
    {
        public readonly double WidthMeter;
        public readonly double HeightMeter;
        public readonly XYZ PointMin;

        public double Square { get {  return WidthMeter * HeightMeter; } }

        public Visiting(double widthmeter, double heightMeter, XYZ pointMin)
        {
            WidthMeter = widthmeter;
            HeightMeter = heightMeter;
            PointMin = pointMin;
        }
    }
}

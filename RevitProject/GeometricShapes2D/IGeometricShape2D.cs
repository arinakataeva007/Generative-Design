using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitProject
{
    public interface IGeometricShape2D
    {
        XYZ[] ExtremePoints { get; }
        Side2D[] Sides { get; }
        double SquareMeter { get; }
        double SquareFeet { get; }

        bool Contains(XYZ point);
        bool Contains(Rectangle2D rectangle);
    }
}

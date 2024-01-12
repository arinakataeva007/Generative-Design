using Autodesk.Revit.DB;

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

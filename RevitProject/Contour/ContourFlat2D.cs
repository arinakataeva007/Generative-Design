using Autodesk.Revit.DB;
using System.Linq;

namespace RevitProject
{
    public class ContourFlat2D
    {
        public readonly IGeometricShape2D GeometricShape;
        public readonly Side2D SideWithDoor;
        public readonly Side2D SideWithWindow;

        public ContourFlat2D(IGeometricShape2D geometricShape, Side2D sideWithDoor, Side2D sideWithWindow)
        {
            GeometricShape = geometricShape;
            SideWithDoor = sideWithDoor;
            SideWithWindow = sideWithWindow;
        }

        //public void SubstractWidthOuterWalls(double widthOuterWall)
        //{
        //    var minX = GeometricShape.ExtremePoints.Min(p => p.X);
        //    var minY = GeometricShape.ExtremePoints.Min(p => p.Y);
        //    var maxX = GeometricShape.ExtremePoints.Max(p => p.X);
        //    var maxY = GeometricShape.ExtremePoints.Max(p => p.Y);
        //    var points = GeometricShape.ExtremePoints;

        //    for (var i = 0; i < points.Length; i++)
        //    {
        //        for (var x = -widthOuterWall; x <= widthOuterWall; x++)
        //        {
        //            for (var y = -widthOuterWall; y <= widthOuterWall; y++)
        //            {
        //                if (x == 0 || y == 0) continue;

        //                if (GeometricShape.Contains(points[i] + new XYZ(x, y, 0)) &&
        //                    (points[i].X == minX || points[i].X == maxX || points[i].Y == minY || points[i].Y == maxY))
        //                    points[i] += new XYZ(x, y, 0);
        //                if (!GeometricShape.Contains(points[i] + new XYZ(x, y, 0)) && points[i].X != minX && points[i].X != maxX
        //                    && points[i].Y != minY && points[i].Y != maxY)
        //                    points[i] -= new XYZ(x, y, 0);
        //            }
        //        }
        //    }
        //}
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitProject
{
    public class Generate
    {
        public static List<Visiting> GetShapes(List<double> sides, List<XYZ> extremePointsSpace)
        {
            //Пока только для прямоугольника, квадрата
            var minPointSpace = extremePointsSpace[0];

            var height = sides[0];
            var width = sides[1];
            var squareSpace = height * width;
            var visitings = GetVisitings(squareSpace);
            var sortVisitings = visitings.OrderByDescending(x => x.SquareMeter).ToList();

            return MoveVisitings(sortVisitings, extremePointsSpace);
        }

        private static List<Visiting> GetVisitings(double square)
        {
            var visitings = new List<Visiting> { new Kitchen(), new Hallway(), new Bathroom() };

            if (square < 28) // студия
                return visitings;
            else if (square < 44) // однокомнатная
                visitings.Add(new LivingRoom());
            else if (square < 56) // двушка
                visitings.AddRange(new List<Visiting> { new LivingRoom(), new LivingRoom() });
            else if (square < 70) // трёшка
                return new List<Visiting> { };
            else
                return new List<Visiting> { };

            return visitings;
        }

        private static List<Visiting> MoveVisitings(List<Visiting> visitings, List<XYZ> extremePointsSpace)
        {
            var spaceMinX = extremePointsSpace[0].X;
            var spaceMinY = extremePointsSpace[0].Y;
            var pointZ = extremePointsSpace[0].Z;
            var maxX = extremePointsSpace[2].X;
            var maxY = extremePointsSpace[2].Y;
            var spaceRectangle = new Rectangle(extremePointsSpace.ToArray());
            var movingVisitings = new List<Visiting>();
            var rectangles = new List<Rectangle>();
            var countVisitings = visitings.Count;

            for (var i = 0; i < countVisitings; i++)
            {
                if (spaceMinX + visitings[i].WidthFeet <= maxX && spaceMinY + visitings[i].HeightFeet <= maxY)
                {
                    if (movingVisitings.Count == 0)
                    {
                        movingVisitings.Add(DefineVisitingPoint(visitings[i], new XYZ(spaceMinX, spaceMinY, pointZ),
                            visitings[i].WidthMeter, visitings[i].HeightMeter, visitings[i].SquareMeter));
                        rectangles.Add(movingVisitings[i].Rectangle);
                    }
                    else
                    {
                        var newVisiting = GetNewVisiting(visitings[i], movingVisitings, spaceRectangle, pointZ);

                        if (newVisiting != null)
                            movingVisitings.Add(newVisiting);
                    }
                }
            }

            ProcessVisitingsDistanceBorders(movingVisitings, spaceRectangle);
            ProcessSizesPlacedVisitings(movingVisitings, spaceRectangle);

            return movingVisitings;
        }

        private static Visiting GetNewVisiting(Visiting visiting, List<Visiting> movingVisitings, Rectangle spaceRectangle, double pointZ)
        {
            for (var j = 0; j < movingVisitings.Count; j++)
            {
                try
                {
                    var extremePoints = movingVisitings[j].GetExtremePoints();

                    foreach (var point in extremePoints)
                    {
                        var newPoints = GetNewPoints(point, movingVisitings);

                        foreach (var newPoint in newPoints)
                        {
                            var newVisiting = DefineVisitingPoint(visiting, newPoint, visiting.WidthMeter, visiting.HeightMeter, visiting.SquareMeter);
                            var rectangle = newVisiting.Rectangle;

                            //Проверка на вхождение в контур
                            if (spaceRectangle.ContainsRectangle(newVisiting.Rectangle))
                            {
                                newVisiting = ProcessingIntersections(newVisiting, rectangle, movingVisitings, spaceRectangle);

                                if (newVisiting != null)
                                    return newVisiting;
                            }
                            else
                            {
                                var intersectRectangle = spaceRectangle.GetIntersectionRectangle(rectangle);
                                newVisiting = DefineVisitingPoint(newVisiting, intersectRectangle.minXminY,
                                    intersectRectangle.WidthMeter, intersectRectangle.HeightMeter, 0);

                                newVisiting = ProcessingIntersections(newVisiting, newVisiting.Rectangle, movingVisitings, spaceRectangle);

                                if (newVisiting != null)
                                    return newVisiting;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("EXCEPTION", $"STR-96 {ex.Message}");
                }

            }
            TaskDialog.Show("PUm", $"STR-120 OUCH {visiting}");

            return null;
        }

        private static bool CheckIntersections(Visiting visiting, List<Visiting> spacedVisitings)
        {
            var intersectionVisitings = spacedVisitings.Where(m => m.Rectangle.IntersectsWith(visiting.Rectangle)).ToList();

            if (intersectionVisitings.Count == 0) return false;
            else if (intersectionVisitings.Count == 1)
            {
                if (intersectionVisitings[0].Name == visiting.Name && 
                    visiting.Rectangle.ContainsRectangle(intersectionVisitings[0].Rectangle))
                    return false;
            }

            return true;
        }

        private static Visiting ProcessingIntersections(Visiting visiting, Rectangle visitingRectangle, List<Visiting> spacedVisitings, Rectangle spaceRectangle)
        {
            if (!CheckIntersections(visiting, spacedVisitings))
                return visiting;

            return null;
        }

        private static Visiting DefineVisitingPoint(Visiting visiting, XYZ position,
            double widthMeter, double heightMeter, double squareMeter)
        {
            Visiting newVisiting;

            if (visiting is Kitchen)
                newVisiting = new Kitchen(position, widthMeter, heightMeter, squareMeter);
            else if (visiting is Hallway)
                newVisiting = new Hallway(position, widthMeter, heightMeter, squareMeter);
            else if (visiting is Bathroom)
                newVisiting = new Bathroom(position, widthMeter, heightMeter, squareMeter);
            else
                newVisiting = new LivingRoom(position, widthMeter, heightMeter, squareMeter);

            return newVisiting;
        }

        private static List<XYZ> GetNewPoints(XYZ position, List<Visiting> movingVisitings)
        {
            var result = new List<XYZ>();
            var allVariants = new XYZ[3]
            {
                new XYZ(position.X + 1, position.Y, position.Z),
                new XYZ(position.X, position.Y + 1, position.Z),
                new XYZ(position.X + 1, position.Y + 1, position.Z)
            };

            foreach (var variant in allVariants)
            {
                try
                {
                    foreach (var visiting in movingVisitings)
                    {
                        foreach (var point in visiting.GetExtremePoints())
                        {
                            if (point.X == variant.X && point.Y == variant.Y)
                                break;

                            result.Add(variant);
                        }
                    }
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("EXCEPTION", $"STR-166 {ex.Message}");
                }
            }

            return result;
        }

        private static void ProcessVisitingsDistanceBorders(List<Visiting> visitings, Rectangle spaceRectangle)
        {
            for (var i = 0; i < visitings.Count; i++)
            { 
                if (visitings[i].Rectangle.minXminY.X != spaceRectangle.minXminY.X)
                {
                    if (!visitings.Any(v => spaceRectangle.minXminY.X <= v.Rectangle.minXminY.X && 
                        v.Rectangle.minXminY.X <= visitings[i].Rectangle.minXminY.X))
                    {
                        var newRectangle = new Rectangle(
                            new XYZ(spaceRectangle.minXminY.X, visitings[i].Rectangle.minXminY.Y,visitings[i].Rectangle.minXminY.Z),
                            visitings[i].Rectangle.maxXmaxY);
                        visitings[i] = DefineVisitingPoint(visitings[i], newRectangle.minXminY, newRectangle.WidthMeter, newRectangle.HeightMeter, 0);
                    }
                }
                if (visitings[i].Rectangle.maxXmaxY.X != spaceRectangle.maxXmaxY.X)
                {
                    if (!visitings.Any(v => visitings[i].Rectangle.maxXmaxY.X <= v.Rectangle.minXminY.X && 
                        v.Rectangle.minXminY.X <= spaceRectangle.maxXmaxY.X))
                    {
                        var newRectangle = new Rectangle(visitings[i].Rectangle.minXminY,
                            new XYZ(spaceRectangle.maxXmaxY.X, visitings[i].Rectangle.maxXmaxY.Y, visitings[i].Rectangle.maxXmaxY.Z));
                        visitings[i] = DefineVisitingPoint(visitings[i], newRectangle.minXminY, newRectangle.WidthMeter, newRectangle.HeightMeter, 0);
                    }
                }
                if (visitings[i].Rectangle.minXminY.Y != spaceRectangle.minXminY.Y)
                {
                    if (!visitings.Any(v => spaceRectangle.minXminY.Y <= v.Rectangle.minXminY.Y && 
                        v.Rectangle.minXminY.Y <= visitings[i].Rectangle.minXminY.Y))
                    {
                        var newRectangle = new Rectangle(new XYZ(visitings[i].Rectangle.minXminY.X, spaceRectangle.minXminY.Y,
                            visitings[i].Rectangle.minXminY.Z), visitings[i].Rectangle.maxXmaxY);
                        visitings[i] = DefineVisitingPoint(visitings[i], newRectangle.minXminY, newRectangle.WidthMeter, newRectangle.HeightMeter, 0);
                    }
                }
                if (visitings[i].Rectangle.maxXmaxY.Y != spaceRectangle.maxXmaxY.Y)
                {
                    if (!visitings.Any(v => visitings[i].Rectangle.maxXmaxY.Y <= v.Rectangle.minXminY.Y &&
                        v.Rectangle.minXminY.Y <= spaceRectangle.maxXmaxY.Y))
                    {
                        var newRectangle = new Rectangle(visitings[i].Rectangle.minXminY, 
                            new XYZ(visitings[i].Rectangle.maxXmaxY.X, spaceRectangle.maxXmaxY.Y, visitings[i].Rectangle.maxXmaxY.Z));
                        visitings[i] = DefineVisitingPoint(visitings[i], newRectangle.minXminY, newRectangle.WidthMeter, newRectangle.HeightMeter, 0);
                    }
                }
            }
        }

        private static void ProcessSizesPlacedVisitings(List<Visiting> visitings, Rectangle spaceRectangle)
        {
            for (var i = 0; i < visitings.Count; i++)
            {
                if (visitings[i].Rectangle.minXminY.X != spaceRectangle.minXminY.X)
                {
                    var needed = visitings.Where(v => spaceRectangle.minXminY.X <= v.Rectangle.minXminY.X &&
                        v.Rectangle.minXminY.X <= visitings[i].Rectangle.minXminY.X)
                        .Where(n => visitings[i].Rectangle.minXminY.X - n.Rectangle.maxXmaxY.X > 1)
                        .Where(n => CheckBoundsOnY(visitings[i].Rectangle, n.Rectangle))
                        .OrderBy(n => visitings[i].Rectangle.minXminY.X - n.Rectangle.maxXmaxY.X)
                        .FirstOrDefault();
                    if (needed != null)
                    {
                        var newRect = new Rectangle(
                            new XYZ(needed.Rectangle.maxXmaxY.X + 1, visitings[i].Rectangle.minXminY.Y, visitings[i].Rectangle.minXminY.Z),
                            visitings[i].Rectangle.maxXmaxY);

                        var newVisiting = DefineVisitingPoint(visitings[i], newRect.minXminY, newRect.WidthMeter, newRect.HeightMeter, 0);
                        newVisiting = ProcessingIntersections(newVisiting, newVisiting.Rectangle, visitings, spaceRectangle);
                        if (newVisiting != null)
                            visitings[i] = newVisiting;
                    }
                }
                if (visitings[i].Rectangle.maxXmaxY.X != spaceRectangle.maxXmaxY.X)
                {
                    var needed = visitings.Where(v => visitings[i].Rectangle.maxXmaxY.X <= v.Rectangle.minXminY.X &&
                        v.Rectangle.minXminY.X <= spaceRectangle.maxXmaxY.X)
                        .Where(n => n.Rectangle.minXminY.X - visitings[i].Rectangle.maxXmaxY.X > 1)
                        .Where(n => CheckBoundsOnY(visitings[i].Rectangle, n.Rectangle))
                        .OrderBy(n => n.Rectangle.minXminY.X - visitings[i].Rectangle.maxXmaxY.X)
                        .FirstOrDefault();
                    if (needed != null)
                    {
                        var newRect = new Rectangle(visitings[i].Rectangle.minXminY,
                        new XYZ(needed.Rectangle.minXminY.X - 1, visitings[i].Rectangle.maxXmaxY.Y, visitings[i].Rectangle.maxXmaxY.Z));
                        var newVisiting = DefineVisitingPoint(visitings[i], newRect.minXminY, newRect.WidthMeter, newRect.HeightMeter, 0);
                        newVisiting = ProcessingIntersections(newVisiting, newVisiting.Rectangle, visitings, spaceRectangle);
                        if (newVisiting != null)
                            visitings[i] = newVisiting;
                    }
                }
                if (visitings[i].Rectangle.minXminY.Y != spaceRectangle.minXminY.Y)
                {
                    var needed = visitings.Where(v => spaceRectangle.minXminY.Y <= v.Rectangle.minXminY.Y &&
                        v.Rectangle.minXminY.Y <= visitings[i].Rectangle.minXminY.Y)
                        .Where(n => visitings[i].Rectangle.minXminY.Y - n.Rectangle.maxXmaxY.Y > 1)
                        .Where(n => CheckBoundsOnX(visitings[i].Rectangle, n.Rectangle))
                        .OrderBy(n => visitings[i].Rectangle.minXminY.Y - n.Rectangle.maxXmaxY.Y)
                        .FirstOrDefault();
                    if (needed != null)
                    {
                        var newRect = new Rectangle(
                            new XYZ(visitings[i].Rectangle.minXminY.X, needed.Rectangle.maxXmaxY.Y + 1, visitings[i].Rectangle.maxXmaxY.Z),
                            visitings[i].Rectangle.minXminY);

                        var newVisiting = DefineVisitingPoint(visitings[i], newRect.minXminY, newRect.WidthMeter, newRect.HeightMeter, 0);
                        newVisiting = ProcessingIntersections(newVisiting, newVisiting.Rectangle, visitings, spaceRectangle);
                        if (newVisiting != null)
                            visitings[i] = newVisiting;
                    }
                }
                if (visitings[i].Rectangle.maxXmaxY.Y != spaceRectangle.maxXmaxY.Y)
                {
                    var needed = visitings.Where(v => visitings[i].Rectangle.maxXmaxY.Y <= v.Rectangle.minXminY.Y &&
                        v.Rectangle.minXminY.Y <= spaceRectangle.maxXmaxY.Y)
                        .Where(n => n.Rectangle.minXminY.Y - visitings[i].Rectangle.maxXmaxY.Y > 1)
                        .Where(n => CheckBoundsOnX(visitings[i].Rectangle, n.Rectangle))
                        .OrderBy(n => n.Rectangle.minXminY.Y - visitings[i].Rectangle.maxXmaxY.Y)
                        .FirstOrDefault();
                    if (needed != null)
                    {
                        var newRect = new Rectangle(visitings[i].Rectangle.minXminY,
                        new XYZ(visitings[i].Rectangle.maxXmaxY.X, needed.Rectangle.minXminY.Y - 1, visitings[i].Rectangle.maxXmaxY.Z));
                        var newVisiting = DefineVisitingPoint(visitings[i], newRect.minXminY, newRect.WidthMeter, newRect.HeightMeter, 0);
                        newVisiting = ProcessingIntersections(newVisiting, newVisiting.Rectangle, visitings, spaceRectangle);
                        if (newVisiting != null)
                            visitings[i] = newVisiting;
                    }
                }
            }
        }

        private static bool CheckBoundsOnY(Rectangle rectangle1, Rectangle rectangle2)
        {
            if (rectangle1.minXminY.Y == rectangle2.minXminY.Y || rectangle1.maxXmaxY.Y == rectangle2.maxXmaxY.Y)
                return true;
            else if (rectangle1.minXminY.Y > rectangle2.minXminY.Y)
                return rectangle1.minXminY.Y <= rectangle2.maxXmaxY.Y;
            else
                return rectangle1.maxXmaxY.Y >= rectangle2.minXminY.Y;
        }

        private static bool CheckBoundsOnX(Rectangle rectangle1, Rectangle rectangle2)
        {
            if (rectangle1.minXminY.X == rectangle2.minXminY.X || rectangle1.maxXmaxY.X == rectangle2.maxXmaxY.X)
                return true;
            else if (rectangle1.minXminY.X > rectangle2.minXminY.X)
                return rectangle1.minXminY.X <= rectangle2.maxXmaxY.X;
            else
                return rectangle1.maxXmaxY.X >= rectangle2.minXminY.X;
        }
    }
}
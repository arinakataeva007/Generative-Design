﻿using System;
using System.Collections.Generic;
using System.Linq;
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

        private static Visiting ProcessingIntersections(Visiting visiting, Rectangle visitingRectangle, List<Visiting> spacedVisitings, Rectangle spaceRectangle)
        {
            if (!spacedVisitings.Any(m => m.Rectangle.IntersectsWith(visiting.Rectangle)))
            {
                if (spaceRectangle.maxXmaxY.X - visitingRectangle.maxXmaxY.X < 3)
                {
                    var newPointX = spaceRectangle.maxXmaxY.X;
                    var newRectangle = new Rectangle(visitingRectangle.minXminY,
                        new XYZ(newPointX, visitingRectangle.minXminY.Y, visitingRectangle.minXminY.Z),
                        new XYZ(newPointX, visitingRectangle.maxXmaxY.Y, visitingRectangle.minXminY.Z), visitingRectangle.minXmaxY);

                    return DefineVisitingPoint(visiting, newRectangle.minXminY, newRectangle.WidthMeter,
                        newRectangle.HeightMeter, 0);
                }

                return visiting;
            }

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

        //private static Visiting DefineNewSizes(Visiting visiting)
        //{
        //    var newVisiting = visiting;



        //    return newVisiting;
        //}

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
                            new XYZ(spaceRectangle.minXminY.X, visitings[i].Rectangle.maxXmaxY.Y, visitings[i].Rectangle.maxXmaxY.Z));
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
                // везде добавить проврку на столкновения

                //if (visitings[i].Rectangle.minXminY.X != spaceRectangle.minXminY.X)
                //{
                //    // здесь нужно изменить сортировочку 
                //    var needed = visitings.Where(v => spaceRectangle.minXminY.X <= v.Rectangle.minXminY.X &&
                //        v.Rectangle.minXminY.X <= visitings[i].Rectangle.minXminY.X);
                //    var needed2 = needed.Where(n => n.Rectangle.maxXmaxY.X - visitings[i].Rectangle.minXminY.X > 1).ToArray();
                //    if (needed2.Length > 0)
                //    {
                //        var newRect = new Rectangle(visitings[i].Rectangle.minXminY,
                //        new XYZ(needed2[0].Rectangle.minXminY.X - 1, visitings[i].Rectangle.maxXmaxY.Y, visitings[i].Rectangle.maxXmaxY.Z));
                //        var newVisiting = DefineVisitingPoint(visitings[i], newRect.minXminY, newRect.WidthMeter, newRect.HeightMeter, 0);
                //        visitings[i] = ProcessingIntersections(newVisiting, newVisiting.Rectangle, visitings, spaceRectangle);
                //    }
                //}
                if (visitings[i].Rectangle.maxXmaxY.X != spaceRectangle.maxXmaxY.X)
                {
                    var needed = visitings.Where(v => visitings[i].Rectangle.maxXmaxY.X <= v.Rectangle.minXminY.X &&
                        v.Rectangle.minXminY.X <= spaceRectangle.maxXmaxY.X);
                    var needed2 = needed.Where(n => n.Rectangle.minXminY.X - visitings[i].Rectangle.maxXmaxY.X > 1).ToArray();
                    if (needed2.Length > 0)
                    {
                        var newRect = new Rectangle(visitings[i].Rectangle.minXminY,
                        new XYZ(needed2[0].Rectangle.minXminY.X - 1, visitings[i].Rectangle.maxXmaxY.Y, visitings[i].Rectangle.maxXmaxY.Z));
                        var newVisiting = DefineVisitingPoint(visitings[i], newRect.minXminY, newRect.WidthMeter, newRect.HeightMeter, 0);
                        visitings[i] = newVisiting;
                    }
                }
                //if (visitings[i].Rectangle.minXminY.Y != spaceRectangle.minXminY.Y)
                //{
                //}
                //if (visitings[i].Rectangle.maxXmaxY.Y != spaceRectangle.maxXmaxY.Y)
                //{
                //}
            }
        }
    }
}
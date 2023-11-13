using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitProject.Visitings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Windows.Controls;

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

            //var kitchen = new Kitchen(new XYZ(), widthMeter: 2.8, squareMeter: 10);
            //kitchen.RotatePerpendicular();
            //var hallway = new Hallway(new XYZ(), widthMeter: 1.2, squareMeter: 2.4);
            //var bathroom = new Bathroom(new XYZ(), widthMeter: 1.7, squareMeter: 4);
            //bathroom.RotatePerpendicular();
            //var livingRoom = new LivingRoom(new XYZ(), widthMeter: 3, squareMeter: 12);


            return MoveVisitings(sortVisitings, extremePointsSpace);
        }

        private static List<Visiting> GetVisitings(double square)
        {
            var visitings = new List<Visiting> { new Kitchen(), new Hallway(), new Bathroom() };

            if (square < 28)
                return visitings;
            else if (square < 44)
                return visitings;
            else if (square < 56)
                visitings.AddRange(new List<Visiting> { new LivingRoom(), new LivingRoom() });
            else if (square < 70)
                return new List<Visiting> { };
            else
                return new List<Visiting> { };

            return visitings;
        }

        private static List<Visiting> MoveVisitings(List<Visiting> visitings, List<XYZ> extremePointsSpace)
        {
            var currentX = extremePointsSpace[0].X;
            var currentY = extremePointsSpace[0].Y;
            var pointZ = extremePointsSpace[0].Z;
            var maxX = extremePointsSpace[2].X;
            var maxY = extremePointsSpace[2].Y;
            var spaceRectangle = new Rectangle(extremePointsSpace.ToArray());
            var movingVisitings = new List<Visiting>();
            var rectangles = new List<Rectangle>();
            var countVisitings = visitings.Count;
            var indexesRemovingElement = new List<int>();

            for (var i = 0; i < countVisitings; i++)
            {
                if (currentX + visitings[i].WidthFeet <= maxX && currentY + visitings[i].HeightFeet <= maxY)
                {
                    if (movingVisitings.Count == 0)
                    {
                        movingVisitings.Add(DefineVisitingPoint(visitings[i], currentX, currentY, pointZ));
                        rectangles.Add(movingVisitings[i].Rectangle);
                    }
                    else
                    {
                        var newVisiting = GetNewVisiting(visitings[i], movingVisitings, spaceRectangle, pointZ);
                        // Костыль Проверка на вывод
                        //TaskDialog.Show("NewVisiting", $"STR-82 {newVisiting}");
                        movingVisitings.Add(newVisiting);
                    }    
                }
            }

            TaskDialog.Show("PPP", $"{movingVisitings.Count}\n{movingVisitings[0]}\n{movingVisitings[1]}\n{movingVisitings[2].minPoint}");

            return movingVisitings;
        }

        private static Visiting GetNewVisiting(Visiting visiting, List<Visiting> movingVisitings, Rectangle spaceRectangle, double pointZ)
        {
            for (var j = 0; j < movingVisitings.Count; j++)
            {
                try
                {
                    var extremePoints = movingVisitings[j].GetExtremePoints().Skip(1);

                    foreach (var point in extremePoints)
                    {
                        var newPoints = GetNewPoints(point, movingVisitings);

                        foreach (var newPoint in newPoints)
                        {
                            var newVisiting = DefineVisitingPoint(visiting, newPoint.X, newPoint.Y, pointZ);

                            // Костыль - проверка на вхождение одного прямоугольника в другой
                            if (newVisiting is Bathroom bathroom)
                            {
                                if (spaceRectangle.ContainsRectangle(newVisiting.Rectangle))
                                    TaskDialog.Show("BathRoom", $"{spaceRectangle.ContainsRectangle(newVisiting.Rectangle)}" +
                                        $"{newVisiting.Rectangle}\n{spaceRectangle}");
                            }

                            if (spaceRectangle.ContainsRectangle(newVisiting.Rectangle))
                            {
                                var isNotIntersect = true;

                                foreach (var movingVisiting in movingVisitings)
                                {
                                    foreach (var p in movingVisiting.GetExtremePoints())
                                    {
                                        // Костыль
                                        //TaskDialog.Show("STR-122", $"{newVisiting.minPoint}\n{p}");
                                        if (newVisiting.minPoint.X == p.X && newVisiting.minPoint.Y == p.Y)
                                        {
                                            isNotIntersect = false;
                                            break;
                                        }
                                    }
                                    // Костыль проверка через Rectangle и IntersectsWith
                                    TaskDialog.Show("Intersect", $"{newVisiting.Rectangle.IntersectsWith(movingVisiting.Rectangle)}" +
                                        $"\n{newVisiting.Rectangle}\n{movingVisiting.Rectangle}");
                                    if (movingVisiting.Rectangle.IntersectsWith(newVisiting.Rectangle))
                                    {
                                        isNotIntersect = false;
                                        break;
                                    }
                                    if (!isNotIntersect)
                                        break;
                                }
                                if (isNotIntersect)
                                    return newVisiting;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("EXCEPTION", $"STR-97 {ex.Message}");
                }
                
            }

            TaskDialog.Show("PUm", $"STR-120 OUCH {visiting}");

            return visiting;
        }

        private static Visiting DefineVisitingPoint(Visiting visiting, double pointX, double pointY, double pointZ)
        {
            Visiting newVisiting;


            if (visiting is Kitchen)
                newVisiting = new Kitchen(new XYZ(pointX, pointY, pointZ), visiting.WidthMeter, visiting.HeightMeter, visiting.SquareMeter);
            else if (visiting is Hallway)
                newVisiting = new Hallway(new XYZ(pointX, pointY, pointZ), visiting.WidthMeter, visiting.HeightMeter, visiting.SquareMeter);
            else if (visiting is Bathroom)
                newVisiting = new Bathroom(new XYZ(pointX, pointY, pointZ), visiting.WidthMeter, visiting.HeightMeter, visiting.SquareMeter);
            else
                newVisiting = new LivingRoom(new XYZ(pointX, pointY, pointZ), visiting.WidthMeter, visiting.HeightMeter, visiting.SquareMeter);

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
                        //if (visiting.GetExtremePoints().Contains(variant))
                        //    break;
                        //result.Add(variant);
                    }
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("EXCEPTION", $"STR-166 {ex.Message}");
                }
            }

            return result;
        }

        private static Visiting DefineNewSizes(Visiting visiting)
        {
            var newVisiting = visiting;



            return newVisiting;
        }
    }
}




//while (true)
//{
//    for (var i=0; i < visitings.Count; i++)
//    {
//        if ((currentX + visitings[i].WidthFeet > maxX && currentY + visitings[i].HeightFeet <= maxY) ||
//            (currentX + visitings[i].WidthFeet <= maxX && currentY + visitings[i].HeightFeet > maxY))
//        {
//            visitings[i].RotatePerpendicular();
//            if (currentX + visitings[i].WidthFeet <= maxX && currentY + visitings[i].HeightFeet <= maxY)
//            {
//                visitings[i] = DefineVisitingPoint(visitings[i], ref currentX, ref currentY, pointZ);
//                TaskDialog.Show("VISIting", $"{visitings[i]}");
//            }
//            else
//            {
//                visitings[i] = DefineVisitingPoint(visitings[i], ref currentX, ref currentY, pointZ);
//                TaskDialog.Show("VISIting", $"{visitings[i]}");
//            }
//        }
//        else if (currentX + visitings[i].WidthFeet <= maxX && currentY + visitings[i].HeightFeet <= maxY)
//            visitings[i] = DefineVisitingPoint(visitings[i], ref currentX, ref currentY, pointZ);
//        else
//            visitings[i] = DefineVisitingPoint(visitings[i], ref currentX, ref currentY, pointZ);

//    }

//    return visitings;
//}





//for (var y = currentY; y <= maxY;)
//{
//    var pointsY = new List<double>();

//    for (var x = currentX; x <= maxX;)
//    {
//        for (var i = 0; i < visitings.Count; i++)
//        {
//            if (x + visitings[i].WidthFeet <= maxX)
//            {
//                if (y + visitings[i].HeightFeet <= maxY)
//                {
//                    movingVisitings.Add(DefineVisitingPoint(visitings[i], ref x, y, pointZ));
//                    pointsY.Add(y + visitings[i].HeightFeet);
//                }
//                else
//                {
//                    visitings[i].RotatePerpendicular();
//                    if (x + visitings[i].WidthFeet <= maxX)
//                    {
//                        movingVisitings.Add(DefineVisitingPoint(visitings[i], ref x, y, pointZ));
//                        pointsY.Add(y + visitings[i].HeightFeet);
//                    }
//                    else
//                        continue;
//                }
//            }
//            else
//                continue;
//        }

//        if (x != maxX)
//            break;
//    }

//    y = pointsY.Max();
//}
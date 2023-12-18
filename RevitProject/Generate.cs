using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitProject
{
    public class Generate 
    {
        private const double WidthOuterWall = 0.82; // в футах (250мм) 
        private const double WidthInnerWall = 0.49; // в футах (150мм)

        public static List<List<Room>> GetShapes(ContourFlat2D contourFlat)
        {
            //Пока только для прямоугольника, квадрата

            var contourWithoutWalls = GetContourWithoutWalls(contourFlat);
            var contourRectangle = (Rectangle2D)contourWithoutWalls.GeometricShape;
            var height = contourRectangle.Width.LengthOnMeter;
            var width = contourRectangle.Height.LengthOnMeter;
            var squareSpace = height * width;
            var rooms = GetRooms(squareSpace);
            var sortRooms = rooms.OrderByDescending(x => x.SquareMeter).ToList();

            return MoveRooms(sortRooms, contourWithoutWalls);
        }

        private static Rectangle2D GetContourRectangleWithoutWalls(XYZ[] extremePointsContur)
        {
            extremePointsContur[0] += new XYZ(WidthOuterWall, WidthOuterWall, 0);
            extremePointsContur[1] += new XYZ(-WidthOuterWall, WidthOuterWall, 0);
            extremePointsContur[2] += new XYZ(-WidthOuterWall, -WidthOuterWall, 0);
            extremePointsContur[3] += new XYZ(WidthOuterWall, -WidthOuterWall, 0);

            return new Rectangle2D(extremePointsContur);
        }

        private static ContourFlat2D GetContourWithoutWalls(ContourFlat2D contourFlat)
        {
            var minX = contourFlat.GeometricShape.ExtremePoints.Min(p => p.X);
            var minY = contourFlat.GeometricShape.ExtremePoints.Min(p => p.Y);
            var maxX = contourFlat.GeometricShape.ExtremePoints.Max(p => p.X);
            var maxY = contourFlat.GeometricShape.ExtremePoints.Max(p => p.Y);
            var points = contourFlat.GeometricShape.ExtremePoints;
            var newExtremePoints = new XYZ[points.Length];

            for (var i = 0; i < points.Length; i++)
            {
                for (var x = -WidthOuterWall; x <= WidthOuterWall; x += WidthOuterWall)
                {
                    for (var y = -WidthOuterWall; y <= WidthOuterWall; y += WidthOuterWall)
                    {
                        if (x == 0 || y == 0) continue;

                        if (contourFlat.GeometricShape.Contains(points[i] + new XYZ(x, y, 0)) &&
                            (points[i].X == minX || points[i].X == maxX || points[i].Y == minY || points[i].Y == maxY))
                            newExtremePoints[i] = points[i] + new XYZ(x, y, 0);
                        if (!contourFlat.GeometricShape.Contains(points[i] + new XYZ(x, y, 0)) && points[i].X != minX && points[i].X != maxX
                            && points[i].Y != minY && points[i].Y != maxY)
                            newExtremePoints[i] = points[i] - new XYZ(x, y, 0);
                    }
                }
            }

            var newSideWithDoor = GetSideWithoutWalls(contourFlat.SideWithDoor, points, newExtremePoints);
            var newSideWithWindow = GetSideWithoutWalls(contourFlat.SideWithWindow, points, newExtremePoints);

            return new ContourFlat2D(new Rectangle2D(newExtremePoints), newSideWithDoor, newSideWithWindow); 
        }

        private static Side2D GetSideWithoutWalls(Side2D side, XYZ[] points, XYZ[] pointsWithoutWalls)
        {
            var newPointMin = new XYZ();
            var newPointMax = new XYZ();

            for (var i = 0; i < points.Length; i++)
            {
                if (side.pointMin == points[i])
                    newPointMin = pointsWithoutWalls[i];
                else if (side.pointMax == points[i])
                    newPointMax = pointsWithoutWalls[i];
            }   
            
            return new Side2D(newPointMin, newPointMax);
        }

        private static List<Room> GetRooms(double square)
        {
            var rooms = new List<Room> { new Kitchen(), new Hallway(), new Bathroom() };

            if (square < 28) // студия
                return rooms;
            else if (square < 44) // однокомнатная
                rooms.Add(new LivingRoom());
            else if (square < 56) // двушка
                rooms.AddRange(new List<Room> { new LivingRoom(), new LivingRoom() });
            else if (square < 70) // трёшка
                rooms.AddRange(new List<Room> { new LivingRoom(), new LivingRoom(), new LivingRoom() });
            else
                return new List<Room> { };

            return rooms;
        }

        private static List<List<Room>> MoveRooms(List<Room> rooms, ContourFlat2D contourFlat)
        {
            var contourRectangle = (Rectangle2D)contourFlat.GeometricShape;
            var variants = GetVariantsFlats(rooms, contourFlat);
            var result = new List<List<Room>>();

            for (var i = 0; i < variants.Count; i++)
            {
                for (var j = 0; j < variants[i].Count; j++)
                {
                    ProcessRoomsDistanceBorders(variants[i], j, contourRectangle);
                    ProcessSizesPlacedRooms(variants[i], j, contourRectangle);
                }

                if (CheckCompilance(result, variants[i], contourFlat))
                    result.Add(variants[i]);
            }

            TaskDialog.Show("Check Variants", $"{result.Count}");

            return result;

            //TaskDialog.Show("Check Variants", $"{variants.Count}");

            //return variants;
        }

        private static bool CheckCompilance(List<List<Room>> flatVariants, List<Room> rooms, ContourFlat2D contourFlat)
        {
            if (rooms.Any(r => r == null) || 
                !rooms.All(r => r.IsCorectPositionRelativeWalls(contourFlat.SideWithDoor, contourFlat.SideWithWindow)))
                return false;


            var previousSimilar = true;

            for (var i = flatVariants.Count - 1; i >= 0; i--)
            {
                for (var j = 0; j < flatVariants[i].Count; j++)
                {
                    if (rooms[j] != null && flatVariants[i][j].Name == rooms[j].Name)
                    {
                        if (flatVariants[i][j].Rectangle.Contains(rooms[j].Rectangle))
                            previousSimilar = true;
                        else
                            previousSimilar = false;
                    }
                }

                if (previousSimilar) return false;
            }

            return true;
        }

        private static List<List<Room>> GetVariantsFlats(List<Room> rooms, ContourFlat2D contourFlat)
        {
            var result = new List<List<Room>>();

            for (var i = 0; i < rooms.Count; i++)
            {
                if (result.Count == 0)
                {
                    FillRoom(result, new List<Room>(), rooms[i], contourFlat, rooms.Count(r => r.Name == rooms[i].Name));
                    continue;
                }
                for (var j = 0; j < result.Count; j++)
                    FillRoom(result, result[j], rooms[i], contourFlat, rooms.Count(r => r.Name == rooms[i].Name));

                result = result.Where(r => r.Count == i + 1).ToList();
            }

            return result.Where(v => v.Count >= rooms.Count-1).ToList();
        }

        private static void FillRoom(List<List<Room>> roomVariants, List<Room> workVariant, Room room, ContourFlat2D contourFlat, 
            int countRoomInVariant)
        {
            var contourRectangle = (Rectangle2D)contourFlat.GeometricShape;
            var possiblePosition = GetPossiblePosition(workVariant, contourRectangle, room);

            foreach (var position in possiblePosition)
            {
                var newRoom = room.CreateNew(new Rectangle2D(position, room.WidthFeet, room.HeightFeet));

                if (contourRectangle.Contains(newRoom.Rectangle))
                {
                    newRoom = ProcessingIntersections(newRoom, workVariant);
                    if (newRoom != null && workVariant.Count(r => r.Name == newRoom.Name) < countRoomInVariant) 
                    {
                        var newVariant = new List<Room>();
                        newVariant.AddRange(workVariant.ToArray());
                        newVariant.Add(newRoom);
                        roomVariants.Add(newVariant);
                    }
                }
            }
        }

        private static List<XYZ> GetPossiblePosition(List<Room> rooms, Rectangle2D contourRectangle, Room room)
        {
            var result = new List<XYZ>();
            result.AddRange(GetPossiblePositionsContour(room, contourRectangle));
            
            for (var i = 0; i < rooms.Count; i++)
            {
                result.AddRange(GetPossiblePositionsRoom(rooms[i].Rectangle, room));
                //var extremePoints = rooms[i].Rectangle.ExtremePoints;

                //foreach (var point in extremePoints)
                //{
                //    for (var x = -WidthInnerWall; x <= WidthInnerWall; x += WidthInnerWall)
                //    {
                //        for (var y = -WidthInnerWall; y <= WidthInnerWall; y += WidthInnerWall)
                //        {
                //            var newPoint = point + new XYZ(x, y, 0);


                //        }
                //    }
                //}
            }

            return result;
        }

        /// <summary>
        /// Возврат возможных позици относительно крайних точек контура помещения
        /// </summary>
        /// <param name="room">Комната для которой создаются точки</param>
        /// <param name="contourRectangle">Контур помещения</param>
        /// <returns></returns>
        private static List<XYZ> GetPossiblePositionsContour(Room room, Rectangle2D contourRectangle)
        {
            return new List<XYZ>()
            {
                contourRectangle.MinXminY,
                contourRectangle.MaxXminY - new XYZ(room.WidthFeet, 0, 0),
                new XYZ(contourRectangle.MaxXmaxY.X - room.WidthFeet, contourRectangle.MaxXmaxY.Y - room.HeightFeet, contourRectangle.MinXminY.Z),
                contourRectangle.MinXmaxY - new XYZ(0, room.WidthFeet, 0),
            };
            
        }

        private static List<XYZ> GetPossiblePositionsRoom(Rectangle2D rectangleInContur, Room room)
        {
            var result = new HashSet<XYZ>();

            var newPoints = new List<XYZ>()
            {
                rectangleInContur.MinXminY + new XYZ(-WidthInnerWall, -WidthInnerWall, 0),
                rectangleInContur.MinXminY + new XYZ(-WidthInnerWall, 0, 0),
                rectangleInContur.MinXminY + new XYZ(0, -WidthInnerWall, 0),
                rectangleInContur.MaxXminY + new XYZ(WidthInnerWall, -WidthInnerWall, 0),
                rectangleInContur.MaxXminY + new XYZ(WidthInnerWall, 0, 0),
                rectangleInContur.MaxXminY + new XYZ(0, -WidthInnerWall, 0),
                rectangleInContur.MaxXmaxY + new XYZ(WidthInnerWall, WidthInnerWall, 0),
                rectangleInContur.MaxXmaxY + new XYZ(WidthInnerWall, 0, 0),
                rectangleInContur.MaxXmaxY + new XYZ(0, WidthInnerWall, 0),
                rectangleInContur.MinXmaxY + new XYZ(-WidthInnerWall, WidthInnerWall, 0),
                rectangleInContur.MinXmaxY + new XYZ(0, WidthInnerWall, 0),
                rectangleInContur.MinXmaxY + new XYZ(-WidthInnerWall, 0, 0)
            };

            foreach (var point in newPoints)
            {
                for (var i = -room.WidthFeet; i <= room.WidthFeet; i += room.WidthFeet)
                {
                    for (var j = -room.HeightFeet; j <= room.HeightFeet; j += room.HeightFeet)
                    {
                        var newPosition1 = point + new XYZ(i, j, 0);

                        if (!result.Contains(newPosition1))
                            result.Add(newPosition1);
                    }
                }
            }

            return result.ToList();
        }

        private static bool CheckIntersections(Room room, List<Room> spacedRooms)
        {
            var intersectionRooms = spacedRooms.Where(m => m.Rectangle.IntersectsWith(room.Rectangle)).ToList();

            if (intersectionRooms.Count == 0) return false;
            else if (intersectionRooms.Count == 1)
            {
                if (intersectionRooms[0].Name == room.Name && (room.Rectangle.Contains(intersectionRooms[0].Rectangle) ||
                    intersectionRooms[0].Rectangle.Contains(room.Rectangle)))
                    return false;
            }

            return true;
        }

        private static Room ProcessingIntersections(Room room, List<Room> spacedRooms)
        {
            if (!CheckIntersections(room, spacedRooms))
                return room;

            return null;
        }

        private static void ProcessRoomsDistanceBorders(List<Room> rooms, int index, Rectangle2D contourRectangle)
        {
            if (!rooms.Any(r => r == null) && rooms[index].Rectangle.MinXminY.X != contourRectangle.MinXminY.X)
            {
                var rect = new Rectangle2D(new XYZ(contourRectangle.MinXminY.X, rooms[index].Rectangle.MinXminY.Y, rooms[index].Rectangle.MinXminY.Z),
                    rooms[index].Rectangle.MinXmaxY - new XYZ(0.1, 0, 0));

                if (!CheckRoomsOnRectangle(rooms, rect))
                    rooms[index] = rooms[index].CreateNew( 
                        new XYZ(contourRectangle.MinXminY.X, rooms[index].Rectangle.MinXminY.Y, rooms[index].Rectangle.MinXminY.Z),
                        rooms[index].Rectangle.MaxXmaxY);
            }
            if (!rooms.Any(r => r == null) && rooms[index].Rectangle.MaxXmaxY.X != contourRectangle.MaxXmaxY.X)
            {
                var rect = new Rectangle2D(rooms[index].Rectangle.MaxXminY + new XYZ(0.1, 0, 0),
                    new XYZ(contourRectangle.MaxXmaxY.X, rooms[index].Rectangle.MaxXmaxY.Y, rooms[index].Rectangle.MaxXmaxY.Z));

                if (!CheckRoomsOnRectangle(rooms, rect))
                    rooms[index] = rooms[index].CreateNew(rooms[index].Rectangle.MinXminY,
                        new XYZ(contourRectangle.MaxXmaxY.X, rooms[index].Rectangle.MaxXmaxY.Y, rooms[index].Rectangle.MaxXmaxY.Z));
            }
            if (!rooms.Any(r => r == null) && rooms[index].Rectangle.MinXminY.Y != contourRectangle.MinXminY.Y)
            {
                var rect = new Rectangle2D(new XYZ(rooms[index].Rectangle.MinXminY.X, contourRectangle.MinXminY.Y, rooms[index].Rectangle.MaxXmaxY.Z),
                    rooms[index].Rectangle.MaxXminY - new XYZ(0, 0.1, 0));

                if (!CheckRoomsOnRectangle(rooms, rect))
                    rooms[index] = rooms[index].CreateNew(new XYZ(rooms[index].Rectangle.MinXminY.X, contourRectangle.MinXminY.Y,
                        rooms[index].Rectangle.MinXminY.Z), rooms[index].Rectangle.MaxXmaxY);
            }
            if (!rooms.Any(r => r == null) && rooms[index].Rectangle.MaxXmaxY.Y != contourRectangle.MaxXmaxY.Y)
            {
                var rect = new Rectangle2D(rooms[index].Rectangle.MinXmaxY + new XYZ(0, 0.1, 0), 
                    new XYZ(rooms[index].Rectangle.MaxXmaxY.X, contourRectangle.MaxXmaxY.Y, rooms[index].Rectangle.MaxXmaxY.Z));

                if (!CheckRoomsOnRectangle(rooms, rect))
                    rooms[index] = rooms[index].CreateNew(rooms[index].Rectangle.MinXminY,
                        new XYZ(rooms[index].Rectangle.MaxXmaxY.X, contourRectangle.MaxXmaxY.Y, rooms[index].Rectangle.MaxXmaxY.Z));
            }
        }

        private static bool CheckRoomsOnRectangle(List<Room> rooms, Rectangle2D rectangle)
        {
            return rooms.Any(v => v.Rectangle.IntersectsWith(rectangle));
        }

        private static void ProcessSizesPlacedRooms(List<Room> rooms, int index, Rectangle2D contourRectangle)
        {
            if (!rooms.Any(r => r == null) && rooms[index].Rectangle.MinXminY.X != contourRectangle.MinXminY.X)
            {
                var nearestRoom = GetNearestRoomAxisX(rooms[index], rooms, contourRectangle.MinXminY.X, rooms[index].Rectangle.MinXminY.X);
                if (nearestRoom != null)
                    if (rooms[index].Rectangle.MinXminY.X - nearestRoom.Rectangle.MaxXmaxY.X >= WidthInnerWall)
                        rooms[index] = ResizeRoom(rooms[index], rooms, contourRectangle,
                            new XYZ(nearestRoom.Rectangle.MaxXmaxY.X + WidthInnerWall, rooms[index].Rectangle.MinXminY.Y, rooms[index].Rectangle.MinXminY.Z),
                            rooms[index].Rectangle.MaxXmaxY);
                    else
                        ReduceRoomHeight(rooms, nearestRoom, rooms[index], contourRectangle);
            }
            if (!rooms.Any(r => r == null) && rooms[index].Rectangle.MaxXmaxY.X != contourRectangle.MaxXmaxY.X)
            {
                var nearestRoom = GetNearestRoomAxisX(rooms[index], rooms, rooms[index].Rectangle.MaxXmaxY.X, contourRectangle.MaxXmaxY.X);
                if (nearestRoom != null)
                    if (nearestRoom.Rectangle.MinXminY.X - rooms[index].Rectangle.MaxXmaxY.X >= WidthInnerWall)
                        rooms[index] = ResizeRoom(rooms[index], rooms, contourRectangle, rooms[index].Rectangle.MinXminY,
                            new XYZ(nearestRoom.Rectangle.MinXminY.X - WidthInnerWall, rooms[index].Rectangle.MaxXmaxY.Y, rooms[index].Rectangle.MaxXmaxY.Z));
                    else
                        ReduceRoomHeight(rooms, rooms[index], nearestRoom, contourRectangle);
            }
            if (!rooms.Any(r => r == null) && rooms[index].Rectangle.MinXminY.Y != contourRectangle.MinXminY.Y)
            {
                var nearestRoom = GetNearestRoomAxisY(rooms[index], rooms, contourRectangle.MinXminY.Y, rooms[index].Rectangle.MinXminY.Y);
                if (nearestRoom != null)
                    if (rooms[index].Rectangle.MinXminY.Y - nearestRoom.Rectangle.MaxXmaxY.Y >= WidthInnerWall)
                        rooms[index] = ResizeRoom(rooms[index], rooms, contourRectangle,
                            new XYZ(rooms[index].Rectangle.MinXminY.X, nearestRoom.Rectangle.MaxXmaxY.Y + WidthInnerWall, rooms[index].Rectangle.MinXminY.Z),
                            rooms[index].Rectangle.MaxXmaxY);
                    else
                        ReduceRoomWidth(rooms, nearestRoom, rooms[index], contourRectangle);
            }
            if (!rooms.Any(r => r == null) && rooms[index].Rectangle.MaxXmaxY.Y != contourRectangle.MaxXmaxY.Y)
            {
                var nearestRoom = GetNearestRoomAxisY(rooms[index], rooms, rooms[index].Rectangle.MaxXmaxY.Y, contourRectangle.MaxXmaxY.Y);
                if (nearestRoom != null)
                    if (nearestRoom.Rectangle.MinXminY.Y - rooms[index].Rectangle.MaxXmaxY.Y >= WidthInnerWall)
                        rooms[index] = ResizeRoom(rooms[index], rooms, contourRectangle, rooms[index].Rectangle.MinXminY,
                            new XYZ(rooms[index].Rectangle.MaxXmaxY.X, nearestRoom.Rectangle.MinXminY.Y - WidthInnerWall, rooms[index].Rectangle.MaxXmaxY.Z));
                    else
                        ReduceRoomWidth(rooms, rooms[index], nearestRoom, contourRectangle);
            }
        }

        private static Room GetNearestRoomAxisX(Room room, List<Room> rooms, double minX, double maxX)
        {
            return rooms.Where(v => minX <= v.Rectangle.MinXminY.X && v.Rectangle.MinXminY.X <= maxX)
                .Where(v => v.Name != room.Name && v.Rectangle.MinXminY != room.Rectangle.MinXminY)
                .Where(v => CheckBoundsOnY(room.Rectangle, v.Rectangle))
                .OrderBy(v =>
                {
                    if (room.Rectangle.MinXminY.X == maxX)
                        return maxX - v.Rectangle.MaxXmaxY.X;
                    return v.Rectangle.MinXminY.X - minX;
                })
                .FirstOrDefault();
        }

        private static Room GetNearestRoomAxisY(Room room, List<Room> rooms, double minY, double maxY)
        {
            return rooms.Where(v => minY <= v.Rectangle.MinXminY.Y && v.Rectangle.MinXminY.Y <= maxY)
                .Where(v => v.Name != room.Name && v.Rectangle.MinXminY != room.Rectangle.MinXminY)
                .Where(v => CheckBoundsOnX(room.Rectangle, v.Rectangle))
                .OrderBy(v =>
                {
                    if (room.Rectangle.MinXminY.Y == maxY)
                        return maxY - v.Rectangle.MaxXmaxY.Y;
                    return v.Rectangle.MinXminY.Y - minY;
                })
                .FirstOrDefault();
        }

        private static bool CheckBoundsOnY(Rectangle2D rectangle1, Rectangle2D rectangle2)
        {
            if (rectangle1.MinXminY.Y == rectangle2.MinXminY.Y || rectangle1.MaxXmaxY.Y == rectangle2.MaxXmaxY.Y)
                return true;
            else if (rectangle1.MinXminY.Y > rectangle2.MinXminY.Y)
                return rectangle1.MinXminY.Y <= rectangle2.MaxXmaxY.Y;
            else
                return rectangle1.MaxXmaxY.Y >= rectangle2.MinXminY.Y;
        }

        private static bool CheckBoundsOnX(Rectangle2D rectangle1, Rectangle2D rectangle2)
        {
            if (rectangle1.MinXminY.X == rectangle2.MinXminY.X || rectangle1.MaxXmaxY.X == rectangle2.MaxXmaxY.X)
                return true;
            else if (rectangle1.MinXminY.X > rectangle2.MinXminY.X)
                return rectangle1.MinXminY.X <= rectangle2.MaxXmaxY.X;
            else
                return rectangle1.MaxXmaxY.X >= rectangle2.MinXminY.X;
        }

        //Подумать про передачу сторон
        private static Room ResizeRoom(Room room, List<Room> rooms, Rectangle2D contourRectangle, XYZ pointMin, XYZ pointMax)
        {
            var newVisiting = room.CreateNew(new Rectangle2D(pointMin, pointMax));
            newVisiting = ProcessingIntersections(newVisiting, rooms);
            if (newVisiting != null)
                room = newVisiting;

            return room;
        }

        private static void ReduceRoomHeight(List<Room> rooms, Room roomLeft, Room roomRight, Rectangle2D contourRectangle)
        {
            var sizeReduction = WidthInnerWall - (roomRight.Rectangle.MinXminY.X - roomLeft.Rectangle.MaxXmaxY.X);
            var indexLeft = rooms.IndexOf(roomLeft);
            var indexRight = rooms.IndexOf(roomRight);

            if (roomLeft.CanReduceHeightBy(sizeReduction))
                rooms[indexLeft] = ResizeRoom(roomLeft, rooms, contourRectangle, roomLeft.Rectangle.MinXminY,
                    roomLeft.Rectangle.MaxXmaxY - new XYZ(sizeReduction, 0, 0));
            else if (roomRight.CanReduceHeightBy(sizeReduction))
                rooms[indexRight] = ResizeRoom(roomRight, rooms, contourRectangle, roomRight.Rectangle.MinXminY + new XYZ(sizeReduction, 0, 0),
                    roomRight.Rectangle.MaxXmaxY);
            else
            {
                rooms[indexLeft] = null;
                rooms[indexRight] = null;
            }
        }

        private static void ReduceRoomWidth(List<Room> rooms, Room roomBottom, Room roomTop, Rectangle2D contourRectangle)
        {
            var sizeReduction = WidthInnerWall - (roomTop.Rectangle.MinXminY.Y - roomBottom.Rectangle.MaxXmaxY.Y);
            var indexBottom = rooms.IndexOf(roomBottom);
            var indexTop = rooms.IndexOf(roomTop);

            if (roomBottom.CanReduceWidthBy(sizeReduction))
                rooms[indexBottom] = ResizeRoom(roomBottom, rooms, contourRectangle, roomBottom.Rectangle.MinXminY,
                    roomBottom.Rectangle.MaxXmaxY - new XYZ(0, sizeReduction, 0));
            else if (roomTop.CanReduceWidthBy(sizeReduction))
                rooms[indexTop] = ResizeRoom(roomTop, rooms, contourRectangle, roomTop.Rectangle.MinXminY + new XYZ(0, sizeReduction, 0),
                    roomTop.Rectangle.MaxXmaxY);
            else
            {
                rooms[indexBottom] = null;
                rooms[indexTop] = null;
            }
        }
    }
}








//Один из способов нескольких вариантов, не работает

//private static List<List<Room>> GetVariants(Room room, List<List<Room>> movingRooms, Rectangle spaceRectangle)
//{
//    var result = new List<List<Room>>();

//    for (var i = 0; i < movingRooms.Count; i++)
//    {
//        var newPositions = GetNewPositions(movingRooms[i], spaceRectangle);

//        foreach (var position in newPositions)
//        {
//            var newRoom = Room.CreateNewRoom(room, position, room.WidthMeter, room.HeightMeter, 0);

//            if (spaceRectangle.ContainsRectangle(newRoom.Rectangle))
//                newRoom = ProcessingIntersections(newRoom, newRoom.Rectangle, movingRooms[i], spaceRectangle);
//            else
//            {
//                var intersectRectangle = spaceRectangle.GetIntersectionRectangle(newRoom.Rectangle);
//                if (intersectRectangle.SquareMeter < room.SquareMeter)
//                    continue;

//                newRoom = Room.CreateNewRoom(newRoom, intersectRectangle);

//                newRoom = ProcessingIntersections(newRoom, newRoom.Rectangle, movingRooms[i], spaceRectangle);
//            }
//            if (newRoom != null)
//            {
//                var newVariant = new List<Room>();
//                newVariant.AddRange(movingRooms[i].ToArray());
//                newVariant.Add(newRoom);

//                result.Add(newVariant);
//            }
//        }
//    }

//    return result;
//}

//private static List<XYZ> GetNewPositions(List<Room> rooms, Rectangle spaceRectangle)
//{
//    var result = new List<XYZ>();

//    for (var i = 0; i < rooms.Count; i++)
//    {
//        var extremePoints = rooms[i].GetExtremePoints();

//        foreach (var point in extremePoints)
//        {
//            for (var x = -1; x <= 1; x++)
//            {
//                for (var y = -1; y <= 1; y++)
//                {
//                    var newPoint = point + new XYZ(x, y, 0);
//                    if (spaceRectangle.ContainsPoint(newPoint) && !rooms.Any(r => r.Rectangle.ContainsPoint(newPoint)))
//                        result.Add(newPoint);
//                }
//            }
//        }
//    }

//    return result;
//}

// Окончание первого способа



// По одному варианту
//var spaceMinX = extremePointsSpace[0].X;
//var spaceMinY = extremePointsSpace[0].Y;
//var pointZ = extremePointsSpace[0].Z;
//var maxX = extremePointsSpace[2].X;
//var maxY = extremePointsSpace[2].Y;
//var movingRooms = new List<Room>();
//var countRooms = rooms.Count;
//var variantsMoving = new List<List<Room>>();

//for (var i = 0; i < countRooms; i++)
//{
//    if (spaceMinX + rooms[i].WidthFeet <= maxX && spaceMinY + rooms[i].HeightFeet <= maxY)
//    {
//        if (movingRooms.Count == 0)
//        {
//            var newRoom = Room.CreateNewRoom(rooms[i], new XYZ(spaceMinX, spaceMinY, pointZ),
//                rooms[i].WidthMeter, rooms[i].HeightMeter, rooms[i].SquareMeter);
//            movingRooms.Add(newRoom);
//            variantsMoving.Add(new List<Room> { newRoom });
//        }
//        else
//        {
//            //var newRoom = GetNewRooms(rooms[i], movingRooms, spaceRectangle);
//            variantsMoving = GetVariants(rooms[i], variantsMoving, spaceRectangle);
//            //TaskDialog.Show("70 Generate", $"{variantsMoving[0].Count}");
//            //if (newRoom != null)
//            //    movingRooms.Add(newRoom);
//        }
//    }
//}
using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

namespace RevitProject
{
    public class Generate
    {
        public static List<List<Room>> GetShapes(List<double> sides, List<XYZ> extremePointsSpace)
        {
            //Пока только для прямоугольника, квадрата
            var minPointSpace = extremePointsSpace[0];

            var height = sides[0];
            var width = sides[1];
            var squareSpace = height * width;
            var rooms = GetRooms(squareSpace);
            var sortRooms = rooms.OrderByDescending(x => x.SquareMeter).ToList();

            return MoveRooms(sortRooms, extremePointsSpace);
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
                return new List<Room> { };
            else
                return new List<Room> { };

            return rooms;
        }

        private static List<List<Room>> MoveRooms(List<Room> rooms, List<XYZ> extremePointsSpace)
        {
            var spaceRectangle = new Rectangle(extremePointsSpace.ToArray());
            var variants = GetVariantsFlats(rooms, spaceRectangle);
            var result = new List<List<Room>>();

            for (var i = 0; i < variants.Count; i++)
            {
                for (var j = 0; j < variants[i].Count; j++)
                {
                    ProcessRoomsDistanceBorders(variants[i], j, spaceRectangle);
                    ProcessSizesPlacedRooms(variants[i], j, spaceRectangle);
                }

                if (CheckCompilance(result, variants[i]))
                    result.Add(variants[i]);
            }

            TaskDialog.Show("Check Variants", $"{result.Count}");

            return result;
        }

        private static bool CheckCompilance(List<List<Room>> roomVariants, List<Room> rooms)
        {
            if (rooms.Any(r => r == null))
                return false;

            if (roomVariants.Count != 0)
            {
                for (var i = roomVariants.Count - 1; i >= 0; i--)
                {
                    for (var j = 0; j < roomVariants[i].Count; j++)
                    {
                        if (rooms[j] != null && (roomVariants[i][j].Name == rooms[j].Name && 
                            roomVariants[i][j].Rectangle.ContainsRectangle(rooms[j].Rectangle)))
                            return false;
                    }
                }
            }            

            return true;
        }

        private static List<List<Room>> GetVariantsFlats(List<Room> rooms, Rectangle spaceRectangle)
        {
            var result = new List<List<Room>>();

            for (var i = 0; i < rooms.Count; i++)
            {
                if (result.Count == 0)
                {
                    var possiblePositions = GetPossiblePosition(new List<Room>(), spaceRectangle, rooms[i]);
                    Fill(result, new List<Room>(), possiblePositions, rooms[i], spaceRectangle, rooms.Count(r => r.Name == rooms[i].Name));
                    continue;
                }
                for (var j = 0; j < result.Count; j++)
                {
                    var possiblePositions = GetPossiblePosition(result[j], spaceRectangle, rooms[i]);
                    Fill(result, result[j], possiblePositions, rooms[i], spaceRectangle, rooms.Count(r => r.Name == rooms[i].Name));
                }

                result = result.Where(r => r.Count == i + 1).ToList();
            }

            return result.Where(v => v.Count >= rooms.Count-1).ToList();
        }

        private static void Fill(List<List<Room>> roomVariants, List<Room> workVariant, List<XYZ> possiblePosition, 
            Room room, Rectangle spaceRectangle, int countRoomInVariant)
        {
            foreach (var position in possiblePosition)
            {
                var rectangles = new Rectangle[2] { new Rectangle(position, room.WidthFeet, room.HeightFeet),
                    new Rectangle(position, room.HeightFeet, room.WidthFeet)};

                foreach (var rectangle in rectangles)
                {
                    var newRoom = Room.CreateNewRoom(room, rectangle);
                    //if (!spaceRectangle.ContainsRectangle(newRoom.Rectangle))
                    //{
                    //    var intersectRectangle = spaceRectangle.GetIntersectionRectangle(newRoom.Rectangle);
                    //    if (intersectRectangle.SquareMeter < room.SquareMeter || intersectRectangle.WidthMeter < room.WidthMeter ||
                    //        intersectRectangle.HeightMeter < room.HeightMeter)
                    //        continue;

                    //    newRoom = Room.CreateNewRoom(newRoom, intersectRectangle);
                    //}
                    if (spaceRectangle.ContainsRectangle(newRoom.Rectangle))
                    {
                        newRoom = ProcessingIntersections(newRoom, newRoom.Rectangle, workVariant, spaceRectangle);
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
        }

        private static List<XYZ> GetPossiblePosition(List<Room> rooms, Rectangle spaceRectangle, Room room)
        {
            var result = new List<XYZ>();

            for (var i = 0; i <= rooms.Count; i++)
            {
                XYZ[] extremePoints;

                if (i == rooms.Count)
                    extremePoints = new XYZ[4] { 
                        spaceRectangle.minXminY, spaceRectangle.maxXminY, spaceRectangle.maxXmaxY, spaceRectangle.minXmaxY };
                else
                    extremePoints = rooms[i].GetExtremePoints();

                foreach (var point in extremePoints)
                {
                    for (var x = -1; x <= 1; x++)
                    {
                        for (var y = -1; y <= 1; y++)
                        {
                            var newPoint = point + new XYZ(x, y, 0);
                            
                            if (CheckIsCorrentPosition(newPoint, result, rooms, spaceRectangle))
                                result.Add(newPoint);
                            else
                            {
                                var sideShift = new XYZ[6] { new XYZ(room.WidthFeet, 0, 0), new XYZ(0, room.HeightFeet, 0),
                                    new XYZ(room.WidthFeet, room.HeightFeet, 0), new XYZ(room.HeightFeet, 0, 0), new XYZ(0, room.WidthFeet, 0),
                                    new XYZ(room.HeightFeet, room.WidthFeet, 0)};
                                for (var j = 0; j < sideShift.Length; j++)
                                {
                                    var shiftNewPoint = newPoint - sideShift[j];
                                    if (CheckIsCorrentPosition(shiftNewPoint, result, rooms, spaceRectangle))
                                        result.Add(shiftNewPoint);
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        private static bool CheckIsCorrentPosition(XYZ position, List<XYZ> existingPositions, List<Room> rooms, Rectangle counturRectangle)
        {
            return !existingPositions.Contains(position) && counturRectangle.ContainsPoint(position) && 
                !rooms.Any(r => r.Rectangle.ContainsPoint(position) || r.Rectangle.GetMinDistanceSideToPoint(position) < 1);
        }


        //private static Room GetNewRooms(Room room, List<Room> movingRooms, Rectangle spaceRectangle)
        //{
        //    var variants = new List<Room>();

        //    for (var i = 0; i < movingRooms.Count; i++)
        //    {
        //        try
        //        {
        //            var extremePoints = movingRooms[i].GetExtremePoints();

        //            foreach (var point in extremePoints)
        //            {
        //                var newPoints = GetNewPoints(point, movingRooms);

        //                foreach (var newPoint in newPoints)
        //                {
        //                    var newRoom = Room.CreateNewRoom(room, newPoint, room.WidthMeter, room.HeightMeter,
        //                        room.SquareMeter);

        //                    if (spaceRectangle.ContainsRectangle(newRoom.Rectangle))
        //                    {
        //                        newRoom = ProcessingIntersections(newRoom, newRoom.Rectangle, movingRooms, spaceRectangle);
        //                        if (newRoom != null)
        //                            return newRoom;
        //                    }
        //                    else
        //                    {
        //                        var intersectRectangle = spaceRectangle.GetIntersectionRectangle(newRoom.Rectangle);
        //                        newRoom = Room.CreateNewRoom(newRoom, intersectRectangle.minXminY, intersectRectangle.WidthMeter,
        //                            intersectRectangle.HeightMeter, 0);
        //                        newRoom = ProcessingIntersections(newRoom, newRoom.Rectangle, movingRooms, spaceRectangle);

        //                        if (newRoom != null)
        //                            return newRoom;
        //                    }
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            TaskDialog.Show("EXCEPTION", $"STR-96 {ex.Message}");
        //        }

        //    }
        //    TaskDialog.Show("PUm", $"STR-120 OUCH {room}");

        //    return null;
        //}

        private static bool CheckIntersections(Room room, List<Room> spacedRooms)
        {
            var intersectionRooms = spacedRooms.Where(m => m.Rectangle.IntersectsWith(room.Rectangle)).ToList();

            if (intersectionRooms.Count == 0) return false;
            else if (intersectionRooms.Count == 1)
            {
                if (intersectionRooms[0].Name == room.Name && (room.Rectangle.ContainsRectangle(intersectionRooms[0].Rectangle) ||
                    intersectionRooms[0].Rectangle.ContainsRectangle(room.Rectangle)))
                    return false;
            }

            return true;
        }

        private static Room ProcessingIntersections(Room room, Rectangle roomRectangle, List<Room> spacedRooms, 
            Rectangle spaceRectangle)
        {
            if (!CheckIntersections(room, spacedRooms))
                return room;

            return null;
        }

        //private static List<XYZ> GetNewPoints(XYZ position, List<Room> movingRooms)
        //{
        //    var result = new List<XYZ>();
        //    var allVariants = new XYZ[8]
        //    {
        //        new XYZ(position.X + 1, position.Y, position.Z),
        //        new XYZ(position.X, position.Y + 1, position.Z),
        //        new XYZ(position.X + 1, position.Y + 1, position.Z),
        //        new XYZ(position.X - 1, position.Y, position.Z),
        //        new XYZ(position.X, position.Y - 1, position.Z),
        //        new XYZ(position.X - 1, position.Y - 1, position.Z),
        //        new XYZ(position.X + 1, position.Y - 1, position.Z),
        //        new XYZ(position.X - 1, position.Y + 1, position.Z)
        //    };

        //    foreach (var variant in allVariants)
        //    {
        //        if (!movingRooms.Any(r => r.Rectangle.ContainsPoint(variant)))
        //            result.Add(variant);
        //    }

        //    return result;
        //}

        private static void ProcessRoomsDistanceBorders(List<Room> rooms, int index, Rectangle spaceRectangle)
        {
            if (!rooms.Any(r => r == null) && rooms[index].Rectangle.minXminY.X != spaceRectangle.minXminY.X)
            {
                var rect = new Rectangle(new XYZ(spaceRectangle.minXminY.X, rooms[index].Rectangle.minXminY.Y, rooms[index].Rectangle.minXminY.Z),
                    rooms[index].Rectangle.minXmaxY - new XYZ(0.1, 0, 0));

                if (!CheckRoomsOnRectangle(rooms, rect))
                    rooms[index] = Room.CreateNewRoom(rooms[index], 
                        new XYZ(spaceRectangle.minXminY.X, rooms[index].Rectangle.minXminY.Y, rooms[index].Rectangle.minXminY.Z),
                        rooms[index].Rectangle.maxXmaxY);
            }
            if (!rooms.Any(r => r == null) && rooms[index].Rectangle.maxXmaxY.X != spaceRectangle.maxXmaxY.X)
            {
                var rect = new Rectangle(rooms[index].Rectangle.maxXminY + new XYZ(0.1, 0, 0),
                    new XYZ(spaceRectangle.maxXmaxY.X, rooms[index].Rectangle.maxXmaxY.Y, rooms[index].Rectangle.maxXmaxY.Z));

                if (!CheckRoomsOnRectangle(rooms, rect))
                    rooms[index] = Room.CreateNewRoom(rooms[index], rooms[index].Rectangle.minXminY,
                        new XYZ(spaceRectangle.maxXmaxY.X, rooms[index].Rectangle.maxXmaxY.Y, rooms[index].Rectangle.maxXmaxY.Z));
            }
            if (!rooms.Any(r => r == null) && rooms[index].Rectangle.minXminY.Y != spaceRectangle.minXminY.Y)
            {
                var rect = new Rectangle(new XYZ(rooms[index].Rectangle.minXminY.X, spaceRectangle.minXminY.Y, rooms[index].Rectangle.maxXmaxY.Z),
                    rooms[index].Rectangle.maxXminY - new XYZ(0, 0.1, 0));

                if (!CheckRoomsOnRectangle(rooms, rect))
                    rooms[index] = Room.CreateNewRoom(rooms[index], new XYZ(rooms[index].Rectangle.minXminY.X, spaceRectangle.minXminY.Y,
                        rooms[index].Rectangle.minXminY.Z), rooms[index].Rectangle.maxXmaxY);
            }
            if (!rooms.Any(r => r == null) && rooms[index].Rectangle.maxXmaxY.Y != spaceRectangle.maxXmaxY.Y)
            {
                var rect = new Rectangle(rooms[index].Rectangle.minXmaxY + new XYZ(0, 0.1, 0), 
                    new XYZ(rooms[index].Rectangle.maxXmaxY.X, spaceRectangle.maxXmaxY.Y, rooms[index].Rectangle.maxXmaxY.Z));

                if (!CheckRoomsOnRectangle(rooms, rect))
                    rooms[index] = Room.CreateNewRoom(rooms[index], rooms[index].Rectangle.minXminY,
                        new XYZ(rooms[index].Rectangle.maxXmaxY.X, spaceRectangle.maxXmaxY.Y, rooms[index].Rectangle.maxXmaxY.Z));
            }
        }

        private static bool CheckRoomsOnRectangle(List<Room> rooms, Rectangle rectangle)
        {
            return rooms.Any(v => v.Rectangle.IntersectsWith(rectangle));
        }

        private static void ProcessSizesPlacedRooms(List<Room> rooms, int index, Rectangle spaceRectangle)
        {
            if (!rooms.Any(r => r == null) && rooms[index].Rectangle.minXminY.X != spaceRectangle.minXminY.X)
            {
                var nearestRoom = GetNearestRoomAxisX(rooms[index], rooms, spaceRectangle.minXminY.X, rooms[index].Rectangle.minXminY.X);
                if (nearestRoom != null)
                    if (rooms[index].Rectangle.minXminY.X - nearestRoom.Rectangle.maxXmaxY.X >= 1)
                        rooms[index] = ResizeRoom(rooms[index], rooms, spaceRectangle,
                            new XYZ(nearestRoom.Rectangle.maxXmaxY.X + 1, rooms[index].Rectangle.minXminY.Y, rooms[index].Rectangle.minXminY.Z),
                            rooms[index].Rectangle.maxXmaxY);
                    else
                        ReduceRoomHeight(rooms, nearestRoom, rooms[index], spaceRectangle);
            }
            if (!rooms.Any(r => r == null) && rooms[index].Rectangle.maxXmaxY.X != spaceRectangle.maxXmaxY.X)
            {
                var nearestRoom = GetNearestRoomAxisX(rooms[index], rooms, rooms[index].Rectangle.maxXmaxY.X, spaceRectangle.maxXmaxY.X);
                if (nearestRoom != null)
                    if (nearestRoom.Rectangle.minXminY.X - rooms[index].Rectangle.maxXmaxY.X >= 1)
                        rooms[index] = ResizeRoom(rooms[index], rooms, spaceRectangle, rooms[index].Rectangle.minXminY,
                            new XYZ(nearestRoom.Rectangle.minXminY.X - 1, rooms[index].Rectangle.maxXmaxY.Y, rooms[index].Rectangle.maxXmaxY.Z));
                    else
                        ReduceRoomHeight(rooms, rooms[index], nearestRoom, spaceRectangle);
            }
            if (!rooms.Any(r => r == null) && rooms[index].Rectangle.minXminY.Y != spaceRectangle.minXminY.Y)
            {
                var nearestRoom = GetNearestRoomAxisY(rooms[index], rooms, spaceRectangle.minXminY.Y, rooms[index].Rectangle.minXminY.Y);
                if (nearestRoom != null)
                    if (rooms[index].Rectangle.minXminY.Y - nearestRoom.Rectangle.maxXmaxY.Y >= 1)
                        rooms[index] = ResizeRoom(rooms[index], rooms, spaceRectangle,
                            new XYZ(rooms[index].Rectangle.minXminY.X, nearestRoom.Rectangle.maxXmaxY.Y + 1, rooms[index].Rectangle.minXminY.Z),
                            rooms[index].Rectangle.maxXmaxY);
                    else
                        ReduceRoomWidth(rooms, nearestRoom, rooms[index], spaceRectangle);
            }
            if (!rooms.Any(r => r == null) && rooms[index].Rectangle.maxXmaxY.Y != spaceRectangle.maxXmaxY.Y)
            {
                var nearestRoom = GetNearestRoomAxisY(rooms[index], rooms, rooms[index].Rectangle.maxXmaxY.Y, spaceRectangle.maxXmaxY.Y);
                if (nearestRoom != null)
                    if (nearestRoom.Rectangle.minXminY.Y - rooms[index].Rectangle.maxXmaxY.Y >= 1)
                        rooms[index] = ResizeRoom(rooms[index], rooms, spaceRectangle, rooms[index].Rectangle.minXminY,
                            new XYZ(rooms[index].Rectangle.maxXmaxY.X, nearestRoom.Rectangle.minXminY.Y - 1, rooms[index].Rectangle.maxXmaxY.Z));
                    else
                        ReduceRoomWidth(rooms, rooms[index], nearestRoom, spaceRectangle);
            }
        }

        private static Room GetNearestRoomAxisX(Room room, List<Room> rooms, double minX, double maxX)
        {
            return rooms.Where(v => minX <= v.Rectangle.minXminY.X && v.Rectangle.minXminY.X <= maxX)
                .Where(v => v.Name != room.Name && v.Rectangle.minXminY != room.Rectangle.minXminY)
                .Where(v => CheckBoundsOnY(room.Rectangle, v.Rectangle))
                .OrderBy(v =>
                {
                    if (room.Rectangle.minXminY.X == maxX)
                        return maxX - v.Rectangle.maxXmaxY.X;
                    return v.Rectangle.minXminY.X - minX;
                })
                .FirstOrDefault();
        }

        private static Room GetNearestRoomAxisY(Room room, List<Room> rooms, double minY, double maxY)
        {
            return rooms.Where(v => minY <= v.Rectangle.minXminY.Y && v.Rectangle.minXminY.Y <= maxY)
                .Where(v => v.Name != room.Name && v.Rectangle.minXminY != room.Rectangle.minXminY)
                .Where(v => CheckBoundsOnX(room.Rectangle, v.Rectangle))
                .OrderBy(v =>
                {
                    if (room.Rectangle.minXminY.Y == maxY)
                        return maxY - v.Rectangle.maxXmaxY.Y;
                    return v.Rectangle.minXminY.Y - minY;
                })
                .FirstOrDefault();
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

        //Подумать про передачу сторон
        private static Room ResizeRoom(Room room, List<Room> rooms, 
            Rectangle spaceRectangle, XYZ pointMin, XYZ pointMax)
        {
            var newVisiting = Room.CreateNewRoom(room, pointMin, pointMax);
            newVisiting = ProcessingIntersections(newVisiting, newVisiting.Rectangle, rooms, spaceRectangle);
            if (newVisiting != null)
                room = newVisiting;

            return room;
        }

        private static void ReduceRoomHeight(List<Room> rooms, Room roomLeft, Room roomRight, Rectangle counturRectangle)
        {
            var sizeReduction = 1 - (roomRight.Rectangle.minXminY.X - roomLeft.Rectangle.maxXmaxY.X);
            var indexLeft = rooms.IndexOf(roomLeft);
            var indexRight = rooms.IndexOf(roomRight);

            if (roomLeft.CanReduceHeightBy(sizeReduction))
                rooms[indexLeft] = ResizeRoom(roomLeft, rooms, counturRectangle, roomLeft.Rectangle.minXminY,
                    roomLeft.Rectangle.maxXmaxY - new XYZ(sizeReduction, 0, 0));
            else if (roomRight.CanReduceHeightBy(sizeReduction))
                rooms[indexRight] = ResizeRoom(roomRight, rooms, counturRectangle, roomRight.Rectangle.minXminY + new XYZ(sizeReduction, 0, 0),
                    roomRight.Rectangle.maxXmaxY);
            else
            {
                rooms[indexLeft] = null;
                rooms[indexRight] = null;
            }
        }

        private static void ReduceRoomWidth(List<Room> rooms, Room roomBottom, Room roomTop, Rectangle counturRectangle)
        {
            var sizeReduction = 1 - (roomTop.Rectangle.minXminY.Y - roomBottom.Rectangle.maxXmaxY.Y);
            var indexBottom = rooms.IndexOf(roomBottom);
            var indexTop = rooms.IndexOf(roomTop);

            if (roomBottom.CanReduceWidthBy(sizeReduction))
                rooms[indexBottom] = ResizeRoom(roomBottom, rooms, counturRectangle, roomBottom.Rectangle.minXminY,
                    roomBottom.Rectangle.maxXmaxY - new XYZ(0, sizeReduction, 0));
            else if (roomTop.CanReduceWidthBy(sizeReduction))
                rooms[indexTop] = ResizeRoom(roomTop, rooms, counturRectangle, roomTop.Rectangle.minXminY + new XYZ(0, sizeReduction, 0),
                    roomTop.Rectangle.maxXmaxY);
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
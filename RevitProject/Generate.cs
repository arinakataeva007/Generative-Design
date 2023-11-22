using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitProject
{
    public class Generate
    {
        public static List<Room> GetShapes(List<double> sides, List<XYZ> extremePointsSpace)
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
            var visitings = new List<Room> { new Kitchen(), new Hallway(), new Bathroom() };

            if (square < 28) // студия
                return visitings;
            else if (square < 44) // однокомнатная
                visitings.Add(new LivingRoom());
            else if (square < 56) // двушка
                visitings.AddRange(new List<Room> { new LivingRoom(), new LivingRoom() });
            else if (square < 70) // трёшка
                return new List<Room> { };
            else
                return new List<Room> { };

            return visitings;
        }

        private static List<Room> MoveRooms(List<Room> rooms, List<XYZ> extremePointsSpace)
        {
            var spaceMinX = extremePointsSpace[0].X;
            var spaceMinY = extremePointsSpace[0].Y;
            var pointZ = extremePointsSpace[0].Z;
            var maxX = extremePointsSpace[2].X;
            var maxY = extremePointsSpace[2].Y;
            var spaceRectangle = new Rectangle(extremePointsSpace.ToArray());
            var movingRooms = new List<Room>();
            var countRooms = rooms.Count;

            for (var i = 0; i < countRooms; i++)
            {
                if (spaceMinX + rooms[i].WidthFeet <= maxX && spaceMinY + rooms[i].HeightFeet <= maxY)
                {
                    if (movingRooms.Count == 0)
                        movingRooms.Add(Room.CreateNewRoom(rooms[i], new XYZ(spaceMinX, spaceMinY, pointZ),
                            rooms[i].WidthMeter, rooms[i].HeightMeter, rooms[i].SquareMeter));
                    else
                    {
                        var newRoom = GetNewRooms(rooms[i], movingRooms, spaceRectangle, pointZ);

                        if (newRoom != null)
                            movingRooms.Add(newRoom);
                    }
                }
            }

            ProcessRoomsDistanceBorders(movingRooms, spaceRectangle);
            ProcessSizesPlacedRooms(movingRooms, spaceRectangle);

            return movingRooms;
        }

        private static Room GetNewRooms(Room room, List<Room> movingRooms, Rectangle spaceRectangle, double pointZ)
        {
            for (var i = 0; i < movingRooms.Count; i++)
            {
                try
                {
                    var extremePoints = movingRooms[i].GetExtremePoints();

                    foreach (var point in extremePoints)
                    {
                        var newPoints = GetNewPoints(point, movingRooms);

                        foreach (var newPoint in newPoints)
                        {
                            var newRoom = Room.CreateNewRoom(room, newPoint, room.WidthMeter, room.HeightMeter,
                                room.SquareMeter);
                            var rectangle = newRoom.Rectangle;

                            if (spaceRectangle.ContainsRectangle(newRoom.Rectangle))
                            {
                                newRoom = ProcessingIntersections(newRoom, rectangle, movingRooms, spaceRectangle);

                                if (newRoom != null)
                                    return newRoom;
                            }
                            else
                            {
                                var intersectRectangle = spaceRectangle.GetIntersectionRectangle(rectangle);
                                newRoom = Room.CreateNewRoom(newRoom, intersectRectangle.minXminY, intersectRectangle.WidthMeter,
                                    intersectRectangle.HeightMeter, 0);
                                newRoom = ProcessingIntersections(newRoom, newRoom.Rectangle, movingRooms, spaceRectangle);

                                if (newRoom != null)
                                    return newRoom;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("EXCEPTION", $"STR-96 {ex.Message}");
                }

            }
            TaskDialog.Show("PUm", $"STR-120 OUCH {room}");

            return null;
        }

        private static bool CheckIntersections(Room room, List<Room> spacedRooms)
        {
            var intersectionRooms = spacedRooms.Where(m => m.Rectangle.IntersectsWith(room.Rectangle)).ToList();

            if (intersectionRooms.Count == 0) return false;
            else if (intersectionRooms.Count == 1)
            {
                if (intersectionRooms[0].Name == room.Name && 
                    room.Rectangle.ContainsRectangle(intersectionRooms[0].Rectangle))
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

        private static List<XYZ> GetNewPoints(XYZ position, List<Room> movingRooms)
        {
            var result = new List<XYZ>();
            var allVariants = new XYZ[8]
            {
                new XYZ(position.X + 1, position.Y, position.Z),
                new XYZ(position.X, position.Y + 1, position.Z),
                new XYZ(position.X + 1, position.Y + 1, position.Z),
                new XYZ(position.X - 1, position.Y, position.Z),
                new XYZ(position.X, position.Y - 1, position.Z),
                new XYZ(position.X - 1, position.Y - 1, position.Z),
                new XYZ(position.X + 1, position.Y - 1, position.Z),
                new XYZ(position.X - 1, position.Y + 1, position.Z)
            };

            foreach (var variant in allVariants)
            {
                foreach (var room in movingRooms)
                {
                    foreach (var point in room.GetExtremePoints())
                    {
                        if (point.X == variant.X && point.Y == variant.Y)
                            break;

                        result.Add(variant);
                    }
                }
            }

            return result;
        }

        private static void ProcessRoomsDistanceBorders(List<Room> rooms, Rectangle spaceRectangle)
        {
            for (var i = 0; i < rooms.Count; i++)
            { 
                if (rooms[i].Rectangle.minXminY.X != spaceRectangle.minXminY.X)
                {
                    var rect = new Rectangle(new XYZ(spaceRectangle.minXminY.X, rooms[i].Rectangle.minXminY.Y, rooms[i].Rectangle.minXminY.Z),
                        rooms[i].Rectangle.minXmaxY - new XYZ(0.1, 0, 0));

                    if (!CheckVisitingsOnRectangle(rooms, rect))
                        rooms[i] = Room.CreateNewRoom(rooms[i], 
                            new XYZ(spaceRectangle.minXminY.X, rooms[i].Rectangle.minXminY.Y, rooms[i].Rectangle.minXminY.Z),
                            rooms[i].Rectangle.maxXmaxY);
                }
                if (rooms[i].Rectangle.maxXmaxY.X != spaceRectangle.maxXmaxY.X)
                {
                    var rect = new Rectangle(rooms[i].Rectangle.maxXminY + new XYZ(0.1, 0, 0),
                        new XYZ(spaceRectangle.maxXmaxY.X, rooms[i].Rectangle.maxXmaxY.Y, rooms[i].Rectangle.maxXmaxY.Z));

                    if (!CheckVisitingsOnRectangle(rooms, rect))
                        rooms[i] = Room.CreateNewRoom(rooms[i], rooms[i].Rectangle.minXminY,
                            new XYZ(spaceRectangle.maxXmaxY.X, rooms[i].Rectangle.maxXmaxY.Y, rooms[i].Rectangle.maxXmaxY.Z));
                }
                if (rooms[i].Rectangle.minXminY.Y != spaceRectangle.minXminY.Y)
                {
                    var rect = new Rectangle(new XYZ(rooms[i].Rectangle.minXminY.X, spaceRectangle.minXminY.Y, rooms[i].Rectangle.maxXmaxY.Z),
                        rooms[i].Rectangle.maxXminY - new XYZ(0, 0.1, 0));

                    if (!CheckVisitingsOnRectangle(rooms, rect))
                        rooms[i] = Room.CreateNewRoom(rooms[i], new XYZ(rooms[i].Rectangle.minXminY.X, spaceRectangle.minXminY.Y,
                            rooms[i].Rectangle.minXminY.Z), rooms[i].Rectangle.maxXmaxY);
                }
                if (rooms[i].Rectangle.maxXmaxY.Y != spaceRectangle.maxXmaxY.Y)
                {
                    var rect = new Rectangle(rooms[i].Rectangle.minXmaxY + new XYZ(0, 0.1, 0), 
                        new XYZ(rooms[i].Rectangle.maxXmaxY.X, spaceRectangle.maxXmaxY.Y, rooms[i].Rectangle.maxXmaxY.Z));

                    if (!CheckVisitingsOnRectangle(rooms, rect))
                        rooms[i] = Room.CreateNewRoom(rooms[i], rooms[i].Rectangle.minXminY,
                            new XYZ(rooms[i].Rectangle.maxXmaxY.X, spaceRectangle.maxXmaxY.Y, rooms[i].Rectangle.maxXmaxY.Z));
                }
            }
        }

        private static bool CheckVisitingsOnRectangle(List<Room> rooms, Rectangle rectangle)
        {
            return rooms.Any(v => v.Rectangle.IntersectsWith(rectangle));
        }

        private static void ProcessSizesPlacedRooms(List<Room> rooms, Rectangle spaceRectangle)
        {
            for (var i = 0; i < rooms.Count; i++)
            {
                if (rooms[i].Rectangle.minXminY.X != spaceRectangle.minXminY.X)
                {
                    var nearestRoom = GetNearestRoomAxisX(rooms[i], rooms, spaceRectangle.minXminY.X, rooms[i].Rectangle.minXminY.X);

                    if (nearestRoom != null)
                        rooms[i] = BoostRoom(rooms[i], rooms, spaceRectangle,
                           new XYZ(nearestRoom.Rectangle.maxXmaxY.X + 1, rooms[i].Rectangle.minXminY.Y, rooms[i].Rectangle.minXminY.Z),
                           rooms[i].Rectangle.minXminY);
                }
                if (rooms[i].Rectangle.maxXmaxY.X != spaceRectangle.maxXmaxY.X)
                {
                    var nearestRoom = GetNearestRoomAxisX(rooms[i], rooms, rooms[i].Rectangle.maxXmaxY.X, spaceRectangle.maxXmaxY.X);
                    
                    if (nearestRoom != null)
                        rooms[i] = BoostRoom(rooms[i], rooms, spaceRectangle, rooms[i].Rectangle.minXminY,
                            new XYZ(nearestRoom.Rectangle.minXminY.X - 1, rooms[i].Rectangle.maxXmaxY.Y, rooms[i].Rectangle.maxXmaxY.Z));
                }
                if (rooms[i].Rectangle.minXminY.Y != spaceRectangle.minXminY.Y)
                {
                    var nearestRoom = GetNearestRoomAxisY(rooms[i], rooms, spaceRectangle.minXminY.Y, rooms[i].Rectangle.minXminY.Y);
                    if (nearestRoom != null)
                        rooms[i] = BoostRoom(rooms[i], rooms, spaceRectangle, 
                            new XYZ(rooms[i].Rectangle.minXminY.X, nearestRoom.Rectangle.maxXmaxY.Y + 1, rooms[i].Rectangle.maxXmaxY.Z), 
                            rooms[i].Rectangle.minXminY);
                }
                if (rooms[i].Rectangle.maxXmaxY.Y != spaceRectangle.maxXmaxY.Y)
                {
                    var nearestRoom = GetNearestRoomAxisY(rooms[i], rooms, rooms[i].Rectangle.maxXmaxY.Y, spaceRectangle.maxXmaxY.Y);
                    if (nearestRoom != null)
                        rooms[i] = BoostRoom(rooms[i], rooms, spaceRectangle, rooms[i].Rectangle.minXminY,
                            new XYZ(rooms[i].Rectangle.maxXmaxY.X, nearestRoom.Rectangle.minXminY.Y - 1, rooms[i].Rectangle.maxXmaxY.Z));
                }
            }
        }

        private static Room GetNearestRoomAxisX(Room visiting, List<Room> visitings, double minX, double maxX)
        {
            return visitings.Where(v => minX <= v.Rectangle.minXminY.X && v.Rectangle.minXminY.X <= maxX)
                .Where(v =>
                {
                    if (visiting.Rectangle.minXminY.X == maxX)
                        return maxX - v.Rectangle.maxXmaxY.X > 1;
                    return v.Rectangle.minXminY.X - minX > 1;
                })
                .Where(v => CheckBoundsOnY(visiting.Rectangle, v.Rectangle))
                .OrderBy(v =>
                {
                    if (visiting.Rectangle.minXminY.X == maxX)
                        return maxX - v.Rectangle.maxXmaxY.X;
                    return v.Rectangle.minXminY.X - minX;
                })
                .FirstOrDefault();
        }

        private static Room GetNearestRoomAxisY(Room visiting, List<Room> visitings, double minY, double maxY)
        {
            return visitings.Where(v => minY <= v.Rectangle.minXminY.Y && v.Rectangle.minXminY.Y <= maxY)
                .Where(v =>
                {
                    if (visiting.Rectangle.minXminY.Y == maxY)
                        return maxY - v.Rectangle.maxXmaxY.Y > 1;
                    return v.Rectangle.minXminY.Y - minY > 1;
                })
                .Where(v => CheckBoundsOnX(visiting.Rectangle, v.Rectangle))
                .OrderBy(v =>
                {
                    if (visiting.Rectangle.minXminY.Y == maxY)
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

        private static Room BoostRoom(Room room, List<Room> rooms, 
            Rectangle spaceRectangle, XYZ pointMin, XYZ pointMax)
        {
            var newVisiting = Room.CreateNewRoom(room, pointMin, pointMax);
            newVisiting = ProcessingIntersections(newVisiting, newVisiting.Rectangle, rooms, spaceRectangle);
            if (newVisiting != null)
                room = newVisiting;

            return room;
        }
    }
}
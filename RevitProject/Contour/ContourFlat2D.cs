namespace RevitProject
{
    public class ContourFlat2D
    {
        public readonly IGeometricShape2D GeometricShape;
        public readonly Side2D SideWithDoor;
        public readonly Side2D SideWithWindow;
        public readonly string Name;

        public ContourFlat2D(IGeometricShape2D geometricShape, Side2D sideWithDoor, Side2D sideWithWindow, string name = "")
        {
            GeometricShape = geometricShape;
            SideWithDoor = sideWithDoor;
            SideWithWindow = sideWithWindow;
            Name = name;
        }

        public override string ToString() => Name;
    }
}
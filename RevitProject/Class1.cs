using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;

namespace RevitProject
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class Class1 : IExternalCommand
    {
        //readonly MainWindow userWindow1 = new MainWindow();

        //private static ExternalCommandData commandData1;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //userWindow1.ShowDialog();
            //commandData1 = commandData;

            var uiApp = commandData.Application;
            var doc = uiApp.ActiveUIDocument.Document;

            var contourRoom = ProcessingContour.GetContourRoom(doc, uiApp);

            //TaskDialog.Show("36", $"{contourRoom.GeometricShape}\n{contourRoom.SideWithDoor}\n{contourRoom.SideWithWindow}");


            //var transform = familyInstance.GetTotalTransform();

            //    GetBoundingBoxRotatedElement(doc, familyInstance, transform, userClickPoint);


            //    //if (transform.BasisX != XYZ.BasisX && transform.BasisY != XYZ.BasisY)
            //    //    boundingBox = GetBoundingBoxRotatedElement(doc, familyInstance, transform, boundingBox.Min);


            var shapes = Generate.GetShapes(contourRoom);

            CreateNewDirectShape(doc, contourRoom.GeometricShape.ExtremePoints[0], contourRoom.GeometricShape.ExtremePoints[2], shapes);

            return Result.Succeeded;
        }

        private static BoundingBoxXYZ GetBoundingBoxRotatedElement(Document document, FamilyInstance familyInstance, 
            Transform transform, XYZ minPosition)
        {
            BoundingBoxXYZ boundingBox;

            var angleX = transform.BasisX.AngleTo(XYZ.BasisX);
            //var angleY = transform.BasisY.AngleTo(XYZ.BasisY);

            if (transform.BasisX.DotProduct(XYZ.BasisX) < 0) angleX = -angleX;
            //if (transform.BasisY.DotProduct(XYZ.BasisY) < 0) angleY = -angleY;

            using (var t = new Transaction(document, "Rotate"))
            {
                t.Start();

                ElementTransformUtils.RotateElement(document, familyInstance.Id, 
                    Line.CreateBound(minPosition, minPosition + XYZ.BasisZ), angleX);

                var geometry = familyInstance.get_Geometry(new Options());
                boundingBox = geometry.GetBoundingBox();

                //ПОВОРОТ ОБРАТНО
                ElementTransformUtils.RotateElement(document, familyInstance.Id,
                    Line.CreateBound(minPosition, minPosition + XYZ.BasisZ), -angleX);

                t.Commit();
            }

            return boundingBox;
        } 

        public static void CreateNewDirectShape(Document doc, XYZ pointMin, XYZ pointMax, List<List<Room>> variants)
        {
            using (Transaction trans = new Transaction(doc, "Create Box DirectShape"))
            {
                trans.Start();
                //Костыль : нужен для отрисовки квартир друг за другом
                var minPosition = new XYZ(0, 0, 0);

                foreach (var variant in variants)
                {
                    foreach (var room in variant)
                    {
                        var points = room.Rectangle.ExtremePoints.Select(p => p + minPosition).ToArray();
                        var cL = new CurveLoop();

                        cL.Append(Line.CreateBound(points[0], points[1]));
                        cL.Append(Line.CreateBound(points[1], points[2]));
                        cL.Append(Line.CreateBound(points[2], points[3]));
                        cL.Append(Line.CreateBound(points[3], points[0]));

                        var curveLoops = new List<CurveLoop>() { cL };
                        var solidOptions = new SolidOptions(ElementId.InvalidElementId, ElementId.InvalidElementId);
                        var solid = GeometryCreationUtilities.CreateExtrusionGeometry(curveLoops, XYZ.BasisZ, pointMax.Z - pointMin.Z, 
                            solidOptions);

                        var directShape = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_GenericModel));
                        directShape.Name = room.Name;
                        directShape.SetShape(new List<GeometryObject>() { solid });
                        //if (transform.BasisX != XYZ.BasisX)
                        //{
                        //    var angleX = transform.BasisX.AngleTo(XYZ.BasisX);

                        //    ElementTransformUtils.RotateElement(doc, directShape.Id,
                        //        Line.CreateBound(points[0], points[0] + XYZ.BasisZ), angleX);
                        //}
                    }

                    minPosition += new XYZ(30, 0, 0);
                }

                TaskDialog.Show("DirectShape", $"Completed");
                trans.Commit();
            }
        }
    }
}


//var categories = doc.Settings.Categories;
//var el = categories.get_Item(BuiltInCategory.OST_Walls);
//AssemblyInstance.Create(doc, new List<ElementId>() { familyInstance.Id, newInstance.Id }, el.Id);





//Создание контура из стен (Не работает!)
//private static void CreateContourOfTheWalls(Document document, BoundingBoxXYZ contour)
//{
//    var movement = new XYZ(contour.Max.X - contour.Min.X + 10, 0, 0);

//    var newPoint1 = contour.Min + movement;
//    var newPoint2 = new XYZ(contour.Max.X, contour.Min.Y, contour.Min.Z) + movement;
//    var newPoint3 = new XYZ(contour.Max.X, contour.Max.Y, contour.Min.Z) + movement;
//    var newPoint4 = new XYZ(contour.Min.X, contour.Max.Y, contour.Min.Z) + movement;

//    using (Transaction transaction = new Transaction(document, "Create contour of the walls"))
//    {
//        transaction.Start();

//        var wallLine1 = Line.CreateBound(newPoint1, newPoint2);
//        var wallLine2 = Line.CreateBound(newPoint2, newPoint3);
//        var wallLine3 = Line.CreateBound(newPoint3, newPoint4);
//        var wallLine4 = Line.CreateBound(newPoint4, newPoint1);

//        var cL = new List<Curve>()
//        {
//            Line.CreateBound(newPoint1, newPoint2),
//            Line.CreateBound(newPoint2, newPoint3),
//            Line.CreateBound(newPoint3, newPoint4),
//            Line.CreateBound(newPoint4, newPoint1)
//        };

//        //var wall1 = Wall.Create(document, wallLine1, new ElementId(BuiltInCategory.OST_Walls), false);
//        //var wall2 = Wall.Create(document, wallLine2, new ElementId(BuiltInCategory.OST_Walls), false);
//        //var wall3 = Wall.Create(document, wallLine3, new ElementId(BuiltInCategory.OST_Walls), false);
//        //var wall4 = Wall.Create(document, wallLine4, new ElementId(BuiltInCategory.OST_Walls), false);

//        var walls = Wall.Create(document, cL, false);

//        transaction.Commit();
//    }
//}

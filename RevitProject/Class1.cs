using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;


namespace RevitProject
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class Class1 : IExternalCommand
    {
        readonly UserWindow1 userWindow1 = new UserWindow1();

        private static ExternalCommandData commandData1;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //userWindow1.ShowDialog();

            commandData1 = commandData;

            var uiApp = commandData.Application;
            var doc = uiApp.ActiveUIDocument.Document;
            var activeView = doc.ActiveView;

            var pickedRef = uiApp.ActiveUIDocument.Selection.PickObject(ObjectType.Subelement, "Выберите стену");
            var userClickPoint = pickedRef.GlobalPoint;
            var selectedElement = doc.GetElement(pickedRef);

            if (selectedElement is FamilyInstance)
            {
                var familyInstance = selectedElement as FamilyInstance;

                var transform = familyInstance.GetTotalTransform();

                GetBoundingBoxRotatedElement(doc, familyInstance, transform, userClickPoint);

                var geometry = familyInstance.get_Geometry(new Options());
                var boundingBox = geometry.GetBoundingBox();
                var geometryInstance = GetGeometryInstance(geometry);
                var geometryElement = geometryInstance.GetInstanceGeometry();
                var solid = GetSolid(geometryElement);
                var computeCentroid = solid.ComputeCentroid();
                var sizes = GetSizeSides(solid);

                //TaskDialog.Show("50", $"{transform.BasisX}\n{transform.BasisY}");

                //if (transform.BasisX != XYZ.BasisX && transform.BasisY != XYZ.BasisY)
                //    boundingBox = GetBoundingBoxRotatedElement(doc, familyInstance, transform, boundingBox.Min);

                var pointMin = boundingBox.Min;
                var pointMax = boundingBox.Max;

                //TaskDialog.Show("FamilyInstance", $"Selected FamilyInstance\n{v.BasisX}\n{v.BasisY}\n{v.BasisZ}" +
                //    $"\n{boundingBox.Max}\n{boundingBox.Min}\n{sizes[0]}\n{sizes[1]}");

                var points = new List<XYZ>()
                {
                    pointMin,
                    new XYZ(pointMax.X, pointMin.Y, pointMin.Z),
                    new XYZ(pointMax.X, pointMax.Y, pointMin.Z),
                    new XYZ(pointMin.X, pointMax.Y, pointMin.Z)
                };

                var shapes = Generate.GetShapes(sizes, points);

                CreateNewDirectShape(doc, pointMin, pointMax, shapes, transform);
            }
            else
                TaskDialog.Show("Null", "Нужно тыкнуть в другое место");

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
        
        public static GeometryInstance GetGeometryInstance(GeometryElement geometryElement)
        {
            foreach (var element in geometryElement)
            {
                if (element is GeometryInstance geometryInstance)
                    return geometryInstance;
            }

            return null;
        }

        public static Solid GetSolid(GeometryElement geometryElement)
        {
            foreach (var element in geometryElement)
            {
                if (element is Solid solid)
                {
                    if (solid.Volume > 0)
                        return solid;
                }
            }

            return null;
        }

        public static List<double> GetSizeSides(Solid solid)
        {
            var sizes = new List<double>();

            var edges = solid.Edges;

            foreach (Edge edge in edges)
            {
                var sizeOfM = Math.Round(edge.ApproximateLength * 0.3048, 3);

                if (!sizes.Contains(sizeOfM))
                    sizes.Add(sizeOfM);
            }

            return sizes;
        }

        public static void CreateNewDirectShape(Document doc, XYZ pointMin, XYZ pointMax, List<List<Room>> variants, Transform transform)
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
                        var points = room.GetExtremePoints().Select(p => p + minPosition).ToArray();
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

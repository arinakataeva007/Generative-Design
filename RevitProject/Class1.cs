using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;


namespace RevitProject
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class Class1 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiApp = commandData.Application;
            var doc = uiApp.ActiveUIDocument.Document;
            var activeView = doc.ActiveView;

            var pickedRef = uiApp.ActiveUIDocument.Selection.PickObject(ObjectType.Subelement, "Выберите стену");
            var userClickPoint = pickedRef.GlobalPoint;
            var selectedElement = doc.GetElement(pickedRef);

            if (selectedElement is FamilyInstance)
            {

                var familyInstance = selectedElement as FamilyInstance;

                var geometry = familyInstance.get_Geometry(new Options());
                var boundingBox = geometry.GetBoundingBox();
                var geometryInstance = GetGeometryInstance(geometry);
                var geometryElement = geometryInstance.GetInstanceGeometry();
                var solid = GetSolid(geometryElement);
                var computeCentroid = solid.ComputeCentroid();
                var sizes = GetSizeSides(solid);

                var pointMin = boundingBox.Min;
                var pointMax = boundingBox.Max;

                TaskDialog.Show("FamilyInstance", $"Selected FamilyInstance");

                //var sketchPlane = SketchPlane.Create(doc, plane);

                var categories = doc.Settings.Categories;
                var el = categories.get_Item(BuiltInCategory.OST_Walls);

                //AssemblyInstance.Create(doc, new List<ElementId>() { familyInstance.Id, newInstance.Id }, el.Id);


                var points = new List<XYZ>() 
                { 
                    pointMin, 
                    new XYZ(pointMax.X, pointMin.X, pointMin.Z),   
                    new XYZ(pointMax.X, pointMax.Y, pointMin.Z),
                    new XYZ(pointMin.X, pointMax.Y, pointMin.Z)
                };

                var shapes = Generate.GetShapes(sizes, points);

                CreateNew(doc, pointMin, pointMax, shapes);
            }
            else
                TaskDialog.Show("Null", "Нужно тыкнуть в другое место");
              
            return Result.Succeeded;
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

        public static void CreateNew(Document doc, XYZ pointMin, XYZ pointMax, List<Visiting> visitings)
        {
            using (Transaction trans = new Transaction(doc, "Create Box DirectShape"))
            {
                trans.Start();

                XYZ min = new XYZ(pointMin.X, pointMin.Y, pointMin.Z);
                XYZ max = new XYZ(pointMax.X - 1, pointMax.Y - 1, pointMax.Z);


                foreach (var visiting in visitings)
                {
                    //TaskDialog.Show("STR-123", "");
                    var points = visiting.GetExtremePoints();
                    var cL = new CurveLoop();

                    cL.Append(Line.CreateBound(points[0], points[1]));
                    cL.Append(Line.CreateBound(points[1], points[2]));
                    cL.Append(Line.CreateBound(points[2], points[3]));
                    cL.Append(Line.CreateBound(points[3], points[0]));

                    var curveLoops = new List<CurveLoop>() { cL };
                    var solidOptions = new SolidOptions(ElementId.InvalidElementId, ElementId.InvalidElementId);
                    var solid = GeometryCreationUtilities.CreateExtrusionGeometry(curveLoops, XYZ.BasisZ, pointMax.Z - pointMin.Z, solidOptions);

                    var directShape = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_GenericModel));
                    directShape.Name = visiting.Name;
                    directShape.SetShape(new List<GeometryObject>() { solid });
                }

                TaskDialog.Show("DirectShape", $"Completed");

                trans.Commit();
            }
        }

        //static List<CurveLoop> GetCurves(XYZ[] points)
        //{
        //    var curves = new List<CurveLoop>();
        //
        //    Line line1 = Line.CreateBound(points[0], points[1]);
        //    Line line2 = Line.CreateBound(points[1], points[2]);
        //    Line line3 = Line.CreateBound(points[2], points[3]);
        //    Line line4 = Line.CreateBound(points[3], points[1]);
        //    CurveLoop curveLoop = new CurveLoop();
        //    curveLoop.Append(line1);
        //    curveLoop.Append(line2);
        //    curveLoop.Append(line3);
        //    curveLoop.Append(line4);
        //}
    }
}








//var document = commandData.Application.ActiveUIDocument.Document;
//var newRoomFilter = new FilteredElementCollector(document);
//var allRooms = newRoomFilter.OfCategory(BuiltInCategory.OST_Assemblies).WhereElementIsNotElementType().ToElements();

//var window = new UserWindow1(allRooms);

//window.ShowDialog();


// Что-то про создание стены
//var symbolId = doc.GetDefaultFamilyTypeId(new ElementId(BuiltInCategory.OST_Walls));

//if (symbolId == ElementId.InvalidElementId)
//{
//    t.RollBack();
//    return Result.Failed;
//}

//var symbol = doc.GetElement(symbolId) as FamilySymbol;

//var level = (Level)doc.GetElement(newWall.LevelId);

//doc.Create.NewFamilyInstance(new XYZ(0, 0, 0), symbol, newWall, level, StructuralType.NonStructural);



// создание двух стен 

//IList<Curve> curves = new List<Curve>();
//XYZ first = newLocation;
//XYZ second = new XYZ(newLocation.X + 200, newLocation.Y, newLocation.Z);
//XYZ third = new XYZ(newLocation.X + 200, newLocation.Y, newLocation.Z + 150);
//XYZ fourth = new XYZ(newLocation.X, newLocation.Y, newLocation.Z + 150);
//var fifth = new XYZ(newLocation.X, newLocation.Y + 200, newLocation.Z);
//var sixth = new XYZ(newLocation.X, newLocation.Y + 200, newLocation.Z + 150);

//curves.Add(Line.CreateBound(first, second));
//curves.Add(Line.CreateBound(second, third));
//curves.Add(Line.CreateBound(third, fourth));
//curves.Add(Line.CreateBound(fourth, first));

//IList<Curve> curves2 = new List<Curve>();

//curves2.Add(Line.CreateBound(first, fifth));
//curves2.Add(Line.CreateBound(fifth, sixth));
//curves2.Add(Line.CreateBound(sixth, fourth));
//curves2.Add(Line.CreateBound(fourth, first));


//var newWall = Wall.Create(doc, curves, false);
//var newWall2 = Wall.Create(doc, curves2, false);










//var newInstance = doc.Create.NewFamilyInstance(newLocation, familyInstance.Symbol, doc.ActiveView.GenLevel, StructuralType.UnknownFraming);

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

            //var docLevel = doc.ActiveView.LevelId;
            //var plane = Plane.CreateByNormalAndOrigin(activeView.ViewDirection, activeView.Origin);

            var pickedRef = uiApp.ActiveUIDocument.Selection.PickObject(ObjectType.Subelement, "Выберите стену");
            var userClickPoint = pickedRef.GlobalPoint;

            var selectedElement = doc.GetElement(pickedRef);

            if (selectedElement is FamilyInstance)
            {

                var familyInstance = selectedElement as FamilyInstance;
                var size = familyInstance.Parameters.Size;

                var transform = familyInstance.GetTotalTransform();
                var location = (familyInstance.Location as LocationPoint).Point;

                var rotateAngle = familyInstance.HandOrientation.AngleOnPlaneTo(XYZ.BasisY, XYZ.BasisZ) * (180 / Math.PI);

                var geometry = familyInstance.get_Geometry(new Options());
                var boundingBox = geometry.GetBoundingBox();
                var geometryInstance = GetGeometryInstance(geometry);
                var geometryElement = geometryInstance.GetInstanceGeometry();
                var solid = GetSolid(geometryElement);
                var computeCentroid = solid.ComputeCentroid();
                var sizes = GetSizeSides(solid);


                var widthParameter = double.Parse(familyInstance.LookupParameter("Width_Model").AsValueString()) / 1000;
                var heightParameter = double.Parse(familyInstance.LookupParameter("Height_Model").AsValueString()) / 1000;
                var square = widthParameter * heightParameter;

                var pointMin = boundingBox.Min;
                var pointMax = boundingBox.Max;

                TaskDialog.Show("FamilyInstance", "Selected FamilyInstance\n" +
                    $"\n{sizes[0]}\n{sizes[1]}\n{sizes[2]}\n{computeCentroid}\n\n{rotateAngle}");

                //var sketchPlane = SketchPlane.Create(doc, plane);
                var newLocation = location + new XYZ(heightParameter + 20 + location.X, 0, 0);

                var categories = doc.Settings.Categories;
                var el = categories.get_Item(BuiltInCategory.OST_Walls);

                //AssemblyInstance.Create(doc, new List<ElementId>() { familyInstance.Id, newInstance.Id }, el.Id);
              

                CreateNew(doc, pointMin, pointMax, rotateAngle);
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
                var sizeOfM = Math.Round(edge.ApproximateLength * 0.3048);

                if (!sizes.Contains(sizeOfM))
                    sizes.Add(sizeOfM);

            }

            return sizes;
        }

        public static void CreateNew(Document doc, XYZ pointMin, XYZ pointMax, double rotateAngle)
        {
            using (Transaction trans = new Transaction(doc, "Create Box DirectShape"))
            {
                trans.Start();

                XYZ min = new XYZ(pointMax.X + 20, pointMin.Y, pointMin.Z);
                XYZ max = min + new XYZ(5 / 0.3048, 3 / 0.3048, 3 / 0.3048);
                XYZ pt1 = new XYZ(min.X, min.Y, min.Z);
                XYZ pt2 = new XYZ(max.X, min.Y, min.Z);
                XYZ pt3 = new XYZ(max.X, max.Y, min.Z);
                XYZ pt4 = new XYZ(min.X, max.Y, min.Z);
                Line line1 = Line.CreateBound(pt1, pt2);
                Line line2 = Line.CreateBound(pt2, pt3);
                Line line3 = Line.CreateBound(pt3, pt4);
                Line line4 = Line.CreateBound(pt4, pt1);
                CurveLoop curveLoop = new CurveLoop();
                curveLoop.Append(line1);
                curveLoop.Append(line2);
                curveLoop.Append(line3);
                curveLoop.Append(line4);

                List<CurveLoop> loops = new List<CurveLoop>() { curveLoop };
                SolidOptions options = new SolidOptions(ElementId.InvalidElementId, ElementId.InvalidElementId);
                Solid solid = GeometryCreationUtilities.CreateExtrusionGeometry(loops, XYZ.BasisZ, pointMax.Z - pointMin.Z, options);
                

                var ds = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_GenericModel));
                ElementTransformUtils.RotateElement(doc, ds.Id, Line.CreateBound(new XYZ(), new XYZ(0, 0, 1)), rotateAngle);
                ds.ApplicationId = "Application id";
                ds.ApplicationDataId = "Geometry object id";
                ds.SetShape(new List<GeometryObject>() { solid });

                
                TaskDialog.Show("DirectShape", $"\n");

                trans.Commit();
            }
        }
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

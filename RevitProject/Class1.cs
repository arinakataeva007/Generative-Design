using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;

namespace RevitProject
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class Class1 : IExternalCommand, IExternalEventHandler
    {
        private UserWindow1 UserWindow { get; set; }

        private ExternalEvent externalEvent1;
        public ExternalEvent ExternalEvent1 { get => externalEvent1; }
        
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            externalEvent1 = ExternalEvent.Create(this);

            UserWindow = new UserWindow1(commandData, this);
            UserWindow.ShowDialog();

            return Result.Succeeded;
        }

        public void Execute(UIApplication uiApp)
        {
            CreateNewDirectShape(UserWindow.Document, UserWindow.ContourRoom.GeometricShape.ExtremePoints[0], UserWindow.ContourRoom.GeometricShape.ExtremePoints[2], UserWindow.Shapes[UserWindow.ShapeIndex]);
        }

        public string GetName() => "CreateNewDirectShape";

        public static void CreateNewDirectShape(Document doc, XYZ pointMin, XYZ pointMax, List<Room> variant)
        {
            using (Transaction trans = new Transaction(doc, "Create Box DirectShape"))
            {
                trans.Start();
   
                foreach (var room in variant)
                {
                    var points = room.Rectangle.ExtremePoints.Select(p => p).ToArray();
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
                }

                trans.Commit();
            }
        }
    }
}
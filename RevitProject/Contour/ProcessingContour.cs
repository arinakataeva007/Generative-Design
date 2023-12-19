using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;
using System;
using System.Linq;

namespace RevitProject
{
    public class ProcessingContour
    {
        public static ContourFlat2D GetContourRoom(Document doc, UIApplication uIApplication)
        {
            var familyInstance = GetSelectedFamilyInstance(doc, uIApplication);
            var contourShape = GetGeometricShape(familyInstance);
            var sideWithDoor = GetSelectedSide(doc, uIApplication, contourShape, "дверью");
            var sideWithWindow = GetSelectedSide(doc, uIApplication, contourShape, "окном");

            return new ContourFlat2D(contourShape, sideWithDoor, sideWithWindow, familyInstance.Name);
        }

        private static FamilyInstance GetSelectedFamilyInstance(Document doc, UIApplication uIApplication)
        {
            while (true)
            {
                var pickedRef = uIApplication.ActiveUIDocument.Selection.PickObject(ObjectType.Subelement, 
                    "Выберите контур будущей квартиры");
                var selectedElement = doc.GetElement(pickedRef);

                if (selectedElement is FamilyInstance familyInstance)
                {
                    TaskDialog.Show("Успешный выбор", "Контур успешно выбран");
                    return familyInstance;
                }
                else
                    TaskDialog.Show("Ошибка выбора", $"Вам нужно выбрать 'Модель в контексте выдавливания'\nПопробуйте ещё раз");
            }
        }
        /// <summary>
        /// Возврат выбранной стены
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="uIApplication"></param>
        /// <param name="contourShape"></param>
        /// <param name="item">Что в себе должна содержать стена(Дверь/Окно)</param>
        /// <returns></returns>
        private static Side2D GetSelectedSide(Document doc, UIApplication uIApplication, IGeometricShape2D contourShape, string item)
        {
            while (true)
            {
                var pickedRef = uIApplication.ActiveUIDocument.Selection.PickObject(ObjectType.Subelement, 
                    $"Выберите границу(стену) контура с {item}");
                var userClickPoint = pickedRef.GlobalPoint;
                var selectedElement = doc.GetElement(pickedRef);

                if (selectedElement is FamilyInstance familyInstance)
                {
                    var side = contourShape.Sides.Where(s => s.DistanceToPoint(userClickPoint) < 1)
                        .OrderBy(s => s.DistanceToPoint(userClickPoint)).ToArray();
                    if (side.Length > 0)
                    {
                        TaskDialog.Show("Успешный выбор", "Стена успешно выбрана");
                        return side[0];
                    }

                    TaskDialog.Show("Ошибка выбора", "Укажите точку на границе(стене) контура");
                }
                else
                    TaskDialog.Show("Ошибка выбора", 
                        $"Вам нужно выбрать 'Модель в контексте выдавливания'\nПопробуйте ещё раз");
            }
        }

        private static IGeometricShape2D GetGeometricShape(FamilyInstance familyInstance)
        {
            var geometry = familyInstance.get_Geometry(new Options());
            var boundingBox = geometry.GetBoundingBox();
            var geometryInstance = GetGeometryInstance(geometry);
            var geometryElement = geometryInstance.GetInstanceGeometry();
            var solid = GetSolid(geometryElement);
            var sizes = GetSizeSides(solid);

            if (sizes.Count - 1 == 2)
                return new Rectangle2D(boundingBox.Min, boundingBox.Max);

            return null;
        }

        private static GeometryInstance GetGeometryInstance(GeometryElement geometryElement)
        {
            foreach (var element in geometryElement)
            {
                if (element is GeometryInstance geometryInstance)
                    return geometryInstance;
            }

            return null;
        }

        private static Solid GetSolid(GeometryElement geometryElement)
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

        private static List<double> GetSizeSides(Solid solid)
        {
            var sizes = new List<double>();

            var edges = solid.Edges;

            foreach (Edge edge in edges)
            {
                var sizeMeter = Math.Round(edge.ApproximateLength * 0.3048, 3);

                if (!sizes.Contains(sizeMeter))
                    sizes.Add(sizeMeter);
            }

            return sizes;
        }
    }
}

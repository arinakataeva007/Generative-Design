using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitProject
{
    public partial class UserWindow1 : Window
    {
        private UIApplication UIApplication { get; set; }
        private Class1 Revit { get; set; }

        protected Document doc;
        public Document Document { get => doc; }

        protected ContourFlat2D contourRoom;
        public ContourFlat2D ContourRoom { get => contourRoom; }

        protected List<List<Room>> shapes = new List<List<Room>>(); 
        public List<List<Room>> Shapes { get => shapes; }

        protected int shapeIndex;
        public int ShapeIndex { get => shapeIndex; }

        private readonly List<Room> Rooms = new List<Room>
        {
            new Kitchen(),
            new Hallway(),
            new Bathroom()
        };

        public UserWindow1(ExternalCommandData commandData, Class1 class1)
        {
            InitializeComponent();

            MouseLeftButtonDown += (sender, e) => DragMove();

            UIApplication = commandData.Application;
            doc = UIApplication.ActiveUIDocument.Document;
            Revit = class1;

            ConfirmButton.IsEnabled = false;

            RadioButton1.IsEnabled = false;
            RadioButton2.IsEnabled = false;
            RadioButton3.IsEnabled = false;
            RadioButton4.IsEnabled = false;
        }


        private void Button_SelectContour(object sender, RoutedEventArgs e)
        {
            Hide();
            contourRoom = ProcessingContour.GetContourRoom(Document, UIApplication);

            RadioButton1.IsEnabled = true;
            RadioButton2.IsEnabled = true;
            RadioButton3.IsEnabled = true;
            RadioButton4.IsEnabled = true;
            
            RadioButton1.IsChecked = false; 
            RadioButton2.IsChecked = false;
            RadioButton3.IsChecked = false;
            RadioButton4.IsChecked = false;

            ConfirmButton.IsEnabled = false;

            FormText.Text = contourRoom.ToString();

            DeleteAllApartmentOptions();
            PreviewField.Children.Clear();

            Show();
        }

        private void Button_Confirm(object sender, RoutedEventArgs e)
        {
            Revit.ExternalEvent1.Raise();
            Close();
        }

        private void RadioButton_Studio(object sender, RoutedEventArgs e)
        {
            var rooms = new List<Room>(Rooms);

            shapes = Generate.GetShapes(ContourRoom, rooms);

            ConfirmButton.IsEnabled = true;

            DeleteAllApartmentOptions();
            PreviewField.Children.Clear();
            MakeApartmentOptions(shapes);
        }

        private void RadioButton_OneBedroom(object sender, RoutedEventArgs e)
        {
            var rooms = new List<Room>(Rooms)
            {
                new LivingRoom()
            };

            shapes = Generate.GetShapes(ContourRoom, rooms);

            ConfirmButton.IsEnabled = true;

            DeleteAllApartmentOptions();
            PreviewField.Children.Clear();
            MakeApartmentOptions(shapes);
        }

        private void RadioButton_TwoBedroom(object sender, RoutedEventArgs e)
        {
            var rooms = new List<Room>(Rooms);
            rooms.AddRange(new List<Room> { new LivingRoom(), new LivingRoom() });

            shapes = Generate.GetShapes(ContourRoom, rooms);

            ConfirmButton.IsEnabled = true;

            DeleteAllApartmentOptions();
            PreviewField.Children.Clear();
            MakeApartmentOptions(shapes);
        }

        private void RadioButton_ThreeBedroom(object sender, RoutedEventArgs e)
        {
            var rooms = new List<Room>(Rooms);
            rooms.AddRange(new List<Room> { new LivingRoom(), new LivingRoom(), new LivingRoom() });  

            shapes = Generate.GetShapes(ContourRoom, rooms);

            ConfirmButton.IsEnabled = true;

            DeleteAllApartmentOptions();
            PreviewField.Children.Clear();
            MakeApartmentOptions(shapes);
        }

        private void MakeApartmentOptions(List<List<Room>> shapes)
        {
            //прохожусь по листу планировок shapes
            //в этом листе прохожусь по всем комнатам, беру их размеры, возможно уменьшаю масштаб
            //и добавляю радиобатоны как ниже
            //должно работать

            //надо придумать как выбранный вариант будет рисоваться в ревите и в окошке.
            //в окошке так же через канвас только в другом компоненте и масштаб больше или оригинальный
            //в ревите надо запоминать индексы всех добавленных радиобаттонов и потом рисовать в ревите из полученного листа shapes

            for (var i = 0; i < shapes.Count; i++)
            {
                MakeApartmentOption(shapes[i], i);
            }
        }

        private void MakeApartmentOption(List<Room> rooms, int shapeIndex)
        {
            var roomScale = 5;
            var radioButton = new RadioButton
            {
                Margin = new Thickness(0, 0, 0, 100),
                Tag = shapeIndex
            };

            var canvas = new Canvas();

            var minX = rooms.Min(room => room.Rectangle.MinXminY.X);
            var maxX = rooms.Max(room => room.Rectangle.MaxXmaxY.X);
            var minY = rooms.Min(room => room.Rectangle.MinXminY.Y);
            var maxY = rooms.Max(room => room.Rectangle.MaxXmaxY.Y);

            for (var i = 0; i < rooms.Count; i++)
            {
                var polygon = new Polygon
                {
                    Fill = Brushes.LightGray,
                    Points = new PointCollection
                    {
                        new System.Windows.Point((rooms[i].Rectangle.MinXminY.X - minX) * roomScale,
                                                (maxY - rooms[i].Rectangle.MinXminY.Y) * roomScale),
                        new System.Windows.Point((rooms[i].Rectangle.MaxXminY.X - minX) * roomScale,
                                                (maxY - rooms[i].Rectangle.MaxXminY.Y) * roomScale),
                        new System.Windows.Point((rooms[i].Rectangle.MaxXmaxY.X - minX) * roomScale,
                                                (maxY - rooms[i].Rectangle.MaxXmaxY.Y) * roomScale),
                        new System.Windows.Point((rooms[i].Rectangle.MinXmaxY.X - minX) * roomScale,
                                                (maxY - rooms[i].Rectangle.MinXmaxY.Y) * roomScale)
                    }
                };

                canvas.Children.Add(polygon);
            }

            radioButton.Content = canvas;
            radioButton.Checked += ApartmentOption_Checked;

            OptionsField.Children.Add(radioButton);
        }

        private void ApartmentOption_Checked(object sender, RoutedEventArgs e)
        {
            var radioButton = (RadioButton)sender;
            var canvas = (Canvas)radioButton.Content;

            var newCanvas = new Canvas();

            shapeIndex = Convert.ToInt32(radioButton.Tag);
            PreviewField.Children.Clear();

            foreach (UIElement child in canvas.Children)
            {
                var xaml = XamlWriter.Save(child);
                var deepCopy = (UIElement)XamlReader.Parse(xaml);

                newCanvas.Children.Add(deepCopy);
            }

            newCanvas.LayoutTransform = new ScaleTransform(2, 2);

            PreviewField.Children.Add(newCanvas);
        }

        private void DeleteAllApartmentOptions()
        {
            for (int i = OptionsField.Children.Count - 1; i > 0; i--)
            {
                OptionsField.Children.RemoveAt(i);
            }
        }
        
    }

    public class Flat
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public override string ToString() => Description == "" ? $"{Name}" : $"{Name}\n{Description}";
    }
}
using System.Collections.Generic;
using System.Windows;
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

        protected List<List<Room>> shapes; 
        public List<List<Room>> Shapes { get => shapes; }

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
        }

        private void RadioButton_OneBedroom(object sender, RoutedEventArgs e)
        {
            var rooms = new List<Room>(Rooms)
            {
                new LivingRoom()
            };

            shapes = Generate.GetShapes(ContourRoom, rooms);

            ConfirmButton.IsEnabled = true;
        }

        private void RadioButton_TwoBedroom(object sender, RoutedEventArgs e)
        {
            var rooms = new List<Room>(Rooms);
            rooms.AddRange(new List<Room> { new LivingRoom(), new LivingRoom() });

            shapes = Generate.GetShapes(ContourRoom, rooms);

            ConfirmButton.IsEnabled = true;
        }

        private void RadioButton_ThreeBedroom(object sender, RoutedEventArgs e)
        {
            var rooms = new List<Room>(Rooms);
            rooms.AddRange(new List<Room> { new LivingRoom(), new LivingRoom(), new LivingRoom() });  

            shapes = Generate.GetShapes(ContourRoom, rooms);

            ConfirmButton.IsEnabled = true;
        }
    }

    public class Flat
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public override string ToString() => Description == "" ? $"{Name}" : $"{Name}\n{Description}";
    }
}
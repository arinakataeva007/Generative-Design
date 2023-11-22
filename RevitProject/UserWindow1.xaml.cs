using System.Collections.Generic;
using System.Windows;
using System.Xml.Linq;
using Autodesk.Revit.DB;

namespace RevitProject
{
    /// <summary>
    /// Логика взаимодействия для UserWindow1.xaml
    /// </summary>
    public partial class UserWindow1 : Window
    {
        public UserWindow1()
        {
            InitializeComponent();



            FlatBox.ItemsSource = new List<Flat>()
            {
                new Flat { Name = "Квартира-студия", Description = "Shit"},
                new Flat { Name = "Однокомнантная квартира", Description = "однушка"},
                new Flat { Name = "Двухкомнатная квартира", Description = "двугка"},
                new Flat { Name = "Трехкомнатная квартира", Description = ""},
            };

            FlatBox.SelectedIndex = 0;
        }

        private void FlatBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (FlatBox.SelectedIndex == 0)
            {
                //...
            }

            if (FlatBox.SelectedIndex == 1)
            {
                //...
            }

            if (FlatBox.SelectedIndex == 2)
            {
                //...
            }

            if (FlatBox.SelectedIndex == 3)
            {
                //...
            }
        }

        private void SelectItem(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void Confirm(object sender, RoutedEventArgs e)
        {

        }
    }
    public class Flat
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public override string ToString() => Description == "" ? $"{Name}" : $"{Name}\n{Description}";
    }
}
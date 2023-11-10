using System.Collections.Generic;
using System.Windows;
using Autodesk.Revit.DB;

namespace RevitProject
{
    /// <summary>
    /// Логика взаимодействия для UserWindow1.xaml
    /// </summary>
    public partial class UserWindow1 : Window
    {
        public UserWindow1(ICollection<Element> elements)
        {
            InitializeComponent();
            AllRoosView.ItemsSource = elements;
        }
    }
}

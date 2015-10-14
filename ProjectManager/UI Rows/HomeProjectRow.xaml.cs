using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProjectManager
{
    /// <summary>
    /// Interaction logic for HomeProjectRow.xaml
    /// </summary>
    public partial class HomeProjectRow : UserControl
    {
        public HomeProjectRow(Project project, Home parentHomeControl)
        {
            InitializeComponent();
            tbl_Name.Text = project.Name;
            CurrentProject = project;
            ParentHomeControl = parentHomeControl;
        }

        private void b_Select_Click(object sender, RoutedEventArgs e)
        {
            ParentHomeControl.GoToProjectMenu(CurrentProject);
        }

        public Project CurrentProject { get; private set; }
        public Home ParentHomeControl { get; private set; }
    }
}

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
    /// Interaction logic for ProjectFileRow.xaml
    /// </summary>
    public partial class ProjectFileRow : UserControl
    {
        public ProjectFileRow(ProjectFile file, ProjectMenu parentProjectMenu)
        {
            InitializeComponent();
            File = file;
            ParentProjectMenu = parentProjectMenu;

            tbl_Filename.Text = file.FileTitle;
        }

        private void b_Launch_Click(object sender, RoutedEventArgs e)
        {
            File.OpenFile();
        }

        private void b_View_Click(object sender, RoutedEventArgs e)
        {
            // Show the dialog. Note that AddFileDialog should modify the file if necessary.
            AddFileDialog window = new AddFileDialog(File);
            if (window.ShowDialog() == true)
            {
                // If something changed, update the parent menu
                ParentProjectMenu.RefreshFilesOnUI();
            }
        }

        //------//
        // Data //
        //------//
        public ProjectFile File { get; private set; }
        public ProjectMenu ParentProjectMenu { get; private set; }
    }
}

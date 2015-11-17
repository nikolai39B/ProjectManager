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
using System.Windows.Shapes;

namespace ProjectManager
{
    /// <summary>
    /// Interaction logic for NewProjectDialog.xaml
    /// </summary>
    public partial class NewProjectDialog : Window
    {
        public NewProjectDialog()
        {
            InitializeComponent();
        }

        /*public NewProjectDialog(string title, string nameDefault, string acceptButtonText)
            : this()
        {
            // This constructor gives some flexibility for editing the project name
            Title = title;
            tbl_Title.Text = title;
            tb_ProjectName.Text = nameDefault;
            b_Create.Content = acceptButtonText;
        }*/

        public string NewProjectName
        {
            get { return tb_ProjectName.Text; }
        }

        private void b_Create_Click(object sender, RoutedEventArgs e)
        {
            // Make sure the project name is valid
            if (NewProjectName == "" || NewProjectName.Contains('|'))
            {
                tbl_ErrorMessage.Visibility = System.Windows.Visibility.Visible;
                tbl_ErrorMessage.Text = "Project name cannot be blank and cannot contain pipes ('|').";
            }
            else
            {
                DialogResult = true;
            }
        }

        private void b_Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}

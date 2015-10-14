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
    /// Interaction logic for NewLogDialog.xaml
    /// </summary>
    public partial class NewLogDialog : Window
    {
        public NewLogDialog()
        {
            InitializeComponent();
        }

        public string NewLogDescription
        {
            get { return tb_LogDescription.Text; }
        }

        private void b_Create_Click(object sender, RoutedEventArgs e)
        {
            // Make sure the description is valid
            if (NewLogDescription == "" || NewLogDescription.Contains('|'))
            {
                tbl_ErrorMessage.Visibility = System.Windows.Visibility.Visible;
                tbl_ErrorMessage.Text = "Description cannot be blank and cannot contain pipes ('|').";
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

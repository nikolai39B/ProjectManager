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
    /// Interaction logic for ConfirmationDialog.xaml
    /// </summary>
    public partial class ConfirmationDialog : Window
    {
        public ConfirmationDialog(string message)
        {
            InitializeComponent();
            tbl_Message.Text = message;
        }

        private void b_Yes_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void b_No_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}

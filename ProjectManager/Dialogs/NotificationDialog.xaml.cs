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
    /// Interaction logic for NotificationDialog.xaml
    /// </summary>
    public partial class NotificationDialog : Window
    {
        public NotificationDialog(string title, string message)
        {
            InitializeComponent();
            Title = title;
            tbl_Title.Text = title;
            tbl_Message.Text = message;
        }

        //---------------//
        // Event Handles //
        //---------------//
        private void b_OK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DialogResult = true;
            }
            catch (InvalidOperationException)
            {
                // If setting dialog result to true failed, this dialog was opened using Show() instead of ShowDialog()
                this.Close();
            }
        }
    }
}

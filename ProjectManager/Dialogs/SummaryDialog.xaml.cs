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
    /// Interaction logic for SummaryDialog.xaml
    /// </summary>
    public partial class SummaryDialog : Window
    {
        public SummaryDialog(string title, List<SummaryRow> items, SummaryRow total)
        {
            InitializeComponent();
            tbl_Title.Text = title;
            foreach (var item in items)
            {
                sp_Entries.Children.Add(item);
            }
            sp_Total.Children.Add(total);
        }

        private void b_Ok_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

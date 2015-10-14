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
    /// Interaction logic for ProjectLogRow.xaml
    /// </summary>
    public partial class ProjectLogRow : UserControl
    {
        public ProjectLogRow(ProjectLog log, ProjectMenu parentProjectMenu)
        {
            InitializeComponent();
            Log = log;
            ParentProjectMenu = parentProjectMenu;

            SetDate(log.Start);
            tbl_Description.Text = log.Description;
        }

        private void SetDate(DateTime date)
        {
            tbl_Date.Text = string.Format("{0}/{1}/{2}", 
                date.Month.ToString("D2"),
                date.Day.ToString("D2"),
                date.Year.ToString("D4"));
        }

        private void b_View_Click(object sender, RoutedEventArgs e)
        {
            // Show the dialog. Note that ViewLogDialog should modify the log if necessary.
            ViewLogDialog window = new ViewLogDialog(Log);
            if (window.ShowDialog() == true)
            {
                // If something changed, update the parent menu
                ParentProjectMenu.RefreshLogsOnUI();
            }
        }

        //------//
        // Data //
        //------//
        public ProjectLog Log { get; private set; }
        public ProjectMenu ParentProjectMenu { get; private set; }
    }
}

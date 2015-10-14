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
    /// Interaction logic for SummaryRow.xaml
    /// </summary>
    public partial class SummaryRow : UserControl
    {
        public SummaryRow()
            : this("", TimeSpan.MinValue)
        {
        }

        public SummaryRow(string name, TimeSpan time)
        {
            InitializeComponent();
            tbl_Name.Text = name;
            Time = time;
            SetTime();
        }

        /// <summary>
        /// Sets the time on the UI.
        /// </summary>
        private void SetTime()
        {
            int hours = Time.Hours + Time.Days * 24;
            tbl_Hours.Text = string.Format(hourTemplate, hours);
            tbl_Minutes.Text = string.Format(minuteTemplate, Time.Minutes);
        }

        //------//
        // Data //
        //------//
        public TimeSpan Time { get; private set; }
        private const string hourTemplate = "{0} hr";
        private const string minuteTemplate ="{0} min";
    }
}

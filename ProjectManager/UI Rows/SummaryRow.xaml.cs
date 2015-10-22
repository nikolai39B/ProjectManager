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
        /// <summary>
        /// Use this constructor if you don't have a project or log instance.
        /// </summary>
        public SummaryRow(string title, TimeSpan time)
            : this(title, time, null, null)
        {
        }

        /// <summary>
        /// Use this constructor if you have a project instance.
        /// </summary>
        public SummaryRow(Project project)
            : this(project.Name, project.GetTotalProjectTime(), project, null)
        {
        }

        /// <summary>
        /// Use this constructor if you have a project log instance.
        /// </summary>
        public SummaryRow(ProjectLog log)
            : this(log.Description, log.End - log.Start, null, log)
        {
        }

        /// <summary>
        /// Base constructor for all options.
        /// </summary>
        private SummaryRow(string title, TimeSpan time, Project project, ProjectLog log)
        {
            InitializeComponent();
            tbl_Name.Text = title;

            Time = time;
            SetTime();

            CurrProject = project;
            CurrLog = log;
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

        public Project CurrProject { get; private set; }
        public ProjectLog CurrLog { get; private set; }
    }
}

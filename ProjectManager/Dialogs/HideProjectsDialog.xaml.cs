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
    /// Interaction logic for HideProjectsDialog.xaml
    /// </summary>
    public partial class HideProjectsDialog : Window
    {
        public HideProjectsDialog()
        {
            InitializeComponent();
            Projects = ProjectOrganizer.Projects;
            FillUIWithProjects();
        }

        //----------------//
        // Helper Methods //
        //----------------//
        /// <summary>
        /// Fills the UI with the projects and checks the necessary check boxes.
        /// </summary>
        private void FillUIWithProjects()
        {
            List<Project> projects = ProjectOrganizer.SortProjects(Projects, UserSettings.ProjectSortingMethod);
            foreach (var project in projects)
            {
                HideProjectRow row = new HideProjectRow(project);
                if (UserSettings.HiddenProjects.Contains(project))
                {
                    row.cb_Hide.IsChecked = true;
                }
                else
                {
                    row.cb_Hide.IsChecked = false;
                }
                sp_Entries.Children.Add(row);
            }
        }

        /// <summary>
        /// Applys the changes on the UI to the user settings.
        /// </summary>
        private void ApplyChangesToUserSettings()
        {
            // Clear and repopulate hidden projects list
            UserSettings.HiddenProjects.Clear();
            foreach (var item in sp_Entries.Children)
            {
                HideProjectRow row = item as HideProjectRow;
                if (row.cb_Hide.IsChecked == true)
                {
                    UserSettings.HiddenProjects.Add(row.CurrProject);
                }
            }
        }

        /// <summary>
        /// Sets all projects on the UI to be eiter hidden or show
        /// </summary>
        /// <param name="projectHidden">Whether to set the projects as hidden or shown.</param>
        private void SetAllProjectsToHiddenOrShown(bool projectHidden)
        {
            foreach (var item in sp_Entries.Children)
            {
                HideProjectRow row = item as HideProjectRow;
                row.cb_Hide.IsChecked = projectHidden;
            }
        }

        //---------------//
        // Event Handles //
        //---------------//
        private void b_Save_Click(object sender, RoutedEventArgs e)
        {
            ApplyChangesToUserSettings();
            DialogResult = true;
        }

        private void b_Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void b_ShowAll_Click(object sender, RoutedEventArgs e)
        {
            SetAllProjectsToHiddenOrShown(false);
        }

        private void b_HideAll_Click(object sender, RoutedEventArgs e)
        {
            SetAllProjectsToHiddenOrShown(true);
        }

        //------//
        // Data //
        //------//
        public List<Project> Projects { get; private set; }
    }
}

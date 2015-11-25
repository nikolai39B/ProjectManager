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
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : UserControl
    {
        // TODO: Other things to implement
        // Inter-project notes file
        // Add custom sorting to home page projects

        public Home()
        {
            InitializeComponent();
            RefreshProjectsOnUI();
        }

        //------------------//
        // External Methods //
        //------------------//
        /// <summary>
        /// Switches the UserControl to the project menu.
        /// </summary>
        /// <param name="project">The project whose project menu to switch to.</param>
        public void GoToProjectMenu(Project project)
        {
            Window parent = Window.GetWindow(this);
            parent.Content = new ProjectMenu(project);
        }

        //----------------//
        // Helper Methods //
        //----------------//
        /// <summary>
        /// Creates a new project with the given name.
        /// </summary>
        /// <param name="projectName">The name of the new project to create and add.</param>
        private void AddProject(string projectName)
        {
            Project newProject = ProjectOrganizer.CreateNewProject(projectName);
            ProjectFileInterface.WriteProjectsToProjectListFile();
            GoToProjectMenu(newProject);
        }

        /// <summary>
        /// Refreshes the projects on the UI based on the projects currently in the
        /// ProjectOrganizer.
        /// </summary>
        private void RefreshProjectsOnUI()
        {
            List<HomeProjectRow> newProjectRows = new List<HomeProjectRow>();
            List<Project> projects = ProjectOrganizer.Projects;

            try
            {
                projects = ProjectOrganizer.SortProjects(projects, UserSettings.ProjectSortingMethod);
            }
            catch (ArgumentException)
            {
                ErrorLogger.AddLog(string.Format("Invalid sorting method '{0}' provided. Cannot sort projects.", UserSettings.ProjectSortingMethod), ErrorSeverity.LOW);
            }

            // Create and add the new project rows
            bool addToLeft = true;
            sp_ProjectsLeft.Children.Clear();
            sp_ProjectsRight.Children.Clear();
            foreach (var project in projects)
            {
                // If we are hiding this project, continue
                if (UserSettings.HiddenProjects.Contains(project))
                {
                    continue;
                }

                HomeProjectRow row = new HomeProjectRow(project, this);
                if (addToLeft)
                {
                    sp_ProjectsLeft.Children.Add(row);
                    addToLeft = false;
                }
                else
                {
                    sp_ProjectsRight.Children.Add(row);
                    addToLeft = true;
                }
            }
        }

        //---------------//
        // Event Handles //
        //---------------//
        private void b_NewProject_Click(object sender, RoutedEventArgs e)
        {
            NewProjectDialog newProjDialog = new NewProjectDialog();
            if (newProjDialog.ShowDialog() == true)
            {
                string newProjectName = newProjDialog.NewProjectName;
                AddProject(newProjectName);
            }
        }

        private void b_Summary_Click(object sender, RoutedEventArgs e)
        {
            // Query all the projects
            List<SummaryRow> items = new List<SummaryRow>();
            //TimeSpan totalTime = new TimeSpan(0);
            foreach (var project in ProjectOrganizer.SortProjects(ProjectOrganizer.Projects, UserSettings.ProjectSortingMethod))
            {
                //TimeSpan projTime = project.GetTotalProjectTime();
                items.Add(new SummaryRow(project));
                //totalTime += projTime;
            }
            //SummaryRow total = new SummaryRow("Total Time:", totalTime);
            
            // Display the window, but no need to wait for a response
            SummaryDialog window = new SummaryDialog("All Projects", items, true);
            window.Show();
        }

        private void b_More_Click(object sender, RoutedEventArgs e)
        {
            Window parent = Window.GetWindow(this);
            parent.Content = new MoreOptions();
        }

        private void b_Quit_Click(object sender, RoutedEventArgs e)
        {
            //ProjectFileInterface.WriteProjectsToListFile(ProjectOrganizer.Projects);
            Window.GetWindow(this).Close();                  
        }
    }
}

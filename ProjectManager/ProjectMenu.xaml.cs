using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for ProjectMenu.xaml
    /// </summary>
    public partial class ProjectMenu : UserControl
    {
        public ProjectMenu(Project project)
        {
            InitializeComponent();
            CurrentProject = project;
            tbl_Title.Text = project.Name;

            // Update the UI
            RefreshLogsOnUI();
            RefreshFilesOnUI();

            // Make the title realign everytime it's changed
            DependencyPropertyDescriptor dp = DependencyPropertyDescriptor.FromProperty(TextBlock.ActualWidthProperty, typeof(TextBlock));
            dp.AddValueChanged(tbl_Title, (object sender, EventArgs e) =>
            {
                SetIdealTitleAlignment();
            });
        }

        //------------------//
        // External Methods //
        //------------------//
        /// <summary>
        /// Refreshes the UI with the logs stored in the current project.
        /// </summary>
        public void RefreshLogsOnUI()
        {
            CurrentProject.SortCompletedEntries();
            sp_Logs.Children.Clear();
            PopulateUIWithProjectLogs();
        }

        /// <summary>
        /// Refreshes the UI with the files stored in the current project.
        /// </summary>
        public void RefreshFilesOnUI()
        {
            CurrentProject.SortFiles();
            sp_Files.Children.Clear();
            PopulateUIWithProjectFiles();
        }
        
        //----------------//
        // Helper Methods //
        //----------------//
        /// <summary>
        /// Populates the UI with the logs currently stored in the project reference.
        /// Does not clear out any currently displayed logs.
        /// </summary>
        private void PopulateUIWithProjectLogs()
        {
            // Handle the incomplete log (or lack of an incomplete log)
            if (CurrentProject.IncompleteLog != null)
            {
                // Allow the user to finish the log
                b_NewLog.Visibility = System.Windows.Visibility.Collapsed;
                g_FinishEntry.Visibility = System.Windows.Visibility.Visible;

                // Add the incomplete log to the bar
                tbl_IncompleteLogDate.Text = string.Format("{0}/{1}/{2}",
                    CurrentProject.IncompleteLog.Start.Month.ToString("D2"),
                    CurrentProject.IncompleteLog.Start.Day.ToString("D2"),
                    CurrentProject.IncompleteLog.Start.Year.ToString("D4"));
                tbl_IncompleteLogDescription.Text = CurrentProject.IncompleteLog.Description;
            }
            else
            {
                // Allow the user to create a new log
                b_NewLog.Visibility = System.Windows.Visibility.Visible;
                g_FinishEntry.Visibility = System.Windows.Visibility.Collapsed;
            }

            // Handle the completed logs
            foreach (var log in CurrentProject.CompletedLogs)
            {
                sp_Logs.Children.Add(new ProjectLogRow(log, this));
            }
        }

        /// <summary>
        /// Populates the UI with the files currently stored in the project reference.
        /// Does not clear out any currently displayed files.
        /// </summary>
        private void PopulateUIWithProjectFiles()
        {
            foreach (var file in CurrentProject.Files)
            {
                sp_Files.Children.Add(new ProjectFileRow(file, this));
            }
        }

        /// <summary>
        /// Creates and adds a new log with the given description. The start date time is
        /// automatically added. This method should not be called if the project already has a
        /// log in progress.
        /// </summary>
        /// <param name="description">The description for the new log.</param>
        private void StartLog(string description)
        {
            // Make sure we don't already have an incomplete log outstanding
            if (CurrentProject.IncompleteLog != null)
            {
                ErrorLogger.AddLog(string.Format("Cannot start a log while there is still a incomplete log outstanding. Log '{0}' ignored.", description), ErrorSeverity.HIGH);
                return;
            }

            // Create the new log
            DateTime start = DateTime.Now;
            int id = ProjectFileInterface.RequestLogId(CurrentProject.Id);
            ProjectLog log = new ProjectLog(id, start, description, CurrentProject);

            // Add the log to the project
            AddIncompleteLogToProject(log);

            // Refresh the UI
            RefreshLogsOnUI();
        }

        /// <summary>
        /// Finished the given log by adding the end time and updating the project references.
        /// </summary>
        /// <param name="log">The log to finish.</param>
        private void FinishLog(ProjectLog log)
        {
            if (log == null)
            {
                ErrorLogger.AddLog("Cannot finish a null log.", ErrorSeverity.HIGH);
                return;
            }

            // End the log
            log.End = DateTime.Now;

            // Add the log to the project
            AddCompleteLogToProject(log);

            // Refresh the UI
            RefreshLogsOnUI();
        }

        /// <summary>
        /// Adds the given log to the project (if it is not already there). Will not add if the project already has an
        /// IncompleteEntry reference.
        /// </summary>
        /// <param name="log">The log to add.</param>
        private void AddIncompleteLogToProject(ProjectLog log)
        {
            if (CurrentProject.IncompleteLog != null && CurrentProject.IncompleteLog != log)
            {
                ErrorLogger.AddLog(string.Format("Cannot add incomplete log '{0}', as project '{1}' already has a different incomplete log '{2}'.",
                    log.Description, CurrentProject.Name, CurrentProject.IncompleteLog.Description), ErrorSeverity.HIGH);
                return;
            }

            // Add the reference in the project
            CurrentProject.IncompleteLog = log;
        }

        /// <summary>
        /// Adds the given log to the project (if it is not already there) and the UI. Will null the project's IncompleteEntry reference
        /// if it is currently referencing the given log.
        /// </summary>
        /// <param name="log">The log to add.</param>
        private void AddCompleteLogToProject(ProjectLog log)
        {
            if (log.End == DateTime.MaxValue)
            {
                ErrorLogger.AddLog(string.Format("Cannot add log '{0}', as it is not complete.", log.Description), ErrorSeverity.HIGH);
                return;
            }

            // Remove this log as the current project incomplete log if necessary
            if (CurrentProject.IncompleteLog == log)
            {
                CurrentProject.IncompleteLog = null;
            }

            // Add and sort the entries if necessary
            if (!CurrentProject.CompletedLogs.Contains(log))
            {
                CurrentProject.CompletedLogs.Add(log);
                CurrentProject.SortCompletedEntries();
            }
        }

        /// <summary>
        /// Adds the given file to the project (if it is not already there).
        /// </summary>
        /// <param name="file">The file to add.</param>
        private void AddFileToProject(ProjectFile file)
        {
            // Add and sort the files if necessary
            if (!CurrentProject.Files.Contains(file))
            {
                CurrentProject.Files.Add(file);
                CurrentProject.SortFiles();
            }
        }

        /// <summary>
        /// Sets the horizontal alignment of the title based on its width.
        /// </summary>
        private void SetIdealTitleAlignment()
        {
            if (tbl_Title.ActualWidth >= this.ActualWidth)
            {
                tbl_Title.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            }
            else
            {
                tbl_Title.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            }
        }

        //---------------//
        // Event Handles //
        //---------------//
        void ProjectMenu_Loaded(object sender, RoutedEventArgs e)
        {
            SetIdealTitleAlignment();
        }

        private void b_NewLog_Click(object sender, RoutedEventArgs e)
        {
            NewLogDialog window = new NewLogDialog();
            if (window.ShowDialog() == true)
            {
                // We can't have a blank description or one that contains a pipe ('|', our delimiting character), 
                // so don't start a log with this quality. Note that NewLogDialog should already filter these out.
                if (window.NewLogDescription != "" && !window.NewLogDescription.Contains('|'))
                {
                    StartLog(window.NewLogDescription);
                }
                else
                {
                    ErrorLogger.AddLog(string.Format("Could not start a log with description '{0}' because it was either blank or contained pipes ('|')",
                         window.NewLogDescription), ErrorSeverity.HIGH);
                    return;
                }
            }
        }

        private void b_FinishLog_Click(object sender, RoutedEventArgs e)
        {
            FinishLog(CurrentProject.IncompleteLog);
        }

        private void b_AddFile_Click(object sender, RoutedEventArgs e)
        {
            AddFileDialog window = new AddFileDialog();
            if (window.ShowDialog() == true)
            {
                // Create the file
                ProjectFile newFile;
                string filename = window.IsFile ? window.Filename : window.Url;
                if (window.OpenWithDefaultProgram)
                {
                    newFile = new ProjectFile(window.FileTitle, filename, window.IsFile, CurrentProject);
                }
                else
                {
                    newFile = new ProjectFile(window.FileTitle, filename, window.IsFile, window.ProgramToOpen, CurrentProject);
                }

                // Add the file
                AddFileToProject(newFile);
                RefreshFilesOnUI();
            }
        }

        private void b_Files_Click(object sender, RoutedEventArgs e)
        {
            // Bar
            sp_LogBarItems.Visibility = System.Windows.Visibility.Collapsed;
            b_AddFile.Visibility = System.Windows.Visibility.Visible;

            // Toggle Buttons
            b_Files.Visibility = System.Windows.Visibility.Collapsed;
            b_Logs.Visibility = System.Windows.Visibility.Visible;

            // Content
            sp_Logs.Visibility = System.Windows.Visibility.Collapsed;
            sp_Files.Visibility = System.Windows.Visibility.Visible;
        }

        private void b_Logs_Click(object sender, RoutedEventArgs e)
        {
            // Bar
            sp_LogBarItems.Visibility = System.Windows.Visibility.Visible;
            b_AddFile.Visibility = System.Windows.Visibility.Collapsed;

            // Toggle Buttons
            b_Files.Visibility = System.Windows.Visibility.Visible;
            b_Logs.Visibility = System.Windows.Visibility.Collapsed;

            // Content
            sp_Logs.Visibility = System.Windows.Visibility.Visible;
            sp_Files.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void b_OpenNotes_Click(object sender, RoutedEventArgs e)
        {
            ProjectFileInterface.OpenNotesFile(CurrentProject);
        }

        private void b_Summary_Click(object sender, RoutedEventArgs e)
        {
            // Query the logs
            List<SummaryRow> items = new List<SummaryRow>();
            foreach (var log in CurrentProject.CompletedLogs)
            {
                items.Add(new SummaryRow(log));
            }

            // Show the summary window, but no need to wait for a response
            SummaryDialog window = new SummaryDialog("Project Summary", items, false);
            window.Show();
        }

        private void b_EditProject_Click(object sender, RoutedEventArgs e)
        {
            // Prompt the user for changes
            EditProjectDialog window = new EditProjectDialog(CurrentProject);

            if (window.ShowDialog() == true)
            { 
                // Delete the project if necessary
                if (window.DeleteProject)
                {
                    ProjectOrganizer.RemoveProject(CurrentProject);
                    Window parent = Window.GetWindow(this);
                    parent.Content = new Home();
                    return;
                }

                // Refresh the project's name on the UI if necessary
                if (window.RefreshProjectNameOnUI)
                {
                    tbl_Title.Text = CurrentProject.Name;
                }

                // Refresh the project's data on the UI if necessary
                if (window.RefreshProjectDataOnUI)
                {
                    RefreshLogsOnUI();
                    RefreshFilesOnUI();
                }
            }
        }

        private void b_Back_Click(object sender, RoutedEventArgs e)
        {
            Window parent = Window.GetWindow(this);
            parent.Content = new Home();
        }

        //------//
        // Data //
        //------//
        public Project CurrentProject { get; private set; }
    }
}

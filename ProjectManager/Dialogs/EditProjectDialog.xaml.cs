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
    /// Interaction logic for EditProjectDialog.xaml
    /// </summary>
    public partial class EditProjectDialog : Window
    {
        public EditProjectDialog(Project currentProject)
        {
            InitializeComponent();

            // Fill out the textblock with the current name
            tb_ProjectName.Text = currentProject.Name;

            // Set the caller action flags
            DeleteProject = false;
            RefreshProjectNameOnUI = false;

            // Set the other data
            CurrentProject = currentProject;
            defaultProjectName = currentProject.Name;
        }

        //----------------//
        // Helper Methods //
        //----------------//
        /// <summary>
        /// Changes the project's name based on the value in the UI field.
        /// </summary>
        /// <returns>True if the name was changed, false otherwise.</returns>
        private bool ChangeName()
        {
            // Get the new name
            string oldName = CurrentProject.Name;
            string newName = tb_ProjectName.Text;

            // Make sure we're actually changing the name
            if (oldName == newName)
            {
                // This is still counted as a success; we "successfully didn't" change the name, and the user should have
                // the result that he or she wants.
                return true;
            }

            // If there's issues, tell the user
            if (newName == "" || newName.Contains('|'))
            {
                return false;
            }

            // Update the project data and files
            CurrentProject.Name = newName;
            ProjectFileInterface.UpdateFilenamesForNewProjectName(CurrentProject, oldName);
            ProjectFileInterface.WriteProjectsToProjectListFile();

            // Log for UI change (caller should refresh the UI to match the new name)
            RefreshProjectNameOnUI = true;
            return true;
        }

        /// <summary>
        /// Clears the current project's data based on the checkboxes on the UI.
        /// </summary>
        private bool ClearData()
        {
            // Get the items to clear from the UI
            bool clearNotes = cb_ClearNotes.IsChecked == true;
            bool clearLogs = cb_ClearLogs.IsChecked == true;
            bool clearFiles = cb_ClearFiles.IsChecked == true;

            // Make sure we're clearing something
            if (!clearNotes && !clearLogs && !clearFiles)
            {
                // This is still counted as a success; we "successfully didn't" clear data, and the user should have
                // the result that he or she wants.
                return true;
            }

            // Confirm action with the user
            StringBuilder itemsToClearText = new StringBuilder();
            if (clearNotes)
            {
                itemsToClearText.Append(" - Project Notes\n");
            }
            if (clearLogs)
            {
                itemsToClearText.Append(" - Project Logs\n");
            }
            if (clearFiles)
            {
                itemsToClearText.Append(" - Project Files\n");
            }
            string confirmationText = string.Format("Are you sure you wish to clear the following items?\n{0}", itemsToClearText.ToString().TrimEnd());

            ConfirmationDialog window = new ConfirmationDialog(confirmationText);

            // Clear the data
            if (window.ShowDialog() == true)
            {
                RefreshProjectDataOnUI = true;

                // Notes
                if (clearNotes)
                {
                    ProjectFileInterface.ResetNotesFile(CurrentProject);
                }

                // Logs
                if (clearLogs)
                {
                    CurrentProject.IncompleteLog = null;
                    CurrentProject.CompletedLogs = new List<ProjectLog>();
                }

                // Files
                if (clearFiles)
                {
                    CurrentProject.Files = new List<ProjectFile>();
                }

                return true;
            }

            // Don't clear the data
            else
            {
                // The user may want to select a different set of data to clear
                return false;
            }
        }

        //---------------//
        // Event Handles //
        //---------------//
        private void b_ResetName_Click(object sender, RoutedEventArgs e)
        {
            tb_ProjectName.Text = defaultProjectName;
        }

        private void b_Apply_Click(object sender, RoutedEventArgs e)
        {
            // Attempt to change the name
            bool success = ChangeName();
            if (!success)
            {
                // If we failed, let the user know
                tbl_ErrorMessage.Visibility = System.Windows.Visibility.Visible;
                tbl_ErrorMessage.Text = "Project name cannot be blank and cannot contain pipes ('|').";
                return;
            }
            else
            {
                tbl_ErrorMessage.Visibility = System.Windows.Visibility.Collapsed;
            }

            // Attempt to clear the data
            success = ClearData();
            if (!success)
            {
                // If we failed, let the user know
                tbl_ErrorMessage.Visibility = System.Windows.Visibility.Visible;
                tbl_ErrorMessage.Text = "Issue clearing project data.";

                // Also change the name back
                tb_ProjectName.Text = defaultProjectName;
                ChangeName();

                return;
            }
            else
            {
                tbl_ErrorMessage.Visibility = System.Windows.Visibility.Collapsed;
            }

            // Return from the dialog
            DialogResult = true;
        }

        private void b_Delete_Click(object sender, RoutedEventArgs e)
        {
            ConfirmationDialog window = new ConfirmationDialog("Are you sure you wish to delete this project?");
            if (window.ShowDialog() == true)
            {
                // Caller is responsible for deleting the project  
                DeleteProject = true;
                DialogResult = true;
            }
        }

        private void b_Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        //------//
        // Data //
        //------//
        public bool DeleteProject { get; private set; }
        public bool RefreshProjectNameOnUI { get; private set; }
        public bool RefreshProjectDataOnUI { get; private set; }

        public Project CurrentProject { get; private set; }
        private string defaultProjectName;
    }
}

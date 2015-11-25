using System;
using System.Collections.Generic;
using System.IO;
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

namespace ProjectManager
{
    /// <summary>
    /// Interaction logic for MoreOptions.xaml
    /// </summary>
    public partial class MoreOptions : UserControl
    {
        // TODO: Add hide project functionality
        
        public MoreOptions()
        {
            InitializeComponent();

            // Create the settings maps
            InitUIControlMaps();

            // Set up the settings on the UI
            SetUpUIFromUserSettings();
        }

        //------------------//
        // External Methods //
        //------------------//
        /// <summary>
        /// Applies the settings from the UI to the static UserSettings class.
        /// </summary>
        public void ApplySettingsToUserSettings()
        {
            foreach (var pair in radioButtonToSortingMethod)
            {
                // If this rb is checked, set it in the UserSettings
                if (pair.Key.IsChecked == true)
                {
                    UserSettings.ProjectSortingMethod = pair.Value;
                }
            }

            UserSettings.DebugModeOn = cb_DebugMode.IsChecked == true;

            UserSettings.UseCustomTemplates = cb_CustomTemplates.IsChecked == true;
            UserSettings.AddTimestampToNotes = cb_TimestampNotes.IsChecked == true;
            UserSettings.DisplayIncompleteLogWarning = cb_IncompleteLogWarning.IsChecked == true;

            // Note: data directory is saved to UserSettings immediately when changed
            // Note: hidden projects dialog will save it's changes directly to UserSettings
        }

        //----------------//
        // Helper Methods //
        //----------------//
        /// <summary>
        /// Applies the settings from the static UserSettings class to the UI.
        /// </summary>
        private void SetUpUIFromUserSettings()
        {
            foreach (var pair in sortingMethodToRadioButton)
            {
                // If this SortingMethod is selected, check the corresponding rb
                pair.Value.IsChecked = pair.Key == UserSettings.ProjectSortingMethod;
            }

            cb_DebugMode.IsChecked = UserSettings.DebugModeOn;

            cb_CustomTemplates.IsChecked = UserSettings.UseCustomTemplates;
            cb_TimestampNotes.IsChecked = UserSettings.AddTimestampToNotes;
            cb_IncompleteLogWarning.IsChecked = UserSettings.DisplayIncompleteLogWarning;

            // Note that calling this will also set the UI
            DataDirectory = UserSettings.DataDirectory; //tbl_DataDirectory.Text = new DirectoryInfo(UserSettings.DataDirectory).Name;
        }

        /// <summary>
        /// Initializes the dictionaries that map settings to UI controls.
        /// </summary>
        private void InitUIControlMaps()
        {
            sortingMethodToRadioButton = new Dictionary<SortingMethod, RadioButton>()
            {
                { SortingMethod.OLD_FIRST, rb_OldFirst },
                { SortingMethod.NEW_FIRST, rb_NewFirst },
                { SortingMethod.NAME_A_TO_Z, rb_NameAToZ },
                { SortingMethod.NAME_Z_TO_A, rb_NameZToA }
            };

            radioButtonToSortingMethod = new Dictionary<RadioButton, SortingMethod>()
            {
                { rb_OldFirst, SortingMethod.OLD_FIRST },
                { rb_NewFirst, SortingMethod.NEW_FIRST },
                { rb_NameAToZ, SortingMethod.NAME_A_TO_Z },
                { rb_NameZToA, SortingMethod.NAME_Z_TO_A }
            };
        }

        /// <summary>
        /// Gets a folder from the user using the Windows Folder dialog.
        /// </summary>
        /// <param name="description">The description for thew folder browser dialog.</param>
        /// <param name="startingFolder">The folder to start the user in.</param>
        private string GetFolderFromUser(string description = "Please select a folder.", string startingFolder = null)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();

            dialog.Description = description;
            if (startingFolder != null)
            {
                dialog.RootFolder = System.Environment.SpecialFolder.DesktopDirectory;
                dialog.SelectedPath = startingFolder;
            }

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string dir = dialog.SelectedPath;
                if (Directory.Exists(dir))
                {
                    // This will also refresh the UI
                    return dir;
                }
            }

            return null;
        }

        /// <summary>
        /// Change's the directory for the project data.
        /// </summary>
        /// <param name="newDir">The new directory for the data.</param>
        private void ChangeDataDirectory(string newDir)
        {
            string oldDir = DataDirectory;

            // Check to see if new dir is empty
            if (Directory.EnumerateFiles(newDir).Any())
            {
                // If it's not empty, warn the user that overwrites may occur
                ConfirmationDialog dialog = new ConfirmationDialog(string.Format(
                    "{0} is not empty. Any files it contains may be overwritten. Proceed?", newDir));
                if (dialog.ShowDialog() != true)
                {
                    // If the user opted not to proceed, abort the operation
                    return;
                }
            }
            
            // Make sure all the project notes are saved before we copy
            NotificationDialog notifDialog = new NotificationDialog("Save Data", "Please save all project notes files. Press OK when all files are saved.");
            notifDialog.ShowDialog();

            // Write all project data to make sure that it's current
            ProjectFileInterface.WriteAllProjectData();

            // Copy the data to the new directory
            CopyDirectoryRecursively(oldDir, newDir, true);

            // At the end, set the new directory and rename the files in PFI
            DataDirectory = newDir;
            UserSettings.DataDirectory = newDir;
            ProjectFileInterface.RenameFullFilenamesForDataDirectoryChange();

            // Let the user know that the operation was successful
            notifDialog = new NotificationDialog("Success", "Data directory successfully changed.");
            notifDialog.ShowDialog();
        }

        /// <summary>
        /// Backup the user's data to the backup directory.
        /// </summary>
        private void BackupData()
        {
            // Get the id for this backup
            int id = ProjectFileInterface.RequestBackupId();
            if (id <= -1)
            {
                NotificationDialog errorDialog = new NotificationDialog("Backup Failed",
                    "Error creating new id for this backup. Please check the log file for more details.");
                errorDialog.ShowDialog();
                return;
            }

            // Create the backup dirs
            string backupDir = Path.Combine(DataDirectory, string.Format("backup\\backup_{0}", id));
            Directory.CreateDirectory(backupDir);

            // Write all project data to make sure that it's current
            ProjectFileInterface.WriteAllProjectData();            
            
            // Copy the data to the backup location
            CopyDirectoryRecursively(DataDirectory, backupDir, true);

            // Delete any backups that we copied
            string copiedBackupsDir = Path.Combine(backupDir, "backup");
            Directory.Delete(copiedBackupsDir, true);

            // Let the user know that the operation succeeded
            NotificationDialog dialog = new NotificationDialog("Backup Successful", string.Format(
                "Backup to\n\n{0}\n\nwas successful.", backupDir));
            dialog.ShowDialog();
        }

        /// <summary>
        /// Copys the directory structure and all files from sourceDir to targetDir.
        /// </summary>
        /// <param name="sourceDir">The directory to copy.</param>
        /// <param name="targetDir">The location to copy the files to.</param>
        /// <param name="overwriteFiles">Whether to overwrite any files that already exist in targetDir.</param>
        private void CopyDirectoryRecursively(string sourceDir, string targetDir, bool overwriteFiles)
        {
            // Create all of the directories
            foreach (string oldDirToCopy in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
            {
                string newDirToCreate = oldDirToCopy.Replace(sourceDir, targetDir);
                if (!Directory.Exists(newDirToCreate))
                {
                    Directory.CreateDirectory(newDirToCreate);
                }
            }

            // Copy all the files
            foreach (string oldFileToCopy in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
            {
                string newFileToCreate = oldFileToCopy.Replace(sourceDir, targetDir);
                File.Copy(oldFileToCopy, newFileToCreate, overwriteFiles);
            }
        }

        //---------------//
        // Event Handles //
        //---------------//
        private void b_HideProjects_Click(object sender, RoutedEventArgs e)
        {
            HideProjectsDialog window = new HideProjectsDialog();
            window.ShowDialog(); // the dialog should save any settings changes
        }

        private void b_ClearProjects_Click(object sender, RoutedEventArgs e)
        {
            ConfirmationDialog window = new ConfirmationDialog("Are you absolutely sure you want to delete all projects?");
            if (window.ShowDialog() == true)
            {
                ProjectOrganizer.RemoveAllProjects();
            }
        }

        private void b_OpenErrorLog_Click(object sender, RoutedEventArgs e)
        {
            ProjectFileInterface.OpenErrorLogFile();
        }

        private void b_BrowseForDataDirectory_Click(object sender, RoutedEventArgs e)
        {
            string newDir = GetFolderFromUser("Please select a folder for the project data.", DataDirectory);
            if (newDir == null)
            {
                return;
            }
            
            ChangeDataDirectory(newDir);
        }

        private void b_BackupData_Click(object sender, RoutedEventArgs e)
        {
            BackupData();
        }

        private void b_OpenNotes_Click(object sender, RoutedEventArgs e)
        {
            // Call this so that the user's possible timestamp setting change is honored
            ApplySettingsToUserSettings();

            ProjectFileInterface.OpenNotesFile();
        }

        private void b_Help_Click(object sender, RoutedEventArgs e)
        {
            ProjectFileInterface.OpenHelpFile();
        }

        private void b_Defaults_Click(object sender, RoutedEventArgs e)
        {
            ConfirmationDialog window = new ConfirmationDialog("Are you sure you wish to reset all settings to their default values?");
            if (window.ShowDialog() == true)
            {
                UserSettings.ResetToDefaults();
                SetUpUIFromUserSettings();
            }
        }

        private void b_Back_Click(object sender, RoutedEventArgs e)
        {
            ApplySettingsToUserSettings();
            Window parent = Window.GetWindow(this);
            parent.Content = new Home();            
        }
        
        //------//
        // Data //
        //------//
        private Dictionary<SortingMethod, RadioButton> sortingMethodToRadioButton;
        private Dictionary<RadioButton, SortingMethod> radioButtonToSortingMethod;

        private string dataDirectory;
        private string DataDirectory
        {
            get { return dataDirectory; }
            set
            {
                dataDirectory = value;
                tbl_DataDirectory.Text = new DirectoryInfo(value).Name;
            }
        }
    }
}

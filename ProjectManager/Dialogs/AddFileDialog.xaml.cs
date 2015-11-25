using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ProjectManager
{
    /// <summary>
    /// Interaction logic for AddFileDialog.xaml
    /// </summary>
    public partial class AddFileDialog : Window
    {
        public AddFileDialog()
        {
            InitializeComponent();
            tbl_Filename.Text = filenameDefaultText;
            tbl_ProgramToOpen.Text = programToOpenDefaultText;

            Filename = "";
            ProgramToOpen = "";
        }

        public AddFileDialog(ProjectFile file)
            : this()
        {
            // If we have a file, we're in edit mode
            CurrentFile = file;

            // Update the UI titles
            string newTitle = "View / Edit File";
            Title = newTitle;
            tbl_Title.Text = newTitle;
            tb_FileTitle.Text = file.FileTitle;

            // Process based on file type
            if (file.IsFile)
            {
                SelectFileAsType();
                ApplyNewFileToWindow(file.Filename);
            }
            else
            {
                SelectUrlAsType();
                ApplyNewUrlToWindow(file.Filename);
            }

            // Program to open
            if (file.ProgramToOpen != null)
            {
                ApplyNewProgramToOpenToWindow(file.ProgramToOpen);
                SetOpenWithDefaultStatus(false);
            }
            else
            {
                SetOpenWithDefaultStatus(true);
            }

            // Update the buttons
            g_AddButtons.Visibility = System.Windows.Visibility.Collapsed;
            g_EditButtons.Visibility = System.Windows.Visibility.Visible;
        }

        /// <summary>
        /// Adds the given file to the UI and backing fields.
        /// </summary>
        /// <param name="filePath">The file to apply.</param>
        private void ApplyNewFileToWindow(string filePath)
        {
            tbl_Filename.Text = Path.GetFileName(filePath);
            Filename = filePath;
        }

        /// <summary>
        /// Adds the given url to the UI and backing fields.
        /// </summary>
        /// <param name="url">The url to apply.</param>
        private void ApplyNewUrlToWindow(string url)
        {
            tb_Url.Text = url;
        }

        /// <summary>
        /// Adds the given program to open to the UI and backing fields.
        /// </summary>
        /// <param name="filePath">The program to apply.</param>
        private void ApplyNewProgramToOpenToWindow(string programPath)
        {
            tbl_ProgramToOpen.Text = Path.GetFileName(programPath);
            ProgramToOpen = programPath;
        }

        /// <summary>
        /// Selects 'File' as the type of file to add.
        /// </summary>
        private void SelectFileAsType()
        {
            rb_File.IsChecked = true;
            sp_Filename.Visibility = System.Windows.Visibility.Visible;
            g_Url.Visibility = System.Windows.Visibility.Collapsed;
        }

        /// <summary>
        /// Selects 'Url' as the type of file to add.
        /// </summary>
        private void SelectUrlAsType()
        {
            rb_Url.IsChecked = true;
            sp_Filename.Visibility = System.Windows.Visibility.Collapsed;
            g_Url.Visibility = System.Windows.Visibility.Visible;
        }

        /// <summary>
        /// Sets the open with default toggle either on or off.
        /// </summary>
        /// <param name="isChecked">Whether to turn the toggle on or off.</param>
        private void SetOpenWithDefaultStatus(bool isChecked)
        {
            cb_OpenWithDefault.IsChecked = isChecked;
            if (isChecked)
            {
                sp_ProgramToOpen.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                sp_ProgramToOpen.Visibility = System.Windows.Visibility.Visible;
            }
        }

        /// <summary>
        /// Checks the UI fields for errors and passes out the error string if errors occured.
        /// </summary>
        /// <param name="errors">The errors in the UI fields.</param>
        /// <returns>True if there were errors, false otherwise.</returns>
        private bool DoesUIContainErrors(out string errorText)
        {
            bool errors = false;
            errorText = "";

            // Check the necessary fields
            // Make sure the title is valid
            if (FileTitle == "" || FileTitle.Contains('|'))
            {
                errorText += "File title cannot be blank and cannot contain pipes ('|').\n";
                errors = true;
            }


            if (IsFile)
            {
                // Make sure filename is valid
                if (Filename == "")
                {
                    errorText += "Must select a file to open.\n";
                    errors = true;
                }
                else
                { 
                    try
                    {
                        Path.GetFullPath(Filename);
                    }
                    catch (ArgumentException)
                    {
                        errorText += "Filename is invalid.\n";
                        errors = true;
                    }
                }
            }
            else if (IsUrl)
            {
                // Make sure url is valid
                string url = Url;
                Uri uriResult;
                bool result = Uri.TryCreate(url, UriKind.Absolute, out uriResult) && uriResult.Scheme == Uri.UriSchemeHttp;

                // If we failed before, try adding 'http://'
                if (!result)
                {
                    url = "http://" + url;
                    result = Uri.TryCreate(url, UriKind.Absolute, out uriResult) && uriResult.Scheme == Uri.UriSchemeHttp;

                    // If it did work, update the text to the valid url
                    if (result)
                    {
                        tb_Url.Text = url;
                    }

                    // Otherwise, we have an error
                    else
                    {
                        errorText += "Url is invalid.\n";
                        errors = true;
                    }
                }
            }
            else
            {
                errorText += "Must select file or url type.\n";
                errors = true;
            }

            if (!OpenWithDefaultProgram)
            {
                // Make sure program to open is valid
                if (ProgramToOpen == "")
                {
                    errorText += "Must select a program to open the file with.\n";
                    errors = true;
                }
                else
                { 
                    try
                    {
                        Path.GetFullPath(ProgramToOpen);
                    }
                    catch (ArgumentException)
                    {
                        errorText += "Program to open filename is invalid.\n";
                        errors = true;
                    }
                }
            }

            // Clean up the error text
            errorText = errorText.TrimEnd('\r', '\n');

            return errors;
        }

        //---------------//
        // Event Handles //
        //---------------//
        private void rb_File_Click(object sender, RoutedEventArgs e)
        {
            SelectFileAsType();
        }

        private void rb_Url_Click(object sender, RoutedEventArgs e)
        {
            SelectUrlAsType();
        }

        private void cb_OpenWithDefault_Click(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            SetOpenWithDefaultStatus(checkBox.IsChecked == true);
        }

        private void b_Add_Click(object sender, RoutedEventArgs e)
        {
            string errorText;
            bool errors = DoesUIContainErrors(out errorText);

            // Note errors if necessary
            if (errors)
            {
                tbl_ErrorMessage.Text = errorText;
                tbl_ErrorMessage.Visibility = System.Windows.Visibility.Visible;
            }

            // Otherwise, return
            else
            {
                DialogResult = true;
            }
        }

        private void b_Save_Click(object sender, RoutedEventArgs e)
        {
            string errorText;
            bool errors = DoesUIContainErrors(out errorText);

            // Note errors if necessary
            if (errors)
            {
                tbl_ErrorMessage.Text = errorText;
                tbl_ErrorMessage.Visibility = System.Windows.Visibility.Visible;
            }

            // Otherwise, update the file and return
            else
            {
                CurrentFile.FileTitle = FileTitle;
                CurrentFile.ChangeFile(IsFile ? Filename : Url, IsFile);
                CurrentFile.ProgramToOpen = cb_OpenWithDefault.IsChecked == true ? null : ProgramToOpen;

                DialogResult = true;
            }
        }

        private void b_Delete_Click(object sender, RoutedEventArgs e)
        {
            ConfirmationDialog window = new ConfirmationDialog("Are you sure you wish to delete this file?");
            if (window.ShowDialog() == true)
            {
                // Delete the file and return
                CurrentFile.ParentProject.RemoveFileEntry(CurrentFile);
                DialogResult = true;
            }
        }

        private void b_Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void b_BrowseForFile_Click(object sender, RoutedEventArgs e)
        {
            // Try to get the initial directory if there is already a file specified
            string initialDirectory = null;
            if (IsFile && Filename != null && Filename != "" && Filename != filenameDefaultText)
            {
                try
                {
                    initialDirectory = Path.GetDirectoryName(Filename);
                }
                catch (Exception ex)
                {
                    // Catch only argument exceptions and path too long exceptions here.
                    // It's safe to ignore the exception in this case. Otherwise, rethrow.
                    if (!(ex is ArgumentException || ex is PathTooLongException))
                    {
                        throw ex;
                    }
                }
            }

            string file = ProjectFileInterface.GetFileWithWindowsDialog(".*", "All files (*.*)|*.*", initialDirectory);
            if (file != null)
            { 
                ApplyNewFileToWindow(file);
            }
        }

        private void b_BrowseForProgramToOpen_Click(object sender, RoutedEventArgs e)
        {
            // Try to get the initial directory if there is already a file specified
            string initialDirectory = null;
            if (ProgramToOpen != null && ProgramToOpen != "" && ProgramToOpen != filenameDefaultText)
            {
                try
                {
                    initialDirectory = Path.GetDirectoryName(ProgramToOpen);
                }
                catch (Exception ex)
                {
                    // Catch only argument exceptions and path too long exceptions here.
                    // It's safe to ignore the exception in this case. Otherwise, rethrow.
                    if (!(ex is ArgumentException || ex is PathTooLongException))
                    {
                        throw ex;
                    }
                }
            }

            string file = ProjectFileInterface.GetFileWithWindowsDialog(".*", "All files (*.*)|*.*", initialDirectory);
            if (file != null)
            {
                ApplyNewProgramToOpenToWindow(file);
            }
        }

        //----------//
        // Metadata //
        //----------//
        private const string filenameDefaultText = "File to open...";
        private const string programToOpenDefaultText = "Open with...";

        //------//
        // Data //
        //------//
        public bool IsFile
        {
            get { return rb_File.IsChecked == true; }
        }
        public bool IsUrl
        {
            get { return rb_Url.IsChecked == true; }
        }
        public bool OpenWithDefaultProgram
        {
            get { return cb_OpenWithDefault.IsChecked == true; }
        }
        public string FileTitle
        {
            get { return tb_FileTitle.Text; }
        }
        public string Filename { get; private set; }
        public string Url
        {
            get { return tb_Url.Text; }
        }
        public string ProgramToOpen { get; private set; }

        private ProjectFile CurrentFile { get; set; }
    }
}

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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // TODO:
        // Add backup feature
        // Warn on close if incomplete logs still present
        // Add files to home page (common files)
        // Move summary to more page
        // Add help file
        // Add rmb option on open notes to not add timestamp
        //    Change delete project to edit project and have it pop up a dialog to edit the name, clear logs, clear notes, clear files, or delete project
        // Add debug mode where errors will get displayed immediately.
        // Add custom data file location

        public MainWindow()
        {
            InitializeComponent();
            this.Content = new Home();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ProjectFileInterface.WriteProjectsToListFile(ProjectOrganizer.Projects);
            // If we quit from the 'More Options' screen, apply the UI settings
            if (Content is MoreOptions)
            {
                ((MoreOptions)Content).ApplySettingsToUserSettings();
            }
            ProjectFileInterface.WriteSettingsToFile();

            // If errors occured, tell the user
            if (ErrorLogger.ErrorsOccured)
            {
                NotificationDialog window = new NotificationDialog("Errors Occured", 
                    string.Format("Errors occured during this session. Please view the log file at\n{0}\nto see what went wrong.", ErrorLogger.ErrorLogFilename));
                window.ShowDialog();
            }
        }
    }
}

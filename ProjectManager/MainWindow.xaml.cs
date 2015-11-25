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
        // Add files to home page (common files)
        // Add backup feature
        // Warn on close if incomplete logs still present
        // Add custom data file location
        // Add option to disable timestamping
        // Add option to use custom templates

        public MainWindow()
        {
            InitializeComponent();
            this.Content = new Home();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // If we quit from the 'More Options' screen, apply the UI settings
            if (Content is MoreOptions)
            {
                ((MoreOptions)Content).ApplySettingsToUserSettings();
            }

            // Write the settings
            ProjectFileInterface.WriteSettingsToFile();

            // Check if any projects have incomplete logs
            if (UserSettings.DisplayIncompleteLogWarning)
            {
                foreach (var project in ProjectOrganizer.Projects)
                {
                    // If the project has an incomplete log, ask the user if they want to complete it
                    if (project.IncompleteLog != null)
                    {
                        ConfirmationDialog incLogDialog = new ConfirmationDialog(string.Format(
                            "Project '{0}' has an incomplete log. Do you wish to complete it?", project.Name));
                        
                        // If they do, finish it
                        if (incLogDialog.ShowDialog() == true)
                        {
                            project.FinishIncompleteLog();
                        }
                    }
                }
            }

            // Write the projects
            ProjectFileInterface.WriteAllProjectData();

            // If errors occured, tell the user
            if (ErrorLogger.ErrorsOccured)
            {
                NotificationDialog window = new NotificationDialog("Errors Occured", 
                    string.Format("Errors occured during this session. Please view the log file at\n\n{0}\n\nto see which errors occurred.", ErrorLogger.ErrorLogFilename));
                window.Show();
            }
        }
    }
}

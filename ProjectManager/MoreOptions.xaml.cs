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

        private void b_OpenNotes_Click(object sender, RoutedEventArgs e)
        {
            ProjectFileInterface.OpenNotesFile();
        }

        private void b_OpenErrorLog_Click(object sender, RoutedEventArgs e)
        {
            ProjectFileInterface.OpenErrorLogFile();
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
    }
}

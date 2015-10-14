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
        public MoreOptions()
        {
            InitializeComponent();
            InitSortingMethodAndRadioButtonMaps();
            SetUpUIFromUserSettings();
        }

        //----------------//
        // Helper Methods //
        //----------------//
        /// <summary>
        /// Initializes the dictionaries that map SortingMethods to
        /// RadioButtons.
        /// </summary>
        private void InitSortingMethodAndRadioButtonMaps()
        {
            sortingMethodToRadioButton = new Dictionary<SortingMethod, RadioButton>()
            {
                { SortingMethod.ID_LOW, rb_IdLow },
                { SortingMethod.ID_HIGH, rb_IdHigh },
                { SortingMethod.NAME_A_TO_Z, rb_NameAToZ },
                { SortingMethod.NAME_Z_TO_A, rb_NameZToA }
            };

            radioButtonToSortingMethod = new Dictionary<RadioButton, SortingMethod>()
            {
                { rb_IdLow, SortingMethod.ID_LOW },
                { rb_IdHigh, SortingMethod.ID_HIGH },
                { rb_NameAToZ, SortingMethod.NAME_A_TO_Z },
                { rb_NameZToA, SortingMethod.NAME_Z_TO_A }
            };
        }

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
        /// Applies the settings from the UI to the static UserSettings class.
        /// </summary>
        private void ApplySettingsToUserSettings()
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

        //---------------//
        // Event Handles //
        //---------------//
        private void b_OpenNotes_Click(object sender, RoutedEventArgs e)
        {

        }

        private void b_Defaults_Click(object sender, RoutedEventArgs e)
        {

        }

        private void b_ClearProjects_Click(object sender, RoutedEventArgs e)
        {
            ConfirmationDialog window = new ConfirmationDialog("Are you absolutely sure you want to delete all projects?");
            if (window.ShowDialog() == true)
            {
                ProjectOrganizer.RemoveAllProjects();
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

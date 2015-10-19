using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManager
{
    public static class UserSettings
    {
        static UserSettings()
        {
            // Init the maps
            InitStringMaps();

            // Set the settings in the class
            ResetToDefaults();
            GetSettingsFromFile();
        }

        //------------------//
        // External Methods //
        //------------------//
        public static void ResetToDefaults()
        {
            ProjectSortingMethod = SortingMethod.ID_LOW;
        }

        /// <summary>
        /// Sets the user settings based on the values stored in the settings file.
        /// </summary>
        public static void GetSettingsFromFile()
        {
            Dictionary<string, string> settings = ProjectFileInterface.GetAllSettingsFromFile();

            // Try to parse the values
            foreach (var pair in settings)
            {
                switch (pair.Key)
                {
                    case sortingMethodKey:
                        // Try to get the setting value from the string
                        if (stringToSortingMethod.Keys.Contains(pair.Value))
                        {
                            ProjectSortingMethod = stringToSortingMethod[pair.Value];
                        }
                        else
                        {
                            ErrorLogger.AddLog(string.Format("Could not parse value '{0}' for setting '{1}'.", pair.Value, pair.Key));
                        }
                        break;

                    default:
                        // Log if we didn't recognize a setting
                        ErrorLogger.AddLog(string.Format("Could not parse setting '{0}' with value '{1}'.", pair.Key, pair.Value));
                        return;
                }
            }
        }

        /// <summary>
        /// Gets the string corresponding to the given soring method. If the method is not recognized,
        /// adds an error the the log and returns the empty string.
        /// </summary>
        /// <param name="method">The method whose string to find.</param>
        /// <returns>The method's string.</returns>
        public static string GetStringFromSortingMethod(SortingMethod method)
        {
            if (sortingMethodToString.Keys.Contains(method))
            {
                return sortingMethodToString[method];
            }
            else
            {
                ErrorLogger.AddLog(string.Format("Did not recognized sorting method '{0}'.", method));
                return "";
            }
        }

        //----------------//
        // Helper Methods //
        //----------------//
        /// <summary>
        /// Initializes the dictionaries that map settings to strings.
        /// </summary>
        private static void InitStringMaps()
        {
            sortingMethodToString = new Dictionary<SortingMethod, string>()
            {
                { SortingMethod.ID_LOW, "id_low" },
                { SortingMethod.ID_HIGH, "id_high" },
                { SortingMethod.NAME_A_TO_Z, "name_a_to_z" },
                { SortingMethod.NAME_Z_TO_A, "name_z_to_a" }
            };

            stringToSortingMethod = new Dictionary<string, SortingMethod>()
            {
                { "id_low", SortingMethod.ID_LOW },
                { "id_high", SortingMethod.ID_HIGH },
                { "name_a_to_z", SortingMethod.NAME_A_TO_Z },
                { "name_z_to_a", SortingMethod.NAME_Z_TO_A }
            };
        }

        //------//
        // Data //
        //------//
        public static SortingMethod ProjectSortingMethod { get; set; }
        public static string ProjectSortingMethodString 
        {
            get { return GetStringFromSortingMethod(ProjectSortingMethod); }
        }

        // string keys
        public const string sortingMethodKey = "sorting_method";

        // string maps
        private static Dictionary<SortingMethod, string> sortingMethodToString;
        private static Dictionary<string, SortingMethod> stringToSortingMethod;
    }

    public enum SortingMethod
    {
        ID_LOW,
        ID_HIGH,
        NAME_A_TO_Z,
        NAME_Z_TO_A,

        NONE
    }
}

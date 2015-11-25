using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManager
{
    public static class UserSettings
    {
        static UserSettings()
        {
            // All initialization should be done explicitly by the initialization methods
        }

        /* Adding a User Setting
         * 
         * For all new settings:
         * - Add the field to store the value at the bottom of this file
         * - Add the string key for this setting at the bottom of this file
         * - Add default value in ResetToDefaults()
         * - Add handler for key in GetSettingsFromFile()
         * - Add handler for setting in WriteSettingsToFile() in ProjectFileInterface
         * - Add UI item to set/view the value
         * - Implement the value in the code
         * 
         * If the new setting has an enum:
         * - Define the enum at the bottom of this file
         * - Create maps to map the possible values to their string equivalents
        */

        //------------------//
        // External Methods //
        //------------------//
        /// <summary>
        /// Run the first part of the UserSettings initialization.
        /// 
        /// Depends on the settings file path being known (stored in the Project File Interface).
        /// 
        /// Completes the following tasks:
        /// - initializes the string/enum maps
        /// - sets the settings to their default values
        /// - loads the settings from the file
        /// - parses most settings
        /// </summary>
        public static void RunFirstInitialization()
        {
            // Initialize the string/enum maps
            InitStringMaps();

            // Set the settings to their default values
            ResetToDefaults();

            // Load the settings from the settings file
            LoadSettingsFromFile();
        }

        /// <summary>
        /// Run the second part of the UserSettings initialization.
        /// 
        /// Depends on the projects list having already been generated (stored in the Project Organizer).
        /// 
        /// Completes the following tasks:
        /// - parses the hidden projects settings value
        /// </summary>
        public static void RunSecondInitialization()
        {
            ParseHiddenProjectsSettingString();
        }

        /// <summary>
        /// Resets the user settings to their default values.
        /// </summary>
        public static void ResetToDefaults()
        {
            // NOTE: None of the default settings should ever rely on other Project Manager classes
            ProjectSortingMethod = SortingMethod.NEW_FIRST;

            HiddenProjects = new List<Project>();
            hiddenProjectsSettingValue = "";

            SummarySortByTime = false;
            SummaryIgnoreHiddenProjects = false;

            DebugModeOn = false;

            UseCustomTemplates = true;
            AddTimestampToNotes = true;
            DisplayIncompleteLogWarning = false;

            DataDirectory = Path.Combine(Environment.CurrentDirectory, "data");
        }

        /// <summary>
        /// Sets the user settings based on the values stored in the settings file.
        /// </summary>
        public static void LoadSettingsFromFile()
        {
            // Get the settings
            Dictionary<string, string> settings = ProjectFileInterface.GetSettingsFromFile();

            // Debug mode must be first, so ErrorLogger knows how to handle errors.
            // Before this point, ErrorLogger assumes that debug mode is off even if
            // the user really wanted it on.
            if (settings.ContainsKey(debugModeOnKey))
            {
                DebugModeOn = settings[debugModeOnKey].ToLower() == "true";

                // If errors occurred before now and we just turned debug mode on, notify the user
                if (DebugModeOn && ErrorLogger.ErrorsOccured)
                {
                    ErrorLogger.AddLog("Errors occurred before debug mode was enabled. Please view the log file for details.", ErrorSeverity.WARNING);
                }
            }

            // Now set up the directories, because a lot of things depend on them
            if (settings.ContainsKey(dataDirectoryKey))
            {
                string newDataDir = settings[dataDirectoryKey];
                if (Directory.Exists(newDataDir))
                { 
                    DataDirectory = newDataDir;
                }
                else
                {
                    ErrorLogger.AddLog(string.Format(
                        "Directory {0} does not exist. Cannot get data from this directory. Leaving data directory as default.", newDataDir), ErrorSeverity.HIGH);
                }
            }

            // Project sorting
            if (settings.ContainsKey(sortingMethodKey))
            {
                string sortingMethodString = settings[sortingMethodKey];
                if (stringToSortingMethod.ContainsKey(sortingMethodString))
                {
                    ProjectSortingMethod = stringToSortingMethod[sortingMethodString];
                }
                else
                {
                    ErrorLogger.AddLog(string.Format(
                        "No sorting method found for string '{0}'. Leaving sorting method as default.", sortingMethodString), ErrorSeverity.MODERATE);
                }
            }

            // Hidden projects
            if (settings.ContainsKey(hiddenProjectsKey))
            {
                hiddenProjectsSettingValue = settings[hiddenProjectsKey];
            }

            // Summary
            if (settings.ContainsKey(summarySortByTimeKey))
            {
                SummarySortByTime = settings[summarySortByTimeKey].ToLower() == "true";
            }
            if (settings.ContainsKey(summaryIgnoreHiddenProjectsKey))
            {
                SummaryIgnoreHiddenProjects = settings[summaryIgnoreHiddenProjectsKey].ToLower() == "true";
            }

            // Data
            if (settings.ContainsKey(useCustomTemplatesKey))
            {
                UseCustomTemplates = settings[useCustomTemplatesKey].ToLower() == "true";
            }
            if (settings.ContainsKey(addTimestampToNotesKey))
            {
                AddTimestampToNotes = settings[addTimestampToNotesKey].ToLower() == "true";
            }
            if (settings.ContainsKey(displayIncompleteLogWarningKey))
            {
                DisplayIncompleteLogWarning = settings[displayIncompleteLogWarningKey].ToLower() == "true";
            }

            // TODO: check for any keys that aren't recognized
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
                ErrorLogger.AddLog(string.Format("Did not recognized sorting method '{0}'.", method), ErrorSeverity.MODERATE);
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
                { SortingMethod.OLD_FIRST, "old_first" },
                { SortingMethod.NEW_FIRST, "new_first" },
                { SortingMethod.NAME_A_TO_Z, "name_a_to_z" },
                { SortingMethod.NAME_Z_TO_A, "name_z_to_a" }
            };

            stringToSortingMethod = new Dictionary<string, SortingMethod>()
            {
                { "old_first", SortingMethod.OLD_FIRST },
                { "new_first", SortingMethod.NEW_FIRST },
                { "name_a_to_z", SortingMethod.NAME_A_TO_Z },
                { "name_z_to_a", SortingMethod.NAME_Z_TO_A }
            };
        }

        /// <summary>
        /// Parses the line from the settings file corresponding to the hidden projects setting.
        /// </summary>
        /// <param name="settingValue">The value for the setting.</param>
        private static void ParseHiddenProjectsSettingString()
        {
            List<string> projectIdsToHide = hiddenProjectsSettingValue.Split('|').ToList();

            // If we have no projects to hide, return
            if (projectIdsToHide.Count == 1 && projectIdsToHide[0] == "")
            {
                return;
            }

            // Loop through all the hidden projects' ids
            foreach (var projectId in projectIdsToHide)
            {
                // Try to find the project with this id
                int idAsInt;
                Project project = null;
                bool success = int.TryParse(projectId, out idAsInt);
                if (success)
                {
                    project = ProjectOrganizer.GetProjectWithId(idAsInt);
                    if (project == null)
                    {
                        success = false;
                    }
                }

                // If we can't, log the error
                if (!success)
                {
                    ErrorLogger.AddLog(string.Format("Could not find any project with id '{0}'.", projectId), ErrorSeverity.MODERATE);
                }

                // Otherwise, note the setting
                else if (!HiddenProjects.Contains(project))
                {
                    HiddenProjects.Add(project);
                }
            }
        }

        //------//
        // Data //
        //------//
        // Project Sorting
        public static SortingMethod ProjectSortingMethod { get; set; }
        public static string ProjectSortingMethodString 
        {
            get { return GetStringFromSortingMethod(ProjectSortingMethod); }
        }

        // Project Hiding
        private static string hiddenProjectsSettingValue;
        public static List<Project> HiddenProjects { get; set; }

        // Summary Options
        public static bool SummarySortByTime { get; set; }
        public static bool SummaryIgnoreHiddenProjects { get; set; }

        // Error Options
        public static bool DebugModeOn { get; set; }

        // Data Options
        public static bool UseCustomTemplates { get; set; }
        public static bool AddTimestampToNotes { get; set; }
        public static bool DisplayIncompleteLogWarning { get; set; }

        public static string DataDirectory { get; set; }

        // String Keys
        public const string sortingMethodKey = "sorting_method";

        public const string hiddenProjectsKey = "hidden_projects";

        public const string summarySortByTimeKey = "summary_sort_by_time";
        public const string summaryIgnoreHiddenProjectsKey = "summary_ignore_hidden_projects";

        public const string debugModeOnKey = "debug_mode_on";

        public const string useCustomTemplatesKey = "use_custom_templates";
        public const string addTimestampToNotesKey = "add_timestamp_to_notes";
        public const string displayIncompleteLogWarningKey = "display_incomplete_log_warning";

        public const string dataDirectoryKey = "data_directory";

        // String Maps
        private static Dictionary<SortingMethod, string> sortingMethodToString;
        private static Dictionary<string, SortingMethod> stringToSortingMethod;
    }

    public enum SortingMethod
    {
        OLD_FIRST,
        NEW_FIRST,
        NAME_A_TO_Z,
        NAME_Z_TO_A,

        NONE
    }
}

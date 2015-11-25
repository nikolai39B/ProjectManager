using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManager
{
    static class ProgramInitializer
    {
        /// <summary>
        /// Initializes all of the static classes in the correct order. Basically, calling this method
        /// gets the program into a usable state. This method should be called exactly once at the very
        /// beginning of program execution.
        /// </summary>
        public static void InitializeProgram()
        {
            // Determines the settings file path
            ProjectFileInterface.RunFirstInitialization();
            
            // Determines the data file path
            UserSettings.RunFirstInitialization();

            // Puts PFI in a state to load projects
            ProjectFileInterface.RunSecondInitialization();

            // Loads projects
            ProjectOrganizer.RunInitialization();

            // Determines hidden projects
            UserSettings.RunSecondInitialization();
        }
    }
}

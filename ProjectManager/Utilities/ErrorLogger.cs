using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManager
{
    static class ErrorLogger
    {
        static ErrorLogger()
        {
            ErrorsOccured = false;

            // Get the full path for the log file and create it if necessary
            errorLogFilename = Path.Combine(Environment.CurrentDirectory, errorLogFilename);

            // Create all the necessary directories
            foreach (var dir in requiredDirectories)
            {
                string fullDir = Path.Combine(Environment.CurrentDirectory, dir);
                if (!Directory.Exists(fullDir))
                {
                    Directory.CreateDirectory(fullDir);
                }
            }

            // Make sure the error log file exists. Create it if necessary.
            if (!File.Exists(errorLogFilename))
            {
                File.WriteAllText(errorLogFilename, "");
            }
        }

        //------------------//
        // External Methods //
        //------------------//
        /// <summary>
        /// Adds the given string to the log file. Automatically adds a timestamp.
        /// </summary>
        /// <param name="log">The log string to add.</param>
        public static void AddLog(string log,
            [CallerMemberName] string currentFunction = "", 
            [CallerFilePath] string currentFile = "",
            [CallerLineNumber] int line = 0)
        {
            try
            {
                // Note that errors have occured
                ErrorsOccured = true;

                // Get the date time as a string and the current file contents
                string dateTimeNowString = DateTime.Now.ToString();
                string oldFileContents = File.ReadAllText(errorLogFilename);

                // Build the new file contents
                StringBuilder newFileContents = new StringBuilder();
                string lineOfDashes = new String('-', 50);

                // Header
                newFileContents.Append(string.Format("{0}\n", lineOfDashes));
                newFileContents.Append(dateTimeNowString);
                newFileContents.Append("\n");
                newFileContents.Append(log);

                // Immediate caller
                newFileContents.Append("\n\nImmediate Context:\n");
                newFileContents.Append(string.Format("    Method {0}()\n", currentFunction));
                newFileContents.Append(string.Format("    in {0}\n", currentFile));
                newFileContents.Append(string.Format("    @ line {0}\n", line));

                // Full callstack
                newFileContents.Append("\nCallstack:\n");
                newFileContents.Append(Environment.StackTrace);

                // End
                newFileContents.Append(string.Format("\n{0}\n\n", lineOfDashes));
                newFileContents.Append(oldFileContents);

                // Write the file
                File.WriteAllText(errorLogFilename, newFileContents.ToString());
            }
            catch (IOException e)
            {
                // If we get here, something went pretty wrong. Let the user know.
                NotificationDialog window = new NotificationDialog("Error",
                    string.Format("This program encountered an error, and the error couldn't get logged.\n\nInitial error:\n{0}\n\nError log write error:\n{1}\n\nIf this problem persists, please contact us and let us know that it's happening.", log, e.Message));

                window.ShowDialog();
            }
        }

        //------//
        // Data //
        //------//
        private static string errorLogFilename = "data\\errorLogs.txt";
        public static string ErrorLogFilename { get { return errorLogFilename; } }

        public static bool ErrorsOccured { get; private set; }

        private static string[] requiredDirectories = new string[] { "data" };
    }
}

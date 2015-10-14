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
                // Get the date time as a string and the current file contents
                string dateTimeNowString = DateTime.Now.ToString();
                string oldFileContents = File.ReadAllText(errorLogFilename);

                // Build the new file contents
                StringBuilder newFileContents = new StringBuilder();
                newFileContents.Append("-----\n");
                newFileContents.Append(dateTimeNowString);
                newFileContents.Append('\n');
                newFileContents.Append(string.Format("Method {0}() in {1} @ line {2}\n", currentFunction, currentFile, line));
                newFileContents.Append(log);
                newFileContents.Append("\n-----\n\n");
                newFileContents.Append(oldFileContents);

                // Write the file
                File.WriteAllText(errorLogFilename, newFileContents.ToString());
            }
            catch (IOException)
            {
                // TODO: Handle this exception
            }
        }

        private static string errorLogFilename = "data\\errorLogs.txt";
        private static string[] requiredDirectories = new string[] { "data" };
    }
}

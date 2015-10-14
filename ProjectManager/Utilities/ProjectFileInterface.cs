﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManager
{
    public static class ProjectFileInterface
    {
        static ProjectFileInterface()
        {
            // Init any fields that need it
            highestLogIdsInUse = new Dictionary<int,int>();

            // Get the full path for all files
            string currDirectory = Environment.CurrentDirectory;
            allProjectsFilename = Path.Combine(currDirectory, allProjectsFilename);
            projectLogsFilename = Path.Combine(currDirectory, projectLogsFilename);
            projectNotesFilename = Path.Combine(currDirectory, projectNotesFilename);
            projectFilesFilename = Path.Combine(currDirectory, projectFilesFilename);

            // Create all the necessary directories
            foreach (var dir in requiredDirectories)
            {
                string fullDir = Path.Combine(currDirectory, dir);
                if (!Directory.Exists(fullDir))
                {
                    Directory.CreateDirectory(fullDir);
                }
            }

            // Make sure the projects file exists. Create it if necessary.
            if (!File.Exists(allProjectsFilename))
            {
                File.WriteAllText(allProjectsFilename, allProjectsFileTemplate);
            }
        }

        //---------------//
        // Query Methods //
        //---------------//
        /// <summary>
        /// Parses the necessary files and returns a list of all projects.
        /// </summary>
        /// <returns>The list of projects.</returns>
        public static List<Project> GetAllProjectsFromFiles()
        {
            List<Project> projects = new List<Project>();

            // Get the non-empty and non-comment lines from the projects file
            List<string> projectRows = GetNonEmptyAndNonCommentLinesFromFile(allProjectsFilename);

            // Parse the remaining lines
            foreach (var line in projectRows)
            {
                // Ensure that the line has the correct format
                string[] lineItems = line.Split('|');
                int numberOfItems = 2;
                if (lineItems.Length != numberOfItems)
                {
                    ErrorLogger.AddLog(string.Format("Could not parse line '{0}'. Skipping entry.", line));
                    continue;
                }
                    
                // Ensure that the first part is a valid id
                int projectId;
                int minId = 0;
                bool parseIdSuccess = int.TryParse(lineItems[0], out projectId);
                if (!parseIdSuccess || projectId < minId)
                {
                    ErrorLogger.AddLog(string.Format("Could not parse project id '{0}'. Skipping entry.", lineItems[0]));
                    continue;
                }

                // Ensure that the second part is a valid project name
                string projectName = lineItems[1];
                if (projectName == "")
                {
                    ErrorLogger.AddLog(string.Format("Invalid project name '{0}'. Skipping entry.", lineItems[1]));
                    continue;
                }

                // If we got here, we have valid project info, so create an instance
                Project project = new Project(projectId, projectName);

                // Update the highest id fields if necessary
                highestProjectIdInUse = Math.Max(highestProjectIdInUse, projectId);
                highestLogIdsInUse[projectId] = -1;

                // Fill the instance out with the logs
                Tuple<List<ProjectLog>, ProjectLog> projectLogs = GetLogsForProject(project);
                project.CompletedLogs = projectLogs.Item1;
                project.IncompleteLog = projectLogs.Item2;

                // Fill the instance with the files
                project.Files = GetFilesForProject(project);

                // Add the reference
                projects.Add(project);
            }

            return projects;
        }

        /// <summary>
        /// Returns a tuple containing the a list of the project's completed logs and the project's incomplete log. 
        /// </summary>
        /// <param name="project">The project whose logs to return.</param>
        /// <returns>A tuple containg the project logs.</returns>
        public static Tuple<List<ProjectLog>, ProjectLog> GetLogsForProject(Project project)
        {
            List<ProjectLog> completeEntries = new List<ProjectLog>();
            ProjectLog incompleteEntry = null;

            // Get the non-empty and non-comment lines from the log file
            List<string> projectLogRows = GetNonEmptyAndNonCommentLinesFromFile(GetFileNameForProject(project, FileType.LOGS));
            Nullable<bool> parsingCompletedEntries = null;

            // Parse the remaining lines
            foreach (var line in projectLogRows)
            {
                // Check if we have a keywork (incomplete or complete)
                if (line == incomplete)
                {
                    parsingCompletedEntries = false;
                    continue;
                }
                else if (line == complete)
                {
                    parsingCompletedEntries = true;
                    continue;
                }

                // Make sure that we know what type of entry we are parsing
                if (parsingCompletedEntries == null)
                {
                    ErrorLogger.AddLog(string.Format("Could not determin with log '{0}' should be complete or incomplete. Skipping entry.", line));
                    continue;
                }

                // If we got here, then we should have a log entry
                // Ensure that the line has the correct format
                string[] lineItems = line.Split('|');
                int numberOfItems = 4;
                if (lineItems.Length != numberOfItems)
                {
                    ErrorLogger.AddLog(string.Format("Could not parse line '{0}'. Skipping entry.", line));
                    continue;
                }

                // Ensure that the first part is a valid id
                int logId;
                int minId = 0;
                bool parseIdSuccess = int.TryParse(lineItems[0], out logId);
                if (!parseIdSuccess || logId < minId)
                {
                    ErrorLogger.AddLog(string.Format("Could not parse log id '{0}'. Skipping entry.", lineItems[0]));
                    continue;
                }

                // Ensure that the second part is a valid datetime
                DateTime start = DateTime.MinValue;
                try
                { 
                    start = DateTime.Parse(lineItems[1]);
                }
                catch (Exception e)
                {
                    if (e is ArgumentNullException || e is FormatException)
                    {
                        ErrorLogger.AddLog(string.Format("Could not parse log start time '{0}'. Skipping entry.", lineItems[1]));
                        continue;
                    }
                    else
                    {
                        throw;
                    }
                }

                // Ensure that the third part is either blank (for incomplete entries) or a valid datetime (for complete entries)
                DateTime end;
                // If we're blank but parsing an incomplete entry, just set end to the default value
                if (lineItems[2] == "" && parsingCompletedEntries == false)
                {
                    end = DateTime.MaxValue;
                }
                // Otherwise, if we're not blank and parsing a complete entry, try to parse the value
                else if (lineItems[2] != "" && parsingCompletedEntries == true)
                { 
                    try
                    {
                        end = DateTime.Parse(lineItems[2]);
                    }
                    catch (FormatException)
                    {
                        ErrorLogger.AddLog(string.Format("Could not parse log end time '{0}'. Skipping entry.", lineItems[1]));
                        continue;
                    }
                }
                // Otherwise, we're invalid
                else
                {
                    ErrorLogger.AddLog(string.Format("Invalid end time '{0}' for log completion status ({1}). Skipping entry.", lineItems[1],
                        parsingCompletedEntries == true ? "complete" : "incomplete"));
                    continue;
                }

                // Ensure that the fourth part is a valid description
                string description = lineItems[3];
                if (description == "")
                {
                    ErrorLogger.AddLog(string.Format("Invalid log description '{0}'. Skipping entry.", lineItems[1]));
                    continue;
                }

                // If we got here, we have valid project log entry, so create an instance
                if (parsingCompletedEntries == true)
                { 
                    // Create a complete entry here
                    ProjectLog logEntry = new ProjectLog(logId, start, end, description, project);
                    highestLogIdsInUse[project.Id] = Math.Max(highestLogIdsInUse[project.Id], logId);
                    completeEntries.Add(logEntry);
                }
                else
                {
                    // Try to create an incomplete entry here, but don't allow multiple incomplete entries
                    if (incompleteEntry == null)
                    {
                        // Create an incomplete entry here
                        ProjectLog logEntry = new ProjectLog(logId, start, description, project);
                        highestLogIdsInUse[project.Id] = Math.Max(highestLogIdsInUse[project.Id], logId);
                        incompleteEntry = logEntry;
                    }
                    else
                    {
                        ErrorLogger.AddLog(string.Format("Only one incomplete entry is allowed. Additional entry '{0}' will be skipped.", description));
                        continue;                        
                    }
                }
            }

            return new Tuple<List<ProjectLog>,ProjectLog>(completeEntries, incompleteEntry);
        }

        /// <summary>
        /// Returns a list of the files associated with this project.
        /// </summary>
        /// <param name="project">The project whose files to return.</param>
        /// <returns>The list of project files.</returns>
        public static List<ProjectFile> GetFilesForProject(Project project)
        {
            List<ProjectFile> projectFiles = new List<ProjectFile>();

            // Get the non-empty and non-comment lines from the log file
            List<string> projectFileRows = GetNonEmptyAndNonCommentLinesFromFile(GetFileNameForProject(project, FileType.FILES));

            // Parse the remaining lines
            foreach (var line in projectFileRows)
            {
                // Ensure that the line has the correct format
                string[] lineItems = line.Split('|');
                int numberOfItems = 4;
                if (lineItems.Length != numberOfItems)
                {
                    ErrorLogger.AddLog(string.Format("Could not parse line '{0}'. Skipping entry.", line));
                    continue;
                }

                // Ensure that the first part is a valid title
                string fileTitle = lineItems[0];
                if (fileTitle == "")
                {
                    ErrorLogger.AddLog(string.Format("Could not parse file title. Value was '{0}'. Skipping entry.", fileTitle));
                    continue;
                }

                // Ensure that the second part is a "true" or "false"
                // Note: We do this before checking the filename because that check depends on this value.
                string isFileString = lineItems[2];
                if (isFileString != "true" && isFileString != "false")
                {
                    ErrorLogger.AddLog(string.Format("Could not parse file type. Value was '{0}'. Skipping entry.", isFileString));
                    continue;
                }
                bool isFile = isFileString == "true" ? true : false;

                // Ensure that the first part is a valid filepath
                string fileName = lineItems[1];
                if (isFile)
                { 
                    try
                    {
                        fileName = Path.GetFullPath(fileName);
                    }
                    catch (ArgumentException e)
                    {
                        ErrorLogger.AddLog(string.Format("Could not parse filename '{0}':\n{1}\nSkipping entry.", fileName, e.Message));
                        continue;
                    }
                }
                else
                {
                    Uri uriResult;
                    bool result = Uri.TryCreate(fileName, UriKind.Absolute, out uriResult) && uriResult.Scheme == Uri.UriSchemeHttp;
                    if (!result)
                    {
                        ErrorLogger.AddLog(string.Format("Could not parse url '{0}':\n{1}\nSkipping entry.", fileName));
                        continue;
                    }
                }
                
                // Ensure that the third part is a valid program or is blank
                string programToOpen = lineItems[3];
                if (programToOpen != "")
                {
                    try
                    {
                        programToOpen = Path.GetFullPath(programToOpen);
                    }
                    catch (ArgumentException e)
                    {
                        ErrorLogger.AddLog(string.Format("Could not parse filename '{0}':\n{1}\nSkipping entry.", programToOpen, e.Message));
                        continue;
                    }
                }

                // If we got here, we have valid project file, so create an instance
                ProjectFile projectFile = null;
                if (programToOpen == "")
                {
                    projectFile = new ProjectFile(fileTitle, fileName, isFile, project);
                }
                else
                {
                    projectFile = new ProjectFile(fileTitle, fileName, isFile, programToOpen, project);
                }

                projectFiles.Add(projectFile);
            }
            return projectFiles;
        }

        /// <summary>
        /// Generates and returns an unused integer id for a project.
        /// </summary>
        /// <returns>The new id.</returns>
        public static int RequestProjectId()
        {
            if (highestProjectIdInUse < 0)
            {
                highestProjectIdInUse = 0;
                return 0;
            }
            else
            {
                return ++highestProjectIdInUse;
            }
        }

        /// <summary>
        /// Generates and returns an unused integer id for a log for the given project.
        /// </summary>
        /// <param name="projectId">The id of the project that the log is for.</param>
        /// <returns>The new id.</returns>
        public static int RequestLogId(int projectId)
        {
            // If we haven't generated any keys yet for this project, return the first key 0
            if (!highestLogIdsInUse.Keys.Contains(projectId))
            {
                highestLogIdsInUse[projectId] = 0;
                return 0;
            }

            // Otherwise, behave as normal
            if (highestLogIdsInUse[projectId] < 0)
            {
                highestLogIdsInUse[projectId] = 0;
                return 0;
            }
            else
            {
                highestLogIdsInUse[projectId] += 1;
                return highestLogIdsInUse[projectId];
            }
        }

        //--------------//
        // File Methods //
        //--------------//
        /// <summary>
        /// Returns a list of non-empty and non-comment lines from the given file.
        /// </summary>
        /// <param name="filename">The file whose lines to return.</param>
        /// <returns>The list of lines.</returns>
        public static List<string> GetNonEmptyAndNonCommentLinesFromFile(string filename)
        {
            try
            {
                return (from l in File.ReadAllLines(filename)
                        let tl = l.Trim()
                        where !string.IsNullOrWhiteSpace(tl) && tl[0] != commentCharacter
                        select tl).ToList();
            }
            catch (IOException e)
            {
                ErrorLogger.AddLog(string.Format("Error parsing file '{0}':\n{1}", allProjectsFilename, e.Message));
                return new List<string>();
            }
        }

        /// <summary>
        /// Writes the given projects to the project list file. Can overwrite or append.
        /// </summary>
        /// <param name="projects">The projects to write.</param>
        /// <param name="append">Whether to append the file or overwrite it.</param>
        /// <param name="writeLogs">Whether to also write the logs for the projects.</param>
        public static void WriteProjectsToListFile(List<Project> projects, bool append = false, bool writeLogs = true, bool writeFiles = true)
        {
            StringBuilder newFileText = new StringBuilder();
            // If we should append, read the file and store its contents.
            if (append)
            {
                newFileText.Append(File.ReadAllText(allProjectsFilename));
            }
            // Otherwise, use the template
            else
            {
                newFileText.Append(allProjectsFileTemplate);
            }

            // Add the projects
            foreach (var project in projects.OrderBy(p => p.Id))
            {
                newFileText.Append(string.Format(allProjectsFileRow, project.Id, project.Name));

                // Also write the logs if necessary
                if (writeLogs)
                {
                    WriteLogsToFile(project);
                }

                // Also write the files if necessary
                if (writeFiles)
                {
                    WriteFilesToFile(project);
                }
            }

            try
            {
                // Write to the file
                File.WriteAllText(allProjectsFilename, newFileText.ToString());
            }
            catch (IOException e)
            {
                ErrorLogger.AddLog(string.Format("Could not write to the project list file. Error:\n{0}", e.Message));
            }
        }

        /// <summary>
        /// Creates (and possibly overwrites) the log, notes, and files files for the given project.
        /// </summary>
        /// <param name="project">The project whose files to create.</param>
        /// <param name="overwrite">Whether or not to overwrite the file if it exists already.</param>
        public static void CreateFilesForProject(Project project, bool overwrite = true)
        {
            // Log file
            string logFilename = GetFileNameForProject(project, FileType.LOGS);
            if (overwrite || !File.Exists(logFilename))
            {
                File.WriteAllText(logFilename, string.Format(projectLogsFileTemplate, project.Name, project.Id));
            }

            // Notes file
            string notesFilename = GetFileNameForProject(project, FileType.NOTES);
            if (overwrite || !File.Exists(notesFilename))
            {
                File.WriteAllText(notesFilename, string.Format(projectNotesFileTemplate, project.Name));
            }

            // Files file
            string filesFilename = GetFileNameForProject(project, FileType.FILES);
            if (overwrite || !File.Exists(filesFilename))
            {
                File.WriteAllText(filesFilename, string.Format(projectFilesFileTemplate, project.Name, project.Id));
            }
        }

        /// <summary>
        /// Updates the project logs, notes, and files filenames. This should be called right after the project name changes.
        /// </summary>
        /// <param name="project">The project (with its new name).</param>
        /// <param name="oldName">The project's old name.</param>
        public static void UpdateFilenamesForNewProjectName(Project project, string oldName)
        {
            // Make sure we have actually changed names
            if (oldName == project.Name)
            {
                ErrorLogger.AddLog(string.Format("No file update necessary; project '{0}' has not changed names.", oldName));
                return;
            }

            // Get the names for the old files
            string oldNotesFilename = GetFileNameForProject(oldName, project.Id, FileType.NOTES);
            string oldLogsFilename = GetFileNameForProject(oldName, project.Id, FileType.LOGS);
            string oldFilesFilename = GetFileNameForProject(oldName, project.Id, FileType.FILES);

            // Get the old notes and ready them for the new notes file
            StringBuilder notesText = new StringBuilder();
            try
            {
                // Form the file headers
                string oldNotesHeader = string.Format(projectNotesFileTemplate, oldName);
                string newNotesHeader = string.Format(projectNotesFileTemplate, project.Name);

                // Read in the notes text
                notesText.Append(File.ReadAllText(oldNotesFilename));

                // Remove the old header if possible
                if (notesText.ToString().IndexOf(oldNotesHeader) == 0)
                {
                    notesText.Remove(0, oldNotesHeader.Length);
                }

                // Add the new header
                notesText.Insert(0, newNotesHeader);
            }
            catch (IOException e)
            {
                ErrorLogger.AddLog(string.Format("Could not read project '{0}' old notes file after name change:\n{1}", project.Name, e.Message));
            }                       

            // Try to delete the old notes and log files
            try
            {
                File.Delete(oldNotesFilename);
                File.Delete(oldLogsFilename);
                File.Delete(oldFilesFilename);
            }
            catch (IOException e)
            {
                ErrorLogger.AddLog(string.Format("Could not delete old project '{0}' file after name change:\n{1}", project.Name, e.Message));
            }
            
            // Create the new files and write to them
            CreateFilesForProject(project);
            try
            {
                // Notes file
                string newNotesFilename = GetFileNameForProject(project, FileType.NOTES);
                File.WriteAllText(newNotesFilename, notesText.ToString());

                // Logs file
                WriteLogsToFile(project);

                // Files file
                WriteFilesToFile(project);
            }
            catch (IOException e)
            {
                ErrorLogger.AddLog(string.Format("Error writing to new project '{0}' file after name change:\n{1}\n\nProject Notes:\n{2}",
                    project.Name, e.Message, notesText.ToString()));
            }
        }

        /// <summary>
        /// Delete the log, notes, and files file for the given project.
        /// </summary>
        /// <param name="project">The project whose files to delete.</param>
        public static void DeleteFilesForProject(Project project)
        {
            try
            {
                string logFilename = GetFileNameForProject(project, FileType.LOGS);
                if (File.Exists(logFilename))
                {
                    File.Delete(logFilename);
                }

                string notesFilename = GetFileNameForProject(project, FileType.NOTES);
                if (File.Exists(notesFilename))
                {
                    File.Delete(notesFilename);
                }

                string filesFilename = GetFileNameForProject(project, FileType.FILES);
                if (File.Exists(filesFilename))
                {
                    File.Delete(filesFilename);
                }
            }
            catch (IOException e)
            {
                ErrorLogger.AddLog(string.Format("Could not delete files for project '{0}':\n{1}", project.Name, e.Message));
                return;
            }
        }

        /// <summary>
        /// Opens the notes file for the given project.
        /// </summary>
        /// <param name="project">The project whose notes file to open.</param>
        public static void OpenNotesFile(Project project)
        {
            // Check to make sure the file exists, and create it if necessary
            string notesFilename = GetFileNameForProject(project, FileType.NOTES);
            if (!File.Exists(notesFilename))
            {
                CreateFilesForProject(project, false);
            }

            // Append a timestamp to the notes file
            try
            {
                File.AppendAllText(notesFilename, string.Format(timeStampTemplate, DateTime.Now.ToString()));
            }
            catch (IOException e)
            {
                ErrorLogger.AddLog(string.Format("Error opening project '{0}' notes file:\n{1}", project.Name, e.Message));
            }

            // Try to open the file
            try
            {
                Process.Start(notesFilename);
            }
            catch (FileNotFoundException e)
            {
                ErrorLogger.AddLog(string.Format("Error opening project '{0}' notes file:\n{1}", project.Name, e.Message));
            }
        }

        /// <summary>
        /// Writes the given project's logs to its log file.
        /// </summary>
        /// <param name="project">The project whose logs to write.</param>
        public static void WriteLogsToFile(Project project)
        {
            // Build the file text
            StringBuilder fileText = new StringBuilder();
            fileText.Append(string.Format(projectLogsFileTemplate, project.Name, project.Id));

            // Add the incomplete entry if necessary
            if (project.IncompleteLog != null)
            {
                fileText.Append(incomplete + "\n");
                fileText.Append(string.Format(projectLogFileRow, 
                    project.IncompleteLog.Id,
                    project.IncompleteLog.Start.ToString(),
                    "",
                    project.IncompleteLog.Description));
                fileText.Append("\n");
            }

            // Add all complete entries
            fileText.Append(complete + "\n");
            foreach (var entry in project.CompletedLogs)
            {
                fileText.Append(string.Format(projectLogFileRow,
                    entry.Id,
                    entry.Start.ToString(),
                    entry.End.ToString(),
                    entry.Description));
            }

            // Get the filename
            string logsFilename = GetFileNameForProject(project, FileType.LOGS);

            // Attempt to write the file
            try
            {
                File.WriteAllText(logsFilename, fileText.ToString());
            }
            catch (IOException e)
            {
                ErrorLogger.AddLog(string.Format("Could not write to project '{0}' log:\n{1}\n\nProject Logs:\n{2}", project.Name, e.Message, fileText.ToString()));
            }
        }

        /// <summary>
        /// Writes the given project's files to its files file.
        /// </summary>
        /// <param name="project">The project whose files to write.</param>
        public static void WriteFilesToFile(Project project)
        {
            // Build the file text
            StringBuilder fileText = new StringBuilder();
            fileText.Append(string.Format(projectFilesFileTemplate, project.Name, project.Id));

            // Add the files
            foreach (var file in project.Files)
            {
                fileText.Append(string.Format(projectFileFileRow,
                    file.FileTitle,
                    file.Filename,
                    file.IsFile ? "true" : "false",
                    file.ProgramToOpen != null ? file.ProgramToOpen : ""));
            }

            // Get the filename
            string filesFilename = GetFileNameForProject(project, FileType.FILES);

            // Attempt to write the file
            try
            {
                File.WriteAllText(filesFilename, fileText.ToString());
            }
            catch (IOException e)
            {
                ErrorLogger.AddLog(string.Format("Could not write to project '{0}' files file:\n{1}\n\nProject Files:\n{2}", project.Name, e.Message, fileText.ToString()));
            }
        }

        //----------//
        // Metadata //
        //----------//
        static int highestProjectIdInUse = -1;
        static Dictionary<int, int> highestLogIdsInUse;
        const char commentCharacter = '#';
        const string incomplete = "Incomplete";
        const string complete = "Complete";

        //-----------//
        // Filenames //
        //-----------//
        private static string allProjectsFilename = "data\\projects.txt";

        private static string projectLogsFilename = "data\\logs\\{0}_{1}_logs.txt";
        private static string projectNotesFilename = "data\\notes\\{0}_{1}_notes.txt";
        private static string projectFilesFilename = "data\\files\\{0}_{1}_files.txt";

        /// <summary>
        /// Returns the filename of the given type for the given project. Will throw an ArgumentException
        /// if the FileType is not recognized.
        /// </summary>
        /// <param name="project">The project whose file to return.</param>
        /// <param name="fileType">The type of file to return.</param>
        /// <returns>The filename.</returns>
        private static string GetFileNameForProject(Project project, FileType fileType)
        {
            return GetFileNameForProject(project.Name, project.Id, fileType);
        }

        /// <summary>
        /// Returns the filename of the given type for the given project. Will throw an ArgumentException
        /// if the FileType is not recognized.
        /// </summary>
        /// <param name="projectName">The name of the project whose file to return.</param>
        /// <param name="projectId">The id of the project whose file to return.</param>
        /// <param name="fileType">The type of file to return.</param>
        /// <returns>The filename.</returns>
        private static string GetFileNameForProject(string projectName, int projectId, FileType fileType)
        {
            // Remove any illegal characters from the project name
            string legalProjectName = projectName;
            string invalidChars = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            foreach (var c in invalidChars)
            {
                legalProjectName = legalProjectName.Replace(c.ToString(), "");
            }

            // Get the template for the given file type
            string filenameTemplate;
            switch (fileType)
            {
                case FileType.LOGS:
                    filenameTemplate = projectLogsFilename;
                    break;

                case FileType.NOTES:
                    filenameTemplate = projectNotesFilename;
                    break;

                case FileType.FILES:
                    filenameTemplate = projectFilesFilename;
                    break;

                default:
                    throw new ArgumentException(string.Format("The provided FileType '{0}' did not match a known type.", fileType));
            }

            // Return the formatted template
            return string.Format(filenameTemplate, projectId, legalProjectName);
        }

        private enum FileType
        {
            LOGS,
            NOTES,
            FILES
        }

        private static string[] requiredDirectories = new string[] { "data", "data\\logs", "data\\notes", "data\\files" };

        //----------------//
        // File Templates //
        //----------------//
        private const string allProjectsFileTemplate = "# Warning: Do not edit by hand\n#\n# DIRECTORY FILE\n#\n# Format: id|name\n\n";
        /* # Warning: Do not edit by hand
         * #
         * # DIRECTORY FILE
         * #
         * # Format: id|name 
         * 
         */
        private const string projectLogsFileTemplate = "# Warning: Do not edit by hand\n#\n# LOG FILE\n#\n# Project: {0}\n# Id: {1}\n#\n# Format: id|start time|end time|description\n\n";
        /* # Warning: Do not edit by hand
         * #
         * # LOG FILE
         * #
         * # Project: {0}
         * # Id: {1}
         * #
         * # Format: id|start time|end time|description
         * 
         */
        private const string projectNotesFileTemplate = "# Notes for Project {0}\n";
        /* # Notes for Project {0} 
         */
        private const string projectFilesFileTemplate = "# Warning: Do not edit by hand\n#\n# FILES FILE\n#\n# Project: {0}\n# Id: {1}\n#\n# Format: file title|file|is file flag|program to open\n\n";
        /* # Warning: Do not edit by hand
         * #
         * # FILES FILE
         * #
         * # Project: {0}
         * # Id: {1}
         * #
         * # Format: file title|file|is file flag|program to open
         * 
         */
        private const string allProjectsFileRow = "{0}|{1}\n";
        private const string projectLogFileRow = "{0}|{1}|{2}|{3}\n";
        private const string projectFileFileRow = "{0}|{1}|{2}|{3}\n";
        private const string timeStampTemplate = "\n** {0} **\n\n";
        /*
         * ** {0} **
         *  
         */
    }
}
using System;
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
        /* File Directory
         * 
         * Initialization Methods
         * File / Folder Creation Methods
         * File Query Methods
         * File Write Methods
         * File Open Methods
         * Id Methods
         * Template Methods
         * File Name Methods
         * Metadata
         * File Names
         * File Templates
         */

        static ProjectFileInterface()
        {
            // All initialization should be done explicitly by the initialization methods
        }

        //------------------------//
        // Initialization Methods //
        //------------------------//
        /// <summary>
        /// Run the first part of the ProjectFileInterface initialization.
        /// 
        /// Depends on no other Project Manager class.
        /// 
        /// Completes the following tasks:
        /// - initializes any null fields
        /// - builds executable relative file paths
        /// - verifies/creates executable based directory structure
        /// - verifies/creates executable based file structure
        /// </summary>
        public static void RunFirstInitialization()
        {
            // Initialize any fields from null
            highestLogIdsInUse = new Dictionary<int, int>();

            // Build the full paths to all executable relative files
            string currDir = Environment.CurrentDirectory;

            fullSettingsFilename = Path.Combine(currDir, settingsFilename);
            fullHelpFilename = Path.Combine(currDir, helpFilename);

            fullCommonNotesFileTemplateFilename = Path.Combine(currDir, commonNotesFileTemplateFilename);
            fullProjectNotesFileTemplateFilename = Path.Combine(currDir, projectNotesFileTemplateFilename);
            fullTimeStampTemplateFilename = Path.Combine(currDir, timeStampTemplateFilename);

            // Verify the executable relative directory structure
            VerifyDirectoryStructure(currDir, requiredExecutableDirectories);

            // Verify the executable relative file structure
            CreateFile(fullSettingsFilename, settingsFileTemplate, false);
            CreateTemplatesFiles(false);
        }

        /// <summary>
        /// Runs the second part of the ProjectFileInterface initialization.
        /// 
        /// Depends on the data file path being known (stored in the User Settings).
        /// 
        /// Completes the following tasks:
        /// - builds data relative file paths
        /// - verifies/creates data based directory structure
        /// - verifies/creates data based file structure
        /// - loads templates
        /// </summary>
        public static void RunSecondInitialization()
        {
            // Build the full paths to all data relative files
            string dataDir = UserSettings.DataDirectory;
            RenameFullFilenamesForDataDirectoryChange();

            // Verify the data relative directory structure
            VerifyDirectoryStructure(dataDir, requiredDataDirectories);

            // Verify the data relative file structure
            CreateFile(fullProjectListFilename, projectListFileTemplate, false);
            CreateFile(fullCommonNotesFilename, CommonNotesFileTemplate, false);
            CreateFile(fullBackupIdFilename, backupIdFileTemplate, false);
            
            // Load the custom templates
            LoadTemplateFromFile(fullCommonNotesFileTemplateFilename, out customCommonNotesFileTemplate);
            LoadTemplateFromFile(fullProjectNotesFileTemplateFilename, out customProjectNotesFileTemplate);
            LoadTemplateFromFile(fullTimeStampTemplateFilename, out customTimeStampTemplate);
        }

        //--------------------------------//
        // File / Folder Creation Methods //
        //--------------------------------//
        /// <summary>
        /// Creates if necessary all required folders based at the given root directory.
        /// </summary>
        /// <param name="rootDirectory">The directory to create the folders at.</param>
        /// <param name="requiredFolders">The folders to create.</param>
        private static void VerifyDirectoryStructure(string rootDirectory, string[] requiredFolders)
        {
            // Make sure the root directory exists
            if (!Directory.Exists(rootDirectory))
            {
                Directory.CreateDirectory(rootDirectory);
            }

            // Create all the subdirectories
            foreach (var folder in requiredFolders)
            {
                string fullFolderPath = Path.Combine(rootDirectory, folder);
                if (!Directory.Exists(fullFolderPath))
                {
                    Directory.CreateDirectory(fullFolderPath);
                }
            }
        }

        /// <summary>
        /// Creates the given file with the given default contents.
        /// </summary>
        /// <param name="filename">The file to create.</param>
        /// <param name="defaultContents">The contents for the new file.</param>
        /// <param name="overwrite">Whether to overwrite the file if it exists.</param>
        private static void CreateFile(string filename, string defaultContents, bool overwrite)
        {
            if (overwrite || !File.Exists(filename))
            {
                try
                {
                    File.WriteAllText(filename, defaultContents);
                }
                catch (IOException e)
                {
                    ErrorLogger.AddLog(string.Format("Error creating file:\n{0}\n\n{1}", filename, e.Message), ErrorSeverity.HIGH);
                }
            }
        }

        /// <summary>
        /// Creates the files holding the custom templates for notes and timestamps.
        /// </summary>
        /// <param name="overwrite">Whether to overwrite the existing templates files if necessary.</param>
        private static void CreateTemplatesFiles(bool overwrite)
        {
            // Map the template filenames to their templates
            Dictionary<string, string> templateFilenamesToTemplatesMap = new Dictionary<string, string>()
            {
                { fullCommonNotesFileTemplateFilename, CommonNotesFileTemplate },
                { fullProjectNotesFileTemplateFilename, ProjectNotesFileTemplate },
                { fullTimeStampTemplateFilename, TimeStampTemplate }
            };

            // Loop through the templates and verify that they exist
            foreach (var pair in templateFilenamesToTemplatesMap)
            {
                CreateFile(pair.Key, pair.Value, overwrite);
            }
        }

        /// <summary>
        /// Creates files for the given project's logs, notes, and files.
        /// </summary>
        /// <param name="project">The project whose files to create.</param>
        /// <param name="overwrite">Whether or not to overwrite the any files that already exist.</param>
        public static void CreateFilesForProject(Project project, bool overwrite)
        {
            // Log file
            CreateFile(
                GetFileNameForProject(project, FileType.LOGS), 
                BuildProjectLogsFileHeader(project.Name, project.Id), 
                overwrite);

            // Notes file
            CreateFile(
                GetFileNameForProject(project, FileType.NOTES),
                BuildProjectNotesFileHeader(project.Name),
                overwrite);

            // Files file
            CreateFile(
                GetFileNameForProject(project, FileType.FILES),
                BuildProjectFilesFileHeader(project.Name, project.Id),
                overwrite);
        }

        //--------------------//
        // File Query Methods //
        //--------------------//
        /// <summary>
        /// Parses the settings file and returns a dictionary containing the settings and their values.
        /// The caller will need to parse the strings.
        /// </summary>
        /// <returns>The dictionary of settings.</returns>
        public static Dictionary<string, string> GetSettingsFromFile()
        {
            Dictionary<string, string> settings = new Dictionary<string, string>();

            // Get the non-empty and non-comment lines from the projects file
            List<string> settingsRows = GetNonEmptyAndNonCommentLinesFromFile(fullSettingsFilename);

            // Parse the remaining lines
            foreach (var line in settingsRows)
            {
                // Ensure that the line has the correct format
                string[] lineItems = line.Split('|');
                int numberOfItems = 2;

                // Allow more than the number of items because the setting value itself could contain delimited values
                if (lineItems.Length < numberOfItems)
                {
                    ErrorLogger.AddLog(string.Format("Could not parse line '{0}'. Skipping entry.", line), ErrorSeverity.HIGH);
                    continue;
                }

                // Give names to the parts of the row
                string setting = lineItems[0];
                string value = line.Substring(lineItems[0].Length + 1); // we can't do lineItems[1] because the setting value could contain delimiters

                // Add the setting, and warn of duplicates
                if (settings.Keys.Contains(setting))
                {
                    ErrorLogger.AddLog(string.Format("Warning: Setting file contains multiple entries for setting '{0}'. The previous value for this setting ('{1}') is being overwritten by '{2}'.",
                        setting, settings[setting], value), ErrorSeverity.WARNING);
                }
                settings[setting] = value;
            }

            return settings;
        }

        /// <summary>
        /// Parses the projects list file and builds a list of Project instances.
        /// </summary>
        /// <returns>The list of projects.</returns>
        public static List<Project> GetProjectListFromFile()
        {
            List<Project> projects = new List<Project>();

            // Get the non-empty and non-comment lines from the projects file
            List<string> projectRows = GetNonEmptyAndNonCommentLinesFromFile(fullProjectListFilename);

            // Parse the remaining lines
            foreach (var line in projectRows)
            {
                // Ensure that the line has the correct format
                string[] lineItems = line.Split('|');
                int numberOfItems = 2;
                if (lineItems.Length != numberOfItems)
                {
                    ErrorLogger.AddLog(string.Format("Could not parse line '{0}'. Skipping entry.", line), ErrorSeverity.HIGH);
                    continue;
                }
                    
                // Ensure that the first part is a valid id
                int projectId;
                int minId = 0;
                bool parseIdSuccess = int.TryParse(lineItems[0], out projectId);
                if (!parseIdSuccess || projectId < minId)
                {
                    ErrorLogger.AddLog(string.Format("Could not parse project id '{0}'. Skipping entry.", lineItems[0]), ErrorSeverity.HIGH);
                    continue;
                }

                // Ensure that the second part is a valid project name
                string projectName = lineItems[1];
                if (projectName == "")
                {
                    ErrorLogger.AddLog(string.Format("Invalid project name '{0}'. Skipping entry.", lineItems[1]), ErrorSeverity.HIGH);
                    continue;
                }

                // If we got here, we have valid project info, so create an instance
                Project project = new Project(projectId, projectName);

                // Update the highest id fields if necessary
                highestProjectIdInUse = Math.Max(highestProjectIdInUse, projectId);
                highestLogIdsInUse[projectId] = -1;

                // Fill the instance out with the logs
                Tuple<List<ProjectLog>, ProjectLog> projectLogs = GetLogsForProjectFromFile(project);
                project.CompletedLogs = projectLogs.Item1;
                project.IncompleteLog = projectLogs.Item2;

                // Fill the instance with the files
                project.Files = GetFilesForProjectFromFile(project);

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
        public static Tuple<List<ProjectLog>, ProjectLog> GetLogsForProjectFromFile(Project project)
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
                    ErrorLogger.AddLog(string.Format("Could not determin with log '{0}' should be complete or incomplete. Skipping entry.", line), ErrorSeverity.HIGH);
                    continue;
                }

                // If we got here, then we should have a log entry
                // Ensure that the line has the correct format
                string[] lineItems = line.Split('|');
                int numberOfItems = 4;
                if (lineItems.Length != numberOfItems)
                {
                    ErrorLogger.AddLog(string.Format("Could not parse line '{0}'. Skipping entry.", line), ErrorSeverity.HIGH);
                    continue;
                }

                // Ensure that the first part is a valid id
                int logId;
                int minId = 0;
                bool parseIdSuccess = int.TryParse(lineItems[0], out logId);
                if (!parseIdSuccess || logId < minId)
                {
                    ErrorLogger.AddLog(string.Format("Could not parse log id '{0}'. Skipping entry.", lineItems[0]), ErrorSeverity.HIGH);
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
                        ErrorLogger.AddLog(string.Format("Could not parse log start time '{0}'. Skipping entry.", lineItems[1]), ErrorSeverity.HIGH);
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
                        ErrorLogger.AddLog(string.Format("Could not parse log end time '{0}'. Skipping entry.", lineItems[1]), ErrorSeverity.HIGH);
                        continue;
                    }
                }
                // Otherwise, we're invalid
                else
                {
                    ErrorLogger.AddLog(string.Format("Invalid end time '{0}' for log completion status ({1}). Skipping entry.", lineItems[1],
                        parsingCompletedEntries == true ? "complete" : "incomplete"), ErrorSeverity.HIGH);
                    continue;
                }

                // Ensure that the fourth part is a valid description
                string description = lineItems[3];
                if (description == "")
                {
                    ErrorLogger.AddLog(string.Format("Invalid log description '{0}'. Skipping entry.", lineItems[1]), ErrorSeverity.HIGH);
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
                        ErrorLogger.AddLog(string.Format("Only one incomplete entry is allowed. Additional entry '{0}' will be skipped.", description), ErrorSeverity.HIGH);
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
        public static List<ProjectFile> GetFilesForProjectFromFile(Project project)
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
                    ErrorLogger.AddLog(string.Format("Could not parse line '{0}'. Skipping entry.", line), ErrorSeverity.HIGH);
                    continue;
                }

                // Ensure that the first part is a valid title
                string fileTitle = lineItems[0];
                if (fileTitle == "")
                {
                    ErrorLogger.AddLog(string.Format("Could not parse file title. Value was '{0}'. Skipping entry.", fileTitle), ErrorSeverity.HIGH);
                    continue;
                }

                // Ensure that the second part is a "true" or "false"
                // Note: We do this before checking the filename because that check depends on this value.
                string isFileString = lineItems[2];
                if (isFileString != "true" && isFileString != "false")
                {
                    ErrorLogger.AddLog(string.Format("Could not parse file type. Value was '{0}'. Skipping entry.", isFileString), ErrorSeverity.HIGH);
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
                        ErrorLogger.AddLog(string.Format("Could not parse filename '{0}':\n{1}\nSkipping entry.", fileName, e.Message), ErrorSeverity.HIGH);
                        continue;
                    }
                }
                else
                {
                    Uri uriResult;
                    bool result = Uri.TryCreate(fileName, UriKind.Absolute, out uriResult) && uriResult.Scheme == Uri.UriSchemeHttp;
                    if (!result)
                    {
                        ErrorLogger.AddLog(string.Format("Could not parse url '{0}':\n{1}\nSkipping entry.", fileName), ErrorSeverity.HIGH);
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
                        ErrorLogger.AddLog(string.Format("Could not parse filename '{0}':\n{1}\nSkipping entry.", programToOpen, e.Message), ErrorSeverity.HIGH);
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
        /// Returns a list of non-empty and non-comment lines from the given file.
        /// </summary>
        /// <param name="filename">The file whose lines to return.</param>
        /// <returns>The list of lines.</returns>
        private static List<string> GetNonEmptyAndNonCommentLinesFromFile(string filename)
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
                ErrorLogger.AddLog(string.Format("Error parsing file '{0}':\n{1}", fullProjectListFilename, e.Message), ErrorSeverity.HIGH);
                return new List<string>();
            }
        }

        //--------------------//
        // File Write Methods //
        //--------------------//
        /// <summary>
        /// Writes the user settings to the settings file.
        /// </summary>
        public static void WriteSettingsToFile()
        {
            // Init the new file text
            StringBuilder newFileText = new StringBuilder();
            newFileText.Append(settingsFileTemplate);

            // Add the sorting method
            newFileText.Append(BuildSettingsRow(UserSettings.sortingMethodKey, UserSettings.ProjectSortingMethodString));

            // Add the hidden projects...
            StringBuilder hiddenProjectsValue = new StringBuilder();
            bool needToRemovePipe = false;
            foreach (var project in UserSettings.HiddenProjects)
            {
                hiddenProjectsValue.Append(string.Format("{0}|", project.Id));
                needToRemovePipe = true;
            }

            // ...and remove the trailing pipe if necessary
            if (needToRemovePipe)
            {
                hiddenProjectsValue.Remove(hiddenProjectsValue.Length - 1, 1);
            }

            newFileText.Append(BuildSettingsRow(UserSettings.hiddenProjectsKey, hiddenProjectsValue.ToString()));

            // Add the summary settings
            newFileText.Append(BuildSettingsRow(UserSettings.summarySortByTimeKey, UserSettings.SummarySortByTime.ToString()));
            newFileText.Append(BuildSettingsRow(UserSettings.summaryIgnoreHiddenProjectsKey, UserSettings.SummaryIgnoreHiddenProjects.ToString()));

            // Add the debug mode setting
            newFileText.Append(BuildSettingsRow(UserSettings.debugModeOnKey, UserSettings.DebugModeOn.ToString()));

            // Add the data settings
            newFileText.Append(BuildSettingsRow(UserSettings.useCustomTemplatesKey, UserSettings.UseCustomTemplates.ToString()));
            newFileText.Append(BuildSettingsRow(UserSettings.addTimestampToNotesKey, UserSettings.AddTimestampToNotes.ToString()));
            newFileText.Append(BuildSettingsRow(UserSettings.displayIncompleteLogWarningKey, UserSettings.DisplayIncompleteLogWarning.ToString()));

            // Add the data directory
            newFileText.Append(BuildSettingsRow(UserSettings.dataDirectoryKey, UserSettings.DataDirectory));

            // Write the file
            try
            {
                File.WriteAllText(fullSettingsFilename, newFileText.ToString());
            }
            catch (IOException e)
            {
                ErrorLogger.AddLog(string.Format("Could not write settings to settings file. Error:\n{0}\n\nSettings:\n{1}.", e.Message, newFileText.ToString()), ErrorSeverity.HIGH);
            }
        }

        /// <summary>
        /// Writes all of the data for all projects stored in ProjectOrganizer.
        /// Specifically, this writes the projects to the project list file and writes
        /// all of the projects' logs files and files files.
        /// </summary>
        public static void WriteAllProjectData()
        {
            // Write the project list file
            WriteProjectsToProjectListFile();

            // Write the project logs file and files file
            foreach (var project in ProjectOrganizer.Projects)
            {
                WriteProjectLogsToFile(project);
                WriteProjectFilesToFile(project);

            }
        }

        public static void WriteProjectsToProjectListFile()
        {
            // Build the project list file text
            StringBuilder projectListFileText = new StringBuilder();
            projectListFileText.Append(projectListFileTemplate);

            // Loop through all the projects and add the rows
            List<Project> projects = ProjectOrganizer.Projects.OrderBy(p => p.Id).ToList();
            foreach (var project in projects)
            {
                projectListFileText.Append(BuildProjectListRow(project.Name, project.Id));
            }

            // Write the project list file
            try
            {
                File.WriteAllText(fullProjectListFilename, projectListFileText.ToString());
            }
            catch (IOException e)
            {
                ErrorLogger.AddLog(string.Format("Could not write to the project list file. Error:\n{0}", e.Message), ErrorSeverity.HIGH);
            }
        }

        /// <summary>
        /// Writes the given project's logs to its log file.
        /// </summary>
        /// <param name="project">The project whose logs to write.</param>
        public static void WriteProjectLogsToFile(Project project)
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
                ErrorLogger.AddLog(string.Format("Could not write to project '{0}' log:\n{1}\n\nProject Logs:\n{2}", project.Name, e.Message, fileText.ToString()), ErrorSeverity.HIGH);
            }
        }

        /// <summary>
        /// Writes the given project's files to its files file.
        /// </summary>
        /// <param name="project">The project whose files to write.</param>
        public static void WriteProjectFilesToFile(Project project)
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
                ErrorLogger.AddLog(string.Format("Could not write to project '{0}' files file:\n{1}\n\nProject Files:\n{2}",
                    project.Name, e.Message, fileText.ToString()), ErrorSeverity.HIGH);
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
                ErrorLogger.AddLog(string.Format("No file update necessary; project '{0}' has not changed names.", oldName), ErrorSeverity.WARNING);
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
                string oldNotesHeader = BuildProjectNotesFileHeader(oldName);
                string newNotesHeader = BuildProjectNotesFileHeader(project.Name);

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
                ErrorLogger.AddLog(string.Format("Could not read project '{0}' old notes file after name change:\n{1}", project.Name, e.Message), ErrorSeverity.HIGH);
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
                ErrorLogger.AddLog(string.Format("Could not delete old project '{0}' file after name change:\n{1}", project.Name, e.Message), ErrorSeverity.MODERATE);
            }
            
            // Create the new files and write to them
            CreateFilesForProject(project, true);
            try
            {
                // Notes file
                string newNotesFilename = GetFileNameForProject(project, FileType.NOTES);
                File.WriteAllText(newNotesFilename, notesText.ToString());

                // Logs file
                WriteProjectLogsToFile(project);

                // Files file
                WriteProjectFilesToFile(project);
            }
            catch (IOException e)
            {
                ErrorLogger.AddLog(string.Format("Error writing to new project '{0}' file after name change:\n{1}\n\nProject Notes:\n{2}",
                    project.Name, e.Message, notesText.ToString()), ErrorSeverity.HIGH);
            }
        }

        /// <summary>
        /// Resets the notes file for the given project.
        /// </summary>
        /// <param name="project">The project whose notes file to open.</param>
        public static void ResetNotesFile(Project project = null)
        {
            if (project == null)
            {
                ErrorLogger.AddLog("Cannot reset notes file for null project.", ErrorSeverity.MODERATE);
            }

            string notesFilename = GetFileNameForProject(project, FileType.NOTES);
            try
            {
                File.WriteAllText(notesFilename, BuildProjectNotesFileHeader(project.Name));
            }
            catch (IOException e)
            {
                ErrorLogger.AddLog(string.Format("Error resetting project {0} notes file:\n{1}", project.Name, e.Message), ErrorSeverity.MODERATE);
                return;
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
                ErrorLogger.AddLog(string.Format("Could not delete files for project '{0}':\n{1}", project.Name, e.Message), ErrorSeverity.MODERATE);
                return;
            }
        }

        //-------------------//
        // File Open Methods //
        //-------------------//
        /// <summary>
        /// Opens the notes file for the given project (or the generic note file if no project given).
        /// </summary>
        /// <param name="project">The project whose notes file to open. Open the generic notes file if null.</param>
        public static void OpenNotesFile(Project project = null)
        {
            // Check to make sure the file exists, and create it if necessary
            string notesFilename;
            
            // Handle the case where we have a project
            if (project != null)
            {
                notesFilename = GetFileNameForProject(project, FileType.NOTES);
                if (!File.Exists(notesFilename))
                {
                    CreateFilesForProject(project, false);
                }
            }

            // Handle the case where we have no project
            else
            {
                notesFilename = fullCommonNotesFilename;
                try 
                { 
                    if (!File.Exists(notesFilename))
                    {
                        File.WriteAllText(notesFilename, CommonNotesFileTemplate);
                    }
                }
                catch (IOException e)
                {
                    ErrorLogger.AddLog(string.Format("Error creating common notes file:\n{0}", e.Message), ErrorSeverity.HIGH);
                    return;
                }
            }

            // Append a timestamp to the notes file
            if (UserSettings.AddTimestampToNotes)
            { 
                try
                {
                    File.AppendAllText(notesFilename, BuildTimeStamp(DateTime.Now.ToString()));
                }
                catch (IOException e)
                {
                    ErrorLogger.AddLog(string.Format("Error opening project '{0}' notes file:\n{1}", project.Name, e.Message), ErrorSeverity.HIGH);
                }
            }

            // Try to open the file
            try
            {
                Process.Start(notesFilename);
            }
            catch (FileNotFoundException e)
            {
                ErrorLogger.AddLog(string.Format("Error opening project '{0}' notes file:\n{1}", project.Name, e.Message), ErrorSeverity.HIGH);
            }
        }

        /// <summary>
        /// Opens the program's error log file.
        /// </summary>
        public static void OpenErrorLogFile()
        {
            try
            {
                Process.Start(ErrorLogger.ErrorLogFilename);
            }
            catch (FileNotFoundException e)
            {
                ErrorLogger.AddLog(string.Format("Error opening error log file:\n{0}", e.Message), ErrorSeverity.HIGH);
            }
        }

        /// <summary>
        /// Opens the program's help file.
        /// </summary>
        public static void OpenHelpFile()
        {
            try
            {
                Process.Start(fullHelpFilename);
            }
            catch (FileNotFoundException e)
            {
                ErrorLogger.AddLog(string.Format("Error opening help file:\n{0}", e.Message), ErrorSeverity.HIGH);
            }
        }

        /// <summary>
        /// Allows the user to select a file using the default windows dialog.
        /// </summary>
        /// <param name="defaultExt">The default extention for the file.</param>
        /// <param name="filter">The filter for the types of files allowed.</param>
        /// <returns>The selected file, or null if no file selected.</returns>
        public static string GetFileWithWindowsDialog(string defaultExt, string filter, string initialDirectory = null)
        {
            // Init the dialog
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = defaultExt;
            dlg.Filter = filter;
            if (initialDirectory != null)
            {
                dlg.InitialDirectory = initialDirectory;
            }

            // Show the dialog and handle the result
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                return dlg.FileName;
            }
            else
            {
                return null;
            }
        }

        //------------//
        // Id Methods //
        //------------//
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
        /// Generates and returns and unused integer id for a backup.
        /// </summary>
        /// <returns>The new id, or -1 if operation failed.</returns>
        public static int RequestBackupId()
        {
            // Try to read the contents of the backup id file
            string backupIdFileContents;
            try
            { 
                backupIdFileContents = File.ReadAllText(fullBackupIdFilename);
            }
            catch (IOException e)
            {
                ErrorLogger.AddLog(string.Format("Cannot open backup id file:\n{0}", e.Message), ErrorSeverity.HIGH);
                return -1;
            }

            // Try to parse the contents
            int id = -1;
            bool success = int.TryParse(backupIdFileContents, out id);
            if (!success || id < -1)
            {
                ErrorLogger.AddLog(string.Format("Backup id file contents '{0}' is not a valid id.", backupIdFileContents), ErrorSeverity.HIGH);
                return -1;
            }

            // Increment the id
            id++;

            // Write back to the file
            try
            {
                File.WriteAllText(fullBackupIdFilename, id.ToString());
            }
            catch (IOException e)
            {
                ErrorLogger.AddLog(string.Format("Cannot write new id to backup id file.", e.Message), ErrorSeverity.HIGH);
            }

            // Return the new id
            return id;
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

        //------------------//
        // Template Methods //
        //------------------//
        /// <summary>
        /// Attempts to load the contents of the given file into the given template variable.
        /// </summary>
        /// <param name="templateFilename">The path to the file containing the template.</param>
        /// <param name="template">The variable to store the template in. Set to null if the template couldn't be read.</param>
        /// <returns>True if successful, false otherwise.</returns>
        private static bool LoadTemplateFromFile(string templateFilename, out string template)
        {
            if (!File.Exists(templateFilename))
            {
                // We couldn't find the file
                template = null;
                ErrorLogger.AddLog(string.Format("Could not find template file {0}.", templateFilename), ErrorSeverity.MODERATE);
                return false;
            }

            // Try to read the template from the file
            try
            {
                template = File.ReadAllText(templateFilename);
                return true;
            }
            catch (IOException e)
            {
                // We failed to read the file
                ErrorLogger.AddLog(string.Format("Failed to load template from file {0}:\n{1}", templateFilename, e.Message), ErrorSeverity.MODERATE);
                template = null;
                return false;
            }
        }

        /// <summary>
        /// Builds the header for the log file for the given project.
        /// </summary>
        /// <param name="projectName">The name of the project.</param>
        /// <param name="projectId">The id of the project.</param>
        /// <returns>The new header.</returns>
        private static string BuildProjectLogsFileHeader(string projectName, int projectId)
        {
            return string.Format(projectLogsFileTemplate, projectName, projectId);
        }

        /// <summary>
        /// Builds the header for the notes file for the given project.
        /// </summary>
        /// <param name="projectName">The name of the project.</param>
        /// <returns>The new header.</returns>
        private static string BuildProjectNotesFileHeader(string projectName)
        {
            try
            {
                return string.Format(ProjectNotesFileTemplate, projectName);
            }
            catch (FormatException e)
            {
                ErrorLogger.AddLog(string.Format("Error formating custom project notes file header:\n{0}\n\nUsing default template instead.", e.Message), ErrorSeverity.LOW);
                return string.Format(defaultProjectNotesFileTemplate, projectName);
            }
        }

        /// <summary>
        /// Builds the header for the files file for the given project.
        /// </summary>
        /// <param name="projectName">The name of the project.</param>
        /// <param name="projectId">The id of the project.</param>
        /// <returns>The new header.</returns>
        private static string BuildProjectFilesFileHeader(string projectName, int projectId)
        {
            return string.Format(projectFilesFileTemplate, projectName, projectId);
        }

        /// <summary>
        /// Gets the time stamp for the given datetime, using the custom template if possible.
        /// </summary>
        /// <param name="datetime">The datetime whose time stamp to return.</param>
        /// <returns>The time stamp.</returns>
        private static string BuildTimeStamp(string datetime)
        {
            string timeStamp = string.Format(defaultTimeStampTemplate, datetime);

            // Use the custom template if possible  
            if (customTimeStampTemplate != null)
            {
                try
                {
                    timeStamp = string.Format(customTimeStampTemplate, datetime);
                }
                catch (FormatException e)
                {
                    ErrorLogger.AddLog(string.Format("Could not use custom time stamp template; using default template instead:\n{0}", e.Message), ErrorSeverity.LOW);
                }
            }

            return timeStamp;
        }

        /// <summary>
        /// Builds a row for the settings file using the given key and value.
        /// </summary>
        /// <param name="key">The key for the given setting.</param>
        /// <param name="value">The value for the given setting.</param>
        /// <returns>The row to put in the settings file.</returns>
        private static string BuildSettingsRow(string key, string value)
        {
            return string.Format(settingsFileRow, key, value);
        }

        /// <summary>
        /// Builds a row for the given project for the project list file.
        /// </summary>
        /// <param name="projectName">The name of the project.</param>
        /// <param name="projectId">The id of the project.</param>
        /// <returns>The row to put in the project list file.</returns>
        private static string BuildProjectListRow(string projectName, int projectId)
        {
            return string.Format(projectListFileRow, projectId, projectName);
        }

        //-------------------//
        // File Name Methods //
        //-------------------//
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
                    filenameTemplate = fullProjectLogsFilename;
                    break;

                case FileType.NOTES:
                    filenameTemplate = fullProjectNotesFilename;
                    break;

                case FileType.FILES:
                    filenameTemplate = fullProjectFilesFilename;
                    break;

                default:
                    throw new ArgumentException(string.Format("The provided FileType '{0}' did not match a known type.", fileType));
            }

            // Return the formatted template
            return string.Format(filenameTemplate, projectId, legalProjectName);
        }

        /// <summary>
        /// Renames all of the full filename fields to reflect a data directory change.
        /// </summary>
        public static void RenameFullFilenamesForDataDirectoryChange()
        {
            string dataDir = UserSettings.DataDirectory;

            fullProjectListFilename = Path.Combine(dataDir, projectListFilename);
            fullCommonNotesFilename = Path.Combine(dataDir, commonNotesFilename);
            fullBackupIdFilename = Path.Combine(dataDir, backupIdFilename);

            fullProjectLogsFilename = Path.Combine(dataDir, projectLogsFilename);
            fullProjectNotesFilename = Path.Combine(dataDir, projectNotesFilename);
            fullProjectFilesFilename = Path.Combine(dataDir, projectFilesFilename);
        }

        //----------//
        // Metadata //
        //----------//
        static int highestProjectIdInUse = -1;
        static Dictionary<int, int> highestLogIdsInUse;

        const char commentCharacter = '#';
        const string incomplete = "Incomplete";
        const string complete = "Complete";

        //------------//
        // File Names //
        //------------//
        /* File Locations
         * 
         * Project Manager requires a number of files and directories to exist in order to operate properly.
         * Some of these files live relative to the Project Manager execuable, and some live relative to the
         * data directory that the user can specify.
         */

        // Common files
        private const string settingsFilename = "runtime_information\\settings.txt";   // Relative to executable directory
        private const string helpFilename = "runtime_information\\help.txt";           // Relative to executable directory
        private const string projectListFilename = "projects.txt";                     // Relative to data directory
        private const string commonNotesFilename = "notes.txt";                        // Relative to data directory
        private const string backupIdFilename = "backup\\backup_id.txt";               // Relative to data directory

        // Project files
        private const string projectLogsFilename = "logs\\{0}_{1}_logs.txt";       // Relative to data directory
        private const string projectNotesFilename = "notes\\{0}_{1}_notes.txt";    // Relative to data directory
        private const string projectFilesFilename = "files\\{0}_{1}_files.txt";    // Relative to data directory

        // Template files
        private const string commonNotesFileTemplateFilename = "runtime_information\\templates\\commonNotesFileTemplate.txt";      // Relative to executable directory
        private const string projectNotesFileTemplateFilename = "runtime_information\\templates\\projectNotesFileTemplate.txt";    // Relative to executable directory
        private const string timeStampTemplateFilename = "runtime_information\\templates\\timeStampTemplate.txt";                  // Relative to executable directory

        // Required directories
        private static string[] requiredExecutableDirectories = new string[] { "runtime_information", "runtime_information\\templates" };
        private static string[] requiredDataDirectories = new string[] { "logs", "notes", "files", "backup" };

        // Full filenames
        private static string fullSettingsFilename;
        private static string fullHelpFilename;
        private static string fullProjectListFilename;
        private static string fullCommonNotesFilename;
        private static string fullBackupIdFilename;

        private static string fullProjectLogsFilename;
        private static string fullProjectNotesFilename;
        private static string fullProjectFilesFilename;

        private static string fullCommonNotesFileTemplateFilename;
        private static string fullProjectNotesFileTemplateFilename;
        private static string fullTimeStampTemplateFilename;

        private enum FileType
        {
            LOGS,
            NOTES,
            FILES
        }

        //----------------//
        // File Templates //
        //----------------//
        private const string projectListFileTemplate = "# Warning: Do not edit by hand\n#\n# PROJECT LIST FILE\n#\n# Format: id|name\n\n";
        /* # Warning: Do not edit by hand
         * #
         * # PROJECT LIST FILE
         * #
         * # Format: id|name 
         * 
         */

        private const string defaultCommonNotesFileTemplate = "# Common Notes File\n";
        /* # Common Notes File
         */
        private static string customCommonNotesFileTemplate = null;
        private static string CommonNotesFileTemplate
        {
            get
            {
                if (customCommonNotesFileTemplate == null || !UserSettings.UseCustomTemplates)
                {
                    return defaultCommonNotesFileTemplate;
                }
                return customCommonNotesFileTemplate;
            }
        }

        private const string settingsFileTemplate = "# Warning: Do not edit by hand\n#\n# SETTINGS FILE\n#\n# Format: setting|value\n\n";
        /* # Warning: Do not edit by hand
         * #
         * # SETTINGS FILE
         * #
         * # Format: setting|value
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

        private const string defaultProjectNotesFileTemplate = "# Notes for Project {0}\n";
        /* # Notes for Project {0} 
         */
        private static string customProjectNotesFileTemplate = null;
        private static string ProjectNotesFileTemplate
        {
            get
            {
                if (customProjectNotesFileTemplate == null || !UserSettings.UseCustomTemplates)
                {
                    return defaultProjectNotesFileTemplate;
                }
                return customProjectNotesFileTemplate;
            }
        }

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

        private const string projectListFileRow = "{0}|{1}\n";
        private const string settingsFileRow = "{0}|{1}\n";
        private const string projectLogFileRow = "{0}|{1}|{2}|{3}\n";
        private const string projectFileFileRow = "{0}|{1}|{2}|{3}\n";

        private const string defaultTimeStampTemplate = "\n** {0} **\n\n\n";
        /*
         * ** {0} **
         *  
         * 
         * 
         */
        private static string customTimeStampTemplate = null;
        private static string TimeStampTemplate
        {
            get
            {
                if (customTimeStampTemplate == null || !UserSettings.UseCustomTemplates)
                {
                    return defaultTimeStampTemplate;
                }
                return customTimeStampTemplate;
            }
        }

        private const string backupIdFileTemplate = "-1";
    }
}

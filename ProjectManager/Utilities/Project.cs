using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManager
{
    public class Project
    {
        public Project(int id, string name, List<ProjectLog> completedLogs = null, ProjectLog incompleteLog = null, List<ProjectFile> files = null)
        {
            Id = id;
            Name = name;
            CompletedLogs = completedLogs != null ? completedLogs : new List<ProjectLog>();
            IncompleteLog = incompleteLog;
            Files = files != null ? files : new List<ProjectFile>();
        }

        //------------------//
        // External Methods //
        //------------------//
        /// <summary>
        /// Finishes this project's incomplete log if possible.
        /// </summary>
        public void FinishIncompleteLog()
        {
            ProjectLog logToFinish = IncompleteLog;

            if (logToFinish == null)
            {
                ErrorLogger.AddLog(string.Format("Project '{0}' has no incomplete log to finish.", Name), ErrorSeverity.LOW);
                return;
            }

            // End the log
            logToFinish.End = DateTime.Now;

            // Update the references
            CompletedLogs.Add(logToFinish);
            IncompleteLog = null;
        }

        /// <summary>
        /// Sorts the project's completed entries by start time (newest first).
        /// </summary>
        public void SortCompletedEntries()
        {
            CompletedLogs = CompletedLogs.OrderBy(l => l.Start).Reverse().ToList();
        }

        /// <summary>
        /// Sorts the project's files by title.
        /// </summary>
        public void SortFiles()
        {
            Files = Files.OrderBy(f => f.FileTitle).ToList();
        }

        /// <summary>
        /// Gets the total time spent on the project.
        /// </summary>
        /// <returns>The total time.</returns>
        public TimeSpan GetTotalProjectTime()
        {
            TimeSpan totalTime = new TimeSpan(0);
            foreach (var log in CompletedLogs)
            {
                totalTime += log.End - log.Start;
            }

            return totalTime;
        }

        /// <summary>
        /// Removes the given log from the project if possible.
        /// </summary>
        /// <param name="log">The log to remove.</param>
        public void RemoveLogEntry(ProjectLog log)
        {
            if (CompletedLogs.Contains(log))
            {
                CompletedLogs.Remove(log);
            }

            if (IncompleteLog == log)
            {
                IncompleteLog = null;
            }
        }

        /// <summary>
        /// Removes the given file from the project if possible.
        /// </summary>
        /// <param name="file">The file to remove.</param>
        public void RemoveFileEntry(ProjectFile file)
        {
            if (Files.Contains(file))
            {
                Files.Remove(file);
            }
        }

        //------//
        // Data //
        //------//
        public int Id { get; private set; }
        public string Name { get; set; }
        public List<ProjectLog> CompletedLogs { get; set; }
        public ProjectLog IncompleteLog { get; set; }
        public List<ProjectFile> Files { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManager
{
    public class ProjectFile
    {
        public ProjectFile(string fileTitle, string filename, bool isFile, Project parentProject)
            : this(fileTitle, filename, isFile, null, parentProject)
        {
        }

        public ProjectFile(string fileTitle, string filename, bool isFile, string programToOpen, Project parentProject)
        {
            FileTitle = fileTitle;
            ChangeFile(filename, isFile);
            ProgramToOpen = programToOpen;
            ParentProject = parentProject;
        }

        //------------------//
        // External Methods //
        //------------------//
        /// <summary>
        /// Opens the file either in the chosen program or the default program if no
        /// specific program has been chosen.
        /// </summary>
        public void OpenFile()
        {
            // Open in custom program if possible
            if (ProgramToOpen != null)
            {
                Process.Start(ProgramToOpen, Filename);
            }

            // Otherwise, open in default program
            else
            {
                Process.Start(Filename);
            }
        }

        /// <summary>
        /// Changes the underlying file for this instance.
        /// </summary>
        /// <param name="newFile">The new file.</param>
        /// <param name="isFile">Whether the file is actually a file or instead a url.</param>
        public void ChangeFile(string newFile, bool isFile)
        {
            IsFile = isFile;        // Note: this MUST be set before setting Filename because...
            Filename = newFile;    // ...this will call DetermineShortFilename(), which depends on IsFile being set.
        }

        //----------------//
        // Helper Methods //
        //----------------//
        /// <summary>
        /// Determines the short filename for the current file.
        /// </summary>
        private void DetermineShortFilename()
        {
            // By default, let the short filename be the same as the filename
            ShortFilename = Filename;

            // Handle the case where it's a file
            if (IsFile)
            {
                try
                {
                    ShortFilename = Path.GetFileName(Filename);
                }
                catch (ArgumentException e)
                {
                    ErrorLogger.AddLog(string.Format("Error getting short filename for file '{0}':\n'{1}'", Filename, e.Message), ErrorSeverity.MODERATE);
                }
            }

            // Handle the case where it's a url
            else
            {
                try
                {
                    var uri = new Uri(Filename);
                    ShortFilename = uri.Host + uri.PathAndQuery;
                }
                catch (UriFormatException e)
                {
                    ErrorLogger.AddLog(string.Format("Error getting short url for url '{0}':\n'{1}'", Filename, e.Message), ErrorSeverity.MODERATE);
                }
            }

        }

        //------//
        // Data //
        //------//
        public string FileTitle { get; set; }

        // Filename could be url
        private string filename;
        public string Filename
        { 
            get { return filename; }
            private set
            {
                filename = value;
                DetermineShortFilename();
            }
         }

        public string ShortFilename { get; private set; }
        public bool IsFile { get; private set; }

        private string programToOpen;
        public string ProgramToOpen
        {
            get { return programToOpen; }
            set
            {
                // If progam to open would be the empty string, instead set it to null
                programToOpen = value == "" ? null : value;
            } 
        }

        public Project ParentProject { get; private set; }
    }
}

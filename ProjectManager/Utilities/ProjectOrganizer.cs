using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManager
{
    public static class ProjectOrganizer
    {
        static ProjectOrganizer()
        {
            LoadAllProjects();
        }

        public static void LoadAllProjects()
        {
            Projects = ProjectFileInterface.GetAllProjectsFromFiles();
        }

        //------------------//
        // External Methods //
        //------------------//
        /// <summary>
        /// Creates a new project with the given name.
        /// </summary>
        /// <param name="projectName">The name of the new project.</param>
        /// <returns>The new project.</returns>
        public static Project CreateNewProject(string projectName)
        {
            // Get the project's new id and create it
            int projectId = ProjectFileInterface.RequestProjectId();
            Project newProject = new Project(projectId, projectName);

            // Add a reference to the new project and return
            Projects.Add(newProject);
            return newProject;
        }

        /// <summary>
        /// Removes the given project from the list if possible.
        /// </summary>
        /// <param name="project">The project to remove.</param>
        public static void RemoveProject(Project project)
        {
            if (Projects.Contains(project))
            {
                Projects.Remove(project);
                ProjectFileInterface.DeleteFilesForProject(project);
            }
            else
            {
                ErrorLogger.AddLog(string.Format("Could not remove project '{0}' from the list, as it was not in the list to begin with.", project.Name));
            }
        }

        /// <summary>
        /// Removes all projects from the organizer.
        /// </summary>
        public static void RemoveAllProjects()
        {
            List<Project> tempList = new List<Project>(Projects);
            foreach (var project in tempList)
            {
                RemoveProject(project);
            }
            Projects = new List<Project>();
        }

        //------//
        // Data //
        //------//
        public static List<Project> Projects { get; private set; }
    }
}

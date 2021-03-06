﻿using System;
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
            // All initialization should be done explicitly by the initialization method
        }

        //------------------//
        // External Methods //
        //------------------//
        /// <summary>
        /// Run the ProjectOrganizer initialization.
        /// 
        /// Depends on Project File Interface being fully initialized.
        /// 
        /// Completes the following tasks:
        /// - loads all projects from the project list file
        /// </summary>
        public static void RunInitialization()
        {
            LoadAllProjects();
        }

        public static void LoadAllProjects()
        {
            Projects = ProjectFileInterface.GetProjectListFromFile();
        }

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
            ProjectFileInterface.CreateFilesForProject(newProject, false);

            // Add a reference to the new project and return
            Projects.Add(newProject);
            return newProject;
        }

        /// <summary>
        /// Removes the given project from the list if possible. Will also delete the project's data files.
        /// This call should be all that is necessary to effectively delete a project.
        /// </summary>
        /// <param name="project">The project to remove.</param>
        public static void RemoveProject(Project project)
        {
            if (Projects.Contains(project))
            {
                Projects.Remove(project);
                ProjectFileInterface.DeleteFilesForProject(project);
                if (UserSettings.HiddenProjects.Contains(project))
                {
                    UserSettings.HiddenProjects.Remove(project);
                }
            }
            else
            {
                ErrorLogger.AddLog(string.Format("Could not remove project '{0}' from the list, as it was not in the list to begin with.", project.Name), ErrorSeverity.WARNING);
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

        /// <summary>
        /// Get the project with the given id.
        /// </summary>
        /// <param name="id">The id whose project to return.</param>
        /// <returns>The corresponding project, or null if no project.</returns>
        public static Project GetProjectWithId(int id)
        {
            foreach (var project in Projects)
            {
                if (project.Id == id)
                {
                    return project;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns a new list containing all of the given elements sorted using the given method.
        /// Throws an ArgumentException if the sorting method is not valid.
        /// </summary>
        /// <param name="projects">The projects to sort.</param>
        /// <param name="method">The method to use to sort the projects.</param>
        /// <returns>The sorted list.</returns>
        public static List<Project> SortProjects(List<Project> projects, SortingMethod method)
        {
            List<Project> sortedProjects = new List<Project>();

            // Sort based on the UserSettings
            switch (method)
            {
                case SortingMethod.OLD_FIRST:
                    sortedProjects = projects.OrderBy(p => p.Id).ToList();
                    break;

                case SortingMethod.NEW_FIRST:
                    sortedProjects = projects.OrderBy(p => p.Id).Reverse().ToList();
                    break;

                case SortingMethod.NAME_A_TO_Z:
                    sortedProjects = projects.OrderBy(p => p.Name).ToList();
                    break;

                case SortingMethod.NAME_Z_TO_A:
                    sortedProjects = projects.OrderBy(p => p.Name).Reverse().ToList();
                    break;

                default:
                    ErrorLogger.AddLog(string.Format("Invalid sorting method {0} given. Cannot sort.", method), ErrorSeverity.MODERATE);
                    sortedProjects = projects;
                    break;
            }

            return sortedProjects;
        }

        //------//
        // Data //
        //------//
        public static List<Project> Projects { get; private set; }
    }
}

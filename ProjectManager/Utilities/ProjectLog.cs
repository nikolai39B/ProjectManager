using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManager
{
    public class ProjectLog
    {
        public ProjectLog(int id, DateTime start, string description, Project parentProject)
            : this(id, start, DateTime.MaxValue, description, parentProject)
        {
        }

        public ProjectLog(int id, DateTime start, DateTime end, string description, Project parentProject)
        {
            Id = id;
            Start = start;
            End = end;
            Description = description;
            ParentProject = parentProject;
        }

        //------//
        // Data //
        //------//
        public int Id { get; private set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Description { get; set; }
        public Project ParentProject { get; private set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManager
{
    public static class UserSettings
    {
        static UserSettings()
        {
            ResetToDefaults();
        }

        //------------------//
        // External Methods //
        //------------------//
        public static void ResetToDefaults()
        {
            ProjectSortingMethod = SortingMethod.ID_LOW;
        }

        //------//
        // Data //
        //------//
        public static SortingMethod ProjectSortingMethod { get; set; }
    }

    public enum SortingMethod
    {
        ID_LOW,
        ID_HIGH,
        NAME_A_TO_Z,
        NAME_Z_TO_A,

        NONE
    }
}

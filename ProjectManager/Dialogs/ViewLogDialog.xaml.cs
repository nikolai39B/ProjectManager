using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit;

namespace ProjectManager
{
    /// <summary>
    /// Interaction logic for ViewLogDialog.xaml
    /// </summary>
    public partial class ViewLogDialog : Window
    {
        // NOTE:
        // We return true on DialogResult if the client should update its list of logs.
        // This could happend due to a change in the log or a delete.

        public ViewLogDialog(ProjectLog log)
        {
            InitializeComponent();
            
            // Init some fields and the UI
            Log = log;
            dtp_StartTime.FormatString = dateTimeFormat;
            dtp_EndTime.FormatString = dateTimeFormat;
            SetUpGuiFromLog();

            // Change the calendar style once we're loaded
            dtp_StartTime.Loaded += DateTimePicker_Loaded;
            dtp_EndTime.Loaded += DateTimePicker_Loaded;
        }
        
        //----------------//
        // Helper Methods //
        //----------------//
        /// <summary>
        /// Sets up the gui from the current log.
        /// </summary>
        private void SetUpGuiFromLog()
        {
            dtp_StartTime.Value = Log.Start;
            dtp_EndTime.Value = Log.End;
            tb_Description.Text = Log.Description;
        }

        //---------------//
        // Event Handles //
        //---------------//
        private void b_Save_Click(object sender, RoutedEventArgs e)
        {
            bool errors = false;
            DateTime newStart = DateTime.MinValue;
            DateTime newEnd = DateTime.MaxValue; 
            tbl_ErrorMessage.Text = "";

            // Make sure DateTimes have values
            if (!dtp_StartTime.Value.HasValue || !dtp_EndTime.Value.HasValue)
            {
                tbl_ErrorMessage.Text += "Error parsing start or end time.\n";
                errors = true;
            }
            // Make sure the new start is before (or equal to) the new end    
            else
            {
                newStart = dtp_StartTime.Value.Value;
                newEnd = dtp_EndTime.Value.Value;
                if (newStart > newEnd)
                { 
                    tbl_ErrorMessage.Text += "Start time must be before end time.\n";
                    errors = true;
                }
            }

            // Make sure description is legal
            string description = tb_Description.Text;
            if (description == "" || description.Contains('|'))
            {
                tbl_ErrorMessage.Text += "Description cannot be blank and cannot contain pipes ('|').\n";
                errors = true;
            }

            // Note errors if necessary
            if (errors)
            {
                tbl_ErrorMessage.Text = tbl_ErrorMessage.Text.TrimEnd('\r', '\n');
                tbl_ErrorMessage.Visibility = System.Windows.Visibility.Visible;
            }

            // Otherwise, update the log and return
            else
            {
                Log.Start = newStart;
                Log.End = newEnd;
                Log.Description = description;
                DialogResult = true;
            }
        }

        private void b_Delete_Click(object sender, RoutedEventArgs e)
        {
            ConfirmationDialog window = new ConfirmationDialog("Are you sure you wish to delete this log?");
            if (window.ShowDialog() == true)
            {
                // Delete the log and return
                Log.ParentProject.RemoveLogEntry(Log);
                DialogResult = true;
            }
        }

        private void b_Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        void DateTimePicker_Loaded(object sender, RoutedEventArgs e)
        {
            // Set the calendar style. We have to do it in this way because wpf is weird.
            // Also, we have to make sure that the control is loaded or it won't find the calendar.
            DateTimePicker dtp = sender as DateTimePicker;
            Calendar cal = dtp.Template.FindName("PART_Calendar", dtp) as Calendar;
            if (cal != null)
            {
                cal.Style = FindResource("cld_Normal") as Style;
            }
            else
            {
                ErrorLogger.AddLog(string.Format("Could not set calendar style for {0}.", dtp.Name), ErrorSeverity.LOW);
            }
        }

        //------//
        // Data //
        //------//
        public ProjectLog Log { get; private set; }
        private const string dateTimeFormat = "MM/dd/yyyy hh:mm tt";
    }
}

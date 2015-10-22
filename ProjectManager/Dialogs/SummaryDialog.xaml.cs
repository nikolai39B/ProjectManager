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

namespace ProjectManager
{
    /// <summary>
    /// Interaction logic for SummaryDialog.xaml
    /// </summary>
    public partial class SummaryDialog : Window
    {
        public SummaryDialog(string title, List<SummaryRow> items, bool areProjects)
        {
            InitializeComponent();

            // Title
            tbl_Title.Text = title;

            // Items
            defaultItems = items;
            AreProjects = areProjects;

            // Bottom Row
            cb_SortByTime.IsChecked = UserSettings.SummarySortByTime;
            if (areProjects)
            {
                cb_IgnoreHiddenProjects.IsChecked = UserSettings.SummaryIgnoreHiddenProjects;
                cb_IgnoreHiddenProjects.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                cb_IgnoreHiddenProjects.IsChecked = false;
                cb_IgnoreHiddenProjects.Visibility = System.Windows.Visibility.Collapsed;
            }

            // Set up the UI
            RefreshUI();
        }

        //----------------//
        // Helper Methods //
        //----------------//
        /// <summary>
        /// Refreshes the elements on the UI.
        /// </summary>
        private void RefreshUI()
        {
            GenerateAndSetCurrentItems();
            SetCurrentItemsOnUI();
            SetTotalTimeOnUI();
        }

        /// <summary>
        /// Generates the list of current items based on the default items, whether sort by time is checked,
        /// and whether ignore hidden projects is clicked.
        /// </summary>
        private void GenerateAndSetCurrentItems()
        {
            currentItems = new List<SummaryRow>();
            foreach (var item in defaultItems)
            {
                if (cb_IgnoreHiddenProjects.IsChecked == false ||
                    item.CurrProject == null || 
                    !UserSettings.HiddenProjects.Contains(item.CurrProject))
                {
                    currentItems.Add(item);
                }
            }

            if (cb_SortByTime.IsChecked == true)
            {
                currentItems = currentItems.OrderBy(i => i.Time).Reverse().ToList();
            }
        }

        /// <summary>
        /// Sets the entries on the UI based on the current items.
        /// </summary>
        private void SetCurrentItemsOnUI()
        {
            sp_Entries.Children.Clear();
            foreach (var item in currentItems)
            {
                sp_Entries.Children.Add(item);
            }
        }

        /// <summary>
        /// Generates the total time from all projects or logs and sets it on the UI.
        /// </summary>
        /// <returns>A SummaryRow containing the total time.</returns>
        private void SetTotalTimeOnUI()
        {
            // Get the time
            TimeSpan timeSpan = new TimeSpan(0);
            foreach (var item in currentItems)
            {
                if (AreProjects && item.CurrProject != null)
                {
                    timeSpan += item.CurrProject.GetTotalProjectTime();
                }
                else if (!AreProjects && item.CurrLog != null)
                {
                    timeSpan += item.CurrLog.End - item.CurrLog.Start;
                }
            }

            // Put it on the UI
            SummaryRow row = new SummaryRow("Total time:", timeSpan);
            sp_Total.Children.Clear();
            sp_Total.Children.Add(row);
        }

        //---------------//
        // Event Handles //
        //---------------//
        private void cb_SortByTime_Click(object sender, RoutedEventArgs e)
        {
            UserSettings.SummarySortByTime = cb_SortByTime.IsChecked == true;
            RefreshUI();
        }

        private void b_Ok_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void cb_IgnoreHiddenProjects_Click(object sender, RoutedEventArgs e)
        {
            UserSettings.SummaryIgnoreHiddenProjects = cb_IgnoreHiddenProjects.IsChecked == true;
            RefreshUI();
        }

        //------//
        // Data //
        //------//
        public bool AreProjects { get; private set; }
        private List<SummaryRow> defaultItems;
        private List<SummaryRow> currentItems;
    }
}

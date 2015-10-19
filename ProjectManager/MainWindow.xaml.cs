﻿using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProjectManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Content = new Home();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ProjectFileInterface.WriteProjectsToListFile(ProjectOrganizer.Projects);
            // If we quit from the 'More Options' screen, apply the UI settings
            if (Content is MoreOptions)
            {
                ((MoreOptions)Content).ApplySettingsToUserSettings();
            }
            ProjectFileInterface.WriteSettingsToFile();
        }
    }
}

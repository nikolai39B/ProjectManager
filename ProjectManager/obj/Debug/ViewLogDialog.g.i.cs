﻿#pragma checksum "..\..\ViewLogDialog.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "69B076A727A9B76FDEED360FAE6C4200"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18444
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;
using Xceed.Wpf.Toolkit;
using Xceed.Wpf.Toolkit.Chromes;
using Xceed.Wpf.Toolkit.Core.Converters;
using Xceed.Wpf.Toolkit.Core.Input;
using Xceed.Wpf.Toolkit.Core.Media;
using Xceed.Wpf.Toolkit.Core.Utilities;
using Xceed.Wpf.Toolkit.Panels;
using Xceed.Wpf.Toolkit.Primitives;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using Xceed.Wpf.Toolkit.PropertyGrid.Commands;
using Xceed.Wpf.Toolkit.PropertyGrid.Converters;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;
using Xceed.Wpf.Toolkit.Zoombox;


namespace ProjectManager {
    
    
    /// <summary>
    /// ViewLogDialog
    /// </summary>
    public partial class ViewLogDialog : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 19 "..\..\ViewLogDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Xceed.Wpf.Toolkit.DateTimePicker dtp_StartTime;
        
        #line default
        #line hidden
        
        
        #line 29 "..\..\ViewLogDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Xceed.Wpf.Toolkit.DateTimePicker dtp_EndTime;
        
        #line default
        #line hidden
        
        
        #line 33 "..\..\ViewLogDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox tb_Description;
        
        #line default
        #line hidden
        
        
        #line 36 "..\..\ViewLogDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock tbl_ErrorMessage;
        
        #line default
        #line hidden
        
        
        #line 46 "..\..\ViewLogDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button b_Save;
        
        #line default
        #line hidden
        
        
        #line 47 "..\..\ViewLogDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button b_Delete;
        
        #line default
        #line hidden
        
        
        #line 48 "..\..\ViewLogDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button b_Cancel;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/ProjectManager;component/viewlogdialog.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\ViewLogDialog.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.dtp_StartTime = ((Xceed.Wpf.Toolkit.DateTimePicker)(target));
            return;
            case 2:
            this.dtp_EndTime = ((Xceed.Wpf.Toolkit.DateTimePicker)(target));
            return;
            case 3:
            this.tb_Description = ((System.Windows.Controls.TextBox)(target));
            return;
            case 4:
            this.tbl_ErrorMessage = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 5:
            this.b_Save = ((System.Windows.Controls.Button)(target));
            
            #line 46 "..\..\ViewLogDialog.xaml"
            this.b_Save.Click += new System.Windows.RoutedEventHandler(this.b_Save_Click);
            
            #line default
            #line hidden
            return;
            case 6:
            this.b_Delete = ((System.Windows.Controls.Button)(target));
            
            #line 47 "..\..\ViewLogDialog.xaml"
            this.b_Delete.Click += new System.Windows.RoutedEventHandler(this.b_Delete_Click);
            
            #line default
            #line hidden
            return;
            case 7:
            this.b_Cancel = ((System.Windows.Controls.Button)(target));
            
            #line 48 "..\..\ViewLogDialog.xaml"
            this.b_Cancel.Click += new System.Windows.RoutedEventHandler(this.b_Cancel_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}


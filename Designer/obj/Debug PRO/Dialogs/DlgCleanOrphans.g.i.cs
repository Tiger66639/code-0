﻿#pragma checksum "..\..\..\Dialogs\DlgCleanOrphans.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "CE70E778BBAF55A5B3394490ABA315F5"
//------------------------------------------------------------------------------
// <auto-generated>
//     Dieser Code wurde von einem Tool generiert.
//     Laufzeitversion:4.0.30319.34209
//
//     Änderungen an dieser Datei können falsches Verhalten verursachen und gehen verloren, wenn
//     der Code erneut generiert wird.
// </auto-generated>
//------------------------------------------------------------------------------

using JaStDev.HAB;
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


namespace JaStDev.HAB.Designer {
    
    
    /// <summary>
    /// DlgCleanOrphans
    /// </summary>
    public partial class DlgCleanOrphans : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 6 "..\..\..\Dialogs\DlgCleanOrphans.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal JaStDev.HAB.Designer.DlgCleanOrphans @this;
        
        #line default
        #line hidden
        
        
        #line 49 "..\..\..\Dialogs\DlgCleanOrphans.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button BtnStartStop;
        
        #line default
        #line hidden
        
        
        #line 57 "..\..\..\Dialogs\DlgCleanOrphans.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button BtnDelete;
        
        #line default
        #line hidden
        
        
        #line 67 "..\..\..\Dialogs\DlgCleanOrphans.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button BtnClose;
        
        #line default
        #line hidden
        
        
        #line 86 "..\..\..\Dialogs\DlgCleanOrphans.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox ChkSelectAll;
        
        #line default
        #line hidden
        
        
        #line 98 "..\..\..\Dialogs\DlgCleanOrphans.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListView LstOrphans;
        
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
            System.Uri resourceLocater = new System.Uri("/HAB.Designer;component/dialogs/dlgcleanorphans.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Dialogs\DlgCleanOrphans.xaml"
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
            this.@this = ((JaStDev.HAB.Designer.DlgCleanOrphans)(target));
            return;
            case 2:
            this.BtnStartStop = ((System.Windows.Controls.Button)(target));
            
            #line 50 "..\..\..\Dialogs\DlgCleanOrphans.xaml"
            this.BtnStartStop.Click += new System.Windows.RoutedEventHandler(this.OnClickStart);
            
            #line default
            #line hidden
            return;
            case 3:
            this.BtnDelete = ((System.Windows.Controls.Button)(target));
            
            #line 58 "..\..\..\Dialogs\DlgCleanOrphans.xaml"
            this.BtnDelete.Click += new System.Windows.RoutedEventHandler(this.OnDelete_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.BtnClose = ((System.Windows.Controls.Button)(target));
            return;
            case 5:
            this.ChkSelectAll = ((System.Windows.Controls.CheckBox)(target));
            
            #line 85 "..\..\..\Dialogs\DlgCleanOrphans.xaml"
            this.ChkSelectAll.Checked += new System.Windows.RoutedEventHandler(this.ChkSelectAl_Checked);
            
            #line default
            #line hidden
            
            #line 87 "..\..\..\Dialogs\DlgCleanOrphans.xaml"
            this.ChkSelectAll.Unchecked += new System.Windows.RoutedEventHandler(this.ChkSelectAl_Unchecked);
            
            #line default
            #line hidden
            
            #line 88 "..\..\..\Dialogs\DlgCleanOrphans.xaml"
            this.ChkSelectAll.Indeterminate += new System.Windows.RoutedEventHandler(this.ChkSelectAl_Indeterminate);
            
            #line default
            #line hidden
            return;
            case 6:
            this.LstOrphans = ((System.Windows.Controls.ListView)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}


﻿#pragma checksum "..\..\..\..\..\Editors\TextPatterns\search\DlgFindText.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "B4223ED945C0E53A39E2E56647EA118A"
//------------------------------------------------------------------------------
// <auto-generated>
//     Dieser Code wurde von einem Tool generiert.
//     Laufzeitversion:4.0.30319.34209
//
//     Änderungen an dieser Datei können falsches Verhalten verursachen und gehen verloren, wenn
//     der Code erneut generiert wird.
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


namespace JaStDev.HAB.Designer.Dialogs {
    
    
    /// <summary>
    /// DlgFindText
    /// </summary>
    public partial class DlgFindText : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 34 "..\..\..\..\..\Editors\TextPatterns\search\DlgFindText.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button BtnFindAll;
        
        #line default
        #line hidden
        
        
        #line 46 "..\..\..\..\..\Editors\TextPatterns\search\DlgFindText.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button BtnReplaceAll;
        
        #line default
        #line hidden
        
        
        #line 55 "..\..\..\..\..\Editors\TextPatterns\search\DlgFindText.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.RadioButton RadFind;
        
        #line default
        #line hidden
        
        
        #line 60 "..\..\..\..\..\Editors\TextPatterns\search\DlgFindText.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.RadioButton RadReplace;
        
        #line default
        #line hidden
        
        
        #line 69 "..\..\..\..\..\Editors\TextPatterns\search\DlgFindText.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox TxtToSearch;
        
        #line default
        #line hidden
        
        
        #line 76 "..\..\..\..\..\Editors\TextPatterns\search\DlgFindText.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox TxtToReplace;
        
        #line default
        #line hidden
        
        
        #line 96 "..\..\..\..\..\Editors\TextPatterns\search\DlgFindText.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox CmbSelectScope;
        
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
            System.Uri resourceLocater = new System.Uri("/HAB.Designer;component/editors/textpatterns/search/dlgfindtext.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\..\Editors\TextPatterns\search\DlgFindText.xaml"
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
            
            #line 11 "..\..\..\..\..\Editors\TextPatterns\search\DlgFindText.xaml"
            ((JaStDev.HAB.Designer.Dialogs.DlgFindText)(target)).Closed += new System.EventHandler(this.Window_Closed);
            
            #line default
            #line hidden
            return;
            case 2:
            
            #line 16 "..\..\..\..\..\Editors\TextPatterns\search\DlgFindText.xaml"
            ((System.Windows.Input.CommandBinding)(target)).Executed += new System.Windows.Input.ExecutedRoutedEventHandler(this.Close_Executed);
            
            #line default
            #line hidden
            return;
            case 3:
            
            #line 28 "..\..\..\..\..\Editors\TextPatterns\search\DlgFindText.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.FindNext_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.BtnFindAll = ((System.Windows.Controls.Button)(target));
            
            #line 33 "..\..\..\..\..\Editors\TextPatterns\search\DlgFindText.xaml"
            this.BtnFindAll.Click += new System.Windows.RoutedEventHandler(this.FindAll_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            
            #line 39 "..\..\..\..\..\Editors\TextPatterns\search\DlgFindText.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.Replace_Click);
            
            #line default
            #line hidden
            return;
            case 6:
            this.BtnReplaceAll = ((System.Windows.Controls.Button)(target));
            
            #line 45 "..\..\..\..\..\Editors\TextPatterns\search\DlgFindText.xaml"
            this.BtnReplaceAll.Click += new System.Windows.RoutedEventHandler(this.ReplaceAll_Click);
            
            #line default
            #line hidden
            return;
            case 7:
            this.RadFind = ((System.Windows.Controls.RadioButton)(target));
            return;
            case 8:
            this.RadReplace = ((System.Windows.Controls.RadioButton)(target));
            return;
            case 9:
            this.TxtToSearch = ((System.Windows.Controls.TextBox)(target));
            return;
            case 10:
            this.TxtToReplace = ((System.Windows.Controls.TextBox)(target));
            return;
            case 11:
            this.CmbSelectScope = ((System.Windows.Controls.ComboBox)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}


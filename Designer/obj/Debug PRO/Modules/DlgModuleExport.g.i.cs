﻿#pragma checksum "..\..\..\Modules\DlgModuleExport.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "53A79A53DE86A9EA35A6D023D2BA233B"
//------------------------------------------------------------------------------
// <auto-generated>
//     Dieser Code wurde von einem Tool generiert.
//     Laufzeitversion:4.0.30319.34209
//
//     Änderungen an dieser Datei können falsches Verhalten verursachen und gehen verloren, wenn
//     der Code erneut generiert wird.
// </auto-generated>
//------------------------------------------------------------------------------

using JaStDev.ControlFramework.Controls;
using JaStDev.ControlFramework.Controls.Primitives;
using JaStDev.ControlFramework.Input;
using JaStDev.ControlFramework.Utility;
using JaStDev.HAB.Designer;
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
    /// DlgModuleExport
    /// </summary>
    public partial class DlgModuleExport : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 7 "..\..\..\Modules\DlgModuleExport.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal JaStDev.HAB.Designer.Dialogs.DlgModuleExport TheWindow;
        
        #line default
        #line hidden
        
        
        #line 93 "..\..\..\Modules\DlgModuleExport.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox TxtName;
        
        #line default
        #line hidden
        
        
        #line 107 "..\..\..\Modules\DlgModuleExport.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox TxtMajor;
        
        #line default
        #line hidden
        
        
        #line 115 "..\..\..\Modules\DlgModuleExport.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox TxtMinor;
        
        #line default
        #line hidden
        
        
        #line 125 "..\..\..\Modules\DlgModuleExport.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox TxtPath;
        
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
            System.Uri resourceLocater = new System.Uri("/HAB.Designer;component/modules/dlgmoduleexport.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Modules\DlgModuleExport.xaml"
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
            this.TheWindow = ((JaStDev.HAB.Designer.Dialogs.DlgModuleExport)(target));
            return;
            case 2:
            
            #line 36 "..\..\..\Modules\DlgModuleExport.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.OnClickOk);
            
            #line default
            #line hidden
            return;
            case 3:
            this.TxtName = ((System.Windows.Controls.TextBox)(target));
            return;
            case 4:
            
            #line 98 "..\..\..\Modules\DlgModuleExport.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.BtnName_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            this.TxtMajor = ((System.Windows.Controls.TextBox)(target));
            return;
            case 6:
            this.TxtMinor = ((System.Windows.Controls.TextBox)(target));
            return;
            case 7:
            this.TxtPath = ((System.Windows.Controls.TextBox)(target));
            return;
            case 8:
            
            #line 129 "..\..\..\Modules\DlgModuleExport.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.BtnPath_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}


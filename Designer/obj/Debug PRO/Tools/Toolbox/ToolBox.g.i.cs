﻿#pragma checksum "..\..\..\..\Tools\Toolbox\ToolBox.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "35360981CE6190563152ED67931ED2FB"
//------------------------------------------------------------------------------
// <auto-generated>
//     Dieser Code wurde von einem Tool generiert.
//     Laufzeitversion:4.0.30319.34209
//
//     Änderungen an dieser Datei können falsches Verhalten verursachen und gehen verloren, wenn
//     der Code erneut generiert wird.
// </auto-generated>
//------------------------------------------------------------------------------

using DnD;
using JaStDev.ControlFramework.Controls;
using JaStDev.ControlFramework.Controls.Primitives;
using JaStDev.ControlFramework.Input;
using JaStDev.ControlFramework.Utility;
using JaStDev.HAB.Designer;
using LearnWPF.Effects;
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
    /// ToolBox
    /// </summary>
    public partial class ToolBox : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 9 "..\..\..\..\Tools\Toolbox\ToolBox.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal JaStDev.HAB.Designer.ToolBox thisToolBox;
        
        #line default
        #line hidden
        
        
        #line 205 "..\..\..\..\Tools\Toolbox\ToolBox.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.RadioButton ToggleItems;
        
        #line default
        #line hidden
        
        
        #line 213 "..\..\..\..\Tools\Toolbox\ToolBox.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.RadioButton ToggleInstructions;
        
        #line default
        #line hidden
        
        
        #line 218 "..\..\..\..\Tools\Toolbox\ToolBox.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button BtnReset;
        
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
            System.Uri resourceLocater = new System.Uri("/HAB.Designer;component/tools/toolbox/toolbox.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\Tools\Toolbox\ToolBox.xaml"
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
            this.thisToolBox = ((JaStDev.HAB.Designer.ToolBox)(target));
            return;
            case 2:
            
            #line 61 "..\..\..\..\Tools\Toolbox\ToolBox.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.MnuItemsCollapsItems_Click);
            
            #line default
            #line hidden
            return;
            case 3:
            
            #line 63 "..\..\..\..\Tools\Toolbox\ToolBox.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.MnuItemsExpandItems_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            
            #line 65 "..\..\..\..\Tools\Toolbox\ToolBox.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.ExportDefaults_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            
            #line 96 "..\..\..\..\Tools\Toolbox\ToolBox.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.MnuInstructionsCollapsItems_Click);
            
            #line default
            #line hidden
            return;
            case 6:
            
            #line 98 "..\..\..\..\Tools\Toolbox\ToolBox.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.MnuInstructionsExpandItems_Click);
            
            #line default
            #line hidden
            return;
            case 7:
            
            #line 100 "..\..\..\..\Tools\Toolbox\ToolBox.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.ExportDefaults_Click);
            
            #line default
            #line hidden
            return;
            case 8:
            this.ToggleItems = ((System.Windows.Controls.RadioButton)(target));
            return;
            case 9:
            this.ToggleInstructions = ((System.Windows.Controls.RadioButton)(target));
            return;
            case 10:
            this.BtnReset = ((System.Windows.Controls.Button)(target));
            
            #line 221 "..\..\..\..\Tools\Toolbox\ToolBox.xaml"
            this.BtnReset.Click += new System.Windows.RoutedEventHandler(this.BtnReset_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}


﻿#pragma checksum "..\..\..\..\Comm channels\Reflection\ReflectionChannelView.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "445A8E17D2FEA2A9A752E80E3E523120"
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
    /// ReflectionChannelView
    /// </summary>
    public partial class ReflectionChannelView : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 82 "..\..\..\..\Comm channels\Reflection\ReflectionChannelView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button BtnLoadCach;
        
        #line default
        #line hidden
        
        
        #line 89 "..\..\..\..\Comm channels\Reflection\ReflectionChannelView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button BtnLoadFile;
        
        #line default
        #line hidden
        
        
        #line 97 "..\..\..\..\Comm channels\Reflection\ReflectionChannelView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Primitives.ToggleButton BtnToggleOpcodes;
        
        #line default
        #line hidden
        
        
        #line 110 "..\..\..\..\Comm channels\Reflection\ReflectionChannelView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ColumnDefinition ColSplitter;
        
        #line default
        #line hidden
        
        
        #line 111 "..\..\..\..\Comm channels\Reflection\ReflectionChannelView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ColumnDefinition ColOpCodes;
        
        #line default
        #line hidden
        
        
        #line 114 "..\..\..\..\Comm channels\Reflection\ReflectionChannelView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TreeView TrvAssemblies;
        
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
            System.Uri resourceLocater = new System.Uri("/HAB.Designer;component/comm%20channels/reflection/reflectionchannelview.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\Comm channels\Reflection\ReflectionChannelView.xaml"
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
            
            #line 9 "..\..\..\..\Comm channels\Reflection\ReflectionChannelView.xaml"
            ((System.Windows.Input.CommandBinding)(target)).CanExecute += new System.Windows.Input.CanExecuteRoutedEventHandler(this.Delete_CanExecute);
            
            #line default
            #line hidden
            
            #line 9 "..\..\..\..\Comm channels\Reflection\ReflectionChannelView.xaml"
            ((System.Windows.Input.CommandBinding)(target)).Executed += new System.Windows.Input.ExecutedRoutedEventHandler(this.Delete_Executed);
            
            #line default
            #line hidden
            return;
            case 2:
            
            #line 10 "..\..\..\..\Comm channels\Reflection\ReflectionChannelView.xaml"
            ((System.Windows.Input.CommandBinding)(target)).CanExecute += new System.Windows.Input.CanExecuteRoutedEventHandler(this.Export_CanExecute);
            
            #line default
            #line hidden
            
            #line 10 "..\..\..\..\Comm channels\Reflection\ReflectionChannelView.xaml"
            ((System.Windows.Input.CommandBinding)(target)).Executed += new System.Windows.Input.ExecutedRoutedEventHandler(this.Export_Executed);
            
            #line default
            #line hidden
            return;
            case 3:
            this.BtnLoadCach = ((System.Windows.Controls.Button)(target));
            
            #line 83 "..\..\..\..\Comm channels\Reflection\ReflectionChannelView.xaml"
            this.BtnLoadCach.Click += new System.Windows.RoutedEventHandler(this.BtnLoadCach_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.BtnLoadFile = ((System.Windows.Controls.Button)(target));
            
            #line 90 "..\..\..\..\Comm channels\Reflection\ReflectionChannelView.xaml"
            this.BtnLoadFile.Click += new System.Windows.RoutedEventHandler(this.BtnLoadFile_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            this.BtnToggleOpcodes = ((System.Windows.Controls.Primitives.ToggleButton)(target));
            
            #line 98 "..\..\..\..\Comm channels\Reflection\ReflectionChannelView.xaml"
            this.BtnToggleOpcodes.Checked += new System.Windows.RoutedEventHandler(this.BtnToggleOpcodes_Checked);
            
            #line default
            #line hidden
            
            #line 99 "..\..\..\..\Comm channels\Reflection\ReflectionChannelView.xaml"
            this.BtnToggleOpcodes.Unchecked += new System.Windows.RoutedEventHandler(this.BtnToggleOpcodes_Unchecked);
            
            #line default
            #line hidden
            return;
            case 6:
            this.ColSplitter = ((System.Windows.Controls.ColumnDefinition)(target));
            return;
            case 7:
            this.ColOpCodes = ((System.Windows.Controls.ColumnDefinition)(target));
            return;
            case 8:
            this.TrvAssemblies = ((System.Windows.Controls.TreeView)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

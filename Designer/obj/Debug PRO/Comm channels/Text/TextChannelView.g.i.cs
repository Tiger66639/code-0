﻿#pragma checksum "..\..\..\..\Comm channels\Text\TextChannelView.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "EE529E4822579856A2748D7EB6068FBD"
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
using JaStDev.HAB;
using JaStDev.HAB.Designer;
using Microsoft.Windows.Themes;
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
    /// TextChannelView
    /// </summary>
    public partial class TextChannelView : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 3 "..\..\..\..\Comm channels\Text\TextChannelView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal JaStDev.HAB.Designer.TextChannelView Ctrl;
        
        #line default
        #line hidden
        
        
        #line 93 "..\..\..\..\Comm channels\Text\TextChannelView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button BtnCopy;
        
        #line default
        #line hidden
        
        
        #line 114 "..\..\..\..\Comm channels\Text\TextChannelView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Primitives.ToggleButton ToggleAudio;
        
        #line default
        #line hidden
        
        
        #line 138 "..\..\..\..\Comm channels\Text\TextChannelView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListBox LstInput;
        
        #line default
        #line hidden
        
        
        #line 156 "..\..\..\..\Comm channels\Text\TextChannelView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListBox LstOutput;
        
        #line default
        #line hidden
        
        
        #line 174 "..\..\..\..\Comm channels\Text\TextChannelView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListBox LstDialog;
        
        #line default
        #line hidden
        
        
        #line 188 "..\..\..\..\Comm channels\Text\TextChannelView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DockPanel DockChatInput;
        
        #line default
        #line hidden
        
        
        #line 193 "..\..\..\..\Comm channels\Text\TextChannelView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox TxtToSend;
        
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
            System.Uri resourceLocater = new System.Uri("/HAB.Designer;component/comm%20channels/text/textchannelview.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\Comm channels\Text\TextChannelView.xaml"
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
            this.Ctrl = ((JaStDev.HAB.Designer.TextChannelView)(target));
            
            #line 10 "..\..\..\..\Comm channels\Text\TextChannelView.xaml"
            this.Ctrl.DataContextChanged += new System.Windows.DependencyPropertyChangedEventHandler(this.Ctrl_DataContextChanged);
            
            #line default
            #line hidden
            return;
            case 2:
            
            #line 67 "..\..\..\..\Comm channels\Text\TextChannelView.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.SaveConv_Click);
            
            #line default
            #line hidden
            return;
            case 3:
            
            #line 77 "..\..\..\..\Comm channels\Text\TextChannelView.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.BtnSend_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.BtnCopy = ((System.Windows.Controls.Button)(target));
            
            #line 94 "..\..\..\..\Comm channels\Text\TextChannelView.xaml"
            this.BtnCopy.Click += new System.Windows.RoutedEventHandler(this.BtnCopy_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            
            #line 101 "..\..\..\..\Comm channels\Text\TextChannelView.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.SaveConv_Click);
            
            #line default
            #line hidden
            return;
            case 6:
            
            #line 106 "..\..\..\..\Comm channels\Text\TextChannelView.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.BtnClear_Click);
            
            #line default
            #line hidden
            return;
            case 7:
            this.ToggleAudio = ((System.Windows.Controls.Primitives.ToggleButton)(target));
            return;
            case 8:
            this.LstInput = ((System.Windows.Controls.ListBox)(target));
            return;
            case 9:
            this.LstOutput = ((System.Windows.Controls.ListBox)(target));
            return;
            case 10:
            this.LstDialog = ((System.Windows.Controls.ListBox)(target));
            return;
            case 11:
            this.DockChatInput = ((System.Windows.Controls.DockPanel)(target));
            return;
            case 12:
            this.TxtToSend = ((System.Windows.Controls.TextBox)(target));
            
            #line 196 "..\..\..\..\Comm channels\Text\TextChannelView.xaml"
            this.TxtToSend.PreviewKeyDown += new System.Windows.Input.KeyEventHandler(this.TxtSend_PrvKeyDown);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

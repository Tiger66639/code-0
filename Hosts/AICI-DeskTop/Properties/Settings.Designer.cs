//-----------------------------------------------------------------------
// <copyright file="Settings.Designer.cs">
//     Copyright (c) Dev\Bragi. All rights reserved.
// </copyright> 
// <authorJan Bogaerts</author>
// <email>Jan.Bogaerts@telenet.be</email>
// <date>15/05/2012</date>
//-----------------------------------------------------------------------
namespace AICI_DeskTop.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("25")]
        public int MaxConcurrentProcessors {
            get {
                return ((int)(this["MaxConcurrentProcessors"]));
            }
            set {
                this["MaxConcurrentProcessors"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("40")]
        public int InitProcessorStackSize {
            get {
                return ((int)(this["InitProcessorStackSize"]));
            }
            set {
                this["InitProcessorStackSize"] = value;
            }
        }
    }
}
// Define stuff we need to download/update/install

;#define use_iis
;#define use_kb835732
;#define use_kb886903
;#define use_kb928366

;#define use_msi20
;#define use_msi31
#define use_msi45
;#define use_ie6

;#define use_dotnetfx11
; German languagepack?
;#define use_dotnetfx11lp

;#define use_dotnetfx20
; German languagepack?
;#define use_dotnetfx20lp

;#define use_dotnetfx35
; German languagepack?
;#define use_dotnetfx35lp

; dotnet_Passive enabled shows the .NET/VC2010 installation progress, as it can take quite some time
#define dotnet_Passive
#define use_dotnetfx40
;#define use_vc2010

;#define use_mdac28
;#define use_jet4sp8
; SQL 3.5 Compact Edition
;#define use_scceruntime
; SQL Express
;#define use_sql2005express
;#define use_sql2008express

; Enable the required define(s) below if a local event function (prepended with Local) is used
;#define haveLocalPrepareToInstall
;#define haveLocalNeedRestart
;#define haveLocalNextButtonClick

// End

#define SetupScriptVersion '0'
;Select the version to compile for (this will automatically switch the source directory, so the value reflects the source path of the app.

;#define VERBASIC
;#define VERBASIC32
;#define VERPRO
;#define VERDESIGNER

#define MyAppVersion "1.4.1"
#define MyAppPublisher "Jan Bogaerts"
#define MyAppURL "http://www.janbogaerts.name/"

#ifdef VERBASIC
#define NNDRELEASEVER "Release basic"
#define MyAppExeName "CBDBasic"
#define ENDOFAPPNAME "basic"
#define MyAppSetupName "Chatbot designer basic"
#endif

#ifdef VERBASIC32
#define NNDRELEASEVER "Release Basic 32"
#define MyAppExeName "CBDBasic32"
#define ENDOFAPPNAME "basic 32"
#define MyAppSetupName "Chatbot designer basic 32"
#endif

#ifdef VERPRO
#define NNDRELEASEVER "Release PRO"
#define MyAppExeName "CBDPro"
#define ENDOFAPPNAME "Pro"
#define MyAppSetupName "Chatbot designer PRO"
#endif

#ifdef VERDESIGNER
#define NNDRELEASEVER "Release"
#define MyAppExeName "NNDSetup"
#define ENDOFAPPNAME ""
#define MyAppSetupName "neural network designer"
#endif

#define SetupScriptVersion '0'

[Setup]
AppName={#MyAppSetupName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppSetupName} {#MyAppVersion}
DefaultGroupName={#MyAppSetupName}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
Uninstallable=true
DirExistsWarning=no
CreateAppDir=true
OutputDir=bin
OutputBaseFilename={#MyAppExeName}
SourceDir=.
AppCopyright=Copyright © Jan Bogaerts 2009-2013
AllowNoIcons=true
AppPublisher={#MyAppPublisher}
UninstallDisplayIcon={app}\HAB.Designer.exe
UninstallDisplayName={#MyAppSetupName} uninstaller
UsePreviousGroup=true
UsePreviousAppDir=true
DefaultDirName={pf}\{#MyAppSetupName}
VersionInfoVersion={#MyAppVersion}
VersionInfoCompany=Jan Bogaerts
VersionInfoCopyright=Copyright © Jan Bogaerts 2009-2013
ShowUndisplayableLanguages=false
LanguageDetectionMethod=uilanguage
InternalCompressLevel=max
SolidCompression=true
Compression=lzma
;required by products
MinVersion=0,5.01
PrivilegesRequired=admin
ShowLanguageDialog=auto
WizardImageFile=compiler:WizModernImage-IS.bmp
WizardSmallImageFile=compiler:WizModernSmallImage-IS.bmp
AppId={{22F9D3E9-2774-473E-8E47-9F99DC23F2DA}
ChangesAssociations=true
LicenseFile=..\LicenseAgreement.rtf

[Languages]
Name: en; MessagesFile: compiler:Default.isl
;Name: de; MessagesFile: compiler:Languages\German.isl

[Tasks]
Name: desktopicon; Description: {cm:CreateDesktopIcon}; GroupDescription: {cm:AdditionalIcons}; Flags: unchecked
Name: quicklaunchicon; Description: {cm:CreateQuickLaunchIcon}; GroupDescription: {cm:AdditionalIcons}; Flags: unchecked

[Dirs]
Name: {userdocs}\NND\Templates\Default\Modules
Name: {userdocs}\NND\Demos\empty_project\Modules
Name: {userdocs}\NND\Demos\empty_project_with_callbacks\Modules

[Files]
Source: ..\Designer\bin\{#NNDRELEASEVER}\HAB.Designer.exe; DestDir: {app}
Source: ..\Designer\bin\{#NNDRELEASEVER}\*.dll; DestDir: {app}
;Source: ..\Designer\bin\{#NNDRELEASEVER}\*.mdf; DestDir: {app}
;Source: ..\Designer\bin\{#NNDRELEASEVER}\*.ldf; DestDir: {app}
Source: ..\Designer\bin\{#NNDRELEASEVER}\*.config; DestDir: {app}
Source: ..\Designer\bin\{#NNDRELEASEVER}\NND.chm; DestDir: {app}
Source: ..\Designer\bin\{#NNDRELEASEVER}\DefaultData\*.*; DestDir: {app}\DefaultData
Source: ..\Designer\bin\{#NNDRELEASEVER}\com.bragisoft.aici.apk; DestDir: {app}
source: ..\external libs\adb\adb.exe; DestDir: {app}
source: ..\external libs\adb\AdbWinApi.dll; DestDir: {app}
source: ..\external libs\adb\AdbWinUsbApi.dll; DestDir: {app}

Source: ..\hosts\mvc3\bin\online.zip; DestDir: {app}

Source: ..\Networks\default.dpl; DestDir: {userdocs}\NND\Templates; DestName: default.dpl; Attribs: readonly; Flags: overwritereadonly
Source: ..\Networks\default\*.*; DestDir: {userdocs}\NND\Templates\Default
Source: ..\Networks\default\DesignerData\*.*; DestDir: {userdocs}\NND\Templates\Default\DesignerData
Source: ..\Networks\default\Data\*.*; DestDir: {userdocs}\NND\Templates\Default\Data
Source: ..\Networks\default\Modules\*.*; DestDir: {userdocs}\NND\Templates\Default\Modules

Source: ..\Networks\Rebuildtemplate.dpl; DestDir: {userdocs}\NND\Templates; DestName: Rebuildtemplate.dpl; Attribs: readonly; Flags: overwritereadonly
Source: ..\Networks\Rebuildtemplate\*.*; DestDir: {userdocs}\NND\Templates\Rebuildtemplate
Source: ..\Networks\Rebuildtemplate\DesignerData\*.*; DestDir: {userdocs}\NND\Templates\Rebuildtemplate\DesignerData
Source: ..\Networks\Rebuildtemplate\Data\*.*; DestDir: {userdocs}\NND\Templates\Rebuildtemplate\Data
Source: ..\Networks\Rebuildtemplate\Modules\*.*; DestDir: {userdocs}\NND\Templates\Rebuildtemplate\Modules

Source: ..\Networks\asset operations.dpl; DestDir: {userdocs}\NND\Demos\old; DestName: asset_operations.dpl; Attribs: readonly; Flags: overwritereadonly
Source: ..\Networks\asset operations\*.*; DestDir: {userdocs}\NND\Demos\old\asset_operations
Source: ..\Networks\asset operations\DesignerData\*.*; DestDir: {userdocs}\NND\Demos\old\asset_operations\DesignerData
Source: ..\Networks\asset operations\Data\*.*; DestDir: {userdocs}\NND\Demos\old\asset_operations\Data
Source: ..\Networks\asset operations\Modules\*.*; DestDir: {userdocs}\NND\Demos\old\asset_operations\Modules

Source: ..\Networks\Thesaurus operations.dpl; DestDir: {userdocs}\NND\Demos\old; DestName: Thesaurus_operations.dpl; Attribs: readonly; Flags: overwritereadonly
Source: ..\Networks\Thesaurus operations\*.*; DestDir: {userdocs}\NND\Demos\old\Thesaurus_operations
Source: ..\Networks\Thesaurus operations\DesignerData\*.*; DestDir: {userdocs}\NND\Demos\old\Thesaurus_operations\DesignerData
Source: ..\Networks\Thesaurus operations\Data\*.*; DestDir: {userdocs}\NND\Demos\old\Thesaurus_operations\Data
Source: ..\Networks\Thesaurus operations\Modules\*.*; DestDir: {userdocs}\NND\Demos\old\Thesaurus_operations\Modules

Source: ..\Networks\SysMan.dpl; DestDir: {userdocs}\NND\Demos\old; DestName: SysMan.dpl; Attribs: readonly; Flags: overwritereadonly
Source: ..\Networks\SysMan\*.*; DestDir: {userdocs}\NND\Demos\old\SysMan
Source: ..\Networks\SysMan\DesignerData\*.*; DestDir: {userdocs}\NND\Demos\old\SysMan\DesignerData
Source: ..\Networks\SysMan\Data\*.*; DestDir: {userdocs}\NND\Demos\old\SysMan\Data
Source: ..\Networks\SysMan\Modules\*.*; DestDir: {userdocs}\NND\Demos\old\SysMan\Modules

Source: ..\Networks\Name_Age.dpl; DestDir: {userdocs}\NND\Demos\old; DestName: Name_Age.dpl; Attribs: readonly; Flags: overwritereadonly
Source: ..\Networks\Name_Age\*.*; DestDir: {userdocs}\NND\Demos\old\Name_Age
Source: ..\Networks\Name_Age\DesignerData\*.*; DestDir: {userdocs}\NND\Demos\old\Name_Age\DesignerData
Source: ..\Networks\Name_Age\Data\*.*; DestDir: {userdocs}\NND\Demos\old\Name_Age\Data
Source: ..\Networks\Name_Age\Modules\*.*; DestDir: {userdocs}\NND\Demos\old\Name_Age\Modules

Source: ..\Networks\weather.dpl; DestDir: {userdocs}\NND\Demos\old; DestName: weather.dpl; Attribs: readonly; Flags: overwritereadonly
Source: ..\Networks\weather\*.*; DestDir: {userdocs}\NND\Demos\old\weather
Source: ..\Networks\weather\DesignerData\*.*; DestDir: {userdocs}\NND\Demos\old\weather\DesignerData
Source: ..\Networks\weather\Data\*.*; DestDir: {userdocs}\NND\Demos\weather\old\Data
Source: ..\Networks\weather\Modules\*.*; DestDir: {userdocs}\NND\Demos\weather\old\Modules

Source: ..\Networks\DateTime.dpl; DestDir: {userdocs}\NND\Demos; DestName: DateTime.dpl; Attribs: readonly; Flags: overwritereadonly
Source: ..\Networks\DateTime\*.*; DestDir: {userdocs}\NND\Demos\DateTime
Source: ..\Networks\DateTime\DesignerData\*.*; DestDir: {userdocs}\NND\Demos\DateTime\DesignerData
Source: ..\Networks\DateTime\Data\*.*; DestDir: {userdocs}\NND\Demos\DateTime\Data
Source: ..\Networks\DateTime\Modules\*.*; DestDir: {userdocs}\NND\Demos\DateTime\Modules

Source: ..\Networks\questionAnswer.dpl; DestDir: {userdocs}\NND\Demos; DestName: questionAnswer.dpl; Attribs: readonly; Flags: overwritereadonly
Source: ..\Networks\questionAnswer\*.*; DestDir: {userdocs}\NND\Demos\questionAnswer
Source: ..\Networks\questionAnswer\DesignerData\*.*; DestDir: {userdocs}\NND\Demos\questionAnswer\DesignerData
Source: ..\Networks\questionAnswer\Data\*.*; DestDir: {userdocs}\NND\Demos\questionAnswer\Data
Source: ..\Networks\questionAnswer\Modules\*.*; DestDir: {userdocs}\NND\Demos\questionAnswer\Modules

Source: ..\Networks\why_because3.dpl; DestDir: {userdocs}\NND\Demos\old; DestName: why_because3.dpl; Attribs: readonly; Flags: overwritereadonly
Source: ..\Networks\why_because3\*.*; DestDir: {userdocs}\NND\Demos\old\why_because3
Source: ..\Networks\why_because3\DesignerData\*.*; DestDir: {userdocs}\NND\Demos\old\why_because3\DesignerData
Source: ..\Networks\why_because3\Data\*.*; DestDir: {userdocs}\NND\Demos\old\why_because3\Data
Source: ..\Networks\why_because3\Modules\*.*; DestDir: {userdocs}\NND\Demos\old\why_because3\Modules

Source: ..\Networks\why_because2.dpl; DestDir: {userdocs}\NND\Demos\old; DestName: why_because2.dpl; Attribs: readonly; Flags: overwritereadonly
Source: ..\Networks\why_because2\*.*; DestDir: {userdocs}\NND\Demos\old\why_because2
Source: ..\Networks\why_because2\DesignerData\*.*; DestDir: {userdocs}\NND\Demos\old\why_because2\DesignerData
Source: ..\Networks\why_because2\Data\*.*; DestDir: {userdocs}\NND\Demos\old\why_because2\Data
Source: ..\Networks\why_because2\Modules\*.*; DestDir: {userdocs}\NND\Demos\old\why_because2\Modules

Source: ..\Networks\why_because.dpl; DestDir: {userdocs}\NND\Demos\old; DestName: why_because.dpl; Attribs: readonly; Flags: overwritereadonly
Source: ..\Networks\why_because\*.*; DestDir: {userdocs}\NND\Demos\old\why_because
Source: ..\Networks\why_because\DesignerData\*.*; DestDir: {userdocs}\NND\Demos\old\why_because\DesignerData
Source: ..\Networks\why_because\Data\*.*; DestDir: {userdocs}\NND\Demos\old\why_because\Data
Source: ..\Networks\why_because\Modules\*.*; DestDir: {userdocs}\NND\Demos\old\why_because\Modules

Source: ..\Networks\CompleteSeq.dpl; DestDir: {userdocs}\NND\Demos\old; DestName: CompleteSeq.dpl; Attribs: readonly; Flags: overwritereadonly
Source: ..\Networks\CompleteSeq\*.*; DestDir: {userdocs}\NND\Demos\old\CompleteSeq
Source: ..\Networks\CompleteSeq\DesignerData\*.*; DestDir: {userdocs}\NND\Demos\old\CompleteSeq\DesignerData
Source: ..\Networks\CompleteSeq\Data\*.*; DestDir: {userdocs}\NND\Demos\old\CompleteSeq\Data
Source: ..\Networks\CompleteSeq\Modules\*.*; DestDir: {userdocs}\NND\Demos\old\CompleteSeq\Modules

Source: ..\Networks\android.dpl; DestDir: {userdocs}\NND\Demos\old; DestName: android.dpl; Attribs: readonly; Flags: overwritereadonly
Source: ..\Networks\android\*.*; DestDir: {userdocs}\NND\Demos\old\android
Source: ..\Networks\android\DesignerData\*.*; DestDir: {userdocs}\NND\Demos\old\android\DesignerData
Source: ..\Networks\android\Data\*.*; DestDir: {userdocs}\NND\Demos\old\android\Data
Source: ..\Networks\android\Modules\*.*; DestDir: {userdocs}\NND\Demos\old\android\Modules

Source: ..\Networks\aicionline.dpl; DestDir: {userdocs}\NND\Demos; DestName: aicionline.dpl; Attribs: readonly; Flags: overwritereadonly
Source: ..\Networks\aicionline\*.*; DestDir: {userdocs}\NND\Demos\aicionline
Source: ..\Networks\aicionline\DesignerData\*.*; DestDir: {userdocs}\NND\Demos\aicionline\DesignerData
Source: ..\Networks\aicionline\Data\*.*; DestDir: {userdocs}\NND\Demos\aicionline\Data
Source: ..\Networks\aicionline\Modules\*.*; DestDir: {userdocs}\NND\Demos\aicionline\Modules

Source: ..\Networks\GetStartedCreateBot.dpl; DestDir: {userdocs}\NND\Demos; DestName: GetStartedCreateBot.dpl; Attribs: readonly; Flags: overwritereadonly
Source: ..\Networks\GetStartedCreateBot\*.*; DestDir: {userdocs}\NND\Demos\GetStartedCreateBot
Source: ..\Networks\GetStartedCreateBot\DesignerData\*.*; DestDir: {userdocs}\NND\Demos\GetStartedCreateBot\DesignerData
Source: ..\Networks\GetStartedCreateBot\Data\*.*; DestDir: {userdocs}\NND\Demos\GetStartedCreateBot\Data
Source: ..\Networks\GetStartedCreateBot\Modules\*.*; DestDir: {userdocs}\NND\Demos\GetStartedCreateBot\Modules

Source: ..\Networks\TimerDemo.dpl; DestDir: {userdocs}\NND\Demos; DestName: TimerDemo.dpl; Attribs: readonly; Flags: overwritereadonly
Source: ..\Networks\TimerDemo\*.*; DestDir: {userdocs}\NND\Demos\TimerDemo
Source: ..\Networks\TimerDemo\DesignerData\*.*; DestDir: {userdocs}\NND\Demos\TimerDemo\DesignerData
Source: ..\Networks\TimerDemo\Data\*.*; DestDir: {userdocs}\NND\Demos\TimerDemo\Data
Source: ..\Networks\TimerDemo\Modules\*.*; DestDir: {userdocs}\NND\Demos\TimerDemo\Modules

Source: ..\Networks\AIMLThat.dpl; DestDir: {userdocs}\NND\Demos; DestName: AIMLThat.dpl; Attribs: readonly; Flags: overwritereadonly
Source: ..\Networks\AIMLThat\*.*; DestDir: {userdocs}\NND\Demos\AIMLThat
Source: ..\Networks\AIMLThat\DesignerData\*.*; DestDir: {userdocs}\NND\Demos\AIMLThat\DesignerData
Source: ..\Networks\AIMLThat\Data\*.*; DestDir: {userdocs}\NND\Demos\AIMLThat\Data
Source: ..\Networks\AIMLThat\Modules\*.*; DestDir: {userdocs}\NND\Demos\AIMLThat\Modules

Source: ..\Networks\TopicFilters.dpl; DestDir: {userdocs}\NND\Demos; DestName: TopicFilters.dpl; Attribs: readonly; Flags: overwritereadonly
Source: ..\Networks\TopicFilters\*.*; DestDir: {userdocs}\NND\Demos\TopicFilters
Source: ..\Networks\TopicFilters\DesignerData\*.*; DestDir: {userdocs}\NND\Demos\TopicFilters\DesignerData
Source: ..\Networks\TopicFilters\Data\*.*; DestDir: {userdocs}\NND\Demos\TopicFilters\Data
Source: ..\Networks\TopicFilters\Modules\*.*; DestDir: {userdocs}\NND\Demos\TopicFilters\Modules

Source: ..\Networks\Alice.dpl; DestDir: {userdocs}\NND\Demos; DestName: Alice.dpl; Attribs: readonly; Flags: overwritereadonly
Source: ..\Networks\Alice\*.*; DestDir: {userdocs}\NND\Demos\Alice
Source: ..\Networks\Alice\DesignerData\*.*; DestDir: {userdocs}\NND\Demos\Alice\DesignerData
Source: ..\Networks\Alice\Data\*.*; DestDir: {userdocs}\NND\Demos\Alice\Data
Source: ..\Networks\Alice\Modules\*.*; DestDir: {userdocs}\NND\Demos\Alice\Modules

Source: ..\Networks\empty project.dpl; DestDir: {userdocs}\NND\Demos; DestName: empty_project.dpl; Attribs: readonly; Flags: overwritereadonly
Source: ..\Networks\empty project\*.*; DestDir: {userdocs}\NND\Demos\empty_project
Source: ..\Networks\empty project\DesignerData\*.*; DestDir: {userdocs}\NND\Demos\empty_project\DesignerData
Source: ..\Networks\empty project\Data\*.*; DestDir: {userdocs}\NND\Demos\empty_project\Data
;Source: ..\Networks\empty project\Modules\*.*; DestDir: {userdocs}\NND\Demos\empty_project\Modules

Source: ..\Networks\empty project with callbacks.dpl; DestDir: {userdocs}\NND\Demos; DestName: empty_project_with_callbacks.dpl; Attribs: readonly; Flags: overwritereadonly
Source: ..\Networks\empty project with callbacks\*.*; DestDir: {userdocs}\NND\Demos\empty_project_with_callbacks
Source: ..\Networks\empty project with callbacks\DesignerData\*.*; DestDir: {userdocs}\NND\Demos\empty_project_with_callbacks\DesignerData
Source: ..\Networks\empty project with callbacks\Data\*.*; DestDir: {userdocs}\NND\Demos\empty_project_with_callbacks\Data
;Source: ..\Networks\empty project with callbacks\Modules\*.*; DestDir: {userdocs}\NND\Demos\empty_project_with_callbacks\Modules


Source: ..\FrameNetSin\Data\*.*; DestDir: {userdocs}\NND\Data\FrameNet
;we need to include the docs of framenet as well, cause otherwise it wont open properly.
Source: ..\FrameNetSin\Data\docs\*.*; DestDir: {userdocs}\NND\Data\FrameNet\docs
Source: ..\WordNetSin\Data\*.*; DestDir: {userdocs}\NND\Data

Source: ..\VerbNetProvider\Data\*.*; DestDir: {userdocs}\NND\Data\VerbNet

Source: ..\Characters\Characters\Tara\*.*; DestDir: {userdocs}\NND\Characters\Tara
Source: ..\Characters\Characters\Tara\images\*.*; DestDir: {userdocs}\NND\Characters\Tara\images

Source: ..\Characters\Characters\Mika\*.*; DestDir: {userdocs}\NND\Characters\Mika
Source: ..\Characters\Characters\Mika\images\*.*; DestDir: {userdocs}\NND\Characters\Mika\images
Source: ..\Characters\Characters\Mika\images\Expressions\*.*; DestDir: {userdocs}\NND\Characters\Mika\images\Expressions
Source: ..\Characters\Characters\Mika\images\eyes\*.*; DestDir: {userdocs}\NND\Characters\Mika\images\eyes
Source: ..\Characters\Characters\Mika\images\other\*.*; DestDir: {userdocs}\NND\Characters\Mika\images\other
Source: ..\Characters\Characters\Mika\images\visemes\*.*; DestDir: {userdocs}\NND\Characters\Mika\images\visemes

Source: ..\Modules\system\*.*; DestDir: {userdocs}\NND\Modules\system
Source: ..\Modules\Machine learning\*.*; DestDir: {userdocs}\NND\Modules\ML
Source: ..\Modules\designer\*.*; DestDir: {userdocs}\NND\Modules\designer
Source: ..\Modules\chatbot\*.*; DestDir: {userdocs}\NND\Modules\Chatbot
Source: ..\Modules\android\*.*; DestDir: {userdocs}\NND\Modules\android

;Source: ..\topics\*.*; DestDir: {userdocs}\NND\topics
Source: ..\topics\AiciOnline\*.*; DestDir: {userdocs}\NND\topics\AiciOnline
Source: ..\topics\GetStartedCreateBot\*.*; DestDir: {userdocs}\NND\topics\GetStartedCreateBot
Source: ..\topics\QuestionAnswer\*.*; DestDir: {userdocs}\NND\topics\QuestionAnswer
Source: ..\topics\TimerDemo\*.*; DestDir: {userdocs}\NND\topics\TimerDemo
Source: ..\topics\that\*.*; DestDir: {userdocs}\NND\topics\that
Source: ..\topics\topic\*.*; DestDir: {userdocs}\NND\topics\topic
Source: ..\thesauri\*.*; DestDir: {userdocs}\NND\thesauri


[Icons]
Name: {group}\{#MyAppSetupName}; Filename: {app}\HAB.Designer.exe
Name: {group}\Viewer; Filename: {app}\HAB.Designer.exe; Parameters: viewer
Name: {group}\Help; Filename: {app}\NND.chm
Name: {group}\{cm:UninstallProgram,{#MyAppSetupName}}; Filename: {uninstallexe}

Name: {group}\Demos\old\Name & Age; Filename: {app}\HAB.Designer.exe; Parameters: """{userdocs}\NND\Demos\old\Name_Age.dpl"""; IconIndex: 0
Name: {group}\Demos\old\SysMan; Filename: {app}\HAB.Designer.exe; Parameters: """{userdocs}\NND\Demos\old\SysMan.dpl"""; IconIndex: 0
Name: {group}\Demos\old\Thesaurus operations; Filename: {app}\HAB.Designer.exe; Parameters: """{userdocs}\NND\Demos\old\Thesaurus_operations.dpl"""; IconIndex: 0
Name: {group}\Demos\old\Asset operations; Filename: {app}\HAB.Designer.exe; Parameters: """{userdocs}\NND\Demos\old\asset_operations.dpl"""; IconIndex: 0
Name: {group}\Demos\GetStartedCreateBot; Filename: {app}\HAB.Designer.exe; Parameters: """{userdocs}\NND\Demos\GetStartedCreateBot.dpl"""; IconIndex: 0
Name: {group}\Demos\Alice; Filename: {app}\HAB.Designer.exe; Parameters: """{userdocs}\NND\Demos\Alice.dpl"""; IconIndex: 0
Name: {group}\Demos\Aici; Filename: {app}\HAB.Designer.exe; Parameters: """{userdocs}\NND\Demos\Aicionline.dpl"""; IconIndex: 0
Name: {group}\Demos\TimerDemo; Filename: {app}\HAB.Designer.exe; Parameters: """{userdocs}\NND\Demos\TimerDemo.dpl"""; IconIndex: 0
Name: {group}\Demos\TopicFilters; Filename: {app}\HAB.Designer.exe; Parameters: """{userdocs}\NND\Demos\TopicFilters.dpl"""; IconIndex: 0
Name: {group}\Demos\AIMLThat; Filename: {app}\HAB.Designer.exe; Parameters: """{userdocs}\NND\Demos\AIMLThat.dpl"""; IconIndex: 0
Name: {group}\Demos\old\weather; Filename: {app}\HAB.Designer.exe; Parameters: """{userdocs}\NND\Demos\old\weather.dpl"""; IconIndex: 0
Name: {group}\Demos\old\have-be-like-why-because; Filename: {app}\HAB.Designer.exe; Parameters: """{userdocs}\NND\Demos\old\why_because.dpl"""; IconIndex: 0
Name: {group}\Demos\old\have-be-like-why-because2; Filename: {app}\HAB.Designer.exe; Parameters: """{userdocs}\NND\Demos\old\why_because2.dpl"""; IconIndex: 0
Name: {group}\Demos\old\have-be-like-why-because3; Filename: {app}\HAB.Designer.exe; Parameters: """{userdocs}\NND\Demos\old\why_because3.dpl"""; IconIndex: 0
Name: {group}\Demos\old\Complete the sequence; Filename: {app}\HAB.Designer.exe; Parameters: """{userdocs}\NND\Demos\old\CompleteSeq.dpl"""; IconIndex: 0
Name: {group}\Demos\old\DateTime-Birthday; Filename: {app}\HAB.Designer.exe; Parameters: """{userdocs}\NND\Demos\DateTime.dpl"""; IconIndex: 0
Name: {group}\Demos\old\question-Answer-redirect; Filename: {app}\HAB.Designer.exe; Parameters: """{userdocs}\NND\Demos\questionAnswer.dpl"""; IconIndex: 0
Name: {group}\Demos\empty-project-with-callbacks; Filename: {app}\HAB.Designer.exe; Parameters: """{userdocs}\NND\Demos\empty_project_with_callbacks.dpl"""; IconIndex: 0
Name: {group}\Demos\empty-project; Filename: {app}\HAB.Designer.exe; Parameters: """{userdocs}\NND\Demos\empty_project.dpl"""; IconIndex: 0


Name: {commondesktop}\{#MyAppSetupName}; Filename: {app}\HAB.Designer.exe; Tasks: desktopicon
Name: {userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppSetupName}; Filename: {app}\HAB.Designer.exe; Tasks: quicklaunchicon

[Run]
;Filename: ngen; Parameters: install; WorkingDir: {app}
Filename: {app}\HAB.Designer.exe; Description: {cm:LaunchProgram,{#MyAppSetupName}}; Flags: nowait postinstall skipifsilent

[Registry]
Root: HKCR; Subkey: .dpl; ValueType: string; ValueName: ; ValueData: NeuralNetworkDesignerProjectLoader; Flags: uninsdeletevalue
Root: HKCR; Subkey: NeuralNetworkDesignerProjectLoader; ValueType: string; ValueName: ; ValueData: chatbot project; Flags: uninsdeletekey
Root: HKCR; Subkey: NeuralNetworkDesignerProjectLoader\DefaultIcon; ValueType: string; ValueName: ; ValueData: {app}\HAB.Designer.EXE,1
Root: HKCR; Subkey: NeuralNetworkDesignerProjectLoader\shell\open\command; ValueType: string; ValueName: ; ValueData: """{app}\HAB.Designer.EXE"" ""%1"""

#include "scripts\products.iss"

#include "scripts\products\winversion.iss"
#include "scripts\products\fileversion.iss"

#ifdef use_iis
#include "scripts\products\iis.iss"
#endif

#ifdef use_kb835732
#include "scripts\products\kb835732.iss"
#endif
#ifdef use_kb886903
#include "scripts\products\kb886903.iss"
#endif
#ifdef use_kb928366
#include "scripts\products\kb928366.iss"
#endif

#ifdef use_msi20
#include "scripts\products\msi20.iss"
#endif
#ifdef use_msi31
#include "scripts\products\msi31.iss"
#endif
#ifdef use_msi45
#include "scripts\products\msi45.iss"
#endif
#ifdef use_ie6
#include "scripts\products\ie6.iss"
#endif

#ifdef use_dotnetfx11
#include "scripts\products\dotnetfx11.iss"
#include "scripts\products\dotnetfx11lp.iss"
#include "scripts\products\dotnetfx11sp1.iss"
#endif

#ifdef use_dotnetfx20
#include "scripts\products\dotnetfx20.iss"
#ifdef use_dotnetfx20lp
#include "scripts\products\dotnetfx20lp.iss"
#endif
#include "scripts\products\dotnetfx20sp1.iss"
#ifdef use_dotnetfx20lp
#include "scripts\products\dotnetfx20sp1lp.iss"
#endif
#include "scripts\products\dotnetfx20sp2.iss"
#ifdef use_dotnetfx20lp
#include "scripts\products\dotnetfx20sp2lp.iss"
#endif
#endif

#ifdef use_dotnetfx35
#include "scripts\products\dotnetfx35.iss"
#ifdef use_dotnetfx35lp
#include "scripts\products\dotnetfx35lp.iss"
#endif
#include "scripts\products\dotnetfx35sp1.iss"
#ifdef use_dotnetfx35lp
#include "scripts\products\dotnetfx35sp1lp.iss"
#endif
#endif

#ifdef use_dotnetfx40
#include "scripts\products\dotnetfx40client.iss"
#include "scripts\products\dotnetfx40full.iss"
#endif
#ifdef use_vc2010
#include "scripts\products\vc2010.iss"
#endif

#ifdef use_mdac28
#include "scripts\products\mdac28.iss"
#endif
#ifdef use_jet4sp8
#include "scripts\products\jet4sp8.iss"
#endif
// SQL 3.5 Compact Edition: 
#ifdef use_scceruntime
#include "scripts\products\scceruntime.iss"
#endif
// SQL Express: 
#ifdef use_sql2005express
#include "scripts\products\sql2005express.iss"
#endif
#ifdef use_sql2008express
#include "scripts\products\sql2008express.iss"
#endif

[CustomMessages]
win2000sp3_title=Windows 2000 Service Pack 3
winxpsp2_title=Windows XP Service Pack 2
winxpsp3_title=Windows XP Service Pack 3

#expr SaveToFile(AddBackslash(SourcePath) + "Preprocessed"+MyAppSetupname+SetupScriptVersion+".iss")

[Code]
function InitializeSetup(): Boolean;
begin
	//init windows version
	initwinversion();

	//check if dotnetfx20 can be installed on this OS
	//if not minwinspversion(5, 0, 3) then begin
	//	MsgBox(FmtMessage(CustomMessage('depinstall_missing'), [CustomMessage('win2000sp3_title')]), mbError, MB_OK);
	//	exit;
	//end;
	if not minwinspversion(5, 1, 3) then begin
		MsgBox(FmtMessage(CustomMessage('depinstall_missing'), [CustomMessage('winxpsp3_title')]), mbError, MB_OK);
		exit;
	end;

#ifdef use_iis
	if (not iis()) then exit;
#endif

#ifdef use_msi20
	msi20('2.0');
#endif
#ifdef use_msi31
	msi31('3.1');
#endif
#ifdef use_msi45
	msi45('4.5');
#endif
#ifdef use_ie6
	ie6('5.0.2919');
#endif

#ifdef use_dotnetfx11
	dotnetfx11();
#ifdef use_dotnetfx11lp
	dotnetfx11lp();
#endif
	dotnetfx11sp1();
#endif
#ifdef use_kb886903
	kb886903(); //better use windows update
#endif
#ifdef use_kb928366
	kb928366(); //better use windows update
#endif

	//install .netfx 2.0 sp2 if possible; if not sp1 if possible; if not .netfx 2.0
#ifdef use_dotnetfx20
	if minwinversion(5, 1) then begin
		dotnetfx20sp2();
#ifdef use_dotnetfx20lp
		dotnetfx20sp2lp();
#endif
	end else begin
		if minwinversion(5, 0) and minwinspversion(5, 0, 4) then begin
#ifdef use_kb835732
			kb835732();
#endif
			dotnetfx20sp1();
#ifdef use_dotnetfx20lp
			dotnetfx20sp1lp();
#endif
		end else begin
			dotnetfx20();
#ifdef use_dotnetfx20lp
			dotnetfx20lp();
#endif
		end;
	end;
#endif

#ifdef use_dotnetfx35
	dotnetfx35();
#ifdef use_dotnetfx35lp
	dotnetfx35lp();
#endif
	dotnetfx35sp1();
#ifdef use_dotnetfx35lp
	dotnetfx35sp1lp();
#endif
#endif

	// If no .NET 4.0 framework found, install the smallest
#ifdef use_dotnetfx40
	if not dotnetfx40client(true) then
	    if not dotnetfx40full(true) then
	        dotnetfx40client(false);
	// Alternatively:
	// dotnetfx40full();
#endif

	// Visual C++ 2010 Redistributable
#ifdef use_vc2010
	vc2010();
#endif

#ifdef use_mdac28
	mdac28('2.7');
#endif
#ifdef use_jet4sp8
	jet4sp8('4.0.8015');
#endif
	// SQL 3.5 CE
#ifdef use_ssceruntime
	ssceruntime();
#endif
	// SQL Express
#ifdef use_sql2005express
	sql2005express();
#endif
#ifdef use_sql2008express
	sql2008express();
#endif

	Result := true;
end;

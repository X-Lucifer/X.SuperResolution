Unicode true
SetCompressor /SOLID lzma
ManifestDPIAware true
ManifestSupportedOS all

!define PROJECT_FILE "..\X.SuperResolution\X.SuperResolution.csproj"
!define DEFAULT_PUBLISH_DIR "..\X.SuperResolution\bin\Release\win-x64\publish"
!define GENERATED_METADATA "Generated.AppInfo.nsh"
!define WIZARD_IMAGE_SOURCE "logo.jpg"
!define WIZARD_IMAGE "Generated.WizardImage.bmp"
!define WELCOME_IMAGE_WIDTH 98u
!define WELCOME_CONTENT_X 108u
!define WELCOME_CONTENT_WIDTH 244u

!system 'powershell -NoProfile -ExecutionPolicy Bypass -File "Generate-NsisMetadata.ps1" -ProjectPath "${PROJECT_FILE}" -PublishDir "${DEFAULT_PUBLISH_DIR}" -OutputPath "${GENERATED_METADATA}"'
!system 'powershell -NoProfile -ExecutionPolicy Bypass -File "Prepare-NsisAssets.ps1" -LogoPath "${WIZARD_IMAGE_SOURCE}" -OutputPath "${WIZARD_IMAGE}"'
!include "${GENERATED_METADATA}"

!define INSTALLER_OUTPUT_DIR "..\artifacts\installer"
!system 'powershell -NoProfile -ExecutionPolicy Bypass -Command "New-Item -ItemType Directory -Force -Path \"${INSTALLER_OUTPUT_DIR}\" | Out-Null"'

!include "MUI2.nsh"
!include "FileFunc.nsh"
!include "LogicLib.nsh"
!include "nsDialogs.nsh"
!include "WinMessages.nsh"

!define MUI_ABORTWARNING
!define MUI_ICON "..\X.SuperResolution\Assets\logo.ico"
!define MUI_UNICON "..\X.SuperResolution\Assets\logo.ico"
!define MUI_FINISHPAGE_RUN "$INSTDIR\${APP_EXE}"
!define MUI_FINISHPAGE_RUN_TEXT "Launch ${APP_NAME}"
!define MUI_FINISHPAGE_LINK "${APP_REPOSITORY_URL}"
!define MUI_FINISHPAGE_LINK_LOCATION "${APP_REPOSITORY_URL}"
!define MUI_LANGDLL_REGISTRY_ROOT HKCU
!define MUI_LANGDLL_REGISTRY_KEY "Software\${APP_NAME}"
!define MUI_LANGDLL_REGISTRY_VALUENAME "InstallerLanguage"

Name "${APP_NAME}"
OutFile "${INSTALLER_OUTPUT_DIR}\${APP_NAME}-Setup-${APP_VERSION}.exe"
InstallDir "$LOCALAPPDATA\Programs\${APP_NAME}"
InstallDirRegKey HKCU "Software\${APP_NAME}" "InstallDir"
RequestExecutionLevel user
ShowInstDetails nevershow
ShowUninstDetails nevershow
BrandingText "${APP_NAME}"
XPStyle on
InstallColors 68217A FFFFFF

VIProductVersion "${APP_VERSION4}"
VIAddVersionKey /LANG=1033 "ProductName" "${APP_NAME}"
VIAddVersionKey /LANG=1033 "CompanyName" "${APP_COMPANY}"
VIAddVersionKey /LANG=1033 "LegalCopyright" "${APP_COPYRIGHT}"
VIAddVersionKey /LANG=1033 "FileDescription" "${APP_NAME} Setup"
VIAddVersionKey /LANG=1033 "FileVersion" "${APP_VERSION}"
VIAddVersionKey /LANG=1033 "ProductVersion" "${APP_VERSION}"

!if /FileExists "${APP_PUBLISH_DIR}\${APP_EXE}"
!else
    !error "Publish output not found. Run dotnet publish before building installer: ${APP_PUBLISH_DIR}\${APP_EXE}"
!endif

Page custom ModernWelcomePageCreate ModernWelcomePageLeave
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES

!insertmacro MUI_LANGUAGE "SimpChinese"
!insertmacro MUI_LANGUAGE "English"

LangString WelcomeVersion ${LANG_SIMPCHINESE} "版本 ${APP_VERSION}"
LangString WelcomeVersion ${LANG_ENGLISH} "Version ${APP_VERSION}"
LangString WelcomeInstallText ${LANG_SIMPCHINESE} "此向导将为当前用户安装 ${APP_NAME}。$\r$\n$\r$\n你可以在下一页更改安装位置。"
LangString WelcomeInstallText ${LANG_ENGLISH} "This wizard will install ${APP_NAME} for the current user.$\r$\n$\r$\nInstall location can be changed on the next page."
LangString UninstallFailedText ${LANG_SIMPCHINESE} "旧版本未能完成卸载，安装程序将退出。"
LangString UninstallFailedText ${LANG_ENGLISH} "The installed version was not removed. Setup will exit."

Function .onInit
    !insertmacro MUI_LANGDLL_DISPLAY

    ReadRegStr $0 HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}" "UninstallString"
    ${If} $0 == ""
        ReadRegStr $0 HKCU "Software\${APP_NAME}" "InstallDir"
        ${If} $0 != ""
            StrCpy $0 "$\"$0\Uninstall.exe$\""
        ${EndIf}
    ${EndIf}

    ${If} $0 != ""
        DetailPrint "Existing ${APP_NAME} installation found. Uninstalling previous version..."
        ExecWait '$0 /S' $1
        ReadRegStr $2 HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}" "UninstallString"
        ${If} $2 != ""
            MessageBox MB_ICONSTOP "$(UninstallFailedText)"
            Abort
        ${EndIf}
    ${EndIf}
FunctionEnd

Function ModernWelcomePageCreate
    InitPluginsDir
    File "/oname=$PLUGINSDIR\WizardImage.bmp" "${WIZARD_IMAGE}"

    nsDialogs::Create 1018
    Pop $0
    ${If} $0 == error
        Abort
    ${EndIf}

    SetCtlColors $0 0x202020 0xFFFFFF

    ${NSD_CreateBitmap} 0 0 ${WELCOME_IMAGE_WIDTH} 100% ""
    Pop $1
    ${NSD_SetImage} $1 "$PLUGINSDIR\WizardImage.bmp" $4

    ${NSD_CreateLabel} ${WELCOME_CONTENT_X} 12u ${WELCOME_CONTENT_WIDTH} 18u "${APP_NAME}"
    Pop $2
    SetCtlColors $2 0x202020 0xFFFFFF
    CreateFont $3 "Segoe UI" 13 700
    SendMessage $2 ${WM_SETFONT} $3 1

    ${NSD_CreateLabel} ${WELCOME_CONTENT_X} 32u ${WELCOME_CONTENT_WIDTH} 11u "$(WelcomeVersion)"
    Pop $2
    SetCtlColors $2 0x404040 0xFFFFFF

    ${NSD_CreateLabel} ${WELCOME_CONTENT_X} 76u ${WELCOME_CONTENT_WIDTH} 54u "$(WelcomeInstallText)"
    Pop $2
    SetCtlColors $2 0x202020 0xFFFFFF
    CreateFont $3 "Segoe UI" 10 400
    SendMessage $2 ${WM_SETFONT} $3 1

    ${NSD_CreateLabel} ${WELCOME_CONTENT_X} 140u ${WELCOME_CONTENT_WIDTH} 44u "${APP_COPYRIGHT}"
    Pop $2
    SetCtlColors $2 0x606060 0xFFFFFF

    nsDialogs::Show
FunctionEnd

Function ModernWelcomePageLeave
FunctionEnd

Section "Install ${APP_NAME}" SecInstall
    SetOutPath "$INSTDIR"
    File /r "${APP_PUBLISH_DIR}\*.*"

    WriteUninstaller "$INSTDIR\Uninstall.exe"
    WriteRegStr HKCU "Software\${APP_NAME}" "InstallDir" "$INSTDIR"

    CreateDirectory "$SMPROGRAMS\${APP_NAME}"
    CreateShortcut "$SMPROGRAMS\${APP_NAME}\${APP_NAME}.lnk" "$INSTDIR\${APP_EXE}" "" "$INSTDIR\${APP_EXE}" 0
    CreateShortcut "$SMPROGRAMS\${APP_NAME}\Uninstall ${APP_NAME}.lnk" "$INSTDIR\Uninstall.exe"
    CreateShortcut "$DESKTOP\${APP_NAME}.lnk" "$INSTDIR\${APP_EXE}" "" "$INSTDIR\${APP_EXE}" 0

    WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}" "DisplayName" "${APP_NAME}"
    WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}" "DisplayVersion" "${APP_VERSION}"
    WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}" "Publisher" "${APP_COMPANY}"
    WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}" "URLInfoAbout" "${APP_REPOSITORY_URL}"
    WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}" "InstallLocation" "$INSTDIR"
    WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}" "DisplayIcon" "$INSTDIR\${APP_EXE}"
    WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}" "UninstallString" "$\"$INSTDIR\Uninstall.exe$\""
    WriteRegDWORD HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}" "NoModify" 1
    WriteRegDWORD HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}" "NoRepair" 1

    ${GetSize} "$INSTDIR" "/S=0K" $0 $1 $2
    WriteRegDWORD HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}" "EstimatedSize" "$0"
SectionEnd

Section "Uninstall"
    Delete "$DESKTOP\${APP_NAME}.lnk"
    Delete "$SMPROGRAMS\${APP_NAME}\${APP_NAME}.lnk"
    Delete "$SMPROGRAMS\${APP_NAME}\Uninstall ${APP_NAME}.lnk"
    RMDir "$SMPROGRAMS\${APP_NAME}"

    DeleteRegKey HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}"
    DeleteRegKey HKCU "Software\${APP_NAME}"

    RMDir /r "$INSTDIR"
SectionEnd

;NSIS Modern User Interface
;Header Bitmap Example Script
;Written by Joost Verburg

!define PRODUCT_NAME "ColorMC"
!define PRODUCT_FILE_NAME "ColorMC.Launcher"
!define PRODUCT_VERSION "23"
!define PRODUCT_PUBLISHER "Coloryr"
!define PRODUCT_COPYRIGHT "Copyright (c) 2023 Coloryr."
!define PRODUCT_BUILD_VERSION "1.23.0.0"
!define PRODUCT_WEB_SITE "https://colormc.coloryr.com"
!define PRODUCT_UNINSTALL_KEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}"

;--------------------------------
;Include Modern UI

  !include "MUI2.nsh"

;--------------------------------
;General

  ;Name and file
  Name "ColorMC"
  OutFile "ColorMC.exe"
  Unicode True
  Caption "${PRODUCT_NAME} Installer"
  BrandingText /TRIMLEFT "${PRODUCT_NAME} A${PRODUCT_VERSION} Installer"
  Icon "game.ico"

  ;Default installation folder
  InstallDir "D:\ColorMC"
  
  ;Get installation folder from registry if available
  InstallDirRegKey HKCU "Software\ColorMC" ""

  ;Request application privileges for Windows Vista
  RequestExecutionLevel user

  VIAddVersionKey ProductName "${PRODUCT_NAME} Installer" ; product name
  VIAddVersionKey ProductVersion "${PRODUCT_VERSION}" ; product version
  VIAddVersionKey Comments "${PRODUCT_NAME}" ; description
  VIAddVersionKey CompanyName "${PRODUCT_PUBLISHER}" ; compnay name
  VIAddVersionKey LegalCopyright "${PRODUCT_COPYRIGHT}" ; copyright
  VIAddVersionKey FileVersion "${PRODUCT_BUILD_VERSION}" ; file version
  VIAddVersionKey FileDescription "${PRODUCT_NAME} Installer" ; file description
  VIProductVersion "${PRODUCT_BUILD_VERSION}" ; product verion(actual replace FileVersion)

;--------------------------------
;Interface Configuration

  !define MUI_HEADERIMAGE
  !define MUI_HEADERIMAGE_BITMAP "game1.bmp" ; optional
  !define MUI_HEADERIMAGE_RIGHT ; Display to right
  !define MUI_ABORTWARNING
  !define MUI_ICON "game.ico"
  !define MUI_UNICON "game.ico"
  !define MUI_WELCOMEFINISHPAGE_BITMAP ".\run.bmp"
  !define MUI_LICENSEPAGE_CHECKBOX
;--------------------------------
;Pages

  !insertmacro MUI_PAGE_WELCOME
  !insertmacro MUI_PAGE_LICENSE "apache.txt"
  !insertmacro MUI_PAGE_DIRECTORY
  !insertmacro MUI_PAGE_INSTFILES
  !insertmacro MUI_PAGE_FINISH

  !insertmacro MUI_UNPAGE_CONFIRM
  !insertmacro MUI_UNPAGE_INSTFILES
  
;--------------------------------
;Languages
 
  !insertmacro MUI_LANGUAGE "English"

;--------------------------------
;Installer Sections

Section "ColorMC" SecDummy

  CreateDirectory "$INSTDIR"
  SetOutPath "$INSTDIR"
  SetOverwrite ifnewer
  
  ;ADD YOUR OWN FILES HERE...
  File "ColorMC.Launcher.exe"
  File "ColorMC.Core.pdb"
  File "ColorMC.Gui.pdb"
  File "ColorMC.Launcher.pdb"
  File "Live2DCSharpSDK.App.pdb"
  File "Live2DCSharpSDK.Framework.pdb"
  File "game.ico"
  
  ;Store installation folder
  WriteRegStr HKCU "Software\${PRODUCT_NAME}" "" "$INSTDIR"

  WriteRegStr HKCU "${PRODUCT_UNINSTALL_KEY}" "DisplayName" "$(^Name)"
  WriteRegStr HKCU "${PRODUCT_UNINSTALL_KEY}" "InstallDir" "$INSTDIR"
  WriteRegStr HKCU "${PRODUCT_UNINSTALL_KEY}" "UninstallString" "$INSTDIR\uninstall.exe"
  WriteRegStr HKCU "${PRODUCT_UNINSTALL_KEY}" "DisplayIcon" "$INSTDIR\game.ico"
  WriteRegStr HKCU "${PRODUCT_UNINSTALL_KEY}" "DisplayVersion" "${PRODUCT_VERSION}"
  WriteRegStr HKCU "${PRODUCT_UNINSTALL_KEY}" "URLInfoAbout" "${PRODUCT_WEB_SITE}"
  WriteRegStr HKCU "${PRODUCT_UNINSTALL_KEY}" "Publisher" "${PRODUCT_PUBLISHER}"
  
  ;Create uninstaller
  WriteUninstaller "$INSTDIR\uninstall.exe"

  CreateShortCut "$DESKTOP\${PRODUCT_NAME}.lnk" "$INSTDIR\${PRODUCT_FILE_NAME}.exe"
  CreateShortCut "$SMPROGRAMS\${PRODUCT_NAME}.lnk" "$INSTDIR\${PRODUCT_FILE_NAME}.exe"

SectionEnd

;--------------------------------
;Uninstaller Section

Section "Uninstall"

  DeleteRegKey HKCU "Software\${PRODUCT_NAME}"
  DeleteRegKey HKCU "${PRODUCT_UNINSTALL_KEY}"

  ;ADD YOUR OWN FILES HERE...
  Delete "$INSTDIR\ColorMC.Launcher.exe"
  Delete "$INSTDIR\ColorMC.Core.pdb"
  Delete "$INSTDIR\ColorMC.Gui.pdb"
  Delete "$INSTDIR\ColorMC.Launcher.pdb"
  Delete "$INSTDIR\Live2DCSharpSDK.App.pdb"
  Delete "$INSTDIR\Live2DCSharpSDK.Framework.pdb"
  Delete "$INSTDIR\game.ico"

  Delete "$INSTDIR\Uninstall.exe"

  ; delete start menu folder 
  Delete "$SMPROGRAMS\${PRODUCT_NAME}.lnk"
  Delete "$DESKTOP\${PRODUCT_NAME}.lnk"

  RMDir "$INSTDIR"

SectionEnd
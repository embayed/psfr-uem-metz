# PSFR AutoCAD Protocol Handler

This application enables opening AutoCAD files (.dwg, .dxf, .dwt) directly from the web browser into AutoCAD desktop application.

## Features

- **Edit Mode**: Opens files in AutoCAD with full editing capabilities
- **View Mode**: Opens files in read-only mode
- **WebDAV Integration**: Downloads files from the PSFR DMS system
- **Automatic AutoCAD Detection**: Finds and launches AutoCAD automatically

## Installation

### Prerequisites

- Windows 10 or later
- .NET 8.0 Runtime
- AutoCAD installed on the system

(Must be run as Administrator)

## Deployment

### Quick Build for DMS Package (Important!)

**IMPORTANT:** Whenever you make changes to the protocol handler code, you must update the executable in the DMS package.

#### Steps to Update DMS Package:

1. **Double-click `publish.bat`** in the project root
   - This will build the application and generate the `.exe` file
   - The executable will be created at: `.\bin\Release\publish\PSFR_AutoCAD_ProtocolHandler.exe`

2. **Copy the generated .exe to DMS package:**
   - Source: `.\bin\Release\publish\PSFR_AutoCAD_ProtocolHandler.exe`
   - Destination: `DMS/wwwroot/lib/AutocadProtocol/Intalio_AutoCAD_ProtocolHandler_Setup.exe` (or your configured DMS wwwroot path)

3. **Deploy the updated DMS package** to your web server

> **Note:** The `publish.bat` script will also create an installer using Inno Setup if it's installed on your machine. The standalone executable is always available even if Inno Setup is not installed.


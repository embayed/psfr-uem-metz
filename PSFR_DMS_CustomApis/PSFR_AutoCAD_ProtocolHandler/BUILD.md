# Build Instructions

## Prerequisites

1. **.NET 8.0 SDK** - Already installed if you can build the project
2. **Inno Setup** - Download from: https://jrsoftware.org/isdl.php
   - Install Inno Setup 6 (free)
   - The build script will detect it automatically

## Building the Installer

### Quick Build

Simply double-click:
```
publish.bat
```

This will:
1. Build the application as a self-contained single-file executable
2. Create a professional installer using Inno Setup
3. Open the output folder automatically

### Output Files

After building, you'll find:

- **Installer**: `.\Setup\Intalio_AutoCAD_ProtocolHandler_Setup.exe`
  - Professional Windows installer
  - Includes uninstaller
  - Automatically registers protocol handlers

- **Standalone EXE** (if needed): `.\bin\Release\publish\PSFR_AutoCAD_ProtocolHandler.exe`
  - Single file, no installation required
  - Must run `/register` manually for protocol handlers

## Distribution

### For End Users (Recommended)

Distribute: `Intalio_AutoCAD_ProtocolHandler_Setup.exe`

Users simply:
1. Double-click the setup file
2. Click "Next" through the wizard
3. Done! Protocol handlers are automatically registered

### Uninstallation

Users can uninstall via:
- Windows Settings > Apps > Intalio AutoCAD Protocol Handler
- Control Panel > Programs and Features

## Manual Build (Command Line)

```bash
# Step 1: Build the application
dotnet publish -c Release --output ".\bin\Release\publish"

# Step 2: Create installer (if Inno Setup is installed)
"C:\Program Files (x86)\Inno Setup 6\ISCC.exe" Setup.iss
```

## Configuration

Edit `Setup.iss` to customize:
- Application name and version
- Installation directory
- Publisher information
- Icons and branding

## Troubleshooting

### Log File Location

All operations and errors are logged to:
```
%TEMP%\PSFR_AutoCAD\autocad_handler.log
```

To view the log:
1. Press `Win + R`
2. Type: `%TEMP%\PSFR_AutoCAD`
3. Open `autocad_handler.log`

The log includes:
- File open/download operations
- Checkout/checkin API calls
- Upload operations
- All exceptions with stack traces
- AutoCAD process lifecycle

### Common Issues

**File not checking in:**
- Check the log file for errors during checkin
- Verify the API URL and token are correct
- Ensure network connectivity to the server

**AutoCAD not launching:**
- Log will show if AutoCAD was found
- Check AutoCAD installation path

**Upload failures:**
- Log shows HTTP status codes
- Check WebDAV URL accessibility

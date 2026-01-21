// AutoCAD Protocol Handler Detection
const AutoCADProtocolDetector = {
    config: {
        setupUrl: '/lib/AutocadProtocol/Intalio_AutoCAD_ProtocolHandler_Setup.exe',
        timeout: 2000
    },

    isProtocolHandlerInstalled: function (callback) {
        const testUrl = 'acad-view:test';
        const iframe = document.createElement('iframe');
        iframe.style.display = 'none';

        let detected = false;
        const timer = setTimeout(function () {
            if (!detected) {
                callback(false);
            }
            if (iframe.parentNode) {
                document.body.removeChild(iframe);
            }
        }, this.config.timeout);

        const onBlur = function () {
            detected = true;
            clearTimeout(timer);
            callback(true);
            window.removeEventListener('blur', onBlur);
            if (iframe.parentNode) {
                document.body.removeChild(iframe);
            }
        };

        window.addEventListener('blur', onBlur);
        iframe.src = testUrl;
        document.body.appendChild(iframe);
    },

    showInstallPrompt: function () {
        if (document.getElementById('autocad-protocol-modal')) {
            document.getElementById('autocad-protocol-modal').style.display = 'flex';
            return;
        }

        const modal = document.createElement('div');
        modal.id = 'autocad-protocol-modal';
        modal.innerHTML = `
            <div class="autocad-modal-overlay">
                <div class="autocad-modal-content">
                    <div class="autocad-modal-header">
                        <h3>${Common.translate("AutoCADProtocolHandlerRequired")}</h3>
                        <button class="autocad-modal-close" onclick="AutoCADProtocolDetector.closeModal()">&times;</button>
                    </div>
                    <div class="autocad-modal-body">
                        <div class="autocad-modal-icon">
                            <svg width="64" height="64" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                <path d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z"></path>
                                <polyline points="7.5 4.21 12 6.81 16.5 4.21"></polyline>
                                <polyline points="7.5 19.79 7.5 14.6 3 12"></polyline>
                                <polyline points="21 12 16.5 14.6 16.5 19.79"></polyline>
                                <polyline points="3.27 6.96 12 12.01 20.73 6.96"></polyline>
                                <line x1="12" y1="22.08" x2="12" y2="12"></line>
                            </svg>
                        </div>
                        <p>${Common.translate("AutoCADProtocolHandlerDescription")}</p>
                        <p class="autocad-modal-note">${Common.translate("AutoCADProtocolHandlerNote")}</p>
                    </div>
                    <div class="autocad-modal-footer">
                        <button class="autocad-btn autocad-btn-secondary" onclick="AutoCADProtocolDetector.closeModal()">
                            ${Common.translate("Cancel")}
                        </button>
                        <a href="${this.config.setupUrl}" class="autocad-btn autocad-btn-primary" download>
                            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path>
                                <polyline points="7 10 12 15 17 10"></polyline>
                                <line x1="12" y1="15" x2="12" y2="3"></line>
                            </svg>
                            ${Common.translate("DownloadInstaller")}
                        </a>
                    </div>
                </div>
            </div>
        `;

        document.body.appendChild(modal);

        if (!document.getElementById('autocad-protocol-styles')) {
            this.addStyles();
        }
    },

    closeModal: function () {
        const modal = document.getElementById('autocad-protocol-modal');
        if (modal) {
            modal.style.display = 'none';
        }
    },

    addStyles: function () {
        const style = document.createElement('style');
        style.id = 'autocad-protocol-styles';
        style.textContent = `
            #autocad-protocol-modal {
                display: flex;
                position: fixed;
                top: 0;
                left: 0;
                width: 100%;
                height: 100%;
                z-index: 10000;
                animation: fadeIn 0.3s ease;
            }
            @keyframes fadeIn {
                from { opacity: 0; }
                to { opacity: 1; }
            }
            .autocad-modal-overlay {
                display: flex;
                align-items: center;
                justify-content: center;
                width: 100%;
                height: 100%;
                background-color: rgba(0, 0, 0, 0.5);
                backdrop-filter: blur(4px);
            }
            .autocad-modal-content {
                background: white;
                border-radius: 12px;
                box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
                max-width: 500px;
                width: 90%;
                animation: slideUp 0.3s ease;
            }
            @keyframes slideUp {
                from { transform: translateY(20px); opacity: 0; }
                to { transform: translateY(0); opacity: 1; }
            }
            .autocad-modal-header {
                display: flex;
                justify-content: space-between;
                align-items: center;
                padding: 24px 24px 16px;
                border-bottom: 1px solid #e5e7eb;
            }
            .autocad-modal-header h3 {
                margin: 0;
                font-size: 20px;
                font-weight: 600;
                color: #111827;
            }
            .autocad-modal-close {
                background: none;
                border: none;
                font-size: 28px;
                color: #6b7280;
                cursor: pointer;
                padding: 0;
                width: 32px;
                height: 32px;
                display: flex;
                align-items: center;
                justify-content: center;
                border-radius: 6px;
                transition: all 0.2s;
            }
            .autocad-modal-close:hover {
                background-color: #f3f4f6;
                color: #111827;
            }
            .autocad-modal-body {
                padding: 24px;
                text-align: center;
            }
            .autocad-modal-icon {
                margin: 0 auto 20px;
                width: 64px;
                height: 64px;
                color: #E51937;
            }
            .autocad-modal-body p {
                margin: 0 0 16px;
                color: #374151;
                font-size: 15px;
                line-height: 1.6;
            }
            .autocad-modal-note {
                font-size: 13px !important;
                color: #6b7280 !important;
                font-style: italic;
            }
            .autocad-modal-footer {
                display: flex;
                gap: 12px;
                padding: 16px 24px 24px;
                justify-content: flex-end;
            }
            .autocad-btn {
                padding: 10px 20px;
                border-radius: 8px;
                font-size: 14px;
                font-weight: 500;
                cursor: pointer;
                transition: all 0.2s;
                border: none;
                display: inline-flex;
                align-items: center;
                gap: 8px;
                text-decoration: none;
            }
            .autocad-btn-primary {
                background-color: #E51937;
                color: white;
            }
            .autocad-btn-primary:hover {
                background-color: #c41530;
                transform: translateY(-1px);
                box-shadow: 0 4px 12px rgba(229, 25, 55, 0.4);
                text-decoration: none;
                color: white;
            }
            .autocad-btn-secondary {
                background-color: #f3f4f6;
                color: #374151;
            }
            .autocad-btn-secondary:hover {
                background-color: #e5e7eb;
            }
            .autocad-btn svg {
                width: 16px;
                height: 16px;
            }
        `;
        document.head.appendChild(style);
    },

    checkAndPrompt: function (url, callback) {
        this.isProtocolHandlerInstalled(function (installed) {
            if (!installed) {
                AutoCADProtocolDetector.showInstallPrompt();
                if (callback) callback(false);
            } else {
                if (callback) callback(true);
                // Use a programmatic link click to preserve user gesture
                const link = document.createElement('a');
                link.href = url;
                link.style.display = 'none';
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
            }
        });
    }
};

window.AutoCADProtocolDetector = AutoCADProtocolDetector;

$(document).ajaxSuccess(function (event, xhr, settings) {
    // Check if the successful AJAX call is GetBasicDetails
    if (settings.url.startsWith("/Files/GetBasicDetails")) {
        // Extract file ID from the URL parameter
        const url = new URL(settings.url, window.location.origin);
        const fileId = url.searchParams.get('id') || url.searchParams.get('Id');

        if (fileId) {
            // Call GetLatestMetadata to get the full metadata
            fetch(`/Files/GetLatestMetadata?Id=${fileId}`)
                .then(response => response.json())
                .then(metadata => {
                    // Process the metadata same as below
                    processFileMetadata(metadata);
                })
                .catch(error => {
                    // Silently handle error
                });
        }
        return;
    }
});

// Process file metadata and show viewer UI for supported files
function processFileMetadata(responseData) {
    if (!responseData) return;

    // 1. Extract metadata
    const extension = responseData.extension;
    const fileId = responseData.id;
    const fileName = responseData.name;
    const version = responseData.version || '1.0';
    const fromAttachment = responseData.fromAttachment || false;

    // Handle checkout status - may be undefined if not available
    const viewerCheckoutByUserId = responseData.viewerCheckoutUserId || null;
    const checkoutByUserId = responseData.checkoutUserId || null;
    const currentUserId = window.UserId || null;

    // Convert to strings for comparison
    const viewerCheckoutUserIdStr = viewerCheckoutByUserId ? String(viewerCheckoutByUserId) : null;
    const checkoutUserIdStr = checkoutByUserId ? String(checkoutByUserId) : null;
    const currentUserIdStr = currentUserId ? String(currentUserId) : null;

    // Determine if file is checked out by another user (check EITHER property)
    const isCheckedOutByOther = (viewerCheckoutUserIdStr && currentUserIdStr && viewerCheckoutUserIdStr !== currentUserIdStr) ||
                                 (checkoutUserIdStr && currentUserIdStr && checkoutUserIdStr !== currentUserIdStr);

    // 2. Check if the extension is supported
    const normalizedExtension = extension ? extension.toLowerCase().replace('.', '') : '';

if (normalizedExtension !== 'mpp' && normalizedExtension !== 'vsdx'){// && normalizedExtension !== 'dwg') {
        return;
    }

    // 3. Get appropriate icon and app name based on extension
    const projectIcon = `
            <svg width="120" height="120" viewBox="0 0 96 96" xmlns="http://www.w3.org/2000/svg">
                <path fill="#31752F" d="M0 0h96v96H0z"/>
                <path fill="#FFF" d="M19 19h58v58H19z" opacity=".1"/>
                <path fill="#FFF" d="M19 19h58v58H19z" opacity=".2"/>
                <path fill="#FFF" d="M19 19h58v58H19z" opacity=".2"/>
                <path fill="#FFF" d="M19 19h58v58H19z" opacity=".2"/>
                <path fill="#FFF" d="M11.25 11.25h73.5v73.5h-73.5z"/>
                <path fill="#31752F" d="M17.75 17.75h60.5v60.5h-60.5z"/>
                <path fill="#FFF" d="M28.344 32.578h10.266c3.844 0 6.891.969 9.094 2.906 2.25 1.938 3.375 4.641 3.375 8.063 0 3.469-1.125 6.188-3.375 8.156-2.203 1.969-5.25 2.953-9.094 2.953h-4.172v11.766h-6.094V32.578zm6.094 5.109v11.672h3.703c1.828 0 3.25-.516 4.266-1.547 1.062-1.078 1.594-2.531 1.594-4.359 0-1.781-.531-3.188-1.594-4.219-1.016-1.078-2.438-1.547-4.266-1.547h-3.703z"/>
            </svg>
    `;

    const visioIcon = `
            <svg width="120" height="120" viewBox="0 0 96 96" xmlns="http://www.w3.org/2000/svg">
                <path fill="#3955A3" d="M0 0h96v96H0z"/>
                <path fill="#FFF" d="M19 19h58v58H19z" opacity=".1"/>
                <path fill="#FFF" d="M19 19h58v58H19z" opacity=".2"/>
                <path fill="#FFF" d="M19 19h58v58H19z" opacity=".2"/>
                <path fill="#FFF" d="M19 19h58v58H19z" opacity=".2"/>
                <path fill="#FFF" d="M11.25 11.25h73.5v73.5h-73.5z"/>
                <path fill="#3955A3" d="M17.75 17.75h60.5v60.5h-60.5z"/>
                <path fill="#FFF" d="M32.656 32.578h6.797l6.609 19.922c.375 1.219.656 2.344.844 3.375.188-1.078.469-2.203.797-3.375l6.703-19.922h6.641L50.625 66.422h-7.406L32.656 32.578z"/>
            </svg>
    `;

  /*  const autocadIcon = `
            <svg width="120" height="120" viewBox="0 0 96 96" xmlns="http://www.w3.org/2000/svg">
                <path fill="#E51937" d="M0 0h96v96H0z"/>
                <path fill="#FFF" d="M19 19h58v58H19z" opacity=".1"/>
                <path fill="#FFF" d="M19 19h58v58H19z" opacity=".2"/>
                <path fill="#FFF" d="M19 19h58v58H19z" opacity=".2"/>
                <path fill="#FFF" d="M19 19h58v58H19z" opacity=".2"/>
                <path fill="#FFF" d="M11.25 11.25h73.5v73.5h-73.5z"/>
                <path fill="#E51937" d="M17.75 17.75h60.5v60.5h-60.5z"/>
                <path fill="#FFF" d="M25 32.5h8.5l12 31h-7.5l-2.2-6h-13l-2.2 6H13l12-31zm4.2 8.5l-4.5 12h9l-4.5-12z"/>
            </svg>
    `;*/

    let appInfo;
    if (normalizedExtension === 'mpp') {
        appInfo = { name: 'Microsoft Project', icon: projectIcon, color: '#00ae8c', bgColor: '#e8f5e9' };
    } else if (normalizedExtension === 'vsdx') {
        appInfo = { name: 'Microsoft Visio', icon: visioIcon, color: '#00ae8c', bgColor: '#e8f5e9' };
    } /*else if (normalizedExtension === 'dwg') {
     appInfo = { name: 'AutoCAD', icon: autocadIcon, color: '#E51937', bgColor: '#ffebee' };
  }*/

    // 4. Create clean, professional viewer UI
    $('[ref="viewerContainer"]').html(`
            <div style="
                display: flex;
                align-items: center;
                justify-content: center;
                height: 75vh;
                width: 100%;
                background: linear-gradient(to bottom right, ${appInfo.bgColor}, #ffffff);
                padding: 20px;
            ">
                <div style="
                    max-width: 800px;
                    width: 100%;
                    text-align: center;
                ">
                    <!-- File name -->
                    <h2 style="
                        margin: 0 0 25px 0;
                        font-size: 22px;
                        font-weight: 600;
                        color: #333;
                        font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
                    ">${fileName}</h2>

                    <!-- Action buttons -->
                    <div style="display: flex; gap: 15px; justify-content: center; align-items: center; flex-wrap: wrap;">
                        <!-- Edit button (only if not checked out by another user) -->
                        ${!isCheckedOutByOther ? `
                        <button
                            onclick="handleOpenInDesktop('${fileId}', '${fileName}', '${extension}', '${version}', ${fromAttachment}, 'edit')"
                            style="
                                background: ${appInfo.color};
                                color: white;
                                font-size: 16px;
                                font-weight: 600;
                                padding: 16px 40px;
                                border: none;
                                border-radius: 12px;
                                cursor: pointer;
                                box-shadow: 0 6px 20px ${appInfo.color}40;
                                transition: all 0.2s ease;
                                font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
                                display: inline-flex;
                                align-items: center;
                                gap: 10px;
                            "
                            onmouseover="this.style.transform='translateY(-3px)'; this.style.boxShadow='0 10px 30px ${appInfo.color}50';"
                            onmouseout="this.style.transform='translateY(0)'; this.style.boxShadow='0 6px 20px ${appInfo.color}40';"
                        >
                            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"></path><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"></path></svg>
                            ${Common.translate("EditIn")} ${appInfo.name}
                        </button>
                        ` : ''}

                        <!-- View button -->
                        <button
                            onclick="handleOpenInDesktop('${fileId}', '${fileName}', '${extension}', '${version}', ${fromAttachment}, 'view')"
                            style="
                                background: white;
                                color: ${appInfo.color};
                                font-size: 16px;
                                font-weight: 600;
                                padding: 16px 40px;
                                border: 2px solid ${appInfo.color};
                                border-radius: 12px;
                                cursor: pointer;
                                box-shadow: 0 6px 20px rgba(0,0,0,0.1);
                                transition: all 0.2s ease;
                                font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
                                display: inline-flex;
                                align-items: center;
                                gap: 10px;
                            "
                            onmouseover="this.style.transform='translateY(-3px)'; this.style.boxShadow='0 10px 30px rgba(0,0,0,0.15)'; this.style.background='${appInfo.bgColor}';"
                            onmouseout="this.style.transform='translateY(0)'; this.style.boxShadow='0 6px 20px rgba(0,0,0,0.1)'; this.style.background='white';"
                        >
                            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path><circle cx="12" cy="12" r="3"></circle></svg>
                            ${Common.translate("ViewOnly")}
                        </button>
                    </div>

                    <!-- Warning message if checked out by another user -->
                    ${isCheckedOutByOther ? `
                    <div style="
                        margin-top: 20px;
                        padding: 12px 20px;
                        background: #fff3cd;
                        border: 1px solid #ffc107;
                        border-radius: 8px;
                        color: #856404;
                        font-size: 14px;
                        font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
                        display: inline-flex;
                        align-items: center;
                        gap: 10px;
                    ">
                        <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"></circle><line x1="12" y1="8" x2="12" y2="12"></line><line x1="12" y1="16" x2="12.01" y2="16"></line></svg>
                        ${Common.translate("FileCheckedOutMessage")}
                    </div>
                    ` : ''}

                    <!-- Help text -->
                    <p style="
                        margin-top: 20px;
                        font-size: 13px;
                        color: #999;
                        font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
                    ">
                       ${Common.translate("MessageOpenInMs")}
                    </p>
                </div>
            </div>

            <style>
                @keyframes fadeInScale {
                    from {
                        opacity: 0;
                        transform: scale(0.8);
                    }
                    to {
                        opacity: 1;
                        transform: scale(1);
                    }
                }
            </style>
    `);
}

// Helper function that prepares JSON data and calls editInMicrosoft or viewInMicrosoft
function handleOpenInDesktop(id, name, extension, version, fromAttachment, mode = 'edit') {
    // Create JSON object with all parameters
    const fileData = {
        id: id,
        name: name,
        extension: extension,
        version: version,
        fromAttachment: fromAttachment
    };

    // Call appropriate function based on mode
    if (mode === 'view') {
        viewInMicrosoft(fileData);
    } else {
        editInMicrosoft(fileData);
    }
}

// Open file in Microsoft application (edit mode)
async function editInMicrosoft(data) {
    await openInMicrosoft(data, 'edit');
}

// Open file in Microsoft application (read-only mode)
async function viewInMicrosoft(data) {
    await openInMicrosoft(data, 'view');
}

// Store active polling intervals
const activeFilePolling = {};

// Common function to open file in Microsoft application or AutoCAD
async function openInMicrosoft(data, mode) {
    const apiUrl = window.CustomWebDevUrl.replace(/\/$/, "");

    // Ensure extension has a dot prefix
    const extension = data.extension.startsWith('.') ? data.extension : '.' + data.extension;
    const normalizedExtension = extension.toLowerCase().replace('.', '');

    // Use fileName if available, otherwise construct from id and extension
    const fileName = `${data.name}${extension}`;

    // Get the access token
    const token = window.IdentityAccessToken;

    // Build query parameters with all required fields including token and mode
    const params = new URLSearchParams({
        fileName: fileName,
        extension: extension,
        version: data.version,
        fromAttachment: data.fromAttachment ?? false,
        token: token,
        mode: mode
    });

    const response = await fetch(`${apiUrl}/webdav/${data.id}/edit-url?${params}`, {
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        }
    });

    if (!response.ok) {
        const errorText = await response.text();
        console.error('WebDAV API error:', errorText);
        throw new Error(`Failed to get ${mode} URL: ${response.status} ${response.statusText}`);
    }

    const result = await response.json();

    // Check if this is an AutoCAD file (.dwg)
   if (normalizedExtension === 'dwg') {
        // Launch AutoCAD directly using protocol URL (preserves user gesture)
        const iframe = document.createElement('iframe');
        iframe.style.display = 'none';
        iframe.src = result.officeUri;
        document.body.appendChild(iframe);

        // Clean up iframe after a short delay
        setTimeout(function() {
            if (iframe.parentNode) {
                document.body.removeChild(iframe);
            }
        }, 1000);

        // Check in parallel if protocol handler is installed
        // If not, show installation prompt
        AutoCADProtocolDetector.isProtocolHandlerInstalled(function(installed) {
            if (!installed) {
                AutoCADProtocolDetector.showInstallPrompt();
            }
        });

        // Start polling to detect when file is closed
        startFileStatusPolling(data.id, data.fromAttachment);
    } else {
        // For other files (mpp, vsdx), open directly
        window.location.href = result.officeUri;

        // Start polling to detect when file is closed
        startFileStatusPolling(data.id, data.fromAttachment);
    }
}

// Callback function to show editInMicrosoft action only for supported files AND not checked out by another user
function editInMicrosoftEnabled(data) {
    // Get the file extension
    const extension = data?.extension || '';

    // Normalize extension (remove dot and convert to lowercase)
    const normalizedExtension = extension.toLowerCase().replace('.', '');

    // Check if file type is supported
    const isSupportedType = normalizedExtension === 'mpp' || normalizedExtension === 'vsdx' || normalizedExtension === 'dwg';

    if (!isSupportedType) {
        return false;
    }

    // Check checkout status
    const viewerCheckoutByUserId = data?.viewerCheckoutUserId || null;
    const checkoutByUserId = data?.checkoutUserId || null;
    const currentUserId = window.UserId || null;

    // Convert to strings for comparison
    const viewerCheckoutUserIdStr = viewerCheckoutByUserId ? String(viewerCheckoutByUserId) : null;
    const checkoutUserIdStr = checkoutByUserId ? String(checkoutByUserId) : null;
    const currentUserIdStr = currentUserId ? String(currentUserId) : null;

    // Show edit action only if file is NOT checked out by another user (check EITHER property)
    const isCheckedOutByOther = (viewerCheckoutUserIdStr && currentUserIdStr && viewerCheckoutUserIdStr !== currentUserIdStr) ||
                                 (checkoutUserIdStr && currentUserIdStr && checkoutUserIdStr !== currentUserIdStr);

    return !isCheckedOutByOther;
}

// Callback function to show viewInMicrosoft action for all supported files
function viewInMicrosoftEnabled(data) {
    // Get the file extension
    const extension = data?.extension || '';

    // Normalize extension (remove dot and convert to lowercase)
    const normalizedExtension = extension.toLowerCase().replace('.', '');

    // Return true for mpp, vsdx, and dwg files (always show view, regardless of checkout status)
    return normalizedExtension === 'mpp' || normalizedExtension === 'vsdx' || normalizedExtension === 'dwg';
}

// Start polling to check if file is still checked out
function startFileStatusPolling(fileId, fromAttachment) {
    // Stop any existing polling for this file
    stopFileStatusPolling(fileId);

    // Store initial checkout status
    let wasCheckedOut = null; // null = unknown, true = checked out, false = not checked out
    let fileWasOpened = false; // Track if we detected the file was actually opened
    let pollCount = 0;
    const maxPolls = 120; // Maximum 10 minutes (120 * 5 seconds)

    // Poll every 5 seconds
    const intervalId = setInterval(async () => {
        pollCount++;

        try {
            // Always use GetLatestMetadata for polling
            const endpoint = `/Files/GetLatestMetadata?Id=${fileId}`;

            const response = await fetch(endpoint);

            if (!response.ok) {
                return;
            }

            const metadata = await response.json();
            const viewerCheckoutUserId = metadata.viewerCheckoutUserId || null;
            const checkoutUserId = metadata.checkoutUserId || null;

            const isCheckedOut = viewerCheckoutUserId !== null;

            // First poll or file just became checked out - user opened the file
            if (wasCheckedOut === null || (!wasCheckedOut && isCheckedOut)) {
				if (isCheckedOut) {
                    fileWasOpened = true;

                    // Refresh the DataTable
                    try {
                        const dataTable = $('[ref="grdItems"]').DataTable();
                        if (dataTable) {
                            dataTable.ajax.reload();
                        }
                    } catch (error) {
                        // Silently handle error
                    }
                }
            }

            // If file was checked out but is now released - user closed the file
            if (wasCheckedOut === true && !isCheckedOut) {
                // Refresh the DataTable
                try {
                    const dataTable = $('[ref="grdItems"]').DataTable();
                    if (dataTable) {
                        dataTable.ajax.reload();
                    }
                } catch (error) {
                    // Silently handle error
                }

                // Hide modal and trigger double-click on the file row (only if modal is open)
                if ($('[ref="modalFileDetails"]').length>0 ){
					let row=$('[ref=grdItems]').find('[data-id="' + fileId + '"]').parent().parent();
					var refreshModal=false;
					if(checkoutUserId!=null&&$("#checkoutswitch").prop("checked")==false){
						refreshModal=true;
					}else{					
						let cabinetId=$('[ref=grdItems]').DataTable().row(row).data().cabinetId;
						if(!getCabinetSettings(cabinetId).requireCheckOut) {
							refreshModal=true;
						}
					}
					if(refreshModal) {
						$('.modal-backdrop').remove();
						$('[ref="modalFileDetails"]').modal('hide');
						row.trigger("dblclick");
					}
				}

                // Stop polling after file is closed
                stopFileStatusPolling(fileId);
            }

            // Update the checkout status for next iteration
            wasCheckedOut = isCheckedOut;

            // Stop polling after max attempts (only if file was never opened or already closed)
            if (pollCount >= maxPolls) {
                stopFileStatusPolling(fileId);
            }

        } catch (error) {
            // Silently handle error
        }
    }, 5000); // Poll every 5 seconds

    // Store the interval ID
    activeFilePolling[fileId] = intervalId;
}

// Stop polling for a specific file
function stopFileStatusPolling(fileId) {
    if (activeFilePolling[fileId]) {
        clearInterval(activeFilePolling[fileId]);
        delete activeFilePolling[fileId];
    }
}

// Clean up all polling when page is unloaded
window.addEventListener('beforeunload', () => {
    Object.keys(activeFilePolling).forEach(fileId => {
        stopFileStatusPolling(fileId);
    });
});

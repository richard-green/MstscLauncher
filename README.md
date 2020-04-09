# MstscLauncher

Allows your system to process Remote Desktop urls of the format: **mstsc://your-computer-name/** or **rdp://company-server-01/**

First, run the program in Administrator mode so it can correctly register the URL with the windows registry.

Query string parameters supported:

* admin – Connects you to the session for administering a server.

* f – Starts Remote Desktop in full-screen mode.

* w=[width] – Specifies the width of the Remote Desktop window.

* h=[height] – Specifies the height of the Remote Desktop window.

* public – Runs Remote Desktop in public mode.

* span – Matches the remote desktop width and height with the local virtual desktop, spanning across multiple monitors, if necessary. To span across monitors, the monitors must be arranged to form a rectangle.

* multimon – Configures the Remote Desktop Services session monitor layout to be identical to the current client-side configuration.

* restrictedAdmin – Connects you to the remote PC or server in Restricted Administration mode. In this mode, credentials won’t be sent to the remote PC or server, which can protect you if you connect to a PC that has been compromised. However, connections made from the remote PC might not be authenticated by other PCs and servers, which might impact app functionality and compatibility. Implies /admin.

* remoteGuard – Connects your device to a remote device using Remote Guard. Remote Guard prevents credentials from being sent to the remote device, which can help protect your credentials if you connect to a remote device that has been compromised. Unlike Restricted Administration mode, Remote Guard also supports connections made from the remote device by redirecting all requests back to your device.

* prompt – Prompts you for your credentials when you connect to the remote PC or server.

* shadow=[sessionID] – Specifies the sessionID that you wish to view.

* control – Allows control of the session.

* noConsentPrompt – Allows shadowing without user consent.

## Example Usages

* **rdp://merlot/?admin=&f=**  _// connect to the machine called "merlot" in admin mode with fullscreen_

* **rdp://sangria/?w=640&h=480** _// connect to the machine called "sangria" with a window size of 640x480_

* **rdp://sangiovese/?multimon=** _// connect to the machine called "sangiovese" using all available monitors_

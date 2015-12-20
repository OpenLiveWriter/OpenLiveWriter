#Test Plan for Ensuring that Installing as an New Install works properly
Steps                 | Desired Results                | Complete | Comments
----------------------|--------------------------------|----------| --------
Ensure that OpenLiveWriter is not installed.  If it is installed, delete registry keys and local files | |
||| Registry Keys are at HKEY_CURRENT_USER\Software\OpenLiveWriter
||| Local files are at %users%\AppData\Local\OpenLiveWriter
Download build | Setup File should be named OpenLiveWriterSetup.exe |  | 
Run OpenLiveWriterSetup.exe | See OpenLiveWriter Splash | |
OpenLiveWriter Opens | User can add a blog | |
 | Post multiple times with each blog | |
 | File Tab exists and contains proper options | |
 | Home Tab exists and contains proper options | |
 | Insert Tab exists and contains proper options | |
 | Blog Tab exists and contains proper options | | 
 | Help Tab exists and contains proper options | |

# Open Live Writer
Open Live Writer makes it easy to write, preview, and post to your blog.
For more information see http://www.OpenLiveWriter.org/.

[![Build status](https://ci.appveyor.com/api/projects/status/8xpga2y53sgwo24g?svg=true)](https://ci.appveyor.com/project/dotnetfoundation/openlivewriter)

### Installation
You can install the latest version of Open Live Writer alongside an [older version of Windows Live Writer](http://windows.microsoft.com/en-us/windows-live/essentials). Visit
http://www.OpenLiveWriter.org to download and install the latest release.

### Latest News
The current version of Open Live Writer is our first open source version.
For a [list of known issues see GitHub](https://github.com/OpenLiveWriter/OpenLiveWriter/issues) or take a
look at the [roadmap](roadmap.md) to see what the current plans are.

For the latest news and updates about Open Live Writer, you can follow us on Twitter 
([@OpenLiveWriter](https://twitter.com/OpenLiveWriter)), by keeping an eye on the website
 http://www.OpenLiveWriter.org or by watching this repo and subscribing to notifications.

### Contributing
Open Live Writer is an open source project and wouldn't exist without the passionate community of volunteer
[contributors](https://github.com/OpenLiveWriter/OpenLiveWriter/graphs/contributors).
If you would like to help out then please see the [Contributing](CONTRIBUTING.md) guide.

This project has adopted the code of conduct defined by the [Contributor Covenant](http://contributor-covenant.org/)
to clarify expected behavior in our community.
For more information see the [.NET Foundation Code of Conduct](http://www.dotnetfoundation.org/code-of-conduct).

[![Gitter](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/OpenLiveWriter/OpenLiveWriter?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)

### License
Open Live Writer proudly uses the [MIT License](license.txt).

### History
The product that became Live Writer was originally created by a small, super-talented team of engineers including 
JJ Allaire, Joe Cheng, Charles Teague, and Spike Washburn. The team was acquired by Microsoft 
in 2006 and organized with the Spaces team. Becky Pezely joined the team and over time, the team grew and shipped
many popular releases of Windows Live Writer.

As Microsoft was planning for the version of Windows Live that would coincide with Windows 8 operating system
release, the teams that built the Windows Live client apps for Windows were encouraged to focus on building a 
smaller set of Windows 8 apps designed to work well with both traditional PC input mechanisms and touch. 
With the rise of micro-blogging platforms and other forms of sharing, eventually this team decided to conclude
their work on Windows Live Writer with Windows Live Writer 2012.

Even though there was no active development, Windows Live Writer continued to be a favorite tool of a passionate
community of Windows PC users for authoring, editing, and publishing blog posts. Data from WordPress.com at the 
time suggested that Windows Live Writer (even two years after active development ended) was the #1 app for authoring
a blog post to WordPress.com from a Windows PC. 

A few employees at Microsoft took an interest in reviving Live Writer as an open source project in their
spare time.  By January 2015, a group of about a half-dozen engineers interested in spending some of their
volunteer time to help release an updated version of Live Writer had found each other and began work on getting
this open source fork of Live Writer formed and ready to ship. In December 2015 Microsoft donated the code
to the .NET Foundation and this passionate group of volunteer engineers rapidly assembled the first open source
version.

### Building
Open Live Writer can be built by running build.cmd found in this directory.   
It can be opened in Visual Studio.  The solution is in src/managed/writer.sln -- if you see errors in Visual Studio run build.cmd from the command prompt and it should be resolved.
The main program is src/managed/OpenLiveWriter/ApplicationMain.cs .
To run from Visual Studio, set the startup project to OpenLiveWriter.

### .NET Foundation

The Open Live Writer project is supported by the [.NET Foundation](http://www.dotnetfoundation.org).

/*
 * This .js file is used (only) during the build process to set
 * a registry value that records the current solution root directory.
 * This is used by various global settings in DEBUG builds only.
 */

if (WScript.Arguments.length < 1)
{
	WScript.Echo('ERROR: Not enough arguments');
	WScript.Quit(1);
}

var shell = WScript.CreateObject('WScript.Shell');
shell.RegWrite('HKLM\\Software\\WriterDebug\\', WScript.Arguments(0));

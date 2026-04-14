// Global using aliases to resolve ambiguity between OpenLiveWriter.Api.DialogResult
// and System.Windows.Forms.DialogResult. The PostEditor project primarily uses
// WinForms DialogResult; the Api DialogResult is used only when calling plugin API methods.
global using DialogResult = System.Windows.Forms.DialogResult;

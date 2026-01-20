using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms.Design;

namespace OpenLiveWriter.CoreServices.Diagnostics
{
    /// <summary>
    /// Designer for unexpected error messages.
    /// NOTE: CodeDomLocalizationProvider API changed in .NET 10.
    /// This class now delegates to the base ComponentDocumentDesigner without localization support.
    /// </summary>
    public class UnexpectedErrorMessageDesigner : ComponentDocumentDesigner
    {
        public override void Initialize(IComponent component)
        {
            base.Initialize(component);
            // CodeDomLocalizationProvider API changed in .NET 10 - localization handled differently
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}

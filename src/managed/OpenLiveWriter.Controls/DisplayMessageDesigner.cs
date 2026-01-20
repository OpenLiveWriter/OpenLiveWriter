using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms.Design;

namespace OpenLiveWriter.Controls
{
    /// <summary>
    /// Designer for display messages.
    /// NOTE: LocalizationExtenderProvider is not available in .NET 10.
    /// This class now delegates to the base ComponentDocumentDesigner without localization support.
    /// </summary>
    public class DisplayMessageDesigner : ComponentDocumentDesigner
    {
        public override void Initialize(IComponent component)
        {
            base.Initialize(component);
            // LocalizationExtenderProvider removed in .NET Core - localization handled differently
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}

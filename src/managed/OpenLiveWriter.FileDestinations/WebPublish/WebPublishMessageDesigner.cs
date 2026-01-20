using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms.Design;

namespace OpenLiveWriter.FileDestinations
{
    public class WebPublishMessageDesigner : ComponentDocumentDesigner
    {
        // NOTE: LocalizationExtenderProvider was removed in .NET Core/.NET 5+
        // This designer functionality is disabled for .NET 10

        // Adds a LocalizationExtenderProvider for the component this designer is initialized to support.
        public override void Initialize(IComponent component)
        {
            base.Initialize(component);
            // LocalizationExtenderProvider is not available in .NET 10
        }

        // If a LocalizationExtenderProvider has been added, removes the extender provider.
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            // LocalizationExtenderProvider is not available in .NET 10
        }
    }
}

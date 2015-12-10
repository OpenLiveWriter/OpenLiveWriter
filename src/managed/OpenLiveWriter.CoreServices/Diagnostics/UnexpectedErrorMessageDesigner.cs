

using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Windows.Forms.Design;

namespace OpenLiveWriter.CoreServices.Diagnostics
{
    public class UnexpectedErrorMessageDesigner : ComponentDocumentDesigner
    {
        // Stores reference to the LocalizationExtenderProvider this designer adds, in order to remove it on Dispose.
        private CodeDomLocalizationProvider codeDomLocalizationProvider;

        // Adds a LocalizationExtenderProvider for the component this designer is initialized to support.
        public override void Initialize(IComponent component)
        {
            base.Initialize(component);

            // If no extender from this designer is active...
            if (codeDomLocalizationProvider == null)
            {
                // Adds a LocalizationExtenderProvider that provides localization support properties to the specified component.
                codeDomLocalizationProvider = new CodeDomLocalizationProvider(component.Site, component);
                codeDomLocalizationProvider.SetLocalizable(component, true);
            }
        }

        // If a LocalizationExtenderProvider has been added, removes the extender provider.
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            // If an extender has been added, remove it
            if (codeDomLocalizationProvider != null)
            {
                // Disposes of the extender provider.  The extender
                // provider removes itself from the extender provider
                // service when it is disposed.
                codeDomLocalizationProvider.Dispose();
                codeDomLocalizationProvider = null;
            }
        }
    }
}

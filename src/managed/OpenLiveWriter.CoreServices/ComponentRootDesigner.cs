using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms.Design;

namespace OpenLiveWriter.CoreServices
{

    public class ComponentRootDesigner : ComponentDocumentDesigner
    {
        // Stores reference to the LocalizationExtenderProvider this designer adds, in order to remove it on Dispose.
        private LocalizationExtenderProvider localizationExtenderProvider;

        // Adds a LocalizationExtenderProvider for the component this designer is initialized to support.
        public override void Initialize(IComponent component)
        {
            base.Initialize(component);

            // If no extender from this designer is active...
            if (localizationExtenderProvider == null)
            {
                // Adds a LocalizationExtenderProvider that provides localization support properties to the specified component.
                localizationExtenderProvider = new LocalizationExtenderProvider(component.Site, component);
            }
        }

        // If a LocalizationExtenderProvider has been added, removes the extender provider.
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            // If an extender has been added, remove it
            if (localizationExtenderProvider != null)
            {
                // Disposes of the extender provider.  The extender
                // provider removes itself from the extender provider
                // service when it is disposed.
                localizationExtenderProvider.Dispose();
                localizationExtenderProvider = null;
            }
        }
    }
}

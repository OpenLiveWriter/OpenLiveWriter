using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace SetupCustomCultures
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 1 && args[0] == "unregister")
            {
                Unregister("ar-ploc");
                Unregister("ja-ploc");
                Unregister("ar-ploc-sa");
                Unregister("ja-ploc-jp");
                Unregister("en-locr-us");
            }
            else
            {
                RegisterCustomCulture("ar-ploc-sa", "ar-sa");
                RegisterCustomCulture("ja-ploc-jp", "ja-jp");
                RegisterCustomCulture("en-locr-us", "en-us");
            }

            foreach (string str in args)
            {
                // exit silently if any string is equal to silent.
                if (str == "silent")
                    return;
            }

            Console.WriteLine("Press enter to continue");
            Console.ReadLine();
            
        }

        private static void Unregister(string cultureName)
        {
            Console.WriteLine("Unregistering...");

            try
            {
                CultureAndRegionInfoBuilder.Unregister(cultureName);
                Console.WriteLine("Success");
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error unregistering");
                Console.WriteLine(ex);
            }

            
            Console.WriteLine();
        }

        private static void RegisterCustomCulture(string customCultureName, string parentCultureName)
        {
            Console.WriteLine("Registering {0}", customCultureName);
            try
            {
                CultureAndRegionInfoBuilder cib =
                    new CultureAndRegionInfoBuilder(customCultureName, CultureAndRegionModifiers.None);
                CultureInfo ci = new CultureInfo(parentCultureName);
                cib.LoadDataFromCultureInfo(ci);

                RegionInfo ri = new RegionInfo(parentCultureName);
                cib.LoadDataFromRegionInfo(ri);
                cib.Register();
                Console.WriteLine("Success.");
            }
            catch (InvalidOperationException)
            {
                // This is OK, means that this is already registered.
                Console.WriteLine("The custom culture {0} was already registered", customCultureName);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Registering the custom culture {0} failed", customCultureName);
                Console.WriteLine(ex);
            }
            Console.WriteLine();

        }


    }
}

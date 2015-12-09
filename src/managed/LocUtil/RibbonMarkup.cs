// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace LocUtil
{
    public static class RibbonMarkup
    {
        public const string XPathPrefix = "ribbon";
        public const string NamespaceUri = "http://schemas.microsoft.com/windows/2009/Ribbon";
        public static class Command
        {
            public const string Id = "Id";
            public const string Name = "Name";
            public const string Symbol = "Symbol";
            public const string Comment = "Comment";
            public const string Keytip = "Keytip";
            public const string LabelTitle = "LabelTitle";
            public const string LabelDescription = "LabelDescription";
            public const string TooltipTitle = "TooltipTitle";
            public const string TooltipDescription = "TooltipDescription";
            public const string LargeImages = "Command.LargeImages";
            public const string SmallImages = "Command.SmallImages";
            public const string LargeHighConstrastImages = "Command.LargeHighContrastImages";
            public const string SmallHighContrastImages = "Command.SmallHighContrastImages";

            // Child nodes of Command --> Command.LargeImages, Command.SmallImages, Command.LargeHighContrastImages, Command.SmallHighContrastImages
            public static class Image
            {
                public const string Source = "Source";
                public const string Id = "Id";
                public const string MinDPI = "MinDPI";
                public const string Symbol = "Symbol";
            }
        }
    }
}

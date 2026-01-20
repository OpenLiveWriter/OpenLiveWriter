// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Windows.Forms;
using OpenLiveWriter.Controls;

namespace OpenLiveWriter.PostEditor.PostPropertyEditing.CategoryControl
{
    /// <summary>
    /// Base class for category display forms. Uses CategoryDisplayFormM1 as the implementation.
    /// </summary>
    internal abstract class CategoryDisplayFormBase : MiniForm
    {
    }

    /// <summary>
    /// Factory class that creates the appropriate category display form.
    /// </summary>
    internal class CategoryDisplayForm : CategoryDisplayFormM1
    {
        public CategoryDisplayForm(Control parentControl, CategoryContext categoryContext)
            : base(parentControl, categoryContext)
        {
        }
    }
}

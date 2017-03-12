// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices.Progress;

namespace OpenLiveWriter.Controls
{
    /// <summary>
    /// Summary description for ProgressHelper.
    /// </summary>
    public class ProgressHelper
    {
        private ProgressHelper()
        {
            //no instances
        }

        public static DialogResult ExecuteWithProgress(string title, ProgressOperation operation, Control control)
        {
            return ExecuteWithProgress(title, operation, control, control.FindForm());
        }

        public static DialogResult ExecuteWithProgress(string title, ProgressOperation operation, ISynchronizeInvoke synchronizeInvoke, IWin32Window owner)
        {
            using (ProgressDialog progress = new ProgressDialog())
            {
                progress.Text = title;
                MultipartAsyncOperation async = new MultipartAsyncOperation(synchronizeInvoke);
                async.AddProgressOperation(operation, new ProgressCategory(null, null), 100);
                progress.ProgressProvider = async;
                async.Start();

                DialogResult result;
                if (!async.IsDone)
                    result = progress.ShowDialog(owner);
                else
                    result = progress.DialogResult;

                if (result == DialogResult.Cancel)
                    throw new OperationCancelledException();
                else if (result == DialogResult.Abort)
                    throw async.Exception;
                else
                    return result;
            }
        }

    }
}

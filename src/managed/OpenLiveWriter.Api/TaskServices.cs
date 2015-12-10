// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices.Threading;

namespace OpenLiveWriter.Api
{
    /// <summary>
    /// Encapsulates a method that has one parameter and returns a value.
    /// </summary>
    /// <typeparam name="TParam">The type of the parameter.</typeparam>
    /// <typeparam name="TResult">The type of the return value.</typeparam>
    /// <param name="arg">The parameter of the method that this delegate encapsulates.</param>
    /// <returns>The return value of the method that this delegate encapsulates.</returns>
    public delegate TResult Task<TParam, TResult>(TParam arg);

    /// <summary>
    /// Encapsulates a method that takes no parameters and does not
    /// directly return a value.
    /// </summary>
    public delegate void Task();

    /// <summary>
    /// Provides utility methods for performing long-running tasks.
    /// </summary>
    public static class TaskServices
    {
        /// <summary>
        /// Executes a potentially long-running task on a background thread
        /// while keeping the UI thread running responsively. This method
        /// does not return until the task has completed executing. Any
        /// exception thrown by the task will be re-thrown on the calling
        /// thread.
        /// </summary>
        /// <remarks>
        /// This method is designed to keep message loops running
        /// responsively. If the current thread does not have an active
        /// message loop, the task is simply executed on the current thread.
        /// </remarks>
        /// <param name="task">The task to execute on the background thread.</param>
        /// <example><code>TaskServices.ExecuteWithResponsiveUI(delegate
        /// {
        ///     DoSomethingExpensive();
        /// });</code></example>
        public static void ExecuteWithResponsiveUI(Task task)
        {
            ExecuteWithResponsiveUI<object, object>(null, delegate { task(); return null; });
        }

        /// <summary>
        /// <para>Executes a potentially long-running task on a background thread
        /// while keeping the UI thread running responsively. This method
        /// does not return until the task has completed executing. Any
        /// exception thrown by the task will be re-thrown on the calling
        /// thread.</para>
        /// <para>This overload takes an argument and returns a result.
        /// The argument is passed to the task, and the value returned
        /// by the task is returned by this method.</para>
        /// </summary>
        /// <remarks>
        /// This method is designed to keep message loops running
        /// responsively. If the current thread does not have an active
        /// message loop, the task is simply executed on the current thread.
        /// </remarks>
        /// <param name="arg">The argument to pass to the task.</param>
        /// <param name="task">The task to execute on the background thread.</param>
        /// <returns>The value returned by the task.</returns>
        /// <example><code>string html = TaskServices.ExecuteWithResponsiveUI("http://www.contoso.com", delegate (string url)
        /// {
        ///     using (WebClient wc = new WebClient())
        ///         return wc.DownloadString(url);
        /// });</code>
        /// </example>
        public static TResult ExecuteWithResponsiveUI<TParam, TResult>(TParam arg, Task<TParam, TResult> task)
        {
            TResult result = default(TResult);

            if (Application.MessageLoop)
            {
                Exception e = null;
                Thread t = ThreadHelper.NewThread(new ThreadStart(delegate
                {
                    try
                    {
                        result = task(arg);
                    }
                    catch (Exception ex)
                    {
                        e = ex;
                    }
                }),
                "ExecWithResponsiveUI",
                true,
                true,
                true);

                t.Start();

                while (!t.Join(50))
                {
                    Application.DoEvents();
                }

                if (e != null)
                {
                    Trace.WriteLine(e.ToString());
                    throw e;
                }
                else
                    return result;
            }
            else
            {
                return task(arg);
            }
        }
    }
}

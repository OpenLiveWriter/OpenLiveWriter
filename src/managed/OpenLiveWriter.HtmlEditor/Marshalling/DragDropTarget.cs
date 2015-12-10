// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.HtmlEditor.Marshalling
{

    public class DragAndDropTarget : IDisposable
    {

        #region Initialization/Disposal

        /// <summary>
        /// Initialize the drag and drop manager with its context. Note that this object
        /// is initialized in the 3 stages: construction, replacement of MSHTML DropTarget
        /// implementation, and finally installation of drag and drop event handlers for
        /// the drag target control. The 3-step initialization is required because MSHTML
        /// also initializes in stages and our initialization must correspond to its/
        /// </summary>
        /// <param name="dataFormats">the data format factory</param>
        public DragAndDropTarget(IDataFormatHandlerFactory dataFormats)
        {
            dataFormatFactory = dataFormats;
        }

        /// <summary>
        /// Disable the MSHTML drop target and enable this control as a drop target. In theory
        /// we don't need to do this as IDocHostUIHandler.GetDropTarget should allow us to
        /// fully replace their implementation with our own (delegating back to MSHTML as
        /// necessary). Alas, this works fine in most scenarios except when a .NET data object
        /// is being dragged over us from within the current process (as in the case of the
        /// MicroView) the .NET object is not marshalled to an OLE IDataObject so MSHTML
        /// doesn't see it and doesn't call us. For this case we need inbound IDataObject
        /// processing to be .NET based, thus the need to disable the MSHTML drop target
        /// </summary>
        /// <param name="dragTargetControl">Control that will be used as the target
        /// for drag and drop operations (passed to us so we can subscribe to it's
        /// drag and drop events)</param>
        public virtual void Initialize(Control dragTargetControl)
        {
            // enable the drop target
            dragTargetControl.AllowDrop = true;

            // hookup drag and drop events directly to the drag and drop manager
            targetControl = dragTargetControl; //cache the control so we can unhook events on dispose.
            dragTargetControl.DragEnter += new DragEventHandler(DragEnter);
            dragTargetControl.DragOver += new DragEventHandler(DragOver);
            dragTargetControl.DragLeave += new EventHandler(DragLeave);
            dragTargetControl.DragDrop += new DragEventHandler(DragDrop);
        }

        /// <summary>
        /// Dispose embedded resources
        /// </summary>
        public virtual void Dispose()
        {
            //unhook drag and drop events
            if (targetControl != null)
            {
                targetControl.DragEnter -= new DragEventHandler(DragEnter);
                targetControl.DragOver -= new DragEventHandler(DragOver);
                targetControl.DragLeave -= new EventHandler(DragLeave);
                targetControl.DragDrop -= new DragEventHandler(DragDrop);
                targetControl = null;
            }

            if (dataFormatFactory != null)
            {
                dataFormatFactory.Dispose();
                dataFormatFactory = null;
            }
        }

        #endregion

        #region Properties
        protected DataFormatHandler ActiveDataFormatHandler
        {
            get
            {
                return dataFormatHandler;
            }
        }
        #endregion

        #region Drag and Drop Event Handlers

        /// <summary>
        /// Handle the DragEnter event for the presentation editor
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        protected virtual void DragEnter(object sender, DragEventArgs e)
        {
            try
            {
                // see if we can get a data format handler for the dragged data
                dataFormatHandler = dataFormatFactory.CreateFrom(new DataObjectMeister(e.Data), DataFormatHandlerContext.DragAndDrop);
                if (dataFormatHandler != null)
                {
                    OnBeforeBeginDrag(e);
                    // tell format handler that we are beginning a drag
                    dataFormatHandler.BeginDrag();
                }
                else
                {
                    // no drop possible
                    e.Effect = DragDropEffects.None;
                }
            }
            catch (Exception ex)
            {
                Trace.Fail(ex.Message, ex.StackTrace);
                e.Effect = DragDropEffects.None;
            }
        }

        protected virtual void OnBeforeBeginDrag(DragEventArgs e)
        {

        }

        /// <summary>
        /// Handle the DragOver event for the presentation editor
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        protected virtual void DragOver(object sender, DragEventArgs e)
        {
            try
            {
                // provide feedback if this is our drag/drop
                if (dataFormatHandler != null)
                {
                    // provide drop feedback
                    e.Effect = dataFormatHandler.ProvideDragFeedback(new Point(e.X, e.Y), e.KeyState, e.AllowedEffect);
                }
                else
                {
                    // no drop possible
                    e.Effect = DragDropEffects.None;
                }
            }
            catch (Exception ex)
            {
                Trace.Fail(ex.Message, ex.StackTrace);
                e.Effect = DragDropEffects.None;
            }
        }

        /// <summary>
        /// Handle the DragLeave event for the presentation editor
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        protected virtual void DragLeave(object sender, EventArgs e)
        {
            // reset data format handler
            ResetDataFormatHandler();
        }

        /// <summary>
        /// Handle the DragDrop event for the presentation editor
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        protected virtual void DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                if (dataFormatHandler != null)
                {
                    // prepare for the drop and fire the event (use wait cursor because
                    // this operation may require large amounts of IO)
                    using (new WaitCursor())
                    {
                        // insert the data
                        bool dropHandled = dataFormatHandler.DataDropped(GetDataAction(e.Effect));
                        if (!dropHandled)
                        {
                            //let the drag source know that the drop failed (prevents move operations from accidentally
                            //deleting the source content)
                            e.Effect = DragDropEffects.None;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.Fail("Drag/drop error: " + ex.Message, ex.StackTrace);
            }
            finally
            {
                // reset data format handler
                ResetDataFormatHandler();
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Reset the state of the data format handler
        /// </summary>
        private void ResetDataFormatHandler()
        {
            if (dataFormatHandler != null)
            {
                dataFormatHandler.EndDrag();
                dataFormatHandler.Dispose();
                dataFormatHandler = null;
            }
        }

        /// <summary>
        /// Get the data action associated with the passed DragDropEffects
        /// </summary>
        /// <param name="effect">effects</param>
        /// <returns>data action</returns>
        private DataAction GetDataAction(DragDropEffects effect)
        {
            // default to copy
            DataAction dataAction = DataAction.Copy;

            // examine effects and set action as appropriate
            if (effect == DragDropEffects.Copy || effect == DragDropEffects.Link)
                dataAction = DataAction.Copy;
            else if (effect == DragDropEffects.Move)
                dataAction = DataAction.Move;
            else if (effect == (DragDropEffects.Copy | DragDropEffects.Link | DragDropEffects.Move))
                dataAction = DataAction.Move;
            else
                Debug.Fail("Unexpected DragDropEffects!");

            // return the action
            return dataAction;
        }

        #endregion

        #region Private Member Variables

        /// <summary>
        /// The set of data format handlers.
        /// </summary>
        private IDataFormatHandlerFactory dataFormatFactory;

        /// <summary>
        /// Handle to the control we are receiving drag/drop events from.
        /// </summary>
        private Control targetControl = null;

        private DataFormatHandler dataFormatHandler;
        #endregion
    }
}

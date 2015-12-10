// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices.Progress;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.Controls.Wizard
{
    /// <summary>
    /// Summary description for WizardController.
    /// </summary>
    public class WizardController
    {
        private IList wizardSteps;
        private int wizardStepIndex = 0;
        private WizardForm _wizardForm;
        public delegate void DisplayCallback(Object stepControl);
        public delegate bool VerifyStepCallback(Object stepControl);
        public delegate void NextCallback(Object stepControl);
        public delegate void BackCallback(Object stepControl);

        public event CancelEventHandler NextCalled;
        public event CancelEventHandler BackCalled;

        public WizardController()
        {
            wizardSteps = new ArrayList();
        }

        internal WizardForm WizardControl
        {
            get
            {
                return _wizardForm;
            }
            set
            {
                _wizardForm = value;
            }
        }

        internal protected virtual void OnWizardLoad()
        {
            DisplayWizardStep();
        }

        /// <summary>
        /// Displays the current wizard step.
        /// </summary>
        protected void DisplayWizardStep()
        {
            WizardStep step = (WizardStep)wizardSteps[wizardStepIndex];
            step.Display();
            _wizardForm.DisplayWizardStep(step, wizardStepIndex, wizardSteps.Count);
        }

        /// <summary>
        /// Moves the wizard forward one step.
        /// </summary>
        public void next()
        {
            try
            {
                WizardStep step = (WizardStep)wizardSteps[wizardStepIndex];
                if (!step.Verify())
                {
                    //the step validation failed, so don't proceed.
                    //Note: This assumes that the step displayed an error message
                    //indicating why the step didn't proceed.
                    return;
                }

                CancelEventArgs cea = new CancelEventArgs();
                if (NextCalled != null)
                    NextCalled(this, cea);
                if (cea.Cancel)
                    return;

                //Note: the step.Next() must occur before incrementing the step
                //index just in case the Next() operation adds substeps.
                step.Next();
                if (wizardStepIndex < wizardSteps.Count - 1)
                {
                    WizardStepIndex++;
                }
                else
                {
                    Finish();
                }
            }
            catch (CancelNextException)
            {
                //next was cancelled.
            }
            catch (OperationCancelledException)
            {
                this.WizardControl.Cancel();
            }
        }

        /// <summary>
        /// Moves the wizard back one step.
        /// </summary>
        public void back()
        {
            try
            {
                CancelEventArgs cea = new CancelEventArgs();
                if (BackCalled != null)
                    BackCalled(this, cea);
                if (cea.Cancel)
                    return;

                WizardStep step = (WizardStep)wizardSteps[wizardStepIndex];
                WizardStepIndex--;

                //notify the current step that its previous operation should be rolled back.
                WizardStep newStep = (WizardStep)wizardSteps[wizardStepIndex];
                newStep.UndoNext();

                //Note: the step.Back() must occur after decrementing the step
                //index just in case the back operation removes substeps.
                step.Back();
            }
            catch (OperationCancelledException)
            {
                this.WizardControl.Cancel();
            }
        }

        /// <summary>
        /// Gets/Sets the currently displayed Wizard step.
        /// Warning: changing this value avoids validation and notification of wizard steps,
        /// which may cause the Wizard's state to become corrupted.  In, general, the next() and back()
        /// operations should be used to switch between wizard steps.
        /// </summary>
        protected int WizardStepIndex
        {
            set
            {
                if (value == wizardStepIndex) return;
                WizardStep step = (WizardStep)wizardSteps[wizardStepIndex];
                OnStepChanging(step, (WizardStep)wizardSteps[value]);

                //hide the previous step control
                //step.Control.Visible = false;

                //display the new step's control
                wizardStepIndex = value;
                DisplayWizardStep();
            }
            get
            {
                return wizardStepIndex;
            }
        }

        /// <summary>
        /// Remove a step from the wizard.
        /// </summary>
        /// <param name="step">The wizard step to remove</param>
        public void removeWizardStep(WizardStep step)
        {
            // save the current index of the step
            int stepIndex = wizardSteps.IndexOf(step);
            if (stepIndex != -1)
            {
                // remove the step
                wizardSteps.Remove(step);

                // if the step index was less than the current step
                // then decrement the current step index
                if (stepIndex < wizardStepIndex)
                    wizardStepIndex--;
            }
            else
            {
                Trace.Fail("Attempted to remove step that doesn't exist!");
            }
        }

        public int GetIndexOfStep(WizardStep step)
        {
            return wizardSteps.IndexOf(step);
        }

        public bool StepExists(WizardStep step)
        {
            return GetIndexOfStep(step) != -1;
        }

        /// <summary>
        /// Adds a step to the wizard.
        ///
        /// </summary>
        /// <param name="step">The new wizard step</param>
        public void addWizardStep(WizardStep step)
        {
            addWizardStep(wizardSteps.Count, step);
        }

        /// <summary>
        /// Add a wizard step at the specified index
        /// </summary>
        /// <param name="step">The new wizard step</param>
        /// <param name="index">The index to insert the step at</param>
        public void addWizardStep(int index, WizardStep step)
        {
            //add the new step
            step.Wizard = this;
            if (index > wizardSteps.Count)
                wizardSteps.Add(step);
            else
                wizardSteps.Insert(index, step);
        }

        /// <summary>
        /// A WizardSubStep is a temporary step that can be added on the fly based on input
        /// from other steps.  Substeps are always removed when the back button is pressed so
        /// that the previous step can have the option of taking a different logic branch that
        /// doesn't trigger the wrong substep again.
        /// </summary>
        public void addWizardSubStep(WizardSubStep step)
        {
            addWizardStep(wizardStepIndex + 1, step);
        }

        public bool NextEnabled
        {
            get { return _wizardForm.NextEnabled; }
            set { _wizardForm.NextEnabled = value; }
        }

        public bool CancelEnabled
        {
            get { return _wizardForm.CancelEnabled; }
            set { _wizardForm.CancelEnabled = value; }
        }

        public void FocusBackButton()
        {
            _wizardForm.FocusBackButton();
        }

        /// <summary>
        /// Overridable method that lets subclasses handle the completion of the Wizard.
        /// </summary>
        virtual protected void Finish()
        {
            _wizardForm.Finish();
        }

        /// <summary>
        /// Let subclasses know when users moves from one page to another inside the wizard.
        /// </summary>
        /// <param name="oldStep"></param>
        /// <param name="newStep"></param>
        virtual protected void OnStepChanging(WizardStep oldStep, WizardStep newStep)
        {
        }

        /// <summary>
        /// Lets subclasses handle when the wizard closes.
        /// </summary>
        public virtual void OnClosing()
        {
        }
    }

    /// <summary>
    /// Represents a step sequence in the wizard.
    /// </summary>
    public class WizardStep
    {
        public UserControl Control;
        public StringId? Header;
        public WizardController.DisplayCallback DisplayHandler;
        public WizardController.VerifyStepCallback VerifyHandler;
        public WizardController.NextCallback NextHandler;
        public WizardController.NextCallback UndoNextHandler;
        public WizardController.BackCallback BackHandler;
        private WizardController wizard;

        /// <summary>
        /// Creates a new Wizard step.
        /// </summary>
        /// <param name="control">The control to display when this step is activated.</param>
        public WizardStep(UserControl control)
            : this(control, null, null, null, null, null, null)
        {
        }

        /// <summary>
        /// Creates a new Wizard step.
        /// </summary>
        /// <param name="header">The image and label header to assign this step</param>
        /// <param name="control">The control to display when this step is activated.</param>
        /// <param name="verify">A verification callback that will be triggered prior to advancing
        /// past this step. If this callback returns false, the step will not advance.</param>
        /// <param name="next">A callback that will be triggered after the step has been verified
        /// and just before advancing to the next step. Steps can use this callback to add additional
        /// substeps prior to moving past this step.</param>
        /// <param name="back">A callback that will be triggered when the Wizard is going back one step.
        /// WizardSteps can use this callback to rollback any stateful side effects that may have occurred
        /// as a result of previously executing this step.</param>
        public WizardStep(UserControl control, StringId? header, WizardController.DisplayCallback display, WizardController.VerifyStepCallback verify, WizardController.NextCallback next, WizardController.NextCallback undoNext, WizardController.BackCallback back)
        {
            Control = control;
            Header = header;
            DisplayHandler = display;
            VerifyHandler = verify;
            NextHandler = next;
            UndoNextHandler = undoNext;
            BackHandler = back;
        }

        /// <summary>
        /// A callback that will be triggered immediately prior to the display of the step
        /// </summary>
        /// <returns></returns>
        public virtual void Display()
        {
            if (DisplayHandler != null)
                DisplayHandler(Control);
        }

        /// <summary>
        /// A verification callback that will be triggered prior to advancing
        /// past this step. If this callback returns false, the step will not advance.
        /// </summary>
        /// <returns></returns>
        public bool Verify()
        {
            return VerifyHandler == null || VerifyHandler(Control);
        }

        /// <summary>
        /// A callback that will be triggered after the step has been verified
        /// and just before advancing to the next step. Steps can use this callback to add additional
        /// substeps prior to moving past this step.
        /// </summary>
        public virtual void Next()
        {
            if (NextHandler != null)
                NextHandler(Control);
        }

        /// <summary>
        /// A callback that will be triggered after the step has been verified
        /// and just before advancing to the next step. Steps can use this callback to add additional
        /// substeps prior to moving past this step.
        /// </summary>
        public virtual void UndoNext()
        {
            if (UndoNextHandler != null)
                UndoNextHandler(Control);
        }

        /// <summary>
        /// A callback that will be triggered when the Wizard is going back one step.
        /// WizardSteps can use this callback to rollback any stateful side effects that may have occurred
        /// as a result of previously executing this step.
        /// </summary>
        public virtual void Back()
        {
            if (BackHandler != null)
                BackHandler(Control);
        }

        public WizardController Wizard
        {
            get
            {
                return wizard;
            }
            set
            {
                wizard = value;
            }
        }

        private string nextButtonLabel;
        /// <summary>
        /// Sets the text on the Next Button.
        /// </summary>
        public string NextButtonLabel
        {
            set { nextButtonLabel = value; }
            get { return nextButtonLabel; }
        }

        private bool finishStep = false;
        private bool wantsFocus = true;

        /// <summary>
        /// Specifies whether this is a finish step or not.
        /// A finish step is given a special animation.
        /// </summary>
        public bool FinishStep
        {
            get { return finishStep; }
            set { finishStep = value; }
        }

        public bool WantsFocus
        {
            get { return wantsFocus; }
            set { wantsFocus = value; }
        }
    }

    /// <summary>
    /// A WizardSubStep is a temporary step that can be added on the fly based on input
    /// from other steps.  Substeps are always removed when the back button is pressed so
    /// that the previous step can have the option of taking a different logic branch that
    /// doesn't trigger the substep again.
    /// </summary>
    public class WizardSubStep : WizardStep
    {
        public WizardSubStep(UserControl control)
            : base(control)
        {
        }
        public WizardSubStep(UserControl control, StringId? header, WizardController.DisplayCallback display, WizardController.VerifyStepCallback verify, WizardController.NextCallback next, WizardController.NextCallback undoNext, WizardController.BackCallback back) :
            base(control, header, display, verify, next, undoNext, back)
        {
        }

        private WizardSubStep nextWizardSubStep;
        /// <summary>
        /// A next substep that will be automatically added once this step advances.
        /// This can be used to easily chaing substeps together without lots of Next() callbacks.
        /// </summary>
        public WizardSubStep NextWizardSubStep
        {
            get
            {
                return nextWizardSubStep;
            }
            set
            {
                nextWizardSubStep = value;
            }
        }

        public override void Next()
        {
            //if there is a next substep, add it now that the next button has been clicked
            if (NextWizardSubStep != null)
            {
                Wizard.addWizardSubStep(NextWizardSubStep);
            }
            base.Next();
        }

        public override void Back()
        {
            //substeps remove themselves since they are added based on input from
            //other steps.
            base.Back();
            Wizard.removeWizardStep(this);
        }
    }

    public class CancelNextException : Exception
    {
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using mshtml;
using OpenLiveWriter.Mshtml;

namespace OpenLiveWriter.HtmlEditor
{
    /// <summary>
    /// Implements services for tracking damage done to an HtmlEditorControl.
    /// </summary>
    public class HtmlEditorControlDamageServices : IHTMLEditorDamageServices, IDisposable
    {
        private HtmlEditorControl _editorControl;
        private MshtmlEditor _mshtmlControl;
        private WordRangeDamager wordRangeDamager;
        private DamageCommitStrategy _commitStrategy;

        //flag used to suppress extra selection change events that are triggered by the delete key
        private bool suppressSelectionChangeForDelete;

        public HtmlEditorControlDamageServices(HtmlEditorControl editorControl, MshtmlEditor mshtmlControl, DamageCommitStrategy commitStrategy)
        {
            _editorControl = editorControl;
            _mshtmlControl = mshtmlControl;
            wordRangeDamager = new WordRangeDamager(editorControl, mshtmlControl);

            _commitStrategy = commitStrategy;
            _commitStrategy.CommitDamage += new EventHandler(damageCommitStrategy_CommitDamage);
        }

        public void Dispose()
        {
            _commitStrategy.CommitDamage -= new EventHandler(damageCommitStrategy_CommitDamage);

            if (_trackDamage)
                StopTrackingDamage();

            wordRangeDamager.Dispose();
        }

        private bool _trackDamage;
        private bool DamageTrackingEnabled
        {
            get { return _trackDamage; }
            set
            {
                if (_trackDamage != value)
                {
                    _trackDamage = value;
                    if (_trackDamage)
                        StartTrackingDamage();
                    else
                        StopTrackingDamage();
                }
            }
        }

        public IDisposable CreateIgnoreDamage()
        {
            return new IgnoreDamage(this);
        }

        public class IgnoreDamage : IDisposable
        {
            public IgnoreDamage(HtmlEditorControlDamageServices services)
            {
                _services = services;
                _services.StopTrackingDamage();
            }
            private HtmlEditorControlDamageServices _services;

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                _services.StartTrackingDamage();
                Debug.Assert(disposing, "Failed to dispose IgnoreDamage");
            }
        }

        private void StartTrackingDamage()
        {
            _editorControl.DocumentEvents.KeyPress += new HtmlEventHandler(DocumentEvents_KeyPress);
            _editorControl.KeyDown += new OpenLiveWriter.Mshtml.HtmlEventHandler(_editorControl_KeyDown);
            _editorControl.SelectionChanged += new EventHandler(_editorControl_SelectionChanged);

            // Fake a selection change to start tracking damage.
            wordRangeDamager.OnSelectionChange();
        }

        private void StopTrackingDamage()
        {
            _editorControl.DocumentEvents.KeyPress -= new HtmlEventHandler(DocumentEvents_KeyPress);
            _editorControl.KeyDown -= new OpenLiveWriter.Mshtml.HtmlEventHandler(_editorControl_KeyDown);
            _editorControl.SelectionChanged -= new EventHandler(_editorControl_SelectionChanged);
        }

        public event DamageListener DamageOccured
        {
            add
            {
                wordRangeDamager.DamageOccured += value;
                DamageTrackingEnabled = true;
            }
            remove
            {
                wordRangeDamager.DamageOccured -= value;
                if (!wordRangeDamager.HasDamageHandlers)
                    DamageTrackingEnabled = false;
            }
        }

        private void _editorControl_KeyDown(object o, OpenLiveWriter.Mshtml.HtmlEventArgs e)
        {
            switch ((Keys)e.htmlEvt.keyCode)
            {
                case Keys.Back:
                    //Bug fix: The backspace key triggers an extra selection change event that causes word damage to be
                    //inappropriately committed, so we use a flag to suppress the selection change event for
                    //this case.
                    suppressSelectionChangeForDelete = true;
                    break;
            }

            wordRangeDamager.OnKeyDown(e);
            _commitStrategy.OnKeyDown(e);
        }

        private void _editorControl_SelectionChanged(object sender, EventArgs e)
        {
            //Trace.WriteLine("SelectionChanged");
            if (!suppressSelectionChangeForDelete)
            {
                //don't propagate if the delete key was just pressed.
                _commitStrategy.OnSelectionChange();
                wordRangeDamager.OnSelectionChange();
            }
            suppressSelectionChangeForDelete = false;
        }

        private void DocumentEvents_KeyPress(object o, HtmlEventArgs e)
        {

            wordRangeDamager.OnKeyPress(e);
            _commitStrategy.OnKeyPress(e);

        }

        public void AddDamage(MarkupRange range)
        {
            AddDamage(range, false);
        }

        public void AddDamage(MarkupRange range, bool includeAdjacentWords)
        {
            if (DamageTrackingEnabled)
                wordRangeDamager.AddDamage(range, includeAdjacentWords);
        }

        private void damageCommitStrategy_CommitDamage(object sender, EventArgs e)
        {
            wordRangeDamager.CommitCurrentDamage();
        }

        public void Reset()
        {
            if (DamageTrackingEnabled)
                wordRangeDamager.Reset();
        }

        public IDisposable CreateDeleteDamageTracker(MarkupRange range)
        {
            if (DamageTrackingEnabled)
            {
                //Bug fix: The delete key triggers an extra selection change event that causes word damage to be
                //inappropriately committed, so we use a flag to suppress the selection change event for
                //this case.
                suppressSelectionChangeForDelete = true;

                return new DeleteDamageTracker(this);
            }
            else
                return null;
        }

        public IDisposable CreateDamageTracker(MarkupRange range, bool includeAdjacentWords)
        {
            if (DamageTrackingEnabled)
                return new DamageTracker(this, range.Start, range.End, includeAdjacentWords);
            else
                return null;
        }

        public IDisposable CreateDamageTracker(MarkupPointer start, MarkupPointer end, bool includeAdjacentWords)
        {
            if (DamageTrackingEnabled)
                return new DamageTracker(this, start, end, includeAdjacentWords);
            else
                return null;
        }

        private class DamageTracker : IDisposable
        {
            private MarkupRange _damageRange;
            private HtmlEditorControlDamageServices _damageServices;
            public DamageTracker(HtmlEditorControlDamageServices damageServices, MarkupPointer start, MarkupPointer end, bool includeAdjacentWords)
            {
                _damageServices = damageServices;
                _damageRange = damageServices.wordRangeDamager.CreateDamageTrackingRange(start, end, includeAdjacentWords);
            }
            public void Dispose()
            {
                _damageServices.AddDamage(_damageRange);
                GC.SuppressFinalize(this);
            }

            ~DamageTracker()
            {
                Debug.Fail("DamageTracker was not disposed.");
            }
        }

        private class DeleteDamageTracker : IDisposable
        {
            private HtmlEditorControlDamageServices _damageServices;
            public DeleteDamageTracker(HtmlEditorControlDamageServices damageServices)
            {
                _damageServices = damageServices;
            }

            public void Dispose()
            {
                _damageServices.wordRangeDamager.OnDelete();
                _damageServices._commitStrategy.OnDelete();
                GC.SuppressFinalize(this);
            }

            ~DeleteDamageTracker()
            {
                Debug.Fail("DamageTracker was not disposed.");
            }
        }
    }

    public abstract class DamageCommitStrategy
    {
        /// <summary>
        /// Event fired to notify the damage service that queued damage should be committed.
        /// </summary>
        public event EventHandler CommitDamage;

        protected virtual void OnCommitDamage(EventArgs e)
        {
            if (CommitDamage != null)
                CommitDamage(this, e);
        }

        public abstract void OnKeyDown(HtmlEventArgs e);

        public abstract void OnKeyPress(HtmlEventArgs e);

        public abstract void OnSelectionChange();

        public abstract void OnDelete();

        public abstract void OnDamaged();
    }

    public interface IWordBasedEditor
    {
        bool IsSelectionMisspelled();
    }

    public class WordBasedDamageCommitStrategy : DamageCommitStrategy
    {
        private IWordBasedEditor _editor;
        public WordBasedDamageCommitStrategy(IWordBasedEditor editor)
        {
            _editor = editor;
        }

        public override void OnKeyDown(HtmlEventArgs e)
        {
        }

        public override void OnKeyPress(HtmlEventArgs e)
        {
            char ch = (char)e.htmlEvt.keyCode;
            if (!Char.IsLetterOrDigit(ch))
            {
                // Do not commit if we're backing in a correct region
                if ((Keys)e.htmlEvt.keyCode == Keys.Back && !_editor.IsSelectionMisspelled())
                    return;

                OnCommitDamage(EventArgs.Empty);
            }
        }

        public override void OnSelectionChange()
        {
            OnCommitDamage(EventArgs.Empty);
        }

        public override void OnDelete()
        {

        }

        public override void OnDamaged()
        {
        }
    }

    public class RealtimeDamageCommitStrategy : DamageCommitStrategy
    {
        public override void OnKeyDown(HtmlEventArgs e)
        {
            OnCommitDamage(EventArgs.Empty);
        }

        public override void OnKeyPress(HtmlEventArgs e)
        {
            OnCommitDamage(EventArgs.Empty);
        }

        public override void OnSelectionChange()
        {
            OnCommitDamage(EventArgs.Empty);
        }

        public override void OnDelete()
        {
            OnCommitDamage(EventArgs.Empty);
        }

        public override void OnDamaged()
        {
            OnCommitDamage(EventArgs.Empty);
        }
    }

    public class DamageTracer
    {
        private void DumpDamagedRegions(MarkupRange[] damagedRegions)
        {
            Trace.WriteLine("---Damaged Regions---");
            foreach (MarkupRange damage in damagedRegions)
            {
                string text = damage.Text;
                if (text != null)
                    Trace.WriteLine("  " + text.Replace('\r', ' ').Replace('\n', ' '));
            }
        }

        public void HandleDamageOccured(object source, DamageEvent evt)
        {
            DumpDamagedRegions(evt.DamageRegions);
        }
    }

    public class DamageEvent : EventArgs
    {
        private MarkupRange[] _damageRegions;
        public DamageEvent(MarkupRange[] damageRegions)
        {
            _damageRegions = damageRegions;
        }

        public MarkupRange[] DamageRegions
        {
            get
            {
                return _damageRegions;
            }
        }
    }
    public delegate void DamageListener(object source, DamageEvent evt);
    internal class WordRangeDamager : IDisposable
    {
        HtmlEditorControl _editorControl;
        MshtmlEditor _mshtmlEditor;
        MarkupRange _currentSelectionDamage;
        DamageRegionQueue damageQueue;
        bool damaged;
        MarkupServicesWordHelper _wordHelper;
        private bool _disposed = false;
        public WordRangeDamager(HtmlEditorControl editorControl, MshtmlEditor mshtmlControl)
        {
            _editorControl = editorControl;
            _mshtmlEditor = mshtmlControl;

            damageQueue = new DamageRegionQueue();
            Reset();
        }

        public void Dispose()
        {
            _disposed = true;
        }

        public void Reset()
        {
            damageQueue.Clear();
            _wordHelper = new MarkupServicesWordHelper(_mshtmlEditor.MshtmlControl.MarkupServices);
            _currentSelectionDamage = _mshtmlEditor.MshtmlControl.MarkupServices.CreateMarkupRange();
            _currentSelectionDamage.Start.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Left;
            _currentSelectionDamage.End.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Right;
        }

        public event DamageListener DamageOccured;
        private void OnDamageOccured(DamageEvent e)
        {
            if (DamageOccured != null)
                DamageOccured(_editorControl, e);
        }
        internal bool HasDamageHandlers
        {
            get { return DamageOccured != null; }
        }

        public void OnKeyDown(HtmlEventArgs e)
        {
            switch ((Keys)e.htmlEvt.keyCode)
            {
                case Keys.Back:
                    //Trace.WriteLine("KeyDown: " + ((Keys)e.htmlEvt.keyCode).ToString());
                    ExpandDamageToPreviousWord(_currentSelectionDamage);
                    OnDamage();
                    break;
            }

            if (CharIsWordSeparator(Convert.ToChar(e.htmlEvt.keyCode)))
                ExpandDamageToAdjacentWords(_currentSelectionDamage);
        }

        public void OnKeyPress(HtmlEventArgs e)
        {
            OnDamage();

            if (CharIsWordSeparator(Convert.ToChar(e.htmlEvt.keyCode)))
                ExpandDamageToAdjacentWords(_currentSelectionDamage);
        }

        public void OnDelete()
        {
            OnDamage();
        }

        private void OnDamage()
        {
            if (!damaged)
            {
                damaged = true;
            }
        }

        private bool CharIsWordSeparator(char c)
        {
            return !char.IsLetterOrDigit(c) && !char.IsControl(c);
        }

        private void ExpandDamageToAdjacentWords(MarkupRange damageRange)
        {
            ExpandDamageToPreviousWord(damageRange);
            ExpandDamageToNextWord(damageRange);
        }

        private void ExpandDamageToNextWord(MarkupRange damageRange)
        {
            MarkupRange nextWordRange = damageRange.Clone();
            nextWordRange.End.MoveUnit(_MOVEUNIT_ACTION.MOVEUNIT_NEXTWORDEND);

            // If we were already at the last word in the document, moving to the next word would have put the
            // MarkupPointer outside the body element.
            if (nextWordRange.End.GetParentElement(ElementFilters.BODY_ELEMENT) != null)
            {
                nextWordRange.Collapse(false);
                nextWordRange.Start.MoveUnit(_MOVEUNIT_ACTION.MOVEUNIT_PREVWORDBEGIN);
                if (!nextWordRange.IsEmpty() && !_editorControl.IgnoreRangeForSpellChecking(nextWordRange))
                {
                    damageRange.End.MoveToPointer(nextWordRange.End);
                }
            }
        }

        private void ExpandDamageToPreviousWord(MarkupRange damageRange)
        {
            MarkupRange previousWordRange = damageRange.Clone();
            previousWordRange.Start.MoveUnit(_MOVEUNIT_ACTION.MOVEUNIT_PREVWORDBEGIN);

            // If we were already at the first word in the document, moving to the previous word would have put the
            // MarkupPointer outside the body element.
            if (previousWordRange.Start.GetParentElement(ElementFilters.BODY_ELEMENT) != null)
            {
                previousWordRange.Collapse(true);
                previousWordRange.End.MoveUnit(_MOVEUNIT_ACTION.MOVEUNIT_NEXTWORDEND);
                if (!previousWordRange.IsEmpty() && !_editorControl.IgnoreRangeForSpellChecking(previousWordRange))
                {
                    damageRange.Start.MoveToPointer(previousWordRange.Start);
                }
            }
        }

        internal void CommitCurrentDamage()
        {
            if (damaged && _currentSelectionDamage.Positioned)
                _AddDamage(_currentSelectionDamage);
            AdjustCurrentWordRange(_editorControl.SelectedMarkupRange);
        }

        internal void AddDamage(MarkupRange range, bool includeAdjacentWords)
        {
            MarkupRange wordRange = range.Clone();
            _wordHelper.MoveToWordStart(wordRange.Start);
            _wordHelper.MoveToWordEnd(wordRange.End);
            if (includeAdjacentWords)
            {
                ExpandDamageToAdjacentWords(wordRange);
            }
            _AddDamage(wordRange);
        }

        private void _AddDamage(MarkupRange range)
        {
            lock (damageQueue)
            {
                damageQueue.EnqueueDamage(range);
                //Debug.WriteLine("Word Damage:" + _currentSelectionDamage.Text);

                _mshtmlEditor.BeginInvoke(new ThreadStart(FireDamageOccurred));
            }
        }

        internal MarkupRange CreateDamageTrackingRange(MarkupPointer start, MarkupPointer end, bool includeAdjecentWords)
        {
            MarkupRange range = _mshtmlEditor.MshtmlControl.MarkupServices.CreateMarkupRange();
            range.Start.MoveToPointer(start);
            range.Start.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Left;
            range.End.MoveToPointer(end);
            range.End.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Right;
            _wordHelper.MoveToWordStart(range.Start);
            _wordHelper.MoveToWordEnd(range.End);
            if (includeAdjecentWords)
            {
                ExpandDamageToAdjacentWords(range);
            }
            return range;
        }

        private void FireDamageOccurred()
        {
            if (!_disposed)
                OnDamageOccured(new DamageEvent(GetDamagedRegions()));
        }

        public void OnSelectionChange()
        {
            AdjustCurrentWordRange(_editorControl.SelectedMarkupRange);
        }

        private void AdjustCurrentWordRange(MarkupRange range)
        {
            if (damaged)
            {
                _currentSelectionDamage = _mshtmlEditor.MshtmlControl.MarkupServices.CreateMarkupRange();
                damaged = false;
            }

            _currentSelectionDamage.MoveToRange(range);

            //Trace.WriteLine("adjacent to word start: " + IsAdjacentToWordStart(range.Start));
            //Trace.WriteLine("adjacent to word end: " + IsAdjacentToWordEnd(range.End));
            _wordHelper.MoveToWordStart(_currentSelectionDamage.Start);
            _wordHelper.MoveToWordEnd(_currentSelectionDamage.End);
            //Trace.WriteLine("Current Word:" + _currentSelectionDamage.Text);
        }

        public MarkupRange[] GetDamagedRegions()
        {
            lock (damageQueue)
            {
                MarkupRange[] damage = damageQueue.DequeueDamage();

                //reset the current selection damage
                MarkupRange newSelectionDamage = _mshtmlEditor.MshtmlControl.MarkupServices.CreateMarkupRange();
                newSelectionDamage.MoveToRange(_currentSelectionDamage);
                _currentSelectionDamage = newSelectionDamage;
                damaged = false;
                return damage;
            }
        }

        private class DamageRegionQueue
        {
            ArrayList damagedRanges = new ArrayList();
            public void EnqueueDamage(MarkupRange range)
            {
                lock (damagedRanges)
                {
                    damagedRanges.Add(range);
                }
            }

            public MarkupRange[] DequeueDamage()
            {
                MarkupRange[] ranges;
                lock (damagedRanges)
                {
                    ranges = (MarkupRange[])damagedRanges.ToArray(typeof(MarkupRange));
                    damagedRanges.Clear();
                }

                //TODO: consider compressing damagedRanges
                return ranges;
            }

            public void Clear()
            {
                lock (damagedRanges)
                {
                    damagedRanges.Clear();
                }
            }
        }
    }

    public class MarkupServicesWordHelper
    {
        private MarkupPointer _p;
        private MarkupPointer _p2;
        private MshtmlMarkupServices MarkupServices;

        public MarkupServicesWordHelper(MshtmlMarkupServices markupServices)
        {
            MarkupServices = markupServices;
            _p = MarkupServices.CreateMarkupPointer();
            _p2 = MarkupServices.CreateMarkupPointer();
        }

        public void MoveToWordStart(MarkupPointer p)
        {
            lock (_p)
            {
                _p.MoveToPointer(p);
                _p2.MoveToPointer(p);
                _p.MoveUnit(_MOVEUNIT_ACTION.MOVEUNIT_PREVWORDEND);
                _p2.MoveUnit(_MOVEUNIT_ACTION.MOVEUNIT_PREVWORDBEGIN);
                if (_p2.IsRightOfOrEqualTo(_p))
                    p.MoveToPointer(_p2);
                //else, the pointer is already at the start of the current word
            }
        }

        public bool IsAdjacentToWordStart(MarkupPointer p)
        {
            lock (_p)
            {
                _p.MoveToPointer(p);
                _p.MoveUnit(_MOVEUNIT_ACTION.MOVEUNIT_PREVCHAR);
                _p.MoveUnit(_MOVEUNIT_ACTION.MOVEUNIT_NEXTWORDBEGIN);
                return _p.IsEqualTo(p);
            }
        }

        public void MoveToWordEnd(MarkupPointer p)
        {
            lock (_p)
            {
                _p.MoveToPointer(p);
                _p2.MoveToPointer(p);
                _p.MoveUnit(_MOVEUNIT_ACTION.MOVEUNIT_NEXTWORDBEGIN);
                _p2.MoveUnit(_MOVEUNIT_ACTION.MOVEUNIT_NEXTWORDEND);
                // If CurrentScope is null, this means we have walked off the end of the
                // document, in that case we don't want to move the pointer, at is already
                // at the end of word.
                if (_p2.IsLeftOfOrEqualTo(_p) && _p.CurrentScope != null)
                    p.MoveToPointer(_p2);
                //else, the pointer is already at the end of the current word
            }
        }

        public bool IsAdjacentToWordEnd(MarkupPointer p)
        {
            lock (_p)
            {
                _p.MoveToPointer(p);
                _p.MoveUnit(_MOVEUNIT_ACTION.MOVEUNIT_NEXTCHAR);
                _p.MoveUnit(_MOVEUNIT_ACTION.MOVEUNIT_PREVWORDEND);
                return _p.IsEqualTo(p);
            }
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using OpenLiveWriter.Mshtml;

namespace OpenLiveWriter.HtmlEditor.Undo
{
    internal class DamageTrackingOleUndoUnit : OleUndoUnit
    {
        private IHTMLEditorDamageServices _damageServices;

        public DamageTrackingOleUndoUnit(MshtmlMarkupServices markupServices, MarkupRange selection, IHTMLEditorDamageServices damageServices)
            : base(markupServices, selection)
        {
            _damageServices = damageServices;
        }

        protected override void HandleUndo()
        {
            AddDamage();
        }

        protected override void HandleRedo()
        {
            AddDamage();
        }

        protected void AddDamage()
        {
            if (_damageServices != null)
            {
                MarkupRange range = GetMarkupRange();
                if (range != null && range.Positioned)
                    _damageServices.AddDamage(range, true);
            }
        }
    }
}

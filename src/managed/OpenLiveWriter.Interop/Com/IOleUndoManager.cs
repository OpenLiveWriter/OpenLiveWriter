// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Text;
using System.Runtime.InteropServices;

namespace OpenLiveWriter.Interop.Com
{

    /// <summary>
    /// Version of IOleDataObject that preserves all of the raw signatures so that
    /// implementors can return the appropriate values
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("D001F200-EF97-11CE-9BC9-00AA00608E01")]
    public interface IOleUndoManager
    {
        void Open();
        void Close();
        void Add(IOleUndoUnit undoUnit);
        void GetOpenParentState();
        void DiscardFrom(IOleUndoUnit undoUnit);
        void UndoTo(IOleUndoUnit undoUnit);
        void RedoTo(IOleUndoUnit undoUnit);
        void EnumUndoable(out IEnumOleUndoUnits ppEnum);
        void EnumRedoable(out IEnumOleUndoUnits ppEnum);
        void GetLastUndoDescription();
        void GetLastRedoDescription();
        void Enable();

        //	virtual HRESULT STDMETHODCALLTYPE Open(
        //	/* [in] */ IOleParentUndoUnit *pPUU) = 0;
        //
        //	virtual HRESULT STDMETHODCALLTYPE Close(
        //	/* [in] */ IOleParentUndoUnit *pPUU,
        //	/* [in] */ BOOL fCommit) = 0;
        //
        //	virtual HRESULT STDMETHODCALLTYPE Add(
        //	/* [in] */ IOleUndoUnit *pUU) = 0;
        //
        //	virtual HRESULT STDMETHODCALLTYPE GetOpenParentState(
        //	/* [out] */ DWORD *pdwState) = 0;
        //
        //	virtual HRESULT STDMETHODCALLTYPE DiscardFrom(
        //	/* [in] */ IOleUndoUnit *pUU) = 0;
        //
        //	virtual HRESULT STDMETHODCALLTYPE UndoTo(
        //	/* [in] */ IOleUndoUnit *pUU) = 0;
        //
        //	virtual HRESULT STDMETHODCALLTYPE RedoTo(
        //	/* [in] */ IOleUndoUnit *pUU) = 0;
        //
        //	virtual HRESULT STDMETHODCALLTYPE EnumUndoable(
        //	/* [out] */ IEnumOleUndoUnits **ppEnum) = 0;
        //
        //	virtual HRESULT STDMETHODCALLTYPE EnumRedoable(
        //	/* [out] */ IEnumOleUndoUnits **ppEnum) = 0;
        //
        //	virtual HRESULT STDMETHODCALLTYPE GetLastUndoDescription(
        //	/* [out] */ BSTR *pBstr) = 0;
        //
        //	virtual HRESULT STDMETHODCALLTYPE GetLastRedoDescription(
        //	/* [out] */ BSTR *pBstr) = 0;
        //
        //	virtual HRESULT STDMETHODCALLTYPE Enable(
        //	/* [in] */ BOOL fEnable) = 0;

    }
}

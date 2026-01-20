// OLW DOM Bridge - Provides element tracking and DOM operations for WebView2 shim
// Elements are tracked by unique IDs assigned when they cross the JS/C# boundary

(function() {
    'use strict';
    
    const OLW = window.OLW = window.OLW || {};
    
    // Element registry - maps IDs to DOM elements
    const elementRegistry = new Map();
    let nextElementId = 1;
    
    // Get or assign an ID to an element
    function getElementId(element) {
        if (!element) return null;
        let id = element.getAttribute('data-olw-id');
        if (!id) {
            id = 'olw-' + (nextElementId++);
            element.setAttribute('data-olw-id', id);
            elementRegistry.set(id, element);
        }
        return id;
    }
    
    // Get element by ID
    function getElementById(id) {
        if (!id) return null;
        let el = elementRegistry.get(id);
        if (!el) {
            el = document.querySelector(`[data-olw-id="${id}"]`);
            if (el) elementRegistry.set(id, el);
        }
        return el;
    }
    
    // Wrap an element for return to C# (returns ID)
    function wrapElement(element) {
        if (!element) return null;
        return getElementId(element);
    }
    
    // Wrap multiple elements
    function wrapElements(elements) {
        if (!elements) return [];
        const result = [];
        for (let i = 0; i < elements.length; i++) {
            result.push(wrapElement(elements[i]));
        }
        return result;
    }
    
    // ===== Element Operations =====
    
    OLW.element = {
        // Get properties
        getInnerHTML: (id) => getElementById(id)?.innerHTML,
        getInnerText: (id) => getElementById(id)?.innerText,
        getOuterHTML: (id) => getElementById(id)?.outerHTML,
        getOuterText: (id) => getElementById(id)?.outerText,
        getClassName: (id) => getElementById(id)?.className,
        getId: (id) => getElementById(id)?.id,
        getTagName: (id) => getElementById(id)?.tagName,
        getTitle: (id) => getElementById(id)?.title,
        getLang: (id) => getElementById(id)?.lang,
        
        // Generic property getter - for any DOM property
        getProperty: (id, prop) => {
            const el = getElementById(id);
            if (!el) return null;
            const val = el[prop];
            // Return primitives directly, convert objects to string
            if (val === null || val === undefined) return null;
            if (typeof val === 'object') return String(val);
            return val;
        },
        
        // Set properties
        setInnerHTML: (id, value) => { const el = getElementById(id); if (el) el.innerHTML = value; },
        setInnerText: (id, value) => { const el = getElementById(id); if (el) el.innerText = value; },
        setOuterHTML: (id, value) => { const el = getElementById(id); if (el) el.outerHTML = value; },
        setOuterText: (id, value) => { const el = getElementById(id); if (el) el.outerText = value; },
        setClassName: (id, value) => { const el = getElementById(id); if (el) el.className = value; },
        setId: (id, value) => { const el = getElementById(id); if (el) el.id = value; },
        setTitle: (id, value) => { const el = getElementById(id); if (el) el.title = value; },
        setLang: (id, value) => { const el = getElementById(id); if (el) el.lang = value; },
        
        // Offset/size properties
        getOffsetLeft: (id) => getElementById(id)?.offsetLeft ?? 0,
        getOffsetTop: (id) => getElementById(id)?.offsetTop ?? 0,
        getOffsetWidth: (id) => getElementById(id)?.offsetWidth ?? 0,
        getOffsetHeight: (id) => getElementById(id)?.offsetHeight ?? 0,
        getOffsetParent: (id) => wrapElement(getElementById(id)?.offsetParent),
        getSourceIndex: (id) => {
            const el = getElementById(id);
            if (!el) return -1;
            const all = document.getElementsByTagName('*');
            for (let i = 0; i < all.length; i++) {
                if (all[i] === el) return i;
            }
            return -1;
        },
        
        // Navigation
        getParentElement: (id) => wrapElement(getElementById(id)?.parentElement),
        getChildren: (id) => wrapElements(getElementById(id)?.children),
        getAllDescendants: (id) => wrapElements(getElementById(id)?.getElementsByTagName('*')),
        
        // Attributes
        getAttribute: (id, name) => getElementById(id)?.getAttribute(name),
        setAttribute: (id, name, value) => getElementById(id)?.setAttribute(name, value),
        removeAttribute: (id, name) => getElementById(id)?.removeAttribute(name) ?? false,
        
        // Methods
        click: (id) => getElementById(id)?.click(),
        scrollIntoView: (id, alignTop) => getElementById(id)?.scrollIntoView(alignTop !== false),
        contains: (id, childId) => getElementById(id)?.contains(getElementById(childId)) ?? false,
        insertAdjacentHTML: (id, position, html) => getElementById(id)?.insertAdjacentHTML(position, html),
        insertAdjacentText: (id, position, text) => getElementById(id)?.insertAdjacentText(position, text),
        
        // Style
        getStyleProperty: (id, prop) => getElementById(id)?.style[prop],
        setStyleProperty: (id, prop, value) => { 
            const el = getElementById(id); 
            if (el) el.style[prop] = value; 
        },
        getComputedStyle: (id, prop) => {
            const el = getElementById(id);
            if (!el) return null;
            return window.getComputedStyle(el)[prop];
        },
        
        // Check if element is text editable
        isTextEdit: (id) => {
            const el = getElementById(id);
            if (!el) return false;
            return el.isContentEditable || el.tagName === 'INPUT' || el.tagName === 'TEXTAREA';
        }
    };
    
    // ===== Document Operations =====
    
    OLW.document = {
        getBody: () => wrapElement(document.body),
        getDocumentElement: () => wrapElement(document.documentElement),
        getActiveElement: () => wrapElement(document.activeElement),
        
        getElementById: (id) => wrapElement(document.getElementById(id)),
        getElementsByTagName: (tagName) => wrapElements(document.getElementsByTagName(tagName)),
        getElementsByClassName: (className) => wrapElements(document.getElementsByClassName(className)),
        querySelector: (selector) => wrapElement(document.querySelector(selector)),
        querySelectorAll: (selector) => wrapElements(document.querySelectorAll(selector)),
        
        createElement: (tagName) => wrapElement(document.createElement(tagName)),
        createTextNode: (text) => {
            // Text nodes need special handling - wrap in a span for tracking
            const span = document.createElement('span');
            span.appendChild(document.createTextNode(text));
            return wrapElement(span);
        },
        
        getReadyState: () => document.readyState,
        getTitle: () => document.title,
        setTitle: (value) => { document.title = value; },
        getURL: () => document.URL,
        
        elementFromPoint: (x, y) => wrapElement(document.elementFromPoint(x, y)),
        
        // execCommand for formatting
        execCommand: (cmd, showUI, value) => document.execCommand(cmd, showUI, value),
        queryCommandState: (cmd) => document.queryCommandState(cmd),
        queryCommandValue: (cmd) => document.queryCommandValue(cmd),
        queryCommandEnabled: (cmd) => document.queryCommandEnabled(cmd),
        
        // HTML content
        write: (html) => document.write(html),
        writeln: (html) => document.writeln(html)
    };
    
    // ===== Control Selection (for images, tables, etc.) =====
    // MSHTML had a "Control" selection type for things like images
    // We simulate this with a tracked selectedControl element
    
    let selectedControl = null;
    const CONTROL_CLASS = 'olw-control-selected';
    
    // Add CSS for control selection if not present
    function ensureControlSelectionStyles() {
        if (document.getElementById('olw-control-styles')) return;
        const style = document.createElement('style');
        style.id = 'olw-control-styles';
        // Keep it simple - images are replaced elements and don't support ::before/::after
        style.textContent = `
            .${CONTROL_CLASS} {
                outline: 2px solid #0078d7 !important;
                outline-offset: 2px !important;
                box-shadow: 0 0 0 4px rgba(0, 120, 215, 0.2) !important;
            }
        `;
        document.head.appendChild(style);
    }
    
    function selectControl(element) {
        console.log('[OLW-JS] selectControl called with:', element?.tagName);
        clearControlSelection();
        if (element) {
            selectedControl = element;
            element.classList.add(CONTROL_CLASS);
            ensureControlSelectionStyles();
            console.log('[OLW-JS] Control selected, posting message');
            // Notify C# about control selection - use setTimeout to avoid blocking the click event
            if (window.chrome?.webview?.postMessage) {
                setTimeout(() => {
                    window.chrome.webview.postMessage(JSON.stringify({ 
                        type: 'controlSelected', 
                        tagName: element.tagName,
                        id: getElementId(element)
                    }));
                }, 0);
            } else {
                console.log('[OLW-JS] No postMessage available');
            }
        }
    }
    
    function clearControlSelection() {
        if (selectedControl) {
            selectedControl.classList.remove(CONTROL_CLASS);
            selectedControl = null;
            // Notify C# that control selection cleared - use setTimeout to avoid blocking
            if (window.chrome?.webview?.postMessage) {
                setTimeout(() => {
                    window.chrome.webview.postMessage(JSON.stringify({ type: 'controlCleared' }));
                }, 0);
            }
        }
    }
    
    // ===== Selection Operations =====
    
    OLW.selection = {
        getType: () => {
            // Check for control selection first (images, tables)
            if (selectedControl) return 'Control';
            const sel = window.getSelection();
            if (!sel || sel.rangeCount === 0) return 'None';
            if (sel.isCollapsed) return 'Caret';
            return 'Text';
        },
        
        getText: () => window.getSelection()?.toString() ?? '',
        
        getHtml: () => {
            const sel = window.getSelection();
            if (!sel || sel.rangeCount === 0) return '';
            const range = sel.getRangeAt(0);
            const div = document.createElement('div');
            div.appendChild(range.cloneContents());
            return div.innerHTML;
        },
        
        getAnchorElement: () => {
            const sel = window.getSelection();
            if (!sel || !sel.anchorNode) return null;
            const node = sel.anchorNode.nodeType === 1 ? sel.anchorNode : sel.anchorNode.parentElement;
            return wrapElement(node);
        },
        
        getFocusElement: () => {
            const sel = window.getSelection();
            if (!sel || !sel.focusNode) return null;
            const node = sel.focusNode.nodeType === 1 ? sel.focusNode : sel.focusNode.parentElement;
            return wrapElement(node);
        },
        
        collapse: (toStart) => {
            const sel = window.getSelection();
            if (sel && sel.rangeCount > 0) {
                if (toStart) sel.collapseToStart();
                else sel.collapseToEnd();
            }
        },
        
        selectAll: () => document.execCommand('selectAll'),
        
        clear: () => {
            clearControlSelection();
            window.getSelection()?.removeAllRanges();
        },
        
        // Select an element's contents
        selectElement: (id) => {
            const el = getElementById(id);
            if (!el) return;
            const range = document.createRange();
            range.selectNodeContents(el);
            const sel = window.getSelection();
            sel.removeAllRanges();
            sel.addRange(range);
        },
        
        // Control selection API (for images, tables, etc.)
        selectControl: (id) => {
            const el = getElementById(id);
            if (el) selectControl(el);
        },
        
        clearControl: () => clearControlSelection(),
        
        getSelectedControl: () => wrapElement(selectedControl),
        
        hasControlSelection: () => selectedControl !== null,
        
        // Get detailed info about the currently selected control
        getSelectedControlInfo: () => {
            if (!selectedControl) return null;
            return {
                tagName: selectedControl.tagName,
                editorId: getElementId(selectedControl),
                id: selectedControl.id || null,
                src: selectedControl.src || null,
                alt: selectedControl.alt || null,
                width: selectedControl.width || selectedControl.offsetWidth || 0,
                height: selectedControl.height || selectedControl.offsetHeight || 0
            };
        }
    };
    
    // ===== Control Selection Event Setup =====
    // Set up click handlers to select images/controls when clicked
    
    let controlSelectionSetup = false;
    
    OLW.setupControlSelection = () => {
        // Only setup once to avoid duplicate listeners
        if (controlSelectionSetup) return 'already setup';
        
        ensureControlSelectionStyles();
        
        const bodyEl = document.getElementById('olw-body');
        if (!bodyEl) return 'no body element';
        
        controlSelectionSetup = true;
        
        // Click handler for images
        bodyEl.addEventListener('click', (e) => {
            const target = e.target;
            console.log('[OLW-JS] Body click, target:', target.tagName, target);
            
            // Check if clicking on an image
            if (target.tagName === 'IMG') {
                console.log('[OLW-JS] IMG clicked, selecting control');
                e.preventDefault();
                e.stopPropagation();
                selectControl(target);
                return;
            }
            
            // Check if clicking on a table (directly on TD/TH/TABLE)
            if (target.tagName === 'TABLE' || target.tagName === 'TD' || target.tagName === 'TH') {
                // For tables, only select on border/edge clicks - let editing happen inside cells
                // This is complex, so for now just allow text editing in tables
            }
            
            // Clicking elsewhere clears control selection
            if (selectedControl && target !== selectedControl) {
                clearControlSelection();
            }
        });
        
        // Double-click on image opens properties
        bodyEl.addEventListener('dblclick', (e) => {
            console.log('[OLW-JS] Double-click on:', e.target.tagName, 'selectedControl:', selectedControl?.tagName);
            if (e.target.tagName === 'IMG') {
                // Select the image first if not already selected
                if (selectedControl !== e.target) {
                    selectControl(e.target);
                }
                console.log('[OLW-JS] Posting controlDoubleClick');
                if (window.chrome?.webview?.postMessage) {
                    setTimeout(() => {
                        window.chrome.webview.postMessage(JSON.stringify({ 
                            type: 'controlDoubleClick',
                            tagName: e.target.tagName,
                            id: getElementId(e.target)
                        }));
                    }, 0);
                }
            }
        });
        
        // Delete key removes selected control
        document.addEventListener('keydown', (e) => {
            if (selectedControl && (e.key === 'Delete' || e.key === 'Backspace')) {
                e.preventDefault();
                selectedControl.remove();
                clearControlSelection();
                // Mark dirty
                if (window.chrome?.webview?.hostObjects?.sync?.olw) {
                    window.chrome.webview.hostObjects.sync.olw.MarkDirty();
                }
            }
        });
        
        return 'control selection setup ok';
    };
    
    // ===== TextRange-like Operations =====
    // Simulates IHTMLTxtRange functionality using modern Range API
    
    const textRanges = new Map();
    let nextRangeId = 1;
    
    OLW.textRange = {
        create: () => {
            const range = document.createRange();
            const id = 'range-' + (nextRangeId++);
            textRanges.set(id, range);
            return id;
        },
        
        createFromSelection: () => {
            const sel = window.getSelection();
            if (!sel || sel.rangeCount === 0) return null;
            const range = sel.getRangeAt(0).cloneRange();
            const id = 'range-' + (nextRangeId++);
            textRanges.set(id, range);
            return id;
        },
        
        dispose: (id) => textRanges.delete(id),
        
        getText: (id) => textRanges.get(id)?.toString() ?? '',
        setText: (id, text) => {
            const range = textRanges.get(id);
            if (range) {
                range.deleteContents();
                range.insertNode(document.createTextNode(text));
            }
        },
        
        getHtml: (id) => {
            const range = textRanges.get(id);
            if (!range) return '';
            const div = document.createElement('div');
            div.appendChild(range.cloneContents());
            return div.innerHTML;
        },
        
        setHtml: (id, html) => {
            const range = textRanges.get(id);
            if (!range) return;
            range.deleteContents();
            const temp = document.createElement('div');
            temp.innerHTML = html;
            const frag = document.createDocumentFragment();
            while (temp.firstChild) {
                frag.appendChild(temp.firstChild);
            }
            range.insertNode(frag);
        },
        
        select: (id) => {
            const range = textRanges.get(id);
            if (!range) return;
            const sel = window.getSelection();
            sel.removeAllRanges();
            sel.addRange(range);
        },
        
        collapse: (id, toStart) => {
            const range = textRanges.get(id);
            if (range) range.collapse(toStart !== false);
        },
        
        moveToElement: (id, elementId) => {
            const range = textRanges.get(id);
            const el = getElementById(elementId);
            if (range && el) range.selectNodeContents(el);
        },
        
        getParentElement: (id) => {
            const range = textRanges.get(id);
            if (!range) return null;
            return wrapElement(range.commonAncestorContainer.nodeType === 1 
                ? range.commonAncestorContainer 
                : range.commonAncestorContainer.parentElement);
        },
        
        execCommand: (id, cmd, value) => {
            const range = textRanges.get(id);
            if (!range) return false;
            // Select the range, execute command, then restore
            const sel = window.getSelection();
            const oldRanges = [];
            for (let i = 0; i < sel.rangeCount; i++) {
                oldRanges.push(sel.getRangeAt(i).cloneRange());
            }
            sel.removeAllRanges();
            sel.addRange(range);
            const result = document.execCommand(cmd, false, value);
            sel.removeAllRanges();
            oldRanges.forEach(r => sel.addRange(r));
            return result;
        }
    };
    
    // ===== Utility =====
    
    OLW.util = {
        // Clean up orphaned element references
        gc: () => {
            const toDelete = [];
            elementRegistry.forEach((el, id) => {
                if (!document.body.contains(el)) {
                    toDelete.push(id);
                }
            });
            toDelete.forEach(id => elementRegistry.delete(id));
            return toDelete.length;
        },
        
        // Get element by OLW ID (for debugging)
        getById: (id) => getElementById(id)
    };
    
    console.log('[OLW] DOM Bridge loaded');
})();

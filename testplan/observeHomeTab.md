###Test Plan for Ensuring Home Tab displays correctly
Steps                  | Desired Results                | Complete | Comments
--------------------------|--------------------------------------------|----------| --------
Open Live Writer  | Open Live Writer Opens at Home Tab with last blog used selected in Publish area  |  |
 **Clipboard** | | | 
Hover on Clipboard | Nothing happens | | 
Click on Clipboard | Nothing happens |  |  
Clipboard is empty | Paste is greyed out | |  
 **Paste** |  |  | 
Hover on Paste | Message explaining paste appears | |  
Click on Paste | Ensure Displays box allowing user to select paste or paste special |  | 
Click on Paste then Paste | Ensure Places contents of the clipboard where the cursor is |  | 
 | If text is in the clipboard, text is inserted, if image is in the clipboard, then image is inserted | |  
Click on Paste then Paste special - image is in clipboard | Error message about using text appears | | 
Click on Paste then Paste special - text is in clipboard | Paste Special dialog box appears | |
Paste text from clipboard at cursor (greyed out in nothing in clipboard) |  |
 **Cut** | | | 
No text selected | Cut is greyed out | | 
Hover on Cut | Ensure Message explaining cut appears | | 
Click on Cut | Ensure Selected content is copied to clipboard and deleted  | | 
 **Copy** | | | 
No text selected | Copy is greyed out | |  
Hover on Copy | Message explaining copy appears | | 
Click on Copy | Ensure Selected content is copied to clipboard | | 
**Publish** |   |   | 
Hover over Publish Icon | Message explaining Publish appears | | 
Click on Publish Icon | Ensure If password is not saved, sign-in dialog box appears  | | 
 | If title is not present, title reminder dialog box appears | | 
 | Once all conditions are satisfied, publish dialog box appears | |
 | Blog is published | |  | 
 Blog account | Ensure Last account appears with down arrow if multiple blogs | | 
 Hover over Blog Account  | Message explaining Blog Account appears | | 
 Click on Blog Account | Ensure List of blog accounts appears and can be chosen | | 
 Hover over Publish Draft Icon | Message explaining Publish Draft appears | | 
Click on Publish Draft Icon | Ensure If password is not saved, sign-in dialog box appears  | | 
 | If title is not present, title reminder dialog box appears | | 
 | Once all conditions are satisfied, publish dialog box appears | |
 | Blog is published in draft status | |  |
**Font** | | | 
Hover over the first large box | Message explaining font family appears | |
Click on the arrow to the right of the first large box | List of available fonts appears | | 
Click on a font family | Ensure that font family is applied | | 
Hover over the second smaller box | Message explaining font size appears | |
Click on the arrow to the right of the second smaller box | List of available fonts appears | |
Click on a font size | Ensure that font size is applied | | 
Hover over icon eraser over Aa | Message explaining clear formatting appears | |
Click on icon eraser over Aa | Ensure that formatting is cleared |  |  
Hover over bold B | Message explaining bold appears | | 
Click on bold B | Ensure that text is formatted bold | | 
Hover over italic I | Message explainng italic appears | | 
Click on italic I | Ensure that text is formatted as italics | |
Hover over underline U | Message explainng underline appears | | 
Click on underline U | Ensure that text is formatted underlined | | 
Hover over strikeover abc | Message explainng strikeover appears | | 
Click on strikeover abc | Ensure that text is formatted strikeover | |
Hover over subscript | Message explainng subscript appears | | 
Click on subscript | Ensure that text is formatted subscript | |
Hover over superscript | Message explainng superscript appears | | 
Click on superscript | Ensure that text is formatted superscript | |
Hover over highlight | Message explainng highlight appears | | 
Click on highlight | Ensure that text is highlighted | |
Hover over text color | Message explainng text color appears | | 
Click on text colr  | Ensure that text is formatted the correct color | |
**Paragraph** | | |
Hover over Bulleted List | Message explaining Bulleted List is displayed | |
Click on Bulleted List | Ensure that a bulleted list displays and formatting applied | |
Hover over Numbered List | Message explaining Numbered List displayed | |
Click on Numbered List | Ensure that a numbered list | |
Hover over Block Quote | Message explaining Block Quote displayed | |
Click on Block Quote | Ensure that text is formatted block quote  | |
Hover over Left Justify | Message explaining Left Justify displayed | |
Click on Left Justify | Ensure that text is formatted left justified | |
Hover over Center Justify | Message explaining Center Justify displayed | |
Click on Center Justify | Ensure that text is formatted center justified | |
Hover over Right Justify | Message explaining Right Justify displayed | |
Click on Right Justify | Ensure that text is formatted right justified | |
Hover over Full Justify | Message explaining Full Justify displayed | |
Click on Full Justify | Ensure that text is formatted full justified  | |
**HTML Styles** | | | 
Hover over Paragraph | Nothing | |
Click on Paragraph | Ensure that text is formatted paragraph | |
Hover over Heading 1 | Nothing | |
Click on Heading 1 | Ensure that text is formatted h1 | |
Hover over Heading 2  | Nothing | |
Click on Heading 2 | Ensure that text is formatted h2 | |
Hover over Heading 3  | Nothing | |
Click on Heading 3 | Ensure that text is formatted h3 | |
Hover over Heading 4  | Nothing | |
Click on Heading 4 | Ensure that text is formatted h4 | |
Hover over Heading 5  | Nothing | |
Click on Heading 5 | Ensure that text is formatted h5 | |
Hover over Heading 6  | Nothing | |
Click on Heading 6 | Ensure that text is formatted h6 | |
**Insert** | | |
Hover over Hyperlink | Message explaining Hyperlink displayed | |
Click on Hyperlink | Ensure Insert Hyperlink Dialog box is displayed | |
Hover over Picture | Message explaining Picture displayed | |
Click on Picture | Ensure Two choices are displayed: From your computer or From the web | |
Click on From your computer | Ensure Insert Picture dialog box is displayed | |
Click on From the web | Ensure Insure Web Image dialog box is displayed | |
Hover over Video | Message explaining Video displayed | |
Click on Video | Ensure Three choices are displayed: From the web or From your computer or From video service  | |
Click on From the web | Ensure Insert Video dialog box is displayed with From the Web tab selected | |
Click on From your computer | Ensure Insert Video dialog box is displayed with From File tab selected | |
Click on From the web | Ensure Insert Video dialog box is displayed with From Video Service tab selected | |
**Editing** | | | 
Hover over Spelling  | Message explaining Spelling  displayed | |
Spelling - currently not implemented and greyed out - work in progress | | |  
Hover over Word Count  | Message explaining Word Count displayed | |
Click on Word Count | Ensure Word Count dialog box is displayed with proper statitics see below | | 
Click on close | Ensure Word count dialog box closes | |
Hover over Find  | Message explaining Find displayed | |
Click on Find | Ensure Find dialog box is displayed | |
Hover over Select All  | Message explaining Select All displayed | |
Click on Select All | Ensure all contents in the blog post are selected | |
**Home post display** | | |
Open Live Writer  | Ensure empty blog post is displayed | |
Click on Preview | Ensure empty blog post is displayed as if the blog post has been published | | 
Click on source | Ensure blog post is displayed as html | | 
Check lower left corner | Ensure Draft - Unsaved is displayed | |  

![Word count dialog box](images\wordCountdiaglogbox.png)
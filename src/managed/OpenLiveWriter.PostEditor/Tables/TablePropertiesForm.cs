// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;
using System.Windows.Forms;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Layout;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.Tables
{

    public partial class TablePropertiesForm : ApplicationDialog
    {
        
        public TablePropertiesForm()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            this.buttonOK.Text = Res.Get(StringId.InsertButtonText);
            this.buttonOK.Name = "buttonInsert"; //  This is used by automation, don't change it!
            this.buttonCancel.Text = Res.Get(StringId.CancelButton);
            this.groupBoxSize.Text = Res.Get(StringId.Size);
            this.labelRows.Text = Res.Get(StringId.TableRowsLabel);
            this.labelColumns.Text = Res.Get(StringId.TableColumnsLabel);
            this.groupBoxAppearance.Text = Res.Get(StringId.Appearance);
            this.labelSpacingPixels.Text = Res.Get(StringId.Pixels);
            this.labelPaddingPixels.Text = Res.Get(StringId.Pixels);
            this.labelBorderPixels.Text = Res.Get(StringId.Pixels);
            this.checkBoxShowBorder.Text = Res.Get(StringId.TableShowBorderLabel);
            this.label3.Text = Res.Get(StringId.TableCellSpacingLabel);
            this.label1.Text = Res.Get(StringId.TableCellPaddingLabel);
            this.Text = Res.Get(StringId.InsertTable);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            using (new AutoGrow(this, AnchorStyles.Right | AnchorStyles.Bottom, false))
            {
                // Fix up the Size group box
                using (new AutoGrow(groupBoxSize, AnchorStyles.Right, false))
                {
                    using (new AutoGrow(panelRowsAndColumns, AnchorStyles.Right, false))
                    {
                        DisplayHelper.AutoFitSystemLabel(labelRows, 0, int.MaxValue);
                        DisplayHelper.AutoFitSystemLabel(labelColumns, 0, int.MaxValue);
                        LayoutHelper.DistributeHorizontally(8, labelRows, numericTextBoxRows);
                        LayoutHelper.DistributeHorizontally(8, labelColumns, numericTextBoxColumns);
                        LayoutHelper.DistributeHorizontally(16,
                            new ControlGroup(labelRows, numericTextBoxRows),
                            new ControlGroup(labelColumns, numericTextBoxColumns));
                    }
                }

                // Fix up the Appearance group box
                using (new AutoGrow(groupBoxAppearance, AnchorStyles.Right | AnchorStyles.Bottom, false))
                {
                    DisplayHelper.AutoFitSystemCheckBox(checkBoxShowBorder, 0, int.MaxValue);
                    DisplayHelper.AutoFitSystemLabel(label1, 0, int.MaxValue);
                    DisplayHelper.AutoFitSystemLabel(label3, 0, int.MaxValue);
                    DisplayHelper.AutoFitSystemLabel(labelBorderPixels, 0, int.MaxValue);
                    DisplayHelper.AutoFitSystemLabel(labelPaddingPixels, 0, int.MaxValue);
                    DisplayHelper.AutoFitSystemLabel(labelSpacingPixels, 0, int.MaxValue);
                    LayoutHelper.DistributeHorizontally(
                        8,
                        new ControlGroup(checkBoxShowBorder, label1, label3),
                        new ControlGroup(numericTextBoxBorder, numericTextBoxCellPadding, numericTextBoxCellSpacing),
                        new ControlGroup(labelBorderPixels, labelPaddingPixels, labelSpacingPixels)
                        );
                }

                // Make the group boxes the same width
                int maxWidth = Math.Max(groupBoxAppearance.Width, groupBoxSize.Width);
                groupBoxAppearance.Width = maxWidth;
                groupBoxSize.Width = maxWidth;
            }

            // Align the OK/Cancel
            ControlGroup okCancelGroup = new ControlGroup(buttonOK, buttonCancel);
            okCancelGroup.Left = groupBoxAppearance.Right - okCancelGroup.Width;
            LayoutHelper.FixupOKCancel(buttonOK, buttonCancel);
        }

        public TableCreationParameters CreateTable(IWin32Window owner)
        {
            // populate the form
            TableCreationParameters creationParameters = CreateDefaultParameters();
            numericTextBoxRows.Text = creationParameters.Rows.ToString(CultureInfo.CurrentCulture);
            numericTextBoxColumns.Text = creationParameters.Columns.ToString(CultureInfo.CurrentCulture);

            var width = creationParameters.Properties.Width;
            columnWidthControl.Text = width.ToString(CultureInfo.CurrentCulture);
            InitializeFormProperties(creationParameters.Properties);

            // show the dialog
            if (ShowDialog(owner) == DialogResult.OK)
            {
                // read input
                TableCreationParameters parameters = new TableCreationParameters(
                    int.Parse(numericTextBoxRows.Text, CultureInfo.CurrentCulture),
                    int.Parse(numericTextBoxColumns.Text, CultureInfo.CurrentCulture),
                    ReadFormProperties());

                // save as default for new tables
                SaveDefaultTableCreationParameters(parameters);

                // return
                return parameters;
            }
            else
            {
                return null;
            }
        }

        public TableProperties EditTable(IWin32Window owner, TableProperties properties)
        {
            // update UI for edit scenario
            Text = Res.Get(StringId.TableTableProperties);
            buttonOK.Text = Res.Get(StringId.OKButtonText);
            int verticalSpaceLoss = columnWidthControl.Top - panelRowsAndColumns.Top;
            panelRowsAndColumns.Visible = false;
            columnWidthControl.Top = panelRowsAndColumns.Top;
            groupBoxSize.Height -= verticalSpaceLoss;
            groupBoxAppearance.Top -= verticalSpaceLoss;
            Height -= verticalSpaceLoss;

            // populate the form
            InitializeFormProperties(properties);

            // show the dialog
            if (ShowDialog(owner) == DialogResult.OK)
            {
                return ReadFormProperties();
            }
            else
            {
                return null;
            }
        }


        private void InitializeFormProperties(TableProperties properties)
        {
            BorderSize = properties.BorderSize;
            numericTextBoxCellPadding.Text = properties.CellPadding;
            numericTextBoxCellSpacing.Text = properties.CellSpacing;
            columnWidthControl.ColumnWidth = properties.Width;

        }

        private TableProperties ReadFormProperties()
        {
            TableProperties properties = new TableProperties();
            properties.CellPadding = numericTextBoxCellPadding.Text.Trim();
            properties.CellSpacing = numericTextBoxCellSpacing.Text.Trim();
            properties.BorderSize = BorderSize;
            properties.Width = columnWidthControl.ColumnWidth;
            return properties;
        }

        private readonly string ZERO = 0.ToString(CultureInfo.CurrentCulture);
        private readonly string ONE = 1.ToString(CultureInfo.CurrentCulture);

        private string BorderSize
        {
            get
            {
                if (checkBoxShowBorder.Checked && numericTextBoxBorder.Text.Trim() != String.Empty)
                    return numericTextBoxBorder.Text;
                else
                    return ZERO;
            }
            set
            {
                if (value != String.Empty && value != ZERO)
                {
                    checkBoxShowBorder.Checked = true;
                    numericTextBoxBorder.Text = value;
                }
                else
                {
                    checkBoxShowBorder.Checked = false;
                    numericTextBoxBorder.Text = String.Empty;
                }

                ManageUIState();
            }
        }

        private void buttonOK_Click(object sender, System.EventArgs e)
        {
            if (ValidateInput())
            {
                DialogResult = DialogResult.OK;
            }
        }

        private void checkBoxShowBorder_CheckedChanged(object sender, System.EventArgs e)
        {
            ManageUIState();
        }

        private void ManageUIState()
        {
            numericTextBoxBorder.Enabled = checkBoxShowBorder.Checked;

            if (checkBoxShowBorder.Checked)
            {
                if (BorderSize == ZERO)
                    numericTextBoxBorder.Text = ONE;
            }
        }

        private bool ValidateInput()
        {
            // only validate row and column if they are visible
            if (panelRowsAndColumns.Visible)
            {
                if (!ValidateTextBoxInteger(Res.Get(StringId.TableRows), numericTextBoxRows, 750))
                    return false;

                if (!ValidateTextBoxPositiveInteger(Res.Get(StringId.TableRows), numericTextBoxRows))
                    return false;

                if (!ValidateTextBoxInteger(Res.Get(StringId.TableColumns), numericTextBoxColumns, 100))
                    return false;

                if (!ValidateTextBoxPositiveInteger(Res.Get(StringId.TableColumns), numericTextBoxColumns))
                    return false;
            }

            if (!columnWidthControl.ValidateInput(1000))
                return false;

            if (checkBoxShowBorder.Checked)
            {
                if (!ValidateTextBoxInteger(Res.Get(StringId.TableBorder), numericTextBoxBorder, 100))
                    return false;

                if (!ValidateTextBoxPositiveInteger(Res.Get(StringId.TableBorder), numericTextBoxBorder))
                    return false;
            }

            string cellPadding = numericTextBoxCellPadding.Text.Trim();
            if (cellPadding != String.Empty)
            {
                if (!ValidateTextBoxInteger(Res.Get(StringId.TableCellPadding), numericTextBoxCellPadding, 100))
                    return false;

                if (!ValidateTextBoxGreaterThanZero(Res.Get(StringId.TableCellPadding), numericTextBoxCellPadding))
                    return false;
            }

            string cellSpacing = numericTextBoxCellSpacing.Text.Trim();
            if (cellSpacing != String.Empty)
            {
                if (!ValidateTextBoxInteger(Res.Get(StringId.TableCellSpacing), numericTextBoxCellSpacing, 100))
                    return false;

                if (!ValidateTextBoxGreaterThanZero(Res.Get(StringId.TableCellSpacing), numericTextBoxCellSpacing))
                    return false;

            }

            // got this far, we are ok
            return true;
        }

        private bool ValidateTextBoxInteger(string name, TextBox textBox, int maxValue)
        {
            string textBoxValue = textBox.Text.Trim();
            if (textBoxValue == String.Empty || !SafeCheckForInt(textBox))
            {
                DisplayMessage.Show(MessageId.UnspecifiedValue, this, name);
                textBox.Focus();
                return false;
            }
            else if (maxValue > 0)
            {
                int value = int.Parse(textBoxValue, CultureInfo.CurrentCulture);
                if (value >= maxValue)
                {
                    DisplayMessage.Show(MessageId.ValueExceedsMaximum, this, maxValue, name);
                    textBox.Focus();
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
        }

        private bool SafeCheckForInt(TextBox textBox)
        {
            try
            {
                int.Parse(textBox.Text.Trim(), CultureInfo.CurrentCulture);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool ValidateTextBoxPositiveInteger(string name, TextBox textBox)
        {
            string textBoxValue = textBox.Text.Trim();
            if (textBoxValue == String.Empty || int.Parse(textBoxValue, CultureInfo.CurrentCulture) <= 0)
            {
                DisplayMessage.Show(MessageId.InvalidNumberPositiveOnly, this, name);
                textBox.Focus();
                return false;
            }
            else
            {
                return true;
            }
        }

        private bool ValidateTextBoxGreaterThanZero(string name, TextBox textBox)
        {
            string textBoxValue = textBox.Text.Trim();
            if (textBoxValue == String.Empty || int.Parse(textBoxValue, CultureInfo.CurrentCulture) < 0)
            {
                DisplayMessage.Show(MessageId.InvalidNumberNonNegative, this, name);
                textBox.Focus();
                return false;
            }
            else
            {
                return true;
            }
        }

        private TableCreationParameters CreateDefaultParameters()
        {
            // get default table properties
            TableProperties tableProperties = new TableProperties();
            tableProperties.CellPadding = TableEditingSettings.DefaultCellPadding;
            tableProperties.CellSpacing = TableEditingSettings.DefaultCellSpacing;
            tableProperties.BorderSize = TableEditingSettings.DefaultBorderSize;
            tableProperties.Width = new PixelPercent();

            // return default parameters
            return new TableCreationParameters(
                TableEditingSettings.DefaultRows,
                TableEditingSettings.DefaultColumns,
                tableProperties);
        }

        private void SaveDefaultTableCreationParameters(TableCreationParameters parameters)
        {
            TableEditingSettings.DefaultRows = parameters.Rows;
            TableEditingSettings.DefaultColumns = parameters.Columns;
            TableEditingSettings.DefaultCellPadding = parameters.Properties.CellPadding;
            TableEditingSettings.DefaultCellSpacing = parameters.Properties.CellSpacing;
            TableEditingSettings.DefaultBorderSize = parameters.Properties.BorderSize;
            TableEditingSettings.DefaultWidth = parameters.Properties.Width;
        }
    }
}

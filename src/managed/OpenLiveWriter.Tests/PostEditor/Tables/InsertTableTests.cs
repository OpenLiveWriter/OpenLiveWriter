using ApprovalTests;
using ApprovalTests.Reporters;

using NUnit.Framework;

using OpenLiveWriter.PostEditor.Tables;

namespace OpenLiveWriter.Tests.PostEditor.Tables
{
    [UseReporter(typeof(DiffReporter))]
    [TestFixture]
    public class InsertTableTests
    {

        [Test]
        public void Table400PixelsWide3Rows4Columns()
        {
            // Arrange
            var editor = new TestHtmlEditor();
            TableProperties tableProperties = new TableProperties();
            tableProperties.Width = new PixelPercent(400, PixelPercentUnits.Pixels);
            int rows = 3;
            int columns = 4;
            TableCreationParameters tableCreationParamters = new TableCreationParameters(rows, columns, tableProperties);

            // Act
            TableEditor.InsertTable(editor, null, tableCreationParamters);

            // Assert
            Approvals.VerifyHtml(editor.Html);
        }

        [Test]
        public void Table100PercentWide3Rows4Columns()
        {
            // Arrange
            var editor = new TestHtmlEditor();
            TableProperties tableProperties = new TableProperties();
            tableProperties.Width = new PixelPercent(100, PixelPercentUnits.Percentage);
            int rows = 3;
            int columns = 4;
            TableCreationParameters tableCreationParamters = new TableCreationParameters(rows, columns, tableProperties);

            // Act
            TableEditor.InsertTable(editor, null, tableCreationParamters);

            // Assert
            Approvals.VerifyHtml(editor.Html);
        }

        [Test]
        public void TableNoWide3Rows4Columns()
        {
            // Arrange
            var editor = new TestHtmlEditor();
            TableProperties tableProperties = new TableProperties();
            tableProperties.Width = new PixelPercent(0, PixelPercentUnits.Undefined);
            int rows = 3;
            int columns = 4;
            TableCreationParameters tableCreationParamters = new TableCreationParameters(rows, columns, tableProperties);

            // Act
            TableEditor.InsertTable(editor, null, tableCreationParamters);

            // Assert
            Approvals.VerifyHtml(editor.Html);
        }

        [Test]
        public void TableNoWide3Rows4ColumnsCellSpacingPadding()
        {
            // Arrange
            var editor = new TestHtmlEditor();
            TableProperties tableProperties = new TableProperties();
            tableProperties.Width = new PixelPercent(0, PixelPercentUnits.Undefined);
            tableProperties.CellSpacing = "2";
            tableProperties.CellPadding = "0";
            int rows = 3;
            int columns = 4;
            TableCreationParameters tableCreationParamters = new TableCreationParameters(rows, columns, tableProperties);

            // Act
            TableEditor.InsertTable(editor, null, tableCreationParamters);

            // Assert
            Approvals.VerifyHtml(editor.Html);
        }
    }
}

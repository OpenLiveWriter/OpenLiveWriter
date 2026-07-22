// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;
using System.Threading.Tasks;
using global::Avalonia.Controls;
using global::Avalonia.Layout;

namespace OpenLiveWriter.App.Avalonia.Dialogs
{
    /// <summary>
    /// Result returned from <see cref="PostPropertiesDialog"/>. A null
    /// <see cref="PublishDateUtc"/> means "publish immediately" — no dateCreated is
    /// sent and the server stamps its own time.
    /// </summary>
    public class PostPropertiesDialogResult
    {
        public DateTime? PublishDateUtc { get; set; }
    }

    /// <summary>
    /// A minimal "Post Properties" dialog (F2): publish date only, matching the
    /// macOS port's P1-9-lite scope. The default is "publish immediately"; choosing
    /// "set publish date" enables a local date + 24-hour time pair that is converted
    /// to UTC and sent as MetaWeblog <c>dateCreated</c> on publish (a future date
    /// schedules the post on servers that honor it, e.g. WordPress).
    /// </summary>
    public class PostPropertiesDialog : Window
    {
        private readonly RadioButton _immediateRadio;
        private readonly RadioButton _scheduleRadio;
        private readonly DatePicker _datePicker;
        private readonly TextBox _timeBox;
        private readonly Button _okButton;

        public PostPropertiesDialogResult Result { get; private set; }

        public PostPropertiesDialog(DateTime? currentPublishDateUtc = null)
        {
            Title = "Post Properties";
            Width = 420;
            MinWidth = 340;
            SizeToContent = SizeToContent.Height;
            CanResize = false;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            _immediateRadio = new RadioButton { Content = "Publish immediately", IsChecked = true };
            _scheduleRadio = new RadioButton { Content = "Set publish date:" };

            DateTime local = DateTime.Now;
            if (currentPublishDateUtc.HasValue)
            {
                local = currentPublishDateUtc.Value.ToLocalTime();
                _immediateRadio.IsChecked = false;
                _scheduleRadio.IsChecked = true;
            }

            _datePicker = new DatePicker { SelectedDate = local.Date };
            _timeBox = new TextBox { Text = local.ToString("HH:mm", CultureInfo.InvariantCulture), Width = 70 };

            _okButton = new Button { Content = "OK", IsDefault = true, MinWidth = 80 };
            var cancelButton = new Button { Content = "Cancel", IsCancel = true, MinWidth = 80 };

            _okButton.Click += (s, e) =>
            {
                Result = new PostPropertiesDialogResult
                {
                    PublishDateUtc = _scheduleRadio.IsChecked == true
                        ? CombineToUtc(_datePicker.SelectedDate, _timeBox.Text)
                        : null
                };
                Close(Result);
            };
            cancelButton.Click += (s, e) => Close(null);

            _scheduleRadio.IsCheckedChanged += (s, e) => UpdateFieldState();
            _timeBox.PropertyChanged += (s, e) =>
            {
                if (e.Property == TextBox.TextProperty) UpdateFieldState();
            };
            _datePicker.SelectedDateChanged += (s, e) => UpdateFieldState();

            var grid = new Grid
            {
                Margin = new global::Avalonia.Thickness(16),
                RowDefinitions = new RowDefinitions("Auto,Auto,Auto,Auto,Auto"),
                ColumnDefinitions = new ColumnDefinitions("Auto,Auto,*")
            };

            Grid.SetRow(_immediateRadio, 0);
            Grid.SetColumnSpan(_immediateRadio, 3);
            grid.Children.Add(_immediateRadio);

            Grid.SetRow(_scheduleRadio, 2);
            Grid.SetColumnSpan(_scheduleRadio, 3);
            _scheduleRadio.Margin = new global::Avalonia.Thickness(0, 8, 0, 0);
            grid.Children.Add(_scheduleRadio);

            _datePicker.Margin = new global::Avalonia.Thickness(24, 4, 8, 4);
            Grid.SetRow(_datePicker, 3);
            grid.Children.Add(_datePicker);

            _timeBox.Margin = new global::Avalonia.Thickness(0, 4, 8, 4);
            _timeBox.VerticalContentAlignment = VerticalAlignment.Center;
            Grid.SetRow(_timeBox, 3);
            Grid.SetColumn(_timeBox, 1);
            grid.Children.Add(_timeBox);

            var timeHint = new TextBlock
            {
                Text = "(24-hour, local time)",
                Opacity = 0.6,
                FontSize = 12,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(timeHint, 3);
            Grid.SetColumn(timeHint, 2);
            grid.Children.Add(timeHint);

            var buttonRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Spacing = 8,
                Margin = new global::Avalonia.Thickness(0, 12, 0, 0)
            };
            buttonRow.Children.Add(cancelButton);
            buttonRow.Children.Add(_okButton);
            Grid.SetRow(buttonRow, 4);
            Grid.SetColumnSpan(buttonRow, 3);
            grid.Children.Add(buttonRow);

            Content = grid;
            UpdateFieldState();
        }

        // The date/time fields and OK button track the schedule radio: fields are
        // only editable when scheduling, and OK requires a valid date + time then.
        private void UpdateFieldState()
        {
            bool scheduling = _scheduleRadio.IsChecked == true;
            _datePicker.IsEnabled = scheduling;
            _timeBox.IsEnabled = scheduling;
            _okButton.IsEnabled = !scheduling ||
                CombineToUtc(_datePicker.SelectedDate, _timeBox.Text).HasValue;
        }

        /// <summary>
        /// Combines a local date and an "HH:mm" (24-hour) time text into a UTC
        /// instant. Returns null when either part is missing or the time text does
        /// not parse — a bad date must never silently become "now".
        /// </summary>
        internal static DateTime? CombineToUtc(DateTimeOffset? selectedDate, string timeText)
        {
            if (!selectedDate.HasValue || !TryParseTimeOfDay(timeText, out int hour, out int minute))
                return null;

            DateTimeOffset date = selectedDate.Value;
            var local = new DateTime(date.Year, date.Month, date.Day, hour, minute, 0, DateTimeKind.Local);
            return local.ToUniversalTime();
        }

        /// <summary>
        /// Parses an "H:mm"/"HH:mm" 24-hour time. A plain TimeSpan parse would
        /// silently wrap out-of-range hours ("25:00" → next day), so the hour and
        /// minute are validated explicitly.
        /// </summary>
        internal static bool TryParseTimeOfDay(string timeText, out int hour, out int minute)
        {
            hour = 0;
            minute = 0;
            if (string.IsNullOrWhiteSpace(timeText))
                return false;

            string[] parts = timeText.Trim().Split(':');
            if (parts.Length != 2)
                return false;

            return int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out hour) &&
                   int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out minute) &&
                   hour >= 0 && hour <= 23 && minute >= 0 && minute <= 59;
        }

        /// <summary>
        /// Shows the dialog modally over <paramref name="owner"/> and returns the
        /// user's input, or null if cancelled.
        /// </summary>
        public static async Task<PostPropertiesDialogResult> ShowAsync(Window owner, DateTime? currentPublishDateUtc = null)
        {
            var dialog = new PostPropertiesDialog(currentPublishDateUtc);
            if (owner != null)
                return await dialog.ShowDialog<PostPropertiesDialogResult>(owner);

            dialog.Show();
            return null;
        }
    }
}

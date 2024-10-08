﻿/************************************************************************/
/* Credits to Federico Berasategui for this implementation.             */
/* https://stackoverflow.com/a/16745054                                 */
/************************************************************************/

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static UnrealBinaryBuilder.MainWindow;

namespace UnrealBinaryBuilder.UserControls
{
    public partial class LogViewer : UserControl
    {
        private ObservableCollection<LogEntry> LogEntries { get; set; }
        private bool AutoScroll = true;
        private bool addingEntry = false;

        public enum EMessageType
        {
            Info,
            Debug,
            Warning,
            Error
        }

        public LogViewer()
        {
            InitializeComponent();
            DataContext = LogEntries = new ObservableCollection<LogEntry>();
        }

        public void AddZipLog(LogEntry InLogEntry, ZipLogInclusionType InType)
		{
            InLogEntry.DateTime = DateTime.Now;
            switch (InType)
			{
                case ZipLogInclusionType.FileIncluded:
                    InLogEntry.MessageColor = Brushes.Green;
                    break;
                case ZipLogInclusionType.FileSkipped:
                    InLogEntry.MessageColor = Brushes.Orange;
                    break;
                case ZipLogInclusionType.ExtensionSkipped:
                    InLogEntry.MessageColor = Brushes.OrangeRed;
                    break;
			}

            Dispatcher.BeginInvoke((Action)(() =>
            {
                if (string.IsNullOrEmpty(InLogEntry.Message))
                {
                    InLogEntry.MsgVisibility = Visibility.Hidden;
                }
                addingEntry = true;
                LogEntries.Add(InLogEntry);
                addingEntry = false;
            }));
        }

        public void AddLogEntry(LogEntry InLogEntry, EMessageType InMessageType)
        {
            InLogEntry.DateTime = DateTime.Now;
            switch (InMessageType)
            {
                case EMessageType.Info:
                    InLogEntry.MessageColor = Brushes.WhiteSmoke;
                    break;
                case EMessageType.Debug:
                    InLogEntry.MessageColor = Brushes.Aqua;
                    break;
                case EMessageType.Warning:
                    InLogEntry.MessageColor = Brushes.Gold;
                    break;
                case EMessageType.Error:
                    InLogEntry.MessageColor = Brushes.Red;
                    break;
                default:
                    break;

            }

            Dispatcher.BeginInvoke((Action)(() =>
            {
                if (string.IsNullOrEmpty(InLogEntry.Message))
                {
                    InLogEntry.MsgVisibility = Visibility.Hidden;
                }

                addingEntry = true;
                LogEntries.Add(InLogEntry);
                addingEntry = false;
            }));
        }

        public void ClearAllLogs()
        {
            Dispatcher.BeginInvoke((Action)(() => LogEntries.Clear()));
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            try
            {
                var scroller = (e.Source as ScrollViewer)!;
                //TODO:: this stops auto scrolling if a message is bigger than the ScrollView
                // User scroll event : set or unset autoscroll mode
                if (e.ExtentHeightChange == 0 && !addingEntry)
                {
                    // Content unchanged : user scroll event
                    // Scroll bar is in bottom
                    // Set autoscroll mode
                    AutoScroll = Math.Abs(scroller.VerticalOffset - scroller.ScrollableHeight) < Double.Epsilon; // Scroll bar isn't in bottom
                    // Unset autoscroll mode
                }

                // Content scroll event : autoscroll eventually
                if (AutoScroll/* && e.ExtentHeightChange != 0*/)
                {   // Content changed and autoscroll mode set
                    // Autoscroll
                    scroller.ScrollToVerticalOffset(scroller.ExtentHeight);
                }
            }
            catch (Exception ex)
            {
                var logEntry = new LogEntry
                {
                    Message = string.Format("APPLICATION ERROR: " + ex.Message),
                    DateTime = DateTime.Now
                };
                AddLogEntry(logEntry, EMessageType.Error);
            }
        }

		private void CopyBtn_Click(object sender, RoutedEventArgs e)
		{
            Clipboard.SetDataObject(((Control)sender).Tag);
            ((MainWindow)Application.Current.MainWindow)!.ShowToastMessage("Copied to clipboard!", EMessageType.Info, true, false, "", 1);
        }
	}

	public class PropertyChangedBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                PropertyChangedEventHandler handler = PropertyChanged;
                handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }));
        }
    }

    public class LogEntry : PropertyChangedBase
    {
        public DateTime DateTime { get; set; }
        public string Message { get; set; }
        public Brush MessageColor { get; set; }
        public Visibility MsgVisibility { get; set; }
    }
}

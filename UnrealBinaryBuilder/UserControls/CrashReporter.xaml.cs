using System;
using Sentry;

namespace UnrealBinaryBuilder.UserControls
{
	/// <summary>
	/// Interaction logic for CrashReporter.xaml
	/// </summary>
	public partial class CrashReporter
	{
		public SentryId CurrentSentryId;

		public CrashReporter(Exception InException)
		{
			InitializeComponent();
			Username.Text = Environment.UserName;
			var StackTraceMessage = $"Source ->\t{InException.Source}\nMessage ->\t{InException.Message}\nTarget ->\t{InException.TargetSite}\nStackTrace ->\n{InException.StackTrace}";
			StackTraceText.Text = StackTraceMessage;
		}

		private void SubmitBtn_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			var CommentText = $"{Comment.Text}\n\nExceptionDetails ->\n{StackTraceText.Text}";
			var sentryFeedback = new SentryFeedback(CommentText, Email.Text, Username.Text, associatedEventId: CurrentSentryId);
			SentrySdk.CaptureFeedback(sentryFeedback);
			HandyControl.Controls.MessageBox.Success("Thank you for submitting the crash report!");
			Close();
		}

		private void CancelBtn_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			Close();
		}
	}
}

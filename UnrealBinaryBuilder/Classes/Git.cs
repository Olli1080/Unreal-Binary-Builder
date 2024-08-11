using LibGit2Sharp;
using System.Windows;

namespace UnrealBinaryBuilder.Classes
{
    public static class Git
    {
		private static Repository repository = null;

		public static string CommitHash
		{
			get
			{
				UpdateRepository();
				return repository?.Head.Tip.Sha;
			}
		}

		public static string CommitHashShort => string.IsNullOrWhiteSpace(CommitHash) ? null : CommitHash.Remove(CommitHash.Length - 33);

		public static string BranchName
		{
			get
			{
				UpdateRepository();
				return repository?.Head.FriendlyName;
			}
		}

		public static string TrackedBranchName
		{
			get
			{
				UpdateRepository();
				if (repository != null)
				{
					return repository.Head.IsTracking ? repository.Head.TrackedBranch.FriendlyName : null;
				}

				return null;
			}
		}

		private static void UpdateRepository()
		{
			MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
			if (repository == null && Repository.IsValid(mainWindow.SetupBatFilePath.Text))
			{
				repository = new Repository(mainWindow.SetupBatFilePath.Text);
			}
		}
	}
}

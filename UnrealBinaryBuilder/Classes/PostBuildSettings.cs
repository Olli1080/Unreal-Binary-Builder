﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.AccessControl;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using UnrealBinaryBuilder.UserControls;

namespace UnrealBinaryBuilder.Classes
{
	public class PostBuildSettings
	{
		Task ZippingTask = null;
		static CancellationTokenSource ZipCancelTokenSource = new CancellationTokenSource();
		CancellationToken ZipCancelToken = ZipCancelTokenSource.Token;
		MainWindow mainWindow = null;

		public PostBuildSettings(MainWindow _mainWindow)
		{
			mainWindow = _mainWindow;
		}

		public bool CanSaveToZip()
		{
			return ShouldSaveToZip() && DirectoryIsWritable(Path.GetDirectoryName(mainWindow.ZipPath.Text));
		}

		public bool ShouldSaveToZip()
		{
			return (bool)mainWindow.bZipBuild.IsChecked && !string.IsNullOrEmpty(mainWindow.ZipPath.Text);
		}

		public bool DirectoryIsWritable(string DirectoryPath)
		{
			if (string.IsNullOrWhiteSpace(DirectoryPath))
			{
				return false;
			}

			DirectoryInfo ZipDirectory = new DirectoryInfo(DirectoryPath);
			bool bDirectoryExists = ZipDirectory.Exists;
			
            if (!bDirectoryExists) 
                return false;

            try
            {
                AuthorizationRuleCollection collection = new DirectoryInfo(ZipDirectory.FullName).GetAccessControl().GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount));
                if (collection.Cast<FileSystemAccessRule>().Any(rule => rule.AccessControlType == AccessControlType.Allow))
                {
                    //bHasWriteAccess
                    return true;
                }
            }
            catch (Exception)
            {

            }

            return false;
		}

		public void PrepareToSave()
		{
			ZipCancelTokenSource.Dispose();
			ZipCancelTokenSource = new CancellationTokenSource();
			ZipCancelToken = ZipCancelTokenSource.Token;
		}

		public async void SavePluginToZip(PluginCard pluginCard, string ZipLocationToSave, bool bZipForMarketplace)
		{
			Application.Current.Dispatcher.Invoke(() =>
			{
				GameAnalyticsCSharp.AddProgressStart("PluginZip", "Progress");
			});

			CompressionLevel CL = (bool)mainWindow.bFastCompression.IsChecked ? CompressionLevel.Fastest : CompressionLevel.SmallestSize;
            await Task.Run(() =>
			{
                using FileStream output = new FileStream(ZipLocationToSave, FileMode.CreateNew);
                using var zipFile = new ZipArchive(output, ZipArchiveMode.Create);
                {
                    IEnumerable<string> files = Directory.EnumerateFiles(pluginCard.DestinationPath, "*.*",
                        SearchOption.AllDirectories);
                    List<string> filesToAdd = new List<string>();

                    foreach (string file in files)
                    {
                        bool bSkipFile = false;
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            string CurrentFilePath = Path.GetFullPath(file).ToLower();
                            if (bZipForMarketplace && (CurrentFilePath.Contains(@"\binaries\") ||
                                                       CurrentFilePath.Contains(@"\intermediate\")))
                            {
                                bSkipFile = true;
                            }

                            if (bSkipFile == false)
                            {
                                filesToAdd.Add(file);
                            }
                        });
                    }

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        pluginCard.ZipProgressbar.IsIndeterminate = false;
                        pluginCard.ZipProgressbar.Value = 0;
                        pluginCard.ZipProgressbar.Maximum = filesToAdd.Count;
                    });

                    int entriesSaved = 0;
                    foreach (string file in filesToAdd)
                    {
                        zipFile.CreateEntryFromFile(file,
                            Path.GetDirectoryName(file)!.Replace(pluginCard.DestinationPath, string.Empty), CL);
                        ++entriesSaved;

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            pluginCard.ZipProgressbar.Value = Convert.ToInt32(entriesSaved);
                        });
                    }
                }
                Application.Current.Dispatcher.Invoke(() =>
                {
                    GameAnalyticsCSharp.AddProgressEnd("PluginZip", "Progress");
                });

                /*zipFile.SaveProgress += (o, args) =>
                {
                    if (args.EventType == ZipProgressEventType.Saving_BeforeWriteEntry)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            pluginCard.ZipProgressbar.Value = Convert.ToInt32(args.EntriesSaved + 1);
                        });
                    }
                    else if (args.EventType == ZipProgressEventType.Saving_Completed)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            GameAnalyticsCSharp.AddProgressEnd("PluginZip", "Progress");
                        });
                    }
                };*/

                //zipFile.UseZip64WhenSaving = Zip64Option.Always;
                //zipFile.Save(ZipLocationToSave);
				
                Application.Current.Dispatcher.Invoke(() => { mainWindow.AddLogEntry($"Plugin zipped and saved to: {ZipLocationToSave}"); });
            });
        }

		public async void SaveToZip(string InBuildDirectory, string ZipLocationToSave)
		{
			Application.Current.Dispatcher.Invoke(() =>
			{
				GameAnalyticsCSharp.AddProgressStart("Zip", "Progress");
				mainWindow.TotalResult.Content = "Hold on...Stats will be displayed soon.";
				mainWindow.CurrentFileSaving.Content = "Waiting...";
				mainWindow.FileSaveState.Content = "State: Preparing...";
				mainWindow.ZipStatusLabel.Visibility = Visibility.Collapsed;
				mainWindow.ZipStatusStackPanel.Visibility = Visibility.Visible;
			});

			CompressionLevel CL = (bool)mainWindow.bFastCompression.IsChecked ? CompressionLevel.Fastest : CompressionLevel.SmallestSize;

			ZippingTask = Task.Run(() =>
			{
                using FileStream output = new FileStream(ZipLocationToSave, FileMode.CreateNew);
                using var zipFile = new ZipArchive(output, ZipArchiveMode.Create);
                {
					Application.Current.Dispatcher.Invoke(() => { mainWindow.FileSaveState.Content = $"State: Be Patient! This might take a long time. Ninjas are finding files in {InBuildDirectory}"; });
					IEnumerable<string> files = Directory.EnumerateFiles(InBuildDirectory, "*.*", SearchOption.AllDirectories);

					ZipCancelToken.ThrowIfCancellationRequested();

					List<string> filesToAdd = new List<string>();

					int SkippedFiles = 0;
					int AddedFiles = 0;
					int TotalFiles = files.Count();

					long TotalSize = 0;
					long TotalSizeToZip = 0;
					long SkippedSize = 0;
					string TotalSizeInString = "0B";
					string TotalSizeToZipInString = "0B";
					string SkippedSizeToZipInString = "0B";
					Application.Current.Dispatcher.Invoke(() => { mainWindow.FileSaveState.Content = "State: Preparing files for zipping..."; });
					foreach (string file in files)
					{
						bool bSkipFile = false;
						Application.Current.Dispatcher.Invoke(() =>
						{
							string CurrentFilePath = Path.GetFullPath(file).ToLower();
							if (mainWindow.bIncludePDB.IsChecked == false && Path.GetExtension(file).ToLower() == ".pdb")
							{
								bSkipFile = true;
							}

							if (mainWindow.bIncludeDEBUG.IsChecked == false && Path.GetExtension(file).ToLower() == ".debug")
							{
								bSkipFile = true;
							}

							if (mainWindow.bIncludeDocumentation.IsChecked == false && CurrentFilePath.Contains(@"\source\") == false && CurrentFilePath.Contains(@"\documentation\"))
							{
								bSkipFile = true;
							}

							if (mainWindow.bIncludeExtras.IsChecked == false && CurrentFilePath.Contains(@"\extras\redist\") == false && CurrentFilePath.Contains(@"\extras\"))
							{
								bSkipFile = true;
							}

							if (mainWindow.bIncludeSource.IsChecked == false && CurrentFilePath.Contains(@"\source\developer\"))
							{
								bSkipFile = true;
							}

							if (mainWindow.bIncludeSource.IsChecked == false && CurrentFilePath.Contains(@"\source\editor\"))
							{
								bSkipFile = true;
							}

							if (mainWindow.bIncludeSource.IsChecked == false && CurrentFilePath.Contains(@"\source\programs\"))
							{
								bSkipFile = true;
							}

							if (mainWindow.bIncludeSource.IsChecked == false && CurrentFilePath.Contains(@"\source\runtime\"))
							{
								bSkipFile = true;
							}

							if (mainWindow.bIncludeSource.IsChecked == false && CurrentFilePath.Contains(@"\source\thirdparty\"))
							{
								bSkipFile = true;
							}

							if (mainWindow.bIncludeFeaturePacks.IsChecked == false && CurrentFilePath.Contains(@"\featurepacks\"))
							{
								bSkipFile = true;
							}

							if (mainWindow.bIncludeSamples.IsChecked == false && CurrentFilePath.Contains(@"\samples\"))
							{
								bSkipFile = true;
							}

							if (mainWindow.bIncludeTemplates.IsChecked == false && CurrentFilePath.Contains(@"\source\") == false && CurrentFilePath.Contains(@"\content\editor") == false && CurrentFilePath.Contains(@"\templates\"))
							{
								bSkipFile = true;
							}

						});

						TotalSize += new FileInfo(file).Length;
						TotalSizeInString = BytesToString(TotalSize);
						if (bSkipFile)
						{
							SkippedFiles++;
							SkippedSize += new FileInfo(file).Length;
							SkippedSizeToZipInString = BytesToString(SkippedSize);
							//Application.Current.Dispatcher.Invoke(() => { mainWindow.AddZipLog($"File Skipped: {file}", MainWindow.ZipLogInclusionType.FileSkipped); });
						}
						else
						{
							filesToAdd.Add(file);
							AddedFiles++;
							TotalSizeToZip += new FileInfo(file).Length;
							TotalSizeToZipInString = BytesToString(TotalSizeToZip);
							//Application.Current.Dispatcher.Invoke(() => { mainWindow.AddZipLog($"File Included: {file}", MainWindow.ZipLogInclusionType.FileIncluded); });
						}

						Application.Current.Dispatcher.Invoke(() => { mainWindow.CurrentFileSaving.Content =
                            $"Total: {TotalFiles}. Added: {AddedFiles}. Skipped: {SkippedFiles}"; });
						ZipCancelToken.ThrowIfCancellationRequested();
					}

					Application.Current.Dispatcher.Invoke(() =>
					{
						mainWindow.TotalResult.Content =
                            $"Total Size: {TotalSizeInString}. To Zip: {TotalSizeToZipInString}. Skipped: {SkippedSizeToZipInString}";
						mainWindow.FileSaveState.Content = "State: Verifying...";
						mainWindow.OverallProgressbar.Maximum = filesToAdd.Count;
					});

                    long ProcessedSize = 0;
					string ProcessSizeInString = "0B"; //shouldn't this be used?

					Application.Current.Dispatcher.Invoke(() =>
					{
						mainWindow.OverallProgressbar.IsIndeterminate = false;
						mainWindow.FileProgressbar.IsIndeterminate = false;
					});

                    int entriesSaved = 0;
                    foreach (string file in filesToAdd)
                    {
                        ZipCancelToken.ThrowIfCancellationRequested();

                        if (entriesSaved == 0)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                mainWindow.CurrentFileSaving.Content = "";
                                mainWindow.FileSaveState.Content =
                                    $"State: Saving zip file ({TotalFiles} files) to {mainWindow.ZipPath.Text}";
                            });
                        }

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            mainWindow.FileSaveState.Content = "State: Begin Writing...";
                            mainWindow.CurrentFileSaving.Content =
                                $"Saving File: {Path.GetFileName(file)} ({(entriesSaved)}/{(TotalFiles)})";
                            mainWindow.OverallProgressbar.Value = Convert.ToInt32(entriesSaved);
                        });

                        zipFile.CreateEntryFromFile(file, Path.GetDirectoryName(file).Replace(InBuildDirectory, string.Empty), CL);
                        ++entriesSaved;

                        ProcessedSize += new FileInfo(Path.Combine(InBuildDirectory, Path.GetFileName(file))).Length;
                        ProcessSizeInString = BytesToString(ProcessedSize);
                        Application.Current.Dispatcher.Invoke(() => {
                            mainWindow.TotalResult.Content =
                                $"Total Size: {TotalSizeInString}. To Zip: {TotalSizeToZipInString}. Skipped: {SkippedSizeToZipInString}. Processed: {ProcessSizeInString}";
                        });
                    }
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        GameAnalyticsCSharp.AddProgressEnd("Zip", "Progress");
                        mainWindow.TryShutdown();
                    });

                    /*
					zipFile.SaveProgress += (o, args) =>
					{
						ZipCancelToken.ThrowIfCancellationRequested();
						if (args.EventType == ZipProgressEventType.Saving_BeforeWriteEntry)
						{
							Application.Current.Dispatcher.Invoke(() =>
							{
								mainWindow.FileSaveState.Content = "State: Begin Writing...";
								mainWindow.CurrentFileSaving.Content =
                                    $"Saving File: {Path.GetFileName(args.CurrentEntry.FileName)} ({(args.EntriesSaved + 1)}/{(args.EntriesTotal)})";
								mainWindow.OverallProgressbar.Value = Convert.ToInt32(args.EntriesSaved + 1);
							});
						}
						else if (args.EventType == ZipProgressEventType.Saving_EntryBytesRead)
						{
							Application.Current.Dispatcher.Invoke(() =>
							{
								mainWindow.FileSaveState.Content = "State: Writing...";
								mainWindow.FileProgressbar.Value = (int)((args.BytesTransferred * 100) / args.TotalBytesToTransfer);
							});
						}
						else if (args.EventType == ZipProgressEventType.Saving_AfterWriteEntry)
						{
							ProcessedSize += new FileInfo(Path.Combine(InBuildDirectory, args.CurrentEntry.FileName)).Length;
							ProcessSizeInString = BytesToString(ProcessedSize);
							Application.Current.Dispatcher.Invoke(() => { mainWindow.TotalResult.Content =
                                $"Total Size: {TotalSizeInString}. To Zip: {TotalSizeToZipInString}. Skipped: {SkippedSizeToZipInString}. Processed: {ProcessSizeInString}"; });
						}
						else if (args.EventType == ZipProgressEventType.Saving_Started)
						{
							Application.Current.Dispatcher.Invoke(() =>
							{
								mainWindow.CurrentFileSaving.Content = "";
								mainWindow.FileSaveState.Content =
                                    $"State: Saving zip file ({TotalFiles} files) to {mainWindow.ZipPath.Text}";
							});
						}
						else if (args.EventType == ZipProgressEventType.Saving_Completed)
						{
							Application.Current.Dispatcher.Invoke(() =>
							{
								GameAnalyticsCSharp.AddProgressEnd("Zip", "Progress");
								mainWindow.TryShutdown();
							});
						}
					};
					*/
                }
                //zipFile.UseZip64WhenSaving = Zip64Option.Always;
                //zipFile.Save(ZipLocationToSave);
                Application.Current.Dispatcher.Invoke(() => 
					{
						mainWindow.CurrentFileSaving.Visibility = mainWindow.OverallProgressbar.Visibility = mainWindow.CancelZipping.Visibility = Visibility.Collapsed;
						mainWindow.FileSaveState.Content = $"State: Saved to {ZipLocationToSave}";
						mainWindow.AddLogEntry($"Done zipping. {ZipLocationToSave}");
					});
				
			}, ZipCancelToken);

			try
			{
				await ZippingTask;
			}
			catch (OperationCanceledException e)
			{
				Application.Current.Dispatcher.Invoke(() =>
				{
					mainWindow.CurrentFileSaving.Content = "";
					mainWindow.FileSaveState.Content = "Operation canceled.";
					mainWindow.AddLogEntry($"{nameof(OperationCanceledException)} with message: {e.Message}");
				});
			}
			finally
			{
				Application.Current.Dispatcher.Invoke(() =>
				{
					mainWindow.CancelZipping.Content = "Cancel Zipping";
					mainWindow.CancelZipping.IsEnabled = true;
				});
			}
		}

		public void CancelTask()
		{
			GameAnalyticsCSharp.AddProgressEnd("Zip", "Progress", true);
			mainWindow.CancelZipping.Content = "Canceling. Please wait...";
			mainWindow.CancelZipping.IsEnabled = false;
			ZipCancelTokenSource.Cancel();
		}

		public static string BytesToString(long byteCount)
		{
			string[] suf = { "B", "KB", "MB", "GB", "TB" };
			if (byteCount == 0)
			{
				return "0" + suf[0];
			}
			long bytes = Math.Abs(byteCount);
			int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
			double num = Math.Round(bytes / Math.Pow(1024, place), 1);
			return (Math.Sign(byteCount) * num).ToString() + suf[place];
		}
	}
}

using System.IO;
using System.Windows;

namespace UnrealBinaryBuilder.UserControls
{
	/// <summary>
	/// Interaction logic for CodeEditor.xaml
	/// </summary>
	public partial class CodeEditor
	{
		private string? Internal_FilePath = null;

		private bool _isDirty = false;
		public bool IsDirty
		{
			get => _isDirty;
            set
            {
                if (_isDirty == value) return;

                _isDirty = value;
                Title = _isDirty ? "Code Editor (Modified)" : "Code Editor";
                SaveBtn.IsEnabled = _isDirty;
            }
		}

		public CodeEditor()
		{
			InitializeComponent();
		}

		public bool LoadFile(string FilePath)
		{
			FileInfo fileInfo = new FileInfo(FilePath);
            if (!fileInfo.Exists) return false;

            Internal_FilePath = FilePath;
            TextEditor.Load(FilePath);
            if (fileInfo.IsReadOnly)
            {
                TextEditor.IsEnabled = false;
                SaveBtn.IsEnabled = false;
            }
            IsDirty = false;
            return true;
        }

		private void MainCodeEditor_Closed(object sender, System.EventArgs e)
		{
			Internal_FilePath = null;
		}

		private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Internal_FilePath == null) return;
			File.WriteAllText(Internal_FilePath, TextEditor.Text);
			IsDirty = false;
		}

		private void TextEditor_TextChanged(object sender, System.EventArgs e)
		{
			IsDirty = true;
		}
	}
}

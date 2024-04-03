using System.IO;
using SadChromaLib.Utils.Convenience;
using SadChromaLib.Specialisations.Dialogue;

namespace ScriptEditor;

public sealed partial class EditorMain: Control
{
    string _lastFilePath;
    DialogueParser _parser;
    CodeEdit _scriptEditor;

	string[] ScriptFileFilter = {
		"*.txt ; Dialogue Script Files, Text Files"
	};

	string[] GraphFileFilter = {
		"*.dgr ; SCHLib Dialogue Graph"
	};

    public EditorMain()
    {
        _lastFilePath = null;
        _parser = new();
    }

    public override void _Ready()
    {
		_scriptEditor = GetNode<CodeEdit>("%ScriptEditor");

		BindButton("%New", OnCreateNew);
		BindButton("%Save", OnSaveConvenient);

		BindButton("%Open", ()
			=> StartFileDialog("Load script", true, ScriptFileFilter, ReadFileFromDisk));

		BindButton("%SaveAs", ()
			=> StartFileDialog("Save script", false, ScriptFileFilter, WriteFileToDisk));

		BindButton("%Export", ()
			=> StartFileDialog("Export dialogue graph", false, GraphFileFilter, CompileAndWriteGraphToDisk));
    }

	#region Events

	private void OnCreateNew()
	{
		_scriptEditor.Text = "";

		_lastFilePath = null;
		UpdateSavedTitle();
	}

	private void OnSaveConvenient()
	{
		if (_lastFilePath is null) {
			StartFileDialog("Save script", false, ScriptFileFilter, WriteFileToDisk);
			return;
		}

		WriteFileToDisk(_lastFilePath);
	}

	#endregion

    #region I/O

    private void ReadFileFromDisk(string filePath)
	{

		if (!File.Exists(filePath)) {
			return;
		}

		FileStream file = File.Open(filePath, FileMode.Open);

		using (StreamReader reader = new(file)) {
			_scriptEditor.Text = reader.ReadToEnd();
		}

		_lastFilePath = filePath;
		UpdateSavedTitle();
	}

	private void WriteFileToDisk(string filePath)
	{

		if (File.Exists(filePath)) {
			File.Delete(filePath);
		}

		FileStream file = File.Open(filePath, FileMode.Create);

		using (StreamWriter writer = new(file)) {
			writer.Write(_scriptEditor.Text);
		}

		_lastFilePath = filePath;
		UpdateSavedTitle();
	}

	private void CompileAndWriteGraphToDisk(string filePath) {
		_parser.CompileToFile(_scriptEditor.Text, FilePathUtils.Expand(filePath));
	}

	#endregion

	#region Helpers

	private void StartFileDialog(
		string title,
		bool readMode,
		string[] fileFilter,
		Action<string> callback)
	{
		FileDialog dialogRef = new() {
			Access = FileDialog.AccessEnum.Filesystem,
			FileMode = readMode ? FileDialog.FileModeEnum.OpenFile : FileDialog.FileModeEnum.SaveFile,
			Title = title,
			Filters = fileFilter
		};

		AddChild(dialogRef);

		dialogRef.FileSelected += (string filePath) => {
			dialogRef.QueueFree();
			callback.Invoke(filePath);
		};

		dialogRef.Canceled += dialogRef.QueueFree;
		dialogRef.PopupCentered(new(300,420));
	}

	private void StartPrompt(
		string title,
		string message,
		Action callback)
	{
		ConfirmationDialog dialogRef = new() {
			Title = title,
			DialogText = message
		};

		AddChild(dialogRef);

		dialogRef.Confirmed += () => {
			dialogRef.QueueFree();
			callback.Invoke();
		};

		dialogRef.CloseRequested += dialogRef.QueueFree;
		dialogRef.Canceled += dialogRef.QueueFree;

		dialogRef.PopupCentered(new(250, 64));
	}

	private void UpdateSavedTitle()
	{
		Label nameLabel = GetNode<Label>("%Title");

		if (_lastFilePath is null) {
			nameLabel.Text = "(Unsaved Script)";
			return;
		}

		nameLabel.Text = System.IO.Path.GetFileName(_lastFilePath);
	}

	private void BindButton(string name, Action callback) {
		GetNode<Button>(name).Pressed += callback;
	}

	#endregion
}
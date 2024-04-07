using System.IO;
using SadChromaLib.Utils.Convenience;
using SadChromaLib.Specialisations.Dialogue;
using SadChromaLib.Specialisations.Dialogue.Types;

namespace ScriptEditor;

public sealed partial class EditorMain: Control
{
    string _lastFilePath;
    DialogueParser _parser;
	DialoguePreview _preview;
    CodeEdit _scriptEditor;

	ScriptAnalyserThread _analyser;
	Timer _analysisDebounce;

	string[] ScriptFileFilter = {
		"*.txt ; Dialogue Script Files/Text Files"
	};

	string[] GraphFileFilter = {
		"*.dgr ; SCHLib Dialogue Files"
	};

    public EditorMain()
    {
		_analyser = new();
        _lastFilePath = null;
        _parser = new(true);

		_analyser.OnDataAvailable += OnAnalysisCompleted;
    }

    public override void _Ready()
    {
		_analysisDebounce = new() {
			WaitTime = 1.0f,
			OneShot = true
		};

		CallDeferred(MethodName.AddChild, _analysisDebounce);

		_scriptEditor = GetNode<CodeEdit>("%ScriptEditor");
		_preview = GetNode<DialoguePreview>("%Preview");

		// Bind Signals //

		_scriptEditor.TextChanged += OnScriptUpdated;

		this.BindButton("%New", OnCreateNew);
		this.BindButton("%Save", OnSaveConvenient);

		this.BindButton("%Open", ()
			=> StartFileDialog("Load script", true, ScriptFileFilter, ReadFileFromDisk));

		this.BindButton("%SaveAs", ()
			=> StartFileDialog("Save script", false, ScriptFileFilter, WriteFileToDisk));

		this.BindButton("%Run", OnShowPreview);

		this.BindButton("%Export", ()
			=> StartFileDialog("Export dialogue file", false, GraphFileFilter, CompileAndWriteGraphToDisk));
    }

    public override void _ExitTree() {
		_analyser.AwaitTasks();
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

	private async void OnScriptUpdated()
	{
		if (!_analysisDebounce.IsStopped())
			return;

		_analysisDebounce.Start();
		await ToSignal(_analysisDebounce, Timer.SignalName.Timeout);

		_analyser.Analyse(_scriptEditor.Text);
	}

	private void OnShowPreview()
	{
		DialogueGraph dialogue = _parser.Compile(_scriptEditor.Text);

		_preview.StartPreview(dialogue);
		_scriptEditor.CallDeferred(Control.MethodName.ReleaseFocus);
	}

	private void OnAnalysisCompleted(ScriptInfoData info)
	{
		// TODO: Implement UI for info
		GD.Print("Character Names: ", info.CharacterNames.Join(", "));
		GD.Print("Variable Names: ", info.VariableNames.Join(", "));
		GD.Print("Tag Names: ", info.TagNames.Join(", "));
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
		OnScriptUpdated();
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
			Filters = fileFilter,
			CurrentDir = OS.GetSystemDir(OS.SystemDir.Documents)
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

	#endregion
}
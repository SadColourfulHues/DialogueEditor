using SadChromaLib.Utils.Timing;
using SadChromaLib.Specialisations.Dialogue.Playback;
using SadChromaLib.Specialisations.Dialogue.Types;

namespace ScriptEditor;

public sealed partial class DialoguePreview: Control, IDialoguePlaybackHandler
{
    [Export]
    PackedScene _pkgChoiceButton;

    TimingController _buildTiming;
    DialoguePlayback _playback;
    DialogueBox _dialogueBox;

    DialogueGraph _currentData;
    Control _printUI;
    Label _labelPrint;

    Control _choicesUI;
    Control _choicesViewport;
    Label _labelChoicePrompt;
    bool _hasMadeChoice;
    bool _needsRestart;

    Timer _timerPrint;
    Tween _tweenChoicePrompt;

    public override void _Ready()
    {
        _hasMadeChoice = false;

        _playback = new(this);
        _buildTiming = new(DialogueNode.MaxChoices);

        _printUI = GetNode<Control>("%Print");
        _labelPrint = GetNode<Label>("%PrintMessage");

        _choicesUI = GetNode<Control>("%Choices");
        _dialogueBox = GetNode<DialogueBox>("%Dialogue");
        _choicesViewport = GetNode<Control>("%ChoiceItems");

        _labelChoicePrompt = _choicesUI.GetNode<Label>(nameof(Label));

        _timerPrint = new() {
            OneShot = true
        };

        _timerPrint.Timeout += _printUI.Hide;
        CallDeferred(MethodName.AddChild, _timerPrint);

        // Configure choice buttons //
        for (int i = 0; i < DialogueNode.MaxChoices; ++i) {
            int index = i;

            Button choiceButton = _pkgChoiceButton.Instantiate<Button>();
            choiceButton.Pressed += () => OnChoiceSelected(index);

            _choicesViewport.AddChild(choiceButton);
            choiceButton.Visible = false;
        }

        _choicesUI.Visible = false;
        Visible = false;

        // Bind signals //

        GetNode<JumpToModal>("%JumpToModal")
            .OnJumpRequested += OnGoToJumpTarget;

        _playback.OnDialoguePrint += OnPrintMessage;

        this.BindButton("%Close", OnClose);
        this.BindButton("%Restart", OnRestart);
        this.BindButton("%Jump", OnShowJumpModal);
    }

    public override void _Process(double delta) {
        _buildTiming.Evaluate(delta);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        switch (@event)
        {
            case InputEventKey:
            case InputEventJoypadButton:
            case InputEventMouseButton:
                // Next dialogue action //
                if (!Input.IsActionJustPressed("advance_dialogue") ||
                    !_playback.HasData())
                {
                    break;
                }

                _needsRestart = false;
                _playback.Next();

                GetViewport().SetInputAsHandled();
                break;
        }
    }

    public void StartPreview(DialogueGraph dialogue)
    {
        _currentData = dialogue;

        if (dialogue is null) {
            Visible = false;
            return;
        }

        _choicesUI.Visible = false;
        _playback.SetData(_currentData);
        _playback.ResetFull();

        Visible = true;
        _playback.ReloadBlock();
    }

    #region Events

    private void OnChoiceSelected(int idx)
    {
        if (_hasMadeChoice)
            return;

        _hasMadeChoice = true;

        BuildOutChoices(() => {
            _playback.SelectChoice(idx);
            _choicesUI.Visible = false;
        });
    }

    private void OnClose() {
        Visible = false;
    }

    private void OnPrintMessage(string message)
    {
        if (!_timerPrint.IsStopped()) {
            _timerPrint.Stop();
        }

        _timerPrint.Start(1.5f + (0.1f * message.Length));

        _labelPrint.Text = message;
        _printUI.Visible = true;
    }

    private async void OnRestart() {
        if (!_playback.HasData())
            return;

        _choicesUI.Visible = false;
        _needsRestart = true;
        _dialogueBox.StopAnimating();

        _playback.ResetFull();

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        _playback.ReloadBlock();
    }

    private void OnShowJumpModal()
    {
        JumpToModal modal = GetNode<JumpToModal>("%JumpToModal");
        modal.Show(_currentData);
    }

    private void OnGoToJumpTarget(int index)
    {
        if (index < 0 || index >= _currentData.Nodes.Length)
            return;

        _choicesUI.Visible = false;
        _needsRestart = true;

        _playback.Jump(index);

        OnPlaybackPresentChoices(_currentData.Nodes[index].Choices);
    }

    #endregion

    #region Playback

    public void OnPlaybackPresentDialogue(string character, string dialogue) {
        _dialogueBox.Update(character, dialogue);
    }

    public async void OnPlaybackPresentChoices(DialogueChoice[] choices)
    {
        if (choices.Length < 1)
            return;

        // Wait until the dialogue finishes before presenting the choiecs
        SceneTree tree = GetTree();

        while (_dialogueBox.IsAnimating()) {
            await ToSignal(tree, SceneTree.SignalName.ProcessFrame);

            if (_needsRestart) {
                _needsRestart = false;
                return;
            }
        }

        TweenUtils.StartTypewriter(ref _tweenChoicePrompt, _labelChoicePrompt, 5);
        _hasMadeChoice = false;

        for (int i = 0; i < choices.Length; ++i) {
            _choicesViewport.GetChild<ChoiceButton>(i).SetText(choices[i].ChoiceText);
        }

        _choicesUI.Visible = true;
        BuildInChoices(choices.Length);
    }

    public void OnPlaybackEvaluateCommand(DialoguePlayback playbackRef, DialogueCommand command) {
    }

    public void OnPlaybackCompleted(DialoguePlayback playbackRef) {
        Visible = false;
    }

    #endregion

    #region Utils

    private void BuildInChoices(int count)
    {
        _buildTiming.ClearTasks();

        for (int i = 0; i < count; ++i) {
            Button button = _choicesViewport.GetChild<Button>(i);
            _buildTiming.AddTask(0.08f * i, () => button.Visible = true);
        }
    }

    private void BuildOutChoices(Action completion)
    {
        _buildTiming.ClearTasks();
        float lastDuration = 0.0f;

        for (int i = 0; i < DialogueNode.MaxChoices; ++i) {
            Button button = _choicesViewport.GetChild<Button>(i);

            if (!button.Visible)
                continue;

            lastDuration = 0.08f * i;
            _buildTiming.AddTask(lastDuration, () => button.Visible = false);
        }

        _buildTiming.AddTask(lastDuration, completion);
    }

    #endregion
}
namespace ScriptEditor;

public sealed partial class DialogueBox: Control
{
    [Export]
    float _wordsPerSecond = 10.0f;

    Label _labelCharacter;
    Label _labelDialogue;

    Tween _tweenCharacter;
    Tween _tweenDialogue;

    public override void _Ready()
    {
        _labelCharacter = GetNode<Label>("%Character");
        _labelDialogue = GetNode<Label>("%Dialogue");
    }

    public void Update(string characterName, string dialogueText)
    {
        if (_labelCharacter.Text != characterName) {
            TweenUtils.StartTypewriter(ref _tweenCharacter, _labelCharacter, _wordsPerSecond);
        }

        _labelCharacter.Text = characterName;
        _labelDialogue.Text = dialogueText;

        TweenUtils.StartTypewriter(ref _tweenDialogue, _labelDialogue, _wordsPerSecond);
    }

    public bool IsAnimating() {
        return IsInstanceValid(_tweenDialogue) && _tweenDialogue.IsRunning();
    }

    public void StopAnimating() {
        TweenUtils.Kill(ref _tweenDialogue);
        TweenUtils.Kill(ref _tweenCharacter);
    }
}
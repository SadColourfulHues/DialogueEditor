namespace ScriptEditor;

public sealed partial class ChoiceButton: Button
{
    public void SetText(string text) {
        GetNode<RichTextLabel>("%Label").Text = text;
    }
}
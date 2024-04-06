using SadChromaLib.UI;
using SadChromaLib.Specialisations.Dialogue.Types;

namespace ScriptEditor;

public sealed partial class JumpToListItem: BaseListItem<DialogueNode>
{
    Label _labelTag;
    Label _labelCharacter;
    Label _labelDialogue;

    public override void _Ready()
    {
        base._Ready();

        _labelTag = GetNode<Label>("%Tag");
        _labelCharacter = GetNode<Label>("%Character");
        _labelDialogue = GetNode<Label>("%Dialogue");
    }

    public override void Update(DialogueNode data)
    {
        _labelTag.Visible = !data.Tag.StartsWith("node_");
        _labelTag.Text = data.Tag;
        _labelCharacter.Text = data.CharacterId;
        _labelDialogue.Text = data.DialogueText;
    }
}
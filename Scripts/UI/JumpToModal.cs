using SadChromaLib.UI;
using SadChromaLib.Specialisations.Dialogue.Types;

namespace ScriptEditor;

public sealed partial class JumpToModal: FancyModalView
{
    public event Action<int> OnJumpRequested;
    JumpToList _list;

    public override void _Ready()
    {
        _list = GetNode<JumpToList>("%List");

        // Bind signals //

        this.BindButton("%Close",
            () => SetVisibility(false));

        _list.OnItemSelected += OnItemSelected;
    }

    public void Show(DialogueGraph data)
    {
        _list.Configure(data);
        SetVisibility(true);
    }

    private void OnItemSelected(int index)
    {
        OnJumpRequested?.Invoke(index);
        SetVisibility(false);
    }
}
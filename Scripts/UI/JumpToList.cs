using SadChromaLib.UI;
using SadChromaLib.Specialisations.Dialogue.Types;

namespace ScriptEditor;

public sealed partial class JumpToList: BaseListView<DialogueNode>
{
    DialogueGraph _data;

    public void Configure(DialogueGraph data)
    {
        if (data is null)
            return;

        _data = data;

        ReserveItems(data.Nodes.Length);
        Regenerate();
        Update();
    }

    #region List

    protected override int GetDataCount()
    {
        if (_data is null ||
            _data.Nodes is null)
        {
            return 0;
        }

        return _data.Nodes.Length;
    }

    protected override DialogueNode GetDataAt(int i) {
        return _data.Nodes[i];
    }

    #endregion
}
namespace ScriptEditor;

public static class Extensions
{
    public static void BindButton(this Node node, string name, Action callback) {
		node.GetNode<Button>(name).Pressed += callback;
	}
}
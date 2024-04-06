namespace ScriptEditor;

public struct ScriptInfoData
{
    public readonly string[] CharacterNames;
    public readonly string[] VariableNames;
    public readonly string[] TagNames;

    public ScriptInfoData(string[] a, string[] b, string[] c)
    {
        CharacterNames = a;
        VariableNames = b;
        TagNames = c;
    }
}
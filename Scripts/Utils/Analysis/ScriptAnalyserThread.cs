using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;

using SadChromaLib.Specialisations.Dialogue;

namespace ScriptEditor;

public sealed class ScriptAnalyserThread
{
    public event Action<ScriptInfoData> OnDataAvailable;

    bool _needsUpdate;

    Task<ScriptInfoData> _task;

    public ScriptAnalyserThread() {
        _needsUpdate = false;
    }

    public void AwaitTasks() {
        _task?.Wait();
    }

    public async void Analyse(string script, Encoding encoding = null)
    {
        _needsUpdate = true;

        if (!_task?.IsCompleted ?? false)
            return;

        _needsUpdate = false;
        _task = Task.Run(() => AnalyserMain(script, ref _needsUpdate));

        await _task;

        OnDataAvailable?.Invoke(_task.Result);
        _task = null;
    }

    private static ScriptInfoData AnalyserMain(string script, ref bool needsUpdate)
    {
        MemoryStream stream = new(Encoding.UTF8.GetBytes(script));

        HashSet<string> characters = new();
        HashSet<string> variables = new();
        HashSet<string> tags = new();

        // Simplified script parser for analysis
        using (StreamReader reader = new(stream)) {
            while (!reader.EndOfStream) {
                if (needsUpdate)
                    break;

                string line = reader.ReadLine();
                DialogueParser.LineType type = DialogueParser.IdentifyBB(line);

                if (line is null)
                    break;

                switch (type)
                {
                    case DialogueParser.LineType.Character:
                        characters.Add(DialogueParser.ParseCharacterName(line));
                        break;

                    case DialogueParser.LineType.Dialogue:
                        DialogueParser.ParseAndResolveVariables(line, (string varName) => {
                            variables.Add(varName);
                            return varName;
                        });
                        break;

                    case DialogueParser.LineType.Tag:
                        tags.Add(DialogueParser.ParseTag(line));
                        break;
                }
            }
        }

        return new ScriptInfoData(
            SetToArray(characters),
            SetToArray(variables),
            SetToArray(tags)
        );
    }

    private static string[] SetToArray(HashSet<string> set)
    {
        var array = new string[set.Count];
        int i = 0;

        foreach (string key in set) {
            array[i] = key;
            i ++;
        }

        return array;
    }
}
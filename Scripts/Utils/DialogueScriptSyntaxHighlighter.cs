using Godot.Collections;
using System.Text.RegularExpressions;

using SadChromaLib.Specialisations.Dialogue;

namespace ScriptEditor;

/// <summary>
/// A syntax highlighter for Dialogue Script files.
/// Attach it to a CodeEdit to use.
/// </summary>
[GlobalClass]
public sealed partial class DialogueScriptSyntaxHighlighter: SyntaxHighlighter
{
	[Export]
	Color _foregroundColour = Colors.Black;

	[Export]
	Color _commentColour = Colors.GreenYellow;

	[Export]
	Color _characterColour = Colors.DarkGray;

	[Export]
	Color _tagColourA = Colors.Orange;

	[Export]
	Color _tagColourB = Colors.Salmon;

	[Export]
	Color _choiceColour = Colors.Wheat;

	[Export]
	Color _commandColour = Colors.Pink;

	[Export]
	Color _variableColour = Colors.SeaGreen;

	Dictionary<DialogueParser.LineType, Dictionary<string, Color>> _colourDicts;
	Dictionary<string, Color> _variableColourDict;
	Dictionary<string, Color> _targetTagColourDict;

	public override Dictionary _GetLineSyntaxHighlighting(int line)
	{
		Initialise();
		Dictionary properties = new();

		string textLine = GetTextEdit().GetLine(line);
		DialogueParser.LineType type = DialogueParser.IdentifyBB(textLine);

		switch (type)
		{
			case DialogueParser.LineType.Choice:
				string strippedLine = DialogueParser.StripEmpty(textLine).ToString();

				// Apply colouring depending on its inner type
				switch (DialogueParser.IdentifyBB(strippedLine))
				{
					case DialogueParser.LineType.Dialogue:
						properties[0] = _colourDicts[DialogueParser.LineType.Choice];
						break;

					case DialogueParser.LineType.Tag:
						properties[0] = _targetTagColourDict;
						break;
				}
				break;

			case DialogueParser.LineType.Dialogue:
				ScanVariables(textLine, ref properties);
				break;

			case DialogueParser.LineType.Comment:
			case DialogueParser.LineType.Character:
			case DialogueParser.LineType.Command:
			case DialogueParser.LineType.Tag:
				properties[0] = _colourDicts[type];
				break;

			default:
				return base._GetLineSyntaxHighlighting(line);
		}

		return properties;
	}

	private void ScanVariables(string line, ref Dictionary result)
	{
		MatchCollection matches = DialogueParser.RegexVars.Matches(line);

		for (int i = 0; i < matches.Count; ++i) {
			int start = matches[i].Index;

			result[start] = _variableColourDict;
			result[start + matches[i].Length] = _foregroundColour;
		}
	}

	private void Initialise()
	{
		if (_colourDicts is not null)
			return;

		_colourDicts = new() {
			[DialogueParser.LineType.Comment] = MakeColourDict(_commentColour),
			[DialogueParser.LineType.Character] = MakeColourDict(_characterColour),
			[DialogueParser.LineType.Tag] = MakeColourDict(_tagColourA),
			[DialogueParser.LineType.Choice] = MakeColourDict(_choiceColour),
			[DialogueParser.LineType.Command] = MakeColourDict(_commandColour),
			[DialogueParser.LineType.Dialogue] = MakeColourDict(_foregroundColour)
		};

		_variableColourDict = MakeColourDict(_variableColour);
		_targetTagColourDict = MakeColourDict(_tagColourB);
	}

	private static Dictionary<string, Color> MakeColourDict(Color colour)
		=> new() { ["color"] = colour };
}
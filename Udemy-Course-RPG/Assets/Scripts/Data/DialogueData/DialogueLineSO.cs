using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Dialogue Data/New Line", fileName = "Line - ")]
public class DialogueLineSO : ScriptableObject
{
    [Header("Dialogue info")]
    public string dialogueGroupName;
    public DialogueSpeakerSO speaker;

    [Header("Text options")]
    [TextArea] public string[] textLine;


    [Header("Choice info")]
    [TextArea] public string playerChoiceAnser;
    public DialogueLineSO[] choiceLines;

    [Header("Dialogue Action")]
    [TextArea] public string actionLine;
    public DialogueActionType actionType;

    public string GetFirstLine() => textLine[0];

    public string GetRandomLine()
    {
        return textLine[Random.Range(0, textLine.Length)];
    }
}

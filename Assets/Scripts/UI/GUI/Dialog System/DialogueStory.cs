using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue Story", menuName = "Dialogue/Story")]
public class DialogueStory : ScriptableObject
{
    public string storyName = "Story Name";

    [TextArea(3, 10)]
    public string[] sentences;
}
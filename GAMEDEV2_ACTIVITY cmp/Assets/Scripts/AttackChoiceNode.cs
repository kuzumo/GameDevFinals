using UnityEngine;
using XNode;

[CreateNodeMenu("Game/Attack Choice")]
public class AttackChoiceNode : BaseNode
{
    [Input] public string entry;
    [Output] public string a;
    [Output] public string b;
    [Output] public string c;

    [TextArea] public string nodeText = "Choose your attack";
    


    public override string getDialogText() => nodeText;
}

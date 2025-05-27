using UnityEngine;
using XNode;

[CreateNodeMenu("Combat/Combat Check")]
public class CombatCheckNode : BaseNode
{

    [Input] public string entry;
    [Output] public string success;
    [Output] public string failed;
    [Output] public string victory;


    [TextArea(5, 15)]
    public string dialogText;
    public Sprite dialogImage;

    public ABILITY abilityUsed;
    public float difficultyCheckValue;

    public override string getDialogText() => dialogText;
    public override Sprite getSprite() => dialogImage;
    public ABILITY getAbility() => abilityUsed;
    public float getDC() => difficultyCheckValue;
}

using System.Collections;
using UnityEngine;
using XNode;

public class SixChoiceDialog : BaseNode
{
    [Input] public string entry;

    [Output] public string a;
    [Output] public string b;
    [Output] public string c;
    [Output] public string d;
    [Output] public string e;
    [Output] public string f;

    [TextArea(7, 20)]
    public string dialogText;
    public Sprite dialogImage;

    public string aText;
    public string bText;
    public string cText;
    public string dText;
    public string eText;
    public string fText;

    public override string getDialogText()
    {
        return dialogText;
    }

    public override Sprite getSprite()
    {
        return dialogImage;
    }
}

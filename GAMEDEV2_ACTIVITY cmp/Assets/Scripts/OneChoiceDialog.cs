using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using XNode;

public class OneChoiceDialog : BaseNode
{
    [Input] public string entry;
    [Output] public string c;


    [TextArea(7, 20)]
    public string dialogText;
    public Sprite dialogImage;

    [TextArea(2, 5)] public string cText;

    public AudioClip bgm;
    public BGM bgmCheck;

    public Sprite actorImage;
    public bool slideInActor;

    public string characterName;

    public override string getCharacterName()
    {
        return characterName;
    }

    public override string getDialogText()
    {
        return dialogText;
    }

    public override Sprite getSprite()
    {
        return dialogImage;
    }

    public override BGM getBGMName()
    {
        return bgmCheck;
    }

    public override Sprite getActorSprite()
    {
        return actorImage;
    }

    public override bool isSliding()
    {
        return slideInActor;
    }

    public string GetChoiceAText()
    {
        return cText;
    }
   
}

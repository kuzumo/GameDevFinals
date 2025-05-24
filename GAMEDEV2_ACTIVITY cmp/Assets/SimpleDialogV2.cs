using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using XNode;

public class SimpleDialogV2 : BaseNode {

	[Input] public string entry;
	[Output] public string exit;

	[TextArea(7,20)]
	public string dialogText;
	public Sprite dialogImage;
    

    public Sprite actorImage;

	public AudioClip bgm;

	public BGM bgmCheck;

	public bool slideInActor;

    public string characterName;

    public string nextButtonLabel = "Continue";


    public override string getCharacterName()
    {
        return characterName;
    }

    public override string getDialogText(){
        return dialogText;
    }

    public override Sprite getSprite(){
        return dialogImage;
    }


    public override Sprite getActorSprite(){
        return actorImage;
    }

	public override BGM getBGMName(){
		return bgmCheck;
	}

	public override bool isSliding(){
		return slideInActor;
	}
    public string getNextButtonLabel()
    {
        return nextButtonLabel;
    }
    
}
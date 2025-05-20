using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

public class MultipleChoiceDialog : BaseNode {

	[Input] public string entry;
	[Output] public string a;
	[Output] public string b;
[TextArea(7,20)]
	public string dialogText;
	public Sprite dialogImage;
	

	 public override string getDialogText(){
        return dialogText;
    }

    public override Sprite getSprite(){
        return dialogImage;
    }
}
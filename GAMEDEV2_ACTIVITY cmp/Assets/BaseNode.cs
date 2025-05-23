﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

public class BaseNode : Node {
    public virtual string getCharacterName()
    {
        return "";
    }

    public virtual string getDialogText(){
        return "";
    }

    public virtual Sprite getSprite(){
        return null;
    }
	
    public virtual ABILITY getAbility(){
        return ABILITY.PERCEPTION;
    }

    public virtual float getDC(){
        return 10;
    }

    
    public virtual Sprite getActorSprite(){
        return null;
    }

	public virtual BGM getBGMName(){
		return BGM.HAPPY;
	}

    public virtual bool isSliding(){
        return false;
    }
}
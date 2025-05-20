using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XNode;
using UnityEngine.EventSystems;
using UnityEngine.AI;
using Random = UnityEngine.Random;
public class NodeReader : MonoBehaviour
{
   public TMP_Text dialogue;
   public Sprite backgroundImage;
   public GameObject ImageGO;
   public NodeGraph graph;
   public BaseNode currentNode;
   public GameObject characterSheet;
   public GameObject buttonA;
   public TMPro.TMP_Text buttonAText;
   public TMPro.TMP_Text buttonBText;
    public GameObject buttonB;

    public AudioClip bgm;

    public AudioSource bgmObject;

    public AudioClip Happy;
    public AudioClip Adventure;
    public AudioClip Drama;
    public AudioClip Suspense;

    public Sprite actor;

    public GameObject actorObject;

   public GameObject nextButtonGO;

   public Animator animationOfActor;

void Start(){
currentNode = getStartNode();
AdvanceDialog();
}

public BaseNode getStartNode(){
    return graph.nodes.Find(node => node is BaseNode && node.name == "Start") as BaseNode;
}

public void displayNode(BaseNode node){
    dialogue.text = node.getDialogText();
    backgroundImage = node.getSprite();
    ImageGO.GetComponent<UnityEngine.UI.Image>().sprite = backgroundImage;

    if(node is MultipleChoiceDialog){
        buttonA.SetActive(true);
        buttonB.SetActive(true);
        nextButtonGO.SetActive(false);
      buttonAText.text = "" + ((MultipleChoiceDialog)node).a;
      buttonBText.text = "" + ((MultipleChoiceDialog)node).b;
    } else {
        buttonA.SetActive(false);
        buttonB.SetActive(false);
        nextButtonGO.SetActive(true);
    }

    if(node is SimpleDialogV2){
        actorObject.SetActive(false);

        Debug.Log(actor);
    
        if(actor != null){
            actorObject.SetActive(true);
        }

        
        
        actor = node.getActorSprite();
        actorObject.GetComponent<UnityEngine.UI.Image>().sprite = actor;
        if(node.getBGMName() == BGM.HAPPY){
            bgm = Happy;
        }else if (node.getBGMName() == BGM.DRAMA){
            bgm = Drama;
        }else if (node.getBGMName() == BGM.ADVENTURE){
            bgm = Adventure;
        }else if (node.getBGMName() == BGM.SUSPENSE){
            bgm = Suspense;
        }
        bgmObject.clip = bgm;
        bgmObject.Play();

        if(node.isSliding()){
            //animation enabled
            actorObject.GetComponent<Animator>().enabled = true;
        }
    }

}

public void AdvanceDialog(){
    var nextNode = GetNextNode(currentNode);
    if(nextNode != null){
        currentNode = nextNode;
        displayNode(currentNode);
    } else {
        Debug.Log("No dialogues are found.");
    }
}

private BaseNode GetNextNode(BaseNode node){
    if(node is MultipleChoiceDialog){
            GameObject gameObject = EventSystem.current.currentSelectedGameObject;
            TMP_Text buttonText = gameObject.GetComponentInChildren<TMP_Text>();

            if(buttonText.text ==("" +((MultipleChoiceDialog)node).a)){
            return currentNode.GetOutputPort("a")?.Connection.node as BaseNode;
            } 
            if(buttonText.text ==("" +((MultipleChoiceDialog)node).b)){
            return currentNode.GetOutputPort("b")?.Connection.node as BaseNode;
            }
            return currentNode.GetOutputPort("exit")?.Connection.node as BaseNode;
    }else if(node is AbilityCheckNode){
            int d20 = Random.Range(0,21);
            if((d20+characterSheet.gameObject.GetComponent<CharacterStats>().survival) >= ((AbilityCheckNode)node).getDC()){
                return currentNode.GetOutputPort("success")?.Connection.node as BaseNode;
            } else {
                return currentNode.GetOutputPort("failed")?.Connection.node as BaseNode;
            }
    }
    else{
return currentNode.GetOutputPort("exit")?.Connection.node as BaseNode;

    }

  }
}

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
   public GameObject buttonB;
    public GameObject buttonC;
    public GameObject buttonD;
    public GameObject buttonE;
    public GameObject buttonF;
    public TMPro.TMP_Text buttonAText;
    public TMPro.TMP_Text buttonBText;
    public TMPro.TMP_Text buttonCText;
    public TMPro.TMP_Text buttonDText;
    public TMPro.TMP_Text buttonEText;
    public TMPro.TMP_Text buttonFText;


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

    public void displayNode(BaseNode node)
    {
        // Set dialogue and background image
        dialogue.text = node.getDialogText();
        backgroundImage = node.getSprite();
        ImageGO.GetComponent<UnityEngine.UI.Image>().sprite = backgroundImage;

        // Hide all buttons by default
        buttonA.SetActive(false);
        buttonB.SetActive(false);
        buttonC.SetActive(false);
        buttonD?.SetActive(false);
        buttonE?.SetActive(false);
        buttonF?.SetActive(false);
        nextButtonGO.SetActive(false);

        // Handle SixChoiceDialog
        if (node is SixChoiceDialog scd)
        {
            buttonA.SetActive(true); buttonAText.text = scd.aText;
            buttonB.SetActive(true); buttonBText.text = scd.bText;
            buttonC.SetActive(true); buttonCText.text = scd.cText;
            buttonD.SetActive(true); buttonDText.text = scd.dText;
            buttonE.SetActive(true); buttonEText.text = scd.eText;
            buttonF.SetActive(true); buttonFText.text = scd.fText;
        }
        // Handle ThreeChoiceDialog
        else if (node is ThreeChoiceDialog tcd)
        {
            buttonA.SetActive(true); buttonAText.text = tcd.aText;
            buttonB.SetActive(true); buttonBText.text = tcd.bText;
            buttonC.SetActive(true); buttonCText.text = tcd.cText;
        }
        // Handle MultipleChoiceDialog (2-choice)
        else if (node is MultipleChoiceDialog mcd)
        {
            buttonA.SetActive(true); buttonAText.text = mcd.aText;
            buttonB.SetActive(true); buttonBText.text = mcd.bText;
        }
        // Handle default node (just continue)
        else
        {
            nextButtonGO.SetActive(true);
        }

        // Handle SimpleDialogV2 actor, BGM, animation
        if (node is SimpleDialogV2 sdv)
        {
            actorObject.SetActive(false);
            actor = node.getActorSprite();

            if (actor != null)
            {
                actorObject.SetActive(true);
                actorObject.GetComponent<UnityEngine.UI.Image>().sprite = actor;
            }

            // Background music
            switch (node.getBGMName())
            {
                case BGM.HAPPY: bgm = Happy; break;
                case BGM.DRAMA: bgm = Drama; break;
                case BGM.ADVENTURE: bgm = Adventure; break;
                case BGM.SUSPENSE: bgm = Suspense; break;
            }

            bgmObject.clip = bgm;
            bgmObject.Play();

            // Animation
            if (node.isSliding())
            {
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

        if (node is ThreeChoiceDialog)
        {
            ThreeChoiceDialog tcd = node as ThreeChoiceDialog;
            GameObject gameObject = EventSystem.current.currentSelectedGameObject;
            TMP_Text buttonText = gameObject.GetComponentInChildren<TMP_Text>();

            if (buttonText.text == tcd.aText)
                return currentNode.GetOutputPort("a")?.Connection.node as BaseNode;
            if (buttonText.text == tcd.bText)
                return currentNode.GetOutputPort("b")?.Connection.node as BaseNode;
            if (buttonText.text == tcd.cText)
                return currentNode.GetOutputPort("c")?.Connection.node as BaseNode;

            Debug.LogWarning("Choice did not match A/B/C.");
            return null;
        }


        if (node is MultipleChoiceDialog)
        {
            // Safely cast the node
            MultipleChoiceDialog mcd = node as MultipleChoiceDialog;

            // Get which button was clicked
            GameObject gameObject = EventSystem.current.currentSelectedGameObject;
            TMP_Text buttonText = gameObject.GetComponentInChildren<TMP_Text>();

            // Match the text with the choices
            if (buttonText.text == mcd.aText)
            {
                return currentNode.GetOutputPort("a")?.Connection.node as BaseNode;
            }
            if (buttonText.text == mcd.bText)
            {
                return currentNode.GetOutputPort("b")?.Connection.node as BaseNode;
            }

            // Fallback
            Debug.LogWarning("Choice did not match A or B.");
            return null;
        }
        else if (node is AbilityCheckNode)
        {
            int d20 = Random.Range(0, 21);
            if ((d20 + characterSheet.gameObject.GetComponent<CharacterStats>().survival) >= ((AbilityCheckNode)node).getDC())
            {
                return currentNode.GetOutputPort("success")?.Connection.node as BaseNode;
            }
            else
            {
                return currentNode.GetOutputPort("failed")?.Connection.node as BaseNode;
            }
        }
        else
        {
            return currentNode.GetOutputPort("exit")?.Connection.node as BaseNode;

        }

  }
}

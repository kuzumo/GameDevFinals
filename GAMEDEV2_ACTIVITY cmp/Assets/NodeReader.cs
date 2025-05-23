﻿using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XNode;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

public class NodeReader : MonoBehaviour
{
    public TMP_Text characterNameText;
    public TMP_Text dialogue;
    public Sprite backgroundImage;
    public GameObject ImageGO;
    public NodeGraph graph;
    public BaseNode currentNode;
    public GameObject endPanel;
    public GameObject characterSheet;

    public GameObject buttonA, buttonB, buttonC, buttonD, buttonE, buttonF;
    public TMP_Text buttonAText, buttonBText, buttonCText, buttonDText, buttonEText, buttonFText;

    public AudioClip bgm;
    public AudioSource bgmObject;
    public AudioClip Happy, Adventure, Drama, Suspense;

    public Sprite actor;
    public GameObject actorObject;
    public GameObject nextButtonGO;
    public TMP_Text nextButtonText;
    public Animator animationOfActor;

    public DiceRollPanelController diceRollPanel; // ✅ Drag in inspector
    public CharacterStats characterStats;         // ✅ Drag in inspector

    void Start()
    {
        currentNode = getStartNode();
        AdvanceDialog();
    }

    public BaseNode getStartNode()
    {
        return graph.nodes.Find(node => node is BaseNode && node.name == "Start") as BaseNode;
    }

    public void displayNode(BaseNode node)
    {
        characterNameText.text = node.getCharacterName();
        dialogue.text = node.getDialogText();
        backgroundImage = node.getSprite();
        ImageGO.GetComponent<Image>().sprite = backgroundImage;

        // Hide buttons
        buttonA.SetActive(false); buttonB.SetActive(false); buttonC.SetActive(false);
        buttonD.SetActive(false); buttonE.SetActive(false); buttonF.SetActive(false);
        nextButtonGO.SetActive(false);

        // Show relevant buttons

        if (node is SixChoiceDialog scd)
        {
            buttonA.SetActive(true); buttonAText.text = scd.aText;
            buttonB.SetActive(true); buttonBText.text = scd.bText;
            buttonC.SetActive(true); buttonCText.text = scd.cText;
            buttonD.SetActive(true); buttonDText.text = scd.dText;
            buttonE.SetActive(true); buttonEText.text = scd.eText;
            buttonF.SetActive(true); buttonFText.text = scd.fText;
        }
        else if (node is ThreeChoiceDialog tcd)
        {
            buttonA.SetActive(true); buttonAText.text = tcd.aText;
            buttonB.SetActive(true); buttonBText.text = tcd.bText;
            buttonC.SetActive(true); buttonCText.text = tcd.cText;
        }
        else if (node is OneChoiceDialog ocd)
        {
            buttonC.SetActive(true);
            buttonCText.text = ocd.GetChoiceAText();
        }
        else if (node is MultipleChoiceDialog mcd)
        {
            buttonA.SetActive(true); buttonAText.text = mcd.aText;
            buttonB.SetActive(true); buttonBText.text = mcd.bText;
        }
        else if (node is AbilityCheckNode acn)
        {
            HandleAbilityCheck(acn);
            return;
        }
        else
        {
            nextButtonGO.SetActive(true);
            nextButtonText.text = "Next";
            if (node is SimpleDialogV2 sdv && !string.IsNullOrEmpty(sdv.nextButtonLabel))
            {
                nextButtonText.text = sdv.nextButtonLabel;
            }
        }

        // Actor & BGM handling
        actorObject.SetActive(false);
        actor = node.getActorSprite();
        if (actor != null)
        {
            actorObject.SetActive(true);
            actorObject.GetComponent<Image>().sprite = actor;
        }

        switch (node.getBGMName())
        {
            case BGM.HAPPY: bgm = Happy; break;
            case BGM.DRAMA: bgm = Drama; break;
            case BGM.ADVENTURE: bgm = Adventure; break;
            case BGM.SUSPENSE: bgm = Suspense; break;
        }

        bgmObject.clip = bgm;
        bgmObject.Play();

        if (node.isSliding())
        {
            actorObject.GetComponent<Animator>().enabled = true;
        }
    }

    public void AdvanceDialog()
    {
        var nextNode = GetNextNode(currentNode);
        if (nextNode != null)
        {
            currentNode = nextNode;
            displayNode(currentNode);
        }
        else
        {
            endPanel.SetActive(true);
        }
    }

    private BaseNode GetNextNode(BaseNode node)
    {
        if (node is SixChoiceDialog scd)
        {
            string clicked = EventSystem.current.currentSelectedGameObject.GetComponentInChildren<TMP_Text>().text;
            if (clicked == scd.aText) return currentNode.GetOutputPort("a")?.Connection.node as BaseNode;
            if (clicked == scd.bText) return currentNode.GetOutputPort("b")?.Connection.node as BaseNode;
            if (clicked == scd.cText) return currentNode.GetOutputPort("c")?.Connection.node as BaseNode;
            if (clicked == scd.dText) return currentNode.GetOutputPort("d")?.Connection.node as BaseNode;
            if (clicked == scd.eText) return currentNode.GetOutputPort("e")?.Connection.node as BaseNode;
            if (clicked == scd.fText) return currentNode.GetOutputPort("f")?.Connection.node as BaseNode;
            return currentNode.GetOutputPort("exit")?.Connection.node as BaseNode;
        }

        if (node is ThreeChoiceDialog tcd)
        {
            string clicked = EventSystem.current.currentSelectedGameObject.GetComponentInChildren<TMP_Text>().text;
            if (clicked == tcd.aText) return currentNode.GetOutputPort("a")?.Connection.node as BaseNode;
            if (clicked == tcd.bText) return currentNode.GetOutputPort("b")?.Connection.node as BaseNode;
            if (clicked == tcd.cText) return currentNode.GetOutputPort("c")?.Connection.node as BaseNode;
            return null;
        }

        if (node is MultipleChoiceDialog mcd)
        {
            string clicked = EventSystem.current.currentSelectedGameObject.GetComponentInChildren<TMP_Text>().text;
            if (clicked == mcd.aText) return currentNode.GetOutputPort("a")?.Connection.node as BaseNode;
            if (clicked == mcd.bText) return currentNode.GetOutputPort("b")?.Connection.node as BaseNode;
            return null;
        }

        if (node is OneChoiceDialog ocd)
        {
            string clicked = EventSystem.current.currentSelectedGameObject.GetComponentInChildren<TMP_Text>().text;
            if (clicked == ocd.GetChoiceAText())
            {
                return currentNode.GetOutputPort("c")?.Connection.node as BaseNode;
            }
            return null;
        }

        if (node is AbilityCheckNode) return null; // wait for panel callback

        return currentNode.GetOutputPort("exit")?.Connection.node as BaseNode;
    }

    private void HandleAbilityCheck(AbilityCheckNode node)
    {
        string skillName = node.getAbility().ToString();
        int dc = Mathf.RoundToInt(node.getDC());
        DisableAllButtons();

        diceRollPanel.Show(skillName, dc, (bool success) =>
        {
            BaseNode nextNode = success
                ? currentNode.GetOutputPort("success")?.Connection.node as BaseNode
                : currentNode.GetOutputPort("failed")?.Connection.node as BaseNode;

            currentNode = nextNode;
            displayNode(currentNode);
        });
    }

    private void DisableAllButtons()
    {
        buttonA.SetActive(false);
        buttonB.SetActive(false);
        buttonC.SetActive(false);
        buttonD.SetActive(false);
        buttonE.SetActive(false);
        buttonF.SetActive(false);
        nextButtonGO.SetActive(false);
    }

    public void RestartScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}

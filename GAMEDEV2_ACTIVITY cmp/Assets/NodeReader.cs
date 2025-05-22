using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XNode;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;
using UnityEngine.Video;
using UnityEditor.Experimental.GraphView;

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
    public GameObject previousButtonGO;
    private BaseNode previousNode;
    public TMP_Text nextButtonText;
    public Animator animationOfActor;
    public RawImage videoBackground;
    public VideoPlayer videoPlayerBG;
    public GameObject videoBGPanel;
    public RenderTexture videoRenderTexture;
    public DiceRollPanelController diceRollPanel; 
    public CharacterStats characterStats;
    public GameObject choicesPanel;
    private bool isTyping = false;
    


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

        if (node is MultipleChoiceDialog || node is ThreeChoiceDialog || node is SixChoiceDialog || node is OneChoiceDialog)
        {
            previousButtonGO.SetActive(true);
        }
        else
        {
            previousButtonGO.SetActive(false);
        }

        StartTyping(node.getDialogText(), node);


        // 🎥 Handle video or image background
        videoPlayerBG.Stop();
        videoBGPanel.SetActive(false);
        ImageGO.SetActive(true);

        VideoClip bgVideo = node.getBackgroundVideo();
        if (bgVideo != null)
        {
            videoPlayerBG.Stop();
            videoPlayerBG.clip = bgVideo;

            // ✅ Explicitly assign target texture (REQUIRED)
            videoPlayerBG.targetTexture = videoRenderTexture;

            videoBGPanel.SetActive(true);
            ImageGO.SetActive(false);

            videoPlayerBG.Play();
        }

        else
        {
            // fallback to image background
            backgroundImage = node.getSprite();
            ImageGO.GetComponent<Image>().sprite = backgroundImage;
        }


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
            if (node is SimpleDialogV2 sdv2 && !string.IsNullOrEmpty(sdv2.nextButtonLabel))
            {
                nextButtonText.text = sdv2.nextButtonLabel;
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
        if (nextNode != null)
        {
            previousNode = currentNode; // Save current before moving
            currentNode = nextNode;
            displayNode(currentNode);
        }

    }



    public float typingSpeed = 0.20f; // Speed per letter

    private Coroutine typingCoroutine;

    public void StartTyping(string fullText, BaseNode node)
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(fullText, node));
    }

    private IEnumerator TypeText(string fullText, BaseNode node)
{
    isTyping = true;
    dialogue.text = "";

    // Hide choices while typing
    choicesPanel.SetActive(false);

    foreach (char c in fullText)
    {
        dialogue.text += c;
        yield return new WaitForSeconds(typingSpeed);
    }

    isTyping = false;
    typingCoroutine = null;

    // Show buttons after typing ends
    ShowChoices(node);
}
    private void ShowChoices(BaseNode node)
    {
        choicesPanel.SetActive(false); // default hidden

        // Reset all buttons inside the panel
        buttonA.SetActive(false); buttonB.SetActive(false);
        buttonC.SetActive(false); buttonD.SetActive(false);
        buttonE.SetActive(false); buttonF.SetActive(false);

        if (node is SixChoiceDialog scd)
        {
            buttonA.SetActive(true); buttonAText.text = scd.aText;
            buttonB.SetActive(true); buttonBText.text = scd.bText;
            buttonC.SetActive(true); buttonCText.text = scd.cText;
            buttonD.SetActive(true); buttonDText.text = scd.dText;
            buttonE.SetActive(true); buttonEText.text = scd.eText;
            buttonF.SetActive(true); buttonFText.text = scd.fText;
            choicesPanel.SetActive(true); // ✅ now show panel
        }
        else if (node is MultipleChoiceDialog mcd)
        {
            buttonA.SetActive(true); buttonAText.text = mcd.aText;
            buttonB.SetActive(true); buttonBText.text = mcd.bText;
            choicesPanel.SetActive(true);
        }
        else if (node is OneChoiceDialog ocd)
        {
            buttonC.SetActive(true); buttonCText.text = ocd.GetChoiceAText();
            choicesPanel.SetActive(true);
        }
        else if (node is ThreeChoiceDialog tcd)
        {
            buttonA.SetActive(true); buttonAText.text = tcd.aText;
            buttonB.SetActive(true); buttonBText.text = tcd.bText;
            buttonC.SetActive(true); buttonCText.text = tcd.cText;
            choicesPanel.SetActive(true);
        }
        else
        {
            // Default: show "Next" button instead
            nextButtonGO.SetActive(true);
        }
    }

    public void SkipTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            dialogue.text = currentNode.getDialogText();
            typingCoroutine = null;

            ShowChoices(currentNode); // show choices after skip
        }
        else
        {
            // Do nothing here to prevent advancing on second click
        }
    }



    private BaseNode GetNextNode(BaseNode node)
    {
        if (node is SixChoiceDialog scd)
        {
            string clicked = EventSystem.current.currentSelectedGameObject.GetComponentInChildren<TMP_Text>().text;
            if (clicked == scd.aText) { previousNode = currentNode; return currentNode.GetOutputPort("a")?.Connection.node as BaseNode; }
            if (clicked == scd.bText) { previousNode = currentNode; return currentNode.GetOutputPort("b")?.Connection.node as BaseNode; }
            if (clicked == scd.cText) { previousNode = currentNode; return currentNode.GetOutputPort("c")?.Connection.node as BaseNode; }
            if (clicked == scd.dText) { previousNode = currentNode; return currentNode.GetOutputPort("d")?.Connection.node as BaseNode; }
            if (clicked == scd.eText) { previousNode = currentNode; return currentNode.GetOutputPort("e")?.Connection.node as BaseNode; }
            if (clicked == scd.fText) { previousNode = currentNode; return currentNode.GetOutputPort("f")?.Connection.node as BaseNode; }
            return currentNode.GetOutputPort("exit")?.Connection.node as BaseNode;
        }

        if (node is ThreeChoiceDialog tcd)
        {
            string clicked = EventSystem.current.currentSelectedGameObject.GetComponentInChildren<TMP_Text>().text;
            if (clicked == tcd.aText) { previousNode = currentNode; return currentNode.GetOutputPort("a")?.Connection.node as BaseNode; }
            if (clicked == tcd.bText) { previousNode = currentNode; return currentNode.GetOutputPort("b")?.Connection.node as BaseNode; }
            if (clicked == tcd.cText) { previousNode = currentNode; return currentNode.GetOutputPort("c")?.Connection.node as BaseNode; }
            return null;
        }

        if (node is MultipleChoiceDialog mcd)
        {
            string clicked = EventSystem.current.currentSelectedGameObject.GetComponentInChildren<TMP_Text>().text;
            if (clicked == mcd.aText) { previousNode = currentNode; return currentNode.GetOutputPort("a")?.Connection.node as BaseNode; }
            if (clicked == mcd.bText) { previousNode = currentNode; return currentNode.GetOutputPort("b")?.Connection.node as BaseNode; }
            return null;
        }

        if (node is OneChoiceDialog ocd)
        {
            string clicked = EventSystem.current.currentSelectedGameObject.GetComponentInChildren<TMP_Text>().text;
            if (clicked == ocd.GetChoiceAText())
            {
                previousNode = currentNode;
                return currentNode.GetOutputPort("c")?.Connection.node as BaseNode;
            }
            return null;
        }

        if (node is AbilityCheckNode)
            return null; // wait for panel callback

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

    public void GoToPreviousNode()
    {
        if (previousNode != null)
        {
            currentNode = previousNode;
            displayNode(currentNode);
        }
        else
        {
            Debug.LogWarning("No previous node to return to.");
        }
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

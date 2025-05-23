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
using UnityEngine.SceneManagement;

public class NodeReader : MonoBehaviour
{
    public TMP_Text characterNameText;
    public TMP_Text dialogue;
    public Sprite backgroundImage;
    public GameObject ImageGO;
    public NodeGraph graph;
    public BaseNode currentNode;
    private Stack<BaseNode> nodeHistory = new Stack<BaseNode>();
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
    private BaseNode lastChoiceNode;

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
    private bool justLoadedFromSave = false;

    void Start()
    {
        lastChoiceNode = null;
        nodeHistory.Clear();

        Debug.Log("NodeReader Start() called.");

        bool isNewGame = PlayerPrefs.GetInt("IsNewGame", 1) == 1;

        if (!isNewGame && (PlayerPrefs.HasKey("SavedNodeGUID") || PlayerPrefs.HasKey("SavedNodeName")))
        {
            Debug.Log("Loading saved node...");
            LoadSavedNode();
        }
        else
        {
            currentNode = getStartNode();
            Debug.Log("Found Start node: " + (currentNode != null ? currentNode.name : "null"));

            var nextNode = currentNode.GetOutputPort("exit")?.Connection.node as BaseNode;
            if (nextNode != null)
            {
                currentNode = nextNode;
                Debug.Log("Displaying first real node: " + currentNode.name);
                displayNode(currentNode);
            }
            else
            {
                Debug.LogWarning("Start node does not lead anywhere.");
            }
        }

        PlayerPrefs.SetInt("IsNewGame", 0);
        PlayerPrefs.Save();
    }

    public BaseNode getStartNode()
    {
        return graph.nodes.Find(node => node is BaseNode && node.name == "Start") as BaseNode;
    }

    public void displayNode(BaseNode node)
    {
        currentNode = node;
        characterNameText.text = node.getCharacterName();

        if (node is MultipleChoiceDialog || node is ThreeChoiceDialog || node is SixChoiceDialog || node is OneChoiceDialog)
        {
            lastChoiceNode = node;
            previousButtonGO.SetActive(true);
        }
        else
        {
            previousButtonGO.SetActive(false);
        }

        StartTyping(node.getDialogText(), node);

        videoPlayerBG.Stop();
        videoBGPanel.SetActive(false);
        ImageGO.SetActive(true);

        VideoClip bgVideo = node.getBackgroundVideo();
        if (bgVideo != null)
        {
            videoPlayerBG.Stop();
            videoPlayerBG.clip = bgVideo;
            videoPlayerBG.targetTexture = videoRenderTexture;
            videoBGPanel.SetActive(true);
            ImageGO.SetActive(false);
            videoPlayerBG.Play();
        }
        else
        {
            backgroundImage = node.getSprite();
            ImageGO.GetComponent<Image>().sprite = backgroundImage;
        }

        buttonA.SetActive(false); buttonB.SetActive(false); buttonC.SetActive(false);
        buttonD.SetActive(false); buttonE.SetActive(false); buttonF.SetActive(false);
        nextButtonGO.SetActive(false);

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
            buttonC.SetActive(true); buttonCText.text = ocd.GetChoiceAText();
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
        if (justLoadedFromSave)
        {
            Debug.Log("⛔ Skipping AdvanceDialog because we just loaded from save.");
            justLoadedFromSave = false;
            return;
        }

        var nextNode = GetNextNode(currentNode);

        if (nextNode != null)
        {
            nodeHistory.Push(currentNode);
            currentNode = nextNode;
            displayNode(currentNode);
        }
        else
        {
            endPanel.SetActive(true);
        }
    }

    public float typingSpeed = 0.20f;
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
        choicesPanel.SetActive(false);

        foreach (char c in fullText)
        {
            dialogue.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        typingCoroutine = null;
        ShowChoices(node);
    }

    private void ShowChoices(BaseNode node)
    {
        choicesPanel.SetActive(false);

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
            choicesPanel.SetActive(true);
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
            ShowChoices(currentNode);
        }
    }

    private BaseNode GetNextNode(BaseNode node)
    {
        string clicked = EventSystem.current.currentSelectedGameObject.GetComponentInChildren<TMP_Text>().text;

        if (node is SixChoiceDialog scd)
        {
            if (clicked == scd.aText) return node.GetOutputPort("a")?.Connection.node as BaseNode;
            if (clicked == scd.bText) return node.GetOutputPort("b")?.Connection.node as BaseNode;
            if (clicked == scd.cText) return node.GetOutputPort("c")?.Connection.node as BaseNode;
            if (clicked == scd.dText) return node.GetOutputPort("d")?.Connection.node as BaseNode;
            if (clicked == scd.eText) return node.GetOutputPort("e")?.Connection.node as BaseNode;
            if (clicked == scd.fText) return node.GetOutputPort("f")?.Connection.node as BaseNode;
            return node.GetOutputPort("exit")?.Connection.node as BaseNode;
        }

        if (node is ThreeChoiceDialog tcd)
        {
            if (clicked == tcd.aText) return node.GetOutputPort("a")?.Connection.node as BaseNode;
            if (clicked == tcd.bText) return node.GetOutputPort("b")?.Connection.node as BaseNode;
            if (clicked == tcd.cText) return node.GetOutputPort("c")?.Connection.node as BaseNode;
        }

        if (node is MultipleChoiceDialog mcd)
        {
            if (clicked == mcd.aText) return node.GetOutputPort("a")?.Connection.node as BaseNode;
            if (clicked == mcd.bText) return node.GetOutputPort("b")?.Connection.node as BaseNode;
        }

        if (node is OneChoiceDialog ocd)
        {
            if (clicked == ocd.GetChoiceAText()) return node.GetOutputPort("c")?.Connection.node as BaseNode;
        }

        return node.GetOutputPort("exit")?.Connection.node as BaseNode;
    }

    private void HandleAbilityCheck(AbilityCheckNode node)
    {
        string skillName = node.getAbility().ToString();
        int dc = Mathf.RoundToInt(node.getDC());
        DisableAllButtons();

        diceRollPanel.Show(skillName, dc, success =>
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
        buttonA.SetActive(false); buttonB.SetActive(false); buttonC.SetActive(false);
        buttonD.SetActive(false); buttonE.SetActive(false); buttonF.SetActive(false);
        nextButtonGO.SetActive(false);
    }

    public void GoToPreviousNode()
    {
        if (nodeHistory.Count > 0)
        {
            currentNode = nodeHistory.Pop();
            endPanel.SetActive(false);
            displayNode(currentNode);
        }
        else
        {
            Debug.LogWarning("No previous node in history.");
        }
    }

    public void RetryLastChoice()
    {
        if (lastChoiceNode != null)
        {
            currentNode = lastChoiceNode;
            endPanel.SetActive(false);
            displayNode(currentNode);
        }
    }

    public void SaveCurrentNode()
    {
        if (currentNode != null)
        {
            PlayerPrefs.SetString("SavedNodeGUID", currentNode.GUID);
            PlayerPrefs.Save();
            Debug.Log("✅ Saved current node GUID: " + currentNode.GUID);
        }
    }

    public void LoadSavedNode()
    {
        string savedGUID = PlayerPrefs.GetString("SavedNodeGUID", "");
        if (!string.IsNullOrEmpty(savedGUID))
        {
            BaseNode savedNode = graph.nodes.Find(node => (node as BaseNode)?.GUID == savedGUID) as BaseNode;
            if (savedNode != null)
            {
                currentNode = savedNode;
                justLoadedFromSave = true;
                displayNode(currentNode);
                endPanel.SetActive(false);
                Debug.Log("✅ Loaded saved node by GUID: " + savedNode.GUID);
                return;
            }
        }

        // Fallback
        string savedName = PlayerPrefs.GetString("SavedNodeName", "");
        if (!string.IsNullOrEmpty(savedName))
        {
            BaseNode savedNode = graph.nodes.Find(node => node.name == savedName) as BaseNode;
            if (savedNode != null)
            {
                currentNode = savedNode;
                justLoadedFromSave = true;
                displayNode(currentNode);
                endPanel.SetActive(false);
                Debug.Log("✅ Loaded saved node by NAME: " + savedNode.name);
            }
        }
    }

    public void OnNewGameClicked()
    {
        PlayerPrefs.DeleteKey("SavedNodeGUID");
        PlayerPrefs.DeleteKey("SavedNodeName");
        PlayerPrefs.SetInt("IsNewGame", 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene("PlayScene");
    }

    public void StartNewGame()
    {
        PlayerPrefs.DeleteKey("SavedNodeGUID");
        PlayerPrefs.DeleteKey("SavedNodeName");
        SceneManager.LoadScene("PlayScene");
    }

    public void RestartScene()
    {
        PlayerPrefs.DeleteKey("SavedNodeGUID");
        PlayerPrefs.DeleteKey("SavedNodeName");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}

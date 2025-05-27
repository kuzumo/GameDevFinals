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
    [SerializeField] private AudioSource voiceAudioSource;


    public Sprite actor;
    public GameObject actorObject;
    public GameObject nextButtonGO;
    public GameObject previousButtonGO;
    private BaseNode previousNode;
    private BaseNode lastChoiceNode;
    public TMP_Text skipButtonText;
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
    private bool isSkipping = false;
    private Coroutine skipCoroutine;
    private float normalTypingSpeed;
    void Start()
    {
        typingSpeed = PlayerPrefs.GetFloat("TypingSpeed", 0.2f);
        lastChoiceNode = null;
        nodeHistory.Clear();
        normalTypingSpeed = PlayerPrefs.GetFloat("TypingSpeed", 0.02f);
        typingSpeed = normalTypingSpeed;
      
        Debug.Log("NodeReader Start() called.");

        bool isNewGame = PlayerPrefs.GetInt("IsNewGame", 1) == 1;

        string selectedCharacter = PlayerPrefs.GetString("SelectedCharacter", "");
        characterStats = FindObjectOfType<CharacterStats>();

        if (characterStats == null)
        {
            Debug.LogError("❌ NodeReader: CharacterStats not found in scene.");
            return;
        }


        if (selectedCharacter == "Veyna")
        {
            characterStats.characterName = "Veyna";
            characterStats.race = "Skyborn Seeker";
            characterStats.characterClass = "Tactical & Ranged";
            characterStats.alignment = "PL";

            characterStats.STR = 6; characterStats.CON = 4; characterStats.WIS = 5;
            characterStats.CHA = 4; characterStats.INT = 1; characterStats.DEX = 6;

            characterStats.hp = 15;
            characterStats.naturalArmor = 5;
            characterStats.sacralGuard = 11;

            characterStats.characteristic1 = "Observant";
            characterStats.characteristic2 = "Agile";

            characterStats.trait1 = "Keen Senses (WIS)";
            characterStats.trait2 = "Empathy (WIS)";
            characterStats.trait3 = "Lunar Sensitivity (WIS)";
            characterStats.trait4 = "Wilderness Instinct (WIS)";

            characterStats.passiveTrait = "Predator’s Focus (DEX) – Enhances aim and reflexes when tracking.";

            characterStats.attacks = new List<Attack>()
    {
        new Attack { name = "Piercing Talon", toHitModifier = 1, damage = 11 },
        new Attack { name = "Skyborne Volley", toHitModifier = 3, damage = 9 },
        new Attack { name = "Falcon’s Dive", toHitModifier = 2, damage = 14 }
    };
        }
        else if (selectedCharacter == "Veyrix")
        {
            characterStats.characterName = "Veyrix";
            characterStats.race = "Abyssal Revenant";
            characterStats.characterClass = "Doom Herald";
            characterStats.alignment = "CA";

            characterStats.STR = 6; characterStats.CON = 4; characterStats.WIS = 3;
            characterStats.CHA = 1; characterStats.INT = 1; characterStats.DEX = 5;

            characterStats.hp = 15;
            characterStats.naturalArmor = 4;
            characterStats.sacralGuard = 9;

            characterStats.characteristic1 = "Shadowborn";
            characterStats.characteristic2 = "Cold Intellect";

            characterStats.trait1 = "Dominance (CHA)";
            characterStats.trait2 = "Curiosity (INT)";
            characterStats.trait3 = "Guardian Instinct (WIS)";
            characterStats.trait4 = "Instinctive Tracking (INT)";

            characterStats.passiveTrait = "Doomspeaker (CHA) – Prophetic whispers weaken enemy morale and fear resistance.";

            characterStats.attacks = new List<Attack>()
    {
        new Attack { name = "Titan’s Crush", toHitModifier = 4, damage = 12 },
        new Attack { name = "Relentless Charge", toHitModifier = 5, damage = 9 },
        new Attack { name = "Savage Rend", toHitModifier = 6, damage = 11 }
    };
        }


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

        else if (node is CombatCheckNode combatNode)
        {
            HandleCombatCheck(combatNode);
            return;
        }

        else if (node is AttackChoiceNode)
        {
            ShowAttackChoiceNode((AttackChoiceNode)node);
            return;
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
        PlayNode(node);

    }

    public DiceRollCombatPanelController diceRollCombat;

    public CharacterStats enemyStats; // assign enemy in inspector

    private void HandleCombatCheck(CombatCheckNode node)
    {
        string ability = node.getAbility().ToString();
        int dc = Mathf.RoundToInt(node.getDC());

        DisableAllButtons();

        // Start combat panel logic
        diceRollCombat.characterStats = characterStats;
        diceRollCombat.Show(ability, dc, result =>
        {
            // 🛡 Check if enemy has been defeated
            if (enemyStats.hp <= 0)
            {
                Debug.Log("🗡 Enemy defeated!");

                // Jump to a special "Victory" port if one exists
                var victoryNode = currentNode.GetOutputPort("victory")?.Connection.node as BaseNode;

                if (victoryNode != null)
                {
                    currentNode = victoryNode;
                    displayNode(currentNode);
                }
                else
                {
                    Debug.LogWarning("⚠ No 'victory' port connected. Staying in current node.");
                }
            }
            else
            {
                // Normal combat outcome → success or fail loop
                BaseNode nextNode = result
                    ? currentNode.GetOutputPort("success")?.Connection.node as BaseNode
                    : currentNode.GetOutputPort("failed")?.Connection.node as BaseNode;

                if (nextNode != null)
                {
                    currentNode = nextNode;
                    displayNode(currentNode);
                }
            }
        }, enemyStats);

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

    public void PlayNode(BaseNode node)
    {
        if (node.voiceOver != null)
        {
            voiceAudioSource.clip = node.voiceOver;
            voiceAudioSource.Play();
        }
    }

    public void SkipTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            dialogue.text = currentNode.getDialogText();
            typingCoroutine = null;
            isTyping = false;
            ShowChoices(currentNode);
        }
    }


    private BaseNode GetNextNode(BaseNode node)
    {
        GameObject clickedObj = EventSystem.current.currentSelectedGameObject;
        string clickedText = clickedObj.GetComponentInChildren<TMP_Text>().text;
        string clickedName = clickedObj.name;

        if (node is AttackChoiceNode)
        {

            if (clickedObj == null)
            {
                Debug.LogWarning("❗ No button was clicked (EventSystem returned null).");
                return null;
            }

            string clickedTag = clickedObj.tag;
            Debug.Log("🔍 Clicked button tag: " + clickedTag);

            if (clickedTag == "ButtonA") return node.GetOutputPort("a")?.Connection.node as BaseNode;
            if (clickedTag == "ButtonB") return node.GetOutputPort("b")?.Connection.node as BaseNode;
            if (clickedTag == "ButtonC") return node.GetOutputPort("c")?.Connection.node as BaseNode;

            Debug.LogWarning("❗ No matching port found for AttackChoiceNode.");
            return null;
        }


        if (node is SixChoiceDialog scd)
        {
            if (clickedText == scd.aText) return node.GetOutputPort("a")?.Connection.node as BaseNode;
            if (clickedText == scd.bText) return node.GetOutputPort("b")?.Connection.node as BaseNode;
            if (clickedText == scd.cText) return node.GetOutputPort("c")?.Connection.node as BaseNode;
            if (clickedText == scd.dText) return node.GetOutputPort("d")?.Connection.node as BaseNode;
            if (clickedText == scd.eText) return node.GetOutputPort("e")?.Connection.node as BaseNode;
            if (clickedText == scd.fText) return node.GetOutputPort("f")?.Connection.node as BaseNode;
        }

        if (node is ThreeChoiceDialog tcd)
        {
            if (clickedText == tcd.aText) return node.GetOutputPort("a")?.Connection.node as BaseNode;
            if (clickedText == tcd.bText) return node.GetOutputPort("b")?.Connection.node as BaseNode;
            if (clickedText == tcd.cText) return node.GetOutputPort("c")?.Connection.node as BaseNode;
        }

        if (node is MultipleChoiceDialog mcd)
        {
            if (clickedText == mcd.aText) return node.GetOutputPort("a")?.Connection.node as BaseNode;
            if (clickedText == mcd.bText) return node.GetOutputPort("b")?.Connection.node as BaseNode;
        }

        if (node is OneChoiceDialog ocd)
        {
            if (clickedText == ocd.GetChoiceAText()) return node.GetOutputPort("c")?.Connection.node as BaseNode;
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

            if (nextNode != null)
            {
                currentNode = nextNode;
                displayNode(currentNode);
            }
            else
            {
                endPanel.SetActive(true); // ✅ Show end if no connected node
            }
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

    public void OnSkipButtonPressed()
    {
        isSkipping = !isSkipping;

        if (isSkipping)
        {
            typingSpeed = 0.001f;
            skipCoroutine = StartCoroutine(SkipThroughNodes());
            UpdateSkipButtonUI(true); // Optional: update label/icon
        }
        else
        {
            StopSkipping();
        }
    }

    private void StopSkipping()
    {
        if (skipCoroutine != null)
        {
            StopCoroutine(skipCoroutine);
            skipCoroutine = null;
        }

        typingSpeed = normalTypingSpeed;
        isSkipping = false;
        UpdateSkipButtonUI(false); // Optional
    }


    private IEnumerator SkipThroughNodes()
    {
        justLoadedFromSave = false; // ✅ Ensure skip is not blocked by save-load flag

        while (isSkipping)
        {
            yield return null;

            if (isTyping)
            {
                SkipTyping();
                yield return new WaitForSeconds(0.01f);
                continue;
            }

            if (IsChoiceNode(currentNode))
            {
                StopSkipping();
                yield break;
            }

            AdvanceDialog();
            yield return new WaitForSeconds(0.05f);
        }
    }

    public void ShowAttackChoiceNode(AttackChoiceNode node)
    {
        var attacks = characterStats.attacks;

        if (attacks.Count < 3)
        {
            Debug.LogWarning("Not enough attacks assigned.");
            return;
        }

        buttonA.SetActive(true); buttonAText.text = attacks[0].name;
        buttonB.SetActive(true); buttonBText.text = attacks[1].name;
        buttonC.SetActive(true); buttonCText.text = attacks[2].name;

        choicesPanel.SetActive(true);
        nextButtonGO.SetActive(false);
    }


    private void UpdateSkipButtonUI(bool skipping)
    {
        if (skipButtonText != null)
        {
            skipButtonText.text = skipping ? "Stop" : "Skip";
        }
    }

    private bool IsChoiceNode(BaseNode node)
{
    return node is MultipleChoiceDialog ||
           node is ThreeChoiceDialog ||
           node is SixChoiceDialog ||
           node is OneChoiceDialog ||
           node is AbilityCheckNode ||
           node is AttackChoiceNode;
}



}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class DiceRollPanelController : MonoBehaviour
{
    public TMP_Text skillTitleText;
    public TMP_Text targetDCText;
    public TMP_Text resultText;
    public TMP_Text outcomeText;
    public Button rollButton;
    public Button continueButton;

    private int dc;
    private string skillName;
    private int modifier;
    private bool successResult;

    private Action<bool> onCompleteCallback;

    public CharacterStats characterStats; // drag this in Inspector

    private void Start()
    {
        rollButton.onClick.AddListener(RollDice);
        continueButton.onClick.AddListener(() => {
            gameObject.SetActive(false);
            onCompleteCallback?.Invoke(successResult);
        });
    }

    public void Show(string skill, int dcValue, Action<bool> callback)
    {
        skillName = skill;
        dc = dcValue;
        modifier = characterStats.GetSkillModifier(skill);
        onCompleteCallback = callback;

        skillTitleText.text = $"Skill Check: {skill}";
        targetDCText.text = $"Target DC: {dc}";
        resultText.text = "";
        outcomeText.text = "";
        successResult = false;

        rollButton.interactable = true;
        continueButton.gameObject.SetActive(false);

        gameObject.SetActive(true);
    }

    private void RollDice()
    {
        int roll = UnityEngine.Random.Range(1, 21);
        int total = roll + modifier;

        resultText.text = $"→ You rolled {roll} + {modifier} = {total}";

        if (total >= dc)
        {
            outcomeText.text = "<color=green>Success!</color>";
            successResult = true;
        }
        else
        {
            outcomeText.text = "<color=red>Failure.</color>";
            successResult = false;
        }

        rollButton.interactable = false;
        continueButton.gameObject.SetActive(true);
    }
}

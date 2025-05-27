using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class DiceRollCombatPanelController : MonoBehaviour
{
    public TMP_Text playerRollText, enemyRollText, combatResultText;
    public Button rollButton, continueButton;

    public CharacterStats characterStats;
    private CharacterStats enemyStats;

    private int dc;
    private string skillName;
    private bool playerSuccess;
    private Action<bool> onCombatComplete;

    private void Start()
    {
        rollButton.onClick.AddListener(PlayerRoll);
        continueButton.onClick.AddListener(() => {
            gameObject.SetActive(false);
            onCombatComplete?.Invoke(playerSuccess);
        });
    }

    public void Show(string skill, int dcValue, Action<bool> callback, CharacterStats enemy)
    {
        skillName = skill;
        dc = dcValue;
        enemyStats = enemy;
        onCombatComplete = callback;

        playerRollText.text = "";
        enemyRollText.text = "";
        combatResultText.text = "";

        rollButton.interactable = true;
        continueButton.gameObject.SetActive(false);

        gameObject.SetActive(true);
    }

    private void PlayerRoll()
    {
        int playerMod = characterStats.GetSkillModifier(skillName);
        int playerRoll = UnityEngine.Random.Range(1, 21);
        int playerTotal = playerRoll + playerMod;

        playerRollText.text = $"You rolled: {playerRoll} + {playerMod} = {playerTotal}";

        if (playerTotal >= dc)
        {
            enemyStats.TakeDamage(2);
            combatResultText.text = "<color=green>You hit! Enemy -2 HP</color>";
            playerSuccess = true;
        }
        else
        {
            combatResultText.text = "<color=red>You missed!</color>";
            playerSuccess = false;
        }

        EnemyRoll();

        rollButton.interactable = false;
        continueButton.gameObject.SetActive(true);
    }

    private void EnemyRoll()
    {
        int enemyMod = enemyStats.GetSkillModifier(skillName);
        int enemyRoll = UnityEngine.Random.Range(1, 21);
        int enemyTotal = enemyRoll + enemyMod;

        enemyRollText.text = $"Enemy rolled: {enemyRoll} + {enemyMod} = {enemyTotal}";

        if (enemyTotal >= dc)
        {
            characterStats.TakeDamage(2);
            combatResultText.text += "\n<color=red>Enemy hits! You -2 HP</color>";
        }
        else
        {
            combatResultText.text += "\n<color=green>Enemy misses!</color>";
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Attack
{
    public string name;
    public int toHitModifier;
    public int damage;
}

public class CharacterStats : MonoBehaviour
{
    // Identity
    public string characterName;
    public string race;
    public string characterClass;
    public string alignment;

    // Core Stats
    public int STR;
    public int CON;
    public int WIS;
    public int CHA;
    public int INT;
    public int DEX;

    // Health and Defense
    public int hp;
    public int naturalArmor;
    public int sacralGuard;

    // Characteristics (you may expand this if needed)
    public string characteristic1;
    public string characteristic2;

    // Traits
    public string trait1;
    public string trait2;
    public string trait3;
    public string trait4;

    // Passive Trait
    [TextArea(2, 5)]
    public string passiveTrait;

    // Attacks used in dice-based combat
    public List<Attack> attacks = new List<Attack>();

    // ✅ Gets a stat or skill modifier by name
    public int GetSkillModifier(string skill)
    {
        switch (skill.ToLower())
        {
            case "str": case "strength": return STR;
            case "con": case "constitution": return CON;
            case "wis": case "wisdom": return WIS;
            case "cha": case "charisma": return CHA;
            case "int": case "intelligence": return INT;
            case "dex": case "dexterity": return DEX;

            // Legacy skill names
            case "survival": return WIS;
            case "persuasion": return CHA;
            case "stealth": return DEX;
            case "athletics": return STR;

            default:
                Debug.LogWarning($"Skill '{skill}' not found in CharacterStats.");
                return 0;
        }
    }

    // ✅ Combat: take damage safely
    public void TakeDamage(int amount)
    {
        hp = Mathf.Max(0, hp - amount);
    }

    // ✅ Optional: heal
    public void Heal(int amount)
    {
        hp += amount;
    }
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);

        // Prevent duplicates if this scene loads again
        if (FindObjectsOfType<CharacterStats>().Length > 1)
        {
            Destroy(gameObject);
        }
    }

}

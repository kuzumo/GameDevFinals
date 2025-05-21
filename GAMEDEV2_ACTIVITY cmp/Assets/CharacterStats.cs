using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    public int survival;
    public int hp;

    // Add more skills if needed
    public int persuasion;
    public int stealth;
    public int athletics;

    // Returns the skill modifier based on skill name
    public int GetSkillModifier(string skill)
    {
        switch (skill.ToLower())
        {
            case "survival": return survival;
            case "persuasion": return persuasion;
            case "stealth": return stealth;
            case "athletics": return athletics;
            default:
                Debug.LogWarning($"Skill '{skill}' not found, returning 0.");
                return 0;
        }
    }
}

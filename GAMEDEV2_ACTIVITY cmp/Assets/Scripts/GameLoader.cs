using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameLoader : MonoBehaviour
{
    public string playSceneName = "PlayScene"; 
    [SerializeField] private AudioSource uiAudioSource;

    public void OnNewGame()
    {
        PlayerPrefs.DeleteKey("SavedNodeName");
        PlayerPrefs.SetInt("IsNewGame", 1);
        PlayerPrefs.Save();

        PlayClickAndLoad();
    }

    public void OnLoadGame()
    {
        PlayerPrefs.SetInt("IsNewGame", 0);
        PlayerPrefs.Save();

        PlayClickAndLoad();
    }

    private void PlayClickAndLoad()
    {
        if (uiAudioSource != null && uiAudioSource.clip != null)
        {
            uiAudioSource.Play();
            StartCoroutine(LoadSceneAfterDelay(0.2f)); // ⏱️ fixed delay regardless of clip length
        }
        else
        {
            SceneManager.LoadScene(playSceneName);
        }
    }


    private IEnumerator LoadSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(playSceneName);
    }

    public CharacterStats characterStats; 
    public void PlayAsVeyna()
    {
        // Identity & Role
        characterStats.characterName = "Veyna";
        characterStats.race = "Skyborn Seeker";
        characterStats.characterClass = "Tactical & Ranged";
        characterStats.alignment = "PL";

        // Core Stats
        characterStats.STR = 6;
        characterStats.CON = 4;
        characterStats.WIS = 5;
        characterStats.CHA = 4;
        characterStats.INT = 1;
        characterStats.DEX = 6;

        // Health & Defense
        characterStats.hp = 15;
        characterStats.naturalArmor = 5;
        characterStats.sacralGuard = 11;

        // Characteristics
        characterStats.characteristic1 = "Observant";
        characterStats.characteristic2 = "Agile"; 

        // Traits
        characterStats.trait1 = "Keen Senses (WIS)";
        characterStats.trait2 = "Empathy (WIS)";
        characterStats.trait3 = "Lunar Sensitivity (WIS)";
        characterStats.trait4 = "Wilderness Instinct (WIS)";

        // Passive Trait
        characterStats.passiveTrait = "Predator’s Focus (DEX) – Enhances aim and reflexes when tracking.";

        characterStats.attacks = new System.Collections.Generic.List<Attack>()
        {
        new Attack { name = "Piercing Talon", toHitModifier = 2, damage = 3 },
        new Attack { name = "Skyborne Volley", toHitModifier = 3, damage = 2 },
        new Attack { name = "Falcon's Dive", toHitModifier = 4, damage = 4 }
    };

        // Optional: remember who was picked
        PlayerPrefs.SetString("SelectedCharacter", "Veyna");
        PlayerPrefs.SetInt("IsNewGame", 1);
        PlayerPrefs.Save();

        // Load game
        PlayClickAndLoad();
    }

    public void PlayAsVeyrix()
    {
        // Identity & Role
        characterStats.characterName = "Veyrix";
        characterStats.race = "Abyssal Revenant";
        characterStats.characterClass = "Doom Herald";
        characterStats.alignment = "CA";

        // Core Stats
        characterStats.STR = 6;
        characterStats.CON = 4;
        characterStats.WIS = 3;
        characterStats.CHA = 1;
        characterStats.INT = 1;
        characterStats.DEX = 5;

        // Health & Defense
        characterStats.hp = 15;
        characterStats.naturalArmor = 4;
        characterStats.sacralGuard = 9;

        // Characteristics
        characterStats.characteristic1 = "Shadowborn";
        characterStats.characteristic2 = "Cold Intellect";

        // Traits
        characterStats.trait1 = "Dominance (CHA)";
        characterStats.trait2 = "Curiosity (INT)";
        characterStats.trait3 = "Guardian Instinct (WIS)";
        characterStats.trait4 = "Instinctive Tracking (INT)";

        // Passive Trait
        characterStats.passiveTrait = "Doomspeaker (CHA) – Prophetic whispers weaken enemy morale and fear resistance.";

        // ✅ Attacks
        characterStats.attacks = new System.Collections.Generic.List<Attack>()
    {
        new Attack { name = "Titan’s Crush", toHitModifier = 4, damage = 12 },
        new Attack { name = "Relentless Charge", toHitModifier = 5, damage = 9 },
        new Attack { name = "Savage Rend", toHitModifier = 6, damage = 11 }
    };

        // Save state and launch scene
        PlayerPrefs.SetString("SelectedCharacter", "Veyrix");
        PlayerPrefs.SetInt("IsNewGame", 1);
        PlayerPrefs.Save();

        PlayClickAndLoad();
    }

}

using UnityEngine;
using UnityEngine.UI; 
using TMPro;         

public class DifficultySettings : MonoBehaviour
{
    public Slider difficultySlider;
    public TextMeshProUGUI difficultyLevelText; 

    public static readonly string DifficultyKey = "AIDifficultyLevel"; // Key for PlayerPrefs
    public static readonly float DefaultDifficulty = 5f;           // Default level (1-10)

    void Start()
    {
        if (difficultySlider == null)
        {
            Debug.LogError("Difficulty Slider is not assigned in DifficultySettings script!");
            return;
        }

        // Load saved difficulty or use default, then update slider and text
        float savedDifficulty = PlayerPrefs.GetFloat(DifficultyKey, DefaultDifficulty);
        difficultySlider.value = savedDifficulty;
        UpdateDifficultyText(savedDifficulty);

        // Add listener for when the slider value changes
        difficultySlider.onValueChanged.AddListener(SetDifficulty);
    }

    public void SetDifficulty(float value)
    {
        // PlayerPrefs stores floats, but we treat it as an integer level
        float difficultyLevel = Mathf.RoundToInt(value);
        PlayerPrefs.SetFloat(DifficultyKey, difficultyLevel);
        PlayerPrefs.Save(); // Important: Save PlayerPrefs after changing
        UpdateDifficultyText(difficultyLevel);
    }

    void UpdateDifficultyText(float value)
    {
        if (difficultyLevelText != null)
        {
            difficultyLevelText.text = "Set Difficulty: " + Mathf.RoundToInt(value).ToString();
        }
    }
}
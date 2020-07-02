using UnityEngine;


[CreateAssetMenu(fileName = "Level Difficulty", menuName = "Worktest/Level Difficulty", order = 0)]
public class LevelDifficulty : ScriptableObject
{
    public Mode DifficultyMode = Mode.Easy;
    public LevelPreset EasyPreset = null;
    public LevelPreset HardPreset = null;

    public void ApplyDifficulty() => Apply(DifficultyMode == Mode.Easy ? EasyPreset : HardPreset);

    private void Apply(LevelPreset preset) => preset.ApplySettingsToScene();

    public enum Mode
    {
        Easy,
        Hard
    }
}
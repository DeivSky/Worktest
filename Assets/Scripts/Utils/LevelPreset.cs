using System.Reflection;
using UnityEngine.SceneManagement;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "Level Preset", menuName = "Worktest/Level Preset", order = 0)]
public class LevelPreset : ScriptableObject
{
    public float GameLength;
    public PlayerSettingsReference PlayerSettings;
    public RockSettingsReference RockSettings;
    public CoinSettingsReference CoinSettings;
    
    private readonly FieldInfo gameLengthField = typeof(GameManager).GetField("gameLength",
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

    private readonly FieldInfo playerSettingsField = typeof(PlayerController).GetField("playerSettings",
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

    private readonly FieldInfo rockSettingsField = typeof(RockThrowerController).GetField("rockSettings",
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

    private readonly FieldInfo coinSettingsField = typeof(CoinSpawnerController).GetField("coinSettings",
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    
    public void ApplySettingsToScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        var objs = scene.GetRootGameObjects();
        for (int i = 0; i < objs.Length; i++)
        {
            if (objs[i].TryGetComponent(out GameManager gameManager))
                gameLengthField.SetValue(gameManager, GameLength);
            else if (objs[i].TryGetComponent(out PlayerController playerController))
                playerSettingsField.SetValue(playerController, PlayerSettings);
            else if (objs[i].TryGetComponent(out RockThrowerController rockThrowerController))
                rockSettingsField.SetValue(rockThrowerController, RockSettings);
            else if (objs[i].TryGetComponent(out CoinSpawnerController coinSpawnerController))
                coinSettingsField.SetValue(coinSpawnerController, CoinSettings);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(LevelPreset))]
[CanEditMultipleObjects]
public class LevelPresetEditor : Editor
{
    private LevelPreset prop;

    private void OnEnable() => prop = (LevelPreset) serializedObject.targetObject;

    public override void OnInspectorGUI()
    {
        prop.GameLength = Mathf.Clamp(EditorGUILayout.FloatField("Duración del juego", prop.GameLength), 5f, 600f);
        prop.PlayerSettings = (PlayerSettingsReference) EditorGUILayout.ObjectField(
            "Player Settings", 
            prop.PlayerSettings, 
            typeof(PlayerSettingsReference), 
            false);
        
        prop.RockSettings = (RockSettingsReference) EditorGUILayout.ObjectField(
            "Rock Settings", 
            prop.RockSettings, 
            typeof(RockSettingsReference), 
            false);
        
        prop.CoinSettings = (CoinSettingsReference) EditorGUILayout.ObjectField(
            "Coin Settings", 
            prop.CoinSettings, 
            typeof(CoinSettingsReference), 
            false);
        
        if (GUILayout.Button("Aplicar"))
            prop.ApplySettingsToScene();
    }
}
#endif

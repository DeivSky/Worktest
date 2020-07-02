using System.Reflection;
using UnityEngine.SceneManagement;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

[CreateAssetMenu(fileName = "Level Preset", menuName = "Worktest/Level Preset", order = 0)]
public class LevelPreset : ScriptableObject
{
    public PlayerSettingsReference PlayerSettings;
    public RockSettingsReference RockSettings;
    public CoinSettingsReference CoinSettings;

    private static readonly FieldInfo playerSettingsField = typeof(PlayerController).GetField("playerSettings",
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

    private static readonly FieldInfo rockSettingsField = typeof(RockThrowerController).GetField("rockSettings",
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

    private static readonly FieldInfo coinSettingsField = typeof(CoinSpawnerController).GetField("coinSettings",
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

    public void ApplySettingsToScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        var objects = scene.GetRootGameObjects();
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i].TryGetComponent(out PlayerController playerController))
            {
#if UNITY_EDITOR
                Undo.RecordObject(playerController, $"Applied level preset: {playerController.name}");
#endif
                playerSettingsField.SetValue(playerController, PlayerSettings);
#if UNITY_EDITOR
                PrefabUtility.RecordPrefabInstancePropertyModifications(playerController);
#endif
            }
            else if (objects[i].TryGetComponent(out RockThrowerController rockThrowerController))
            {
#if UNITY_EDITOR
                Undo.RecordObject(rockThrowerController, $"Applied level preset: {rockThrowerController.name}");
#endif
                rockSettingsField.SetValue(rockThrowerController, RockSettings);
#if UNITY_EDITOR
                PrefabUtility.RecordPrefabInstancePropertyModifications(rockThrowerController);
#endif
            }
            else if (objects[i].TryGetComponent(out CoinSpawnerController coinSpawnerController))
            {
#if UNITY_EDITOR
                Undo.RecordObject(coinSpawnerController, $"Applied level preset: {coinSpawnerController.name}");
#endif
                coinSettingsField.SetValue(coinSpawnerController, CoinSettings);
#if UNITY_EDITOR
                PrefabUtility.RecordPrefabInstancePropertyModifications(coinSpawnerController);
#endif
            }
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
        DrawDefaultInspector();

        if (GUILayout.Button("Aplicar"))
            prop.ApplySettingsToScene();
    }
}
#endif
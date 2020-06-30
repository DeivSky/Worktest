using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "Coin Settings", menuName = "Worktest/Coin Settings", order = 14)]
public class CoinSettingsReference : DataReferenceAsset<CoinSettings> { }

#if UNITY_EDITOR
[CustomEditor(typeof(CoinSettingsReference))]
[CanEditMultipleObjects]
public class CoinSettingsEditor : Editor
{
    private CoinSettingsReference prop;

    private void OnEnable() => prop = (CoinSettingsReference) serializedObject.targetObject;

    public override void OnInspectorGUI()
    {
        prop.Value.ScoreToSpawn = EditorGUILayout.IntSlider(
            "Puntos necesarios",
            prop.Value.ScoreToSpawn,
            1,
            10);

        prop.Value.Lifetime = EditorGUILayout.Slider(
            "Duración de las monedas",
            prop.Value.Lifetime,
            0.1f,
            10f);
        
        prop.Value.AnimationDuration = EditorGUILayout.Slider(
            "Duración de la animación",
            prop.Value.AnimationDuration,
            0.1f,
            5f);

        prop.Value.CoinPrefab = (GameObject) EditorGUILayout.ObjectField(
            "Prefab de moneda",
            prop.Value.CoinPrefab,
            typeof(GameObject),
            false);
    }
}
#endif
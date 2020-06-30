using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText = null;
    [SerializeField] private TextMeshProUGUI coinsText = null;
    [SerializeField] private TextMeshProUGUI timeText = null;
    [SerializeField] private InputSystemUIInputModule uiInputModule = null;
    private DataReference<int> score = null;
    private DataReference<int> coins = null;
    private DataReference<float> time = null;

    private void Awake()
    {
        var gm = GameManager.Instance;
        score = gm.Score;
        coins = gm.Coins;
        time = gm.GameTime;
        uiInputModule.cancel.action.performed += ctx => SceneManager.LoadScene(0, LoadSceneMode.Single);
    }

    private void Update()
    {
        scoreText.text = score.Value.ToString();
        coinsText.text = coins.Value.ToString();
        int minutes = Math.DivRem(Mathf.FloorToInt(time.Value), 60, out int seconds);
        timeText.text = new TimeSpan(0, minutes, seconds).ToString();

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class RockThrowerController : MonoBehaviour
{
    [SerializeField] private RockThrowerSettingsReference rockThrowerSettings = null;

    private Animator animator = null;
    private Queue<GameObject> rocks = null;
    private Transform rockSpawn = null;
    private readonly int throwHash = Animator.StringToHash("Throw");
    
    private void Awake()
    {
        animator = GetComponent<Animator>();

        var settingsValue = rockThrowerSettings.Value;
        Transform goal = GameObject.Find("Goal").transform;
        rockSpawn = transform.GetChild(0);
        float minRad = settingsValue.MinThrowAngle * Mathf.Deg2Rad;
        float maxRad = settingsValue.MaxThrowAngle * Mathf.Deg2Rad;
        
        rocks = new Queue<GameObject>(settingsValue.RockCount);
        for (int i = 0; i < settingsValue.RockCount; i++)
        {
            var rock = Instantiate(settingsValue.RockPrefab, rockSpawn.position, Quaternion.identity);
            rock.SetActive(false);
            var rockController = rock.GetComponent<RockController>();
            rockController.SetVelocity(
                Random.Range(settingsValue.MinRockVelocity, settingsValue.MaxRockVelocity), 
                Random.Range(minRad, maxRad));
            rockController.Goal = goal;
            rocks.Enqueue(rock);
        }
    }

    private void Start() => StartCoroutine(Throw(rockThrowerSettings.Value.TimeBetweenThrows));
    
    private IEnumerator Throw(float timeBetween)
    {
        var waitForAnimation = new WaitForSeconds(timeBetween);
        int count = rocks.Count;
        for (int i = 0; i < count; i++)
        {
            yield return waitForAnimation;
            animator.SetTrigger(throwHash);
        }
    }
    
    private void DoThrow() => rocks.Dequeue().SetActive(true);
}

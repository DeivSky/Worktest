using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Animator))]
public class RockThrowerController : MonoBehaviour
{
    private RockThrowerSettingsReference rockThrowerSettings = null;

    private Animator animator = null;
    private readonly int throwHash = Animator.StringToHash("Throw");
    private SpawnPool<RockController> spawnPool = null;
    
    private void Awake()
    {
        rockThrowerSettings = GameManager.Instance.RockThrowerSettings;
        animator = GetComponent<Animator>();
        var settings = rockThrowerSettings.Value;
        spawnPool = new SpawnPool<RockController>(settings.RockCount, settings.RockPrefab);
    }

    private IEnumerator Start()
    {
        Transform rockSpawn = transform.GetChild(0);
        Transform pool = GameObject.Find("rockPool").transform;
        var settings = rockThrowerSettings.Value;
        float minRad = settings.MinThrowAngle * Mathf.Deg2Rad;
        float maxRad = settings.MaxThrowAngle * Mathf.Deg2Rad;

        yield return StartCoroutine(spawnPool.InstantiateAllCoroutine(5,
            rock =>
            {
                rock.GetComponent<RockController>().SetVelocity(
                    Random.Range(settings.MinRockVelocity, settings.MaxRockVelocity),
                    Random.Range(minRad, maxRad));
                var transform = rock.transform;
                transform.parent = pool;
                transform.position = rockSpawn.position;
            }));
        
        StartCoroutine(Throw(settings.RockCount, settings.TimeBetweenThrows));
    }

    private void FixedUpdate()
    {
        var rocks = spawnPool.ActiveComponents;
        float minHeight = float.MaxValue;
        for (int i = 0; i < rocks.Length; i++)
        {
            var rock = rocks[i];
        }


    }

    private IEnumerator Throw(int count, float timeBetween)
    {
        Assert.IsTrue(timeBetween > 0f);
        
        var wait = new WaitForSeconds(timeBetween);
        for (int i = 0; i < count; i++)
        {
            yield return wait;
            animator.SetTrigger(throwHash);
        }
    }

    private void DoThrow() => spawnPool.Spawn();
}

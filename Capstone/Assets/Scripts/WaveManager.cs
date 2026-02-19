using UnityEngine;
using System.Collections;

public class WaveManager : MonoBehaviour {
    [Header("Arena Wave Variables")]
    [SerializeField] int wave = 0;
    [SerializeField] int maxWaves = 10;
    [Header("Enemy Prefabs")]
    // can rename enemy names
    [SerializeField] GameObject[] enemies;
    int enemyCount = 0;
    bool endlessMode = false;

    void Start() {
        StartCoroutine(StartNextWave());
    }

    void CalculateEnemyCount()
    {
        enemyCount = wave + 2; // waveCount + metric ...
        Debug.Log("Enemy Count: " + enemyCount);
    }

    void SpawnEnemies()
    {   
        for(int i = 0; i < enemyCount; ++i) {
            Instantiate(enemies[Random.Range(0, enemies.Length)], Vector3.zero, Quaternion.identity); // replace Vector3.zero with a fixed position or rand pos
        }
    }

    IEnumerator StartNextWave()
    {
        while(endlessMode || wave <= maxWaves) { 
            if(enemyCount > 0) { 
                yield return new WaitForSeconds(5f);
                continue;
            }
            wave++;
            CalculateEnemyCount();
            SpawnEnemies();

            Debug.Log("Starting Wave: " + wave);
        }
    }

    // Button cmds for testing purposes
    public void RemoveEnemy()
    {
        enemyCount--;
        Debug.Log("Enemy Count: " + enemyCount);
    }

    public void AddEnemy()
    {
        enemyCount++;
        Debug.Log("Enemy Count: " + enemyCount);
    }



}

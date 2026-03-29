using System;
using System.Collections;
using UnityEngine;


[Serializable]
class EnemyData {
	public Transform spawnPoint;
	public GameObject enemie;
}
[Serializable]
class WaveData {
	public EnemyData[] enemies;
	public GameObject waveParentGameObject;
}

public class WaveManager : MonoBehaviour {
    [Header("Arena Wave Variables")]
    [SerializeField] int currentWaveIndex = 0;
    [SerializeField] int maxWaves = 10;
    [Header("Enemy Prefabs")]
    // can rename enemy names
    [SerializeField] GameObject[] enemies;
    int enemyCount = 0;
    bool endlessMode = false;

    [SerializeField] WaveData[] waves;

    void Start() {
        //StartCoroutine(StartNextWave());
    }

    void CalculateEnemyCount()
    {
        enemyCount = currentWaveIndex + 2; // waveCount + metric ...
        Debug.Log("Enemy Count: " + enemyCount);
    }

// Can change the params to wave # and use a switch case to have manual spawns based on the wave
    void SpawnEnemies()
    {   
        for(int i = 0; i < enemyCount; ++i) {
            Instantiate(enemies[UnityEngine.Random.Range(0, enemies.Length)], Vector3.zero, Quaternion.identity); // replace Vector3.zero with a fixed position or rand pos
        }
    }

    public void StartNextWave(){

        WaveData currentWave = waves[currentWaveIndex];

        currentWave.waveParentGameObject.SetActive(true);
        foreach(EnemyData enemyData in currentWave.enemies) {
			GameObject spawnedEnemy = Instantiate(enemyData.enemie, enemyData.spawnPoint);

			IDamageable damageable = spawnedEnemy.GetComponent<IDamageable>();

			if (damageable != null) {
				enemyCount++;
				damageable.OnDeath += EnemyDied;
				EnemyBase aiScript = spawnedEnemy.GetComponent<EnemyBase>();
                if(aiScript != null) {
					aiScript.enabled = true;
				}
			}

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
    public void EnemyDied() {
		enemyCount--;
		Debug.Log("Enemy Count: " + enemyCount);

		if (enemyCount <= 0) {
			Debug.Log("Wave Cleared!");
			currentWaveIndex++;
            // Logic to trigger next wave or show victory UI
            currentWaveIndex++;
            StartNextWave();
		}
	}



}

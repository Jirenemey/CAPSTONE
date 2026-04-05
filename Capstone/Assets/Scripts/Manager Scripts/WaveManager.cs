using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;


[Serializable]
public class EnemyData {
	public Transform spawnPoint;
	public GameObject enemie;
}
[Serializable]
public class WaveData {
	public EnemyData[] enemies;
	public GameObject waveParentGameObject;
}

public class WaveManager : MonoBehaviour {
    [Header("Arena Wave Variables")]
    [SerializeField] public int currentWaveIndex = 0;
    [SerializeField] int maxWaves = 10;
    [Header("Enemy Prefabs")]
    // can rename enemy names
    [SerializeField] GameObject[] enemies;
    int enemyCount = 0;
    bool endlessMode = false;

    [SerializeField] public WaveData[] waves;

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

    [ServerRpc]
    public void SpawnEnemiesServerRpc()
    {
        SpawnEnemies();
    }

    [ServerRpc]
    public void StartNextWaveServerRpc()
    {
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

            spawnedEnemy.GetComponent<NetworkObject>().Spawn();
		}
    }

    [ClientRpc]
    public void DisplayWaveObjectsClientRpc(WaveData currentWave)
    {
        currentWave.waveParentGameObject.SetActive(true);
    }
    

    public void StartNextWave(){

        if(currentWaveIndex >= waves.Length) {
            // game over TODO: make this point to the win screen
            if (NetworkManager.Singleton) {
			    NetworkManager.Singleton.SceneManager.LoadScene(
				    "MainMenu",
			        LoadSceneMode.Single
		        );
            } else {
                SceneManager.LoadScene(
					"MainMenu",
					LoadSceneMode.Single
                );
            }
		}
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
            // Logic to trigger next wave or show victory UI
            currentWaveIndex++;
            if (NetworkManager.Singleton)
            {
                if(NetworkManager.Singleton.IsHost) StartNextWaveServerRpc();
                if(NetworkManager.Singleton.IsClient) DisplayWaveObjectsClientRpc(waves[currentWaveIndex]);
            } else {
                StartNextWave();
            }
		}
	}



}

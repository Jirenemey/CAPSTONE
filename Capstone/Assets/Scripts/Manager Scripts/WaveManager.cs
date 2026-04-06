using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
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

public class WaveManager : NetworkBehaviour {
    [Header("Arena Wave Variables")]
    [SerializeField] public int currentWaveIndex = 0;
    [SerializeField] int maxWaves = 10;
    [Header("Enemy Prefabs")]
    // can rename enemy names
    [SerializeField] GameObject[] enemies;
    int enemyCount = 0;
    [SerializeField] public WaveData[] waves;
    [SerializeField] public Transform respawnAnchor;
    [SerializeField] public Transform deathAnchor;
    [SerializeField] ArenaManager arenaManager;

    [SerializeField] GameOver gameOver;

    public NetworkVariable<int> networkWaveIndex = new NetworkVariable<int>(0);

    bool spawned = false;


    void Start() {
        arenaManager = GetComponent<ArenaManager>();
        if(!gameOver) gameOver = GameObject.Find("GameOverContainer").GetComponent<GameOver>();
        if(NetworkManager.Singleton) OnNetworkSpawn();
    }

    public void StartNextWave(){

        if(currentWaveIndex >= waves.Length) {
            gameOver.SetVictory();
		}

        WaveData currentWave = waves[currentWaveIndex];
        if(currentWaveIndex != 0) waves[currentWaveIndex-1].waveParentGameObject.SetActive(false);
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

    public void EnemyDied() {
		enemyCount--;
		Debug.Log("Enemy Count: " + enemyCount);

		if (enemyCount <= 0) {
			Debug.Log("Wave Cleared!");
            // Logic to trigger next wave or show victory UI
            currentWaveIndex++;
            StartNextWave();
            
		}
	}

// Multiplayer related methods

    [ClientRpc]
    public void SetVictoryClientRpc()
    {
        gameOver.SetVictory();
    }

    [ServerRpc(RequireOwnership=false)]
    public void StartNextWaveServerRpc()
    {

        if(networkWaveIndex.Value >= waves.Length) {
            SetVictoryClientRpc();
        }

        ReviveAllDeadPlayersClientRpc();     
        DisplayWaveObjectsClientRpc();
        
        // only let server instantiate once
        if(!spawned){
            spawned = true;
            Debug.Log("Network Wave: " + networkWaveIndex);
            WaveData currentWave = waves[networkWaveIndex.Value];
            foreach(EnemyData enemyData in currentWave.enemies) {
                GameObject spawnedEnemy = Instantiate(enemyData.enemie, enemyData.spawnPoint);
                IDamageable damageable = spawnedEnemy.GetComponent<IDamageable>();
                spawnedEnemy.GetComponent<NetworkObject>().Spawn();

                if (damageable != null) {
                    enemyCount++;
                    damageable.OnDeath += EnemyDeathEventServerRpc;
                    EnemyBase aiScript = spawnedEnemy.GetComponent<EnemyBase>();
                    if(aiScript != null) {
                        aiScript.enabled = true;
                    }
                }
            }
        }
    }

    [ClientRpc]
    void DisplayWaveObjectsClientRpc()
    {
        if(networkWaveIndex.Value != 0) waves[networkWaveIndex.Value-1].waveParentGameObject.SetActive(false);
        waves[networkWaveIndex.Value].waveParentGameObject.SetActive(true);
    }

    [ServerRpc(RequireOwnership=false)]
    public void EnemyDeathEventServerRpc()
    {
        enemyCount--;
		Debug.Log("Enemy Count: " + enemyCount);

		if (enemyCount <= 0) {
            spawned = false;
            networkWaveIndex.Value += 1;
        }
		
    }

    public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();
		
    	networkWaveIndex.OnValueChanged += OnWaveIndexChanged;
	}

	private void OnWaveIndexChanged(int oldValue, int newValue)
	{
		Debug.Log("New wave started!");
        StartNextWaveServerRpc();
	}

    [ClientRpc]
    public void ReviveAllDeadPlayersClientRpc()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var player = client.PlayerObject;
            var stats = player.GetComponent<PlayerStats>();

            if(stats.isDead.Value == true){
                stats.isDead.Value = false;
                stats.Heal(stats.GetMaxHp() / 2);
                
                player.GetComponent<PlayerInput>().enabled = true;
                player.GetComponent<SpriteRenderer>().enabled = true;
                player.transform.position = respawnAnchor.position;

                if(player.GetComponent<Player>().IsOwner) 
                    arenaManager.SetCameraTarget(player.transform);
            }
        }
    }
}

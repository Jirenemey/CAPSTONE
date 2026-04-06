using System;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;


public class PlayerStats : NetworkBehaviour {
	[Header("Health")]
	[SerializeField] private int maxHp = 5;
	[SerializeField] private int currentHp;

	[Header("Soul")]
	[SerializeField] private int maxSoul = 3;
	[SerializeField] private int currentSoul;

	// Events that your UI scripts will subscribe to
	public event Action<int, int> OnHealthChanged; // Sends (currentHp, maxHp)
	public event Action<int, int> OnSoulChanged;   // Sends (currentSoul, maxSoul)
	public event System.Action OnPlayerDeath;

	public NetworkVariable<bool> isDead = new NetworkVariable<bool>();

	private void Start() {
		currentHp = maxHp;
		currentSoul = 0;

		// Notify of starting values once the game starts
		OnHealthChanged?.Invoke(currentHp, maxHp);
		OnSoulChanged?.Invoke(currentSoul, maxSoul);

		print("PlayerStats created");
		if(!NetworkManager.Singleton)
			FindFirstObjectByType<HealthSoulUI>().Initialize(this);

	}

    public void SetPlayerStats()
    {
		if(IsOwner)
        	FindFirstObjectByType<HealthSoulUI>().Initialize(this);
    }


	public int GetCurrentHp() { return currentHp; }
	public int GetMaxHp() {  return maxHp; }
	public int GetCurrentSoul() {  return currentSoul; }
	public int GetMaxSoul() {  return maxSoul; }

	public void TakeDamage(int damageAmount) {
		if(!GetComponent<Player>().IsOwner && NetworkManager.Singleton) return;

		currentHp -= damageAmount;
		currentHp = Mathf.Clamp(currentHp, 0, maxHp);

		// Fire the event so the UI knows to update a health mask/heart container
		OnHealthChanged?.Invoke(currentHp, maxHp);

		if (currentHp <= 0) {
			Die();
		}
	}

	public void Heal(int healAmount) {
		if(!GetComponent<Player>().IsOwner && NetworkManager.Singleton) return;
		
		currentHp += healAmount;
		currentHp = Mathf.Clamp(currentHp, 0, maxHp);
		OnHealthChanged?.Invoke(currentHp, maxHp);
	}

	public void AddSoul(int soulAmount) {
		if(!GetComponent<Player>().IsOwner && NetworkManager.Singleton) return;
		
		currentSoul += soulAmount;
		currentSoul = Mathf.Clamp(currentSoul, 0, maxSoul);
		OnSoulChanged?.Invoke(currentSoul, maxSoul);
	}

	public bool TryConsumeSoul(int amount) {
		if(!GetComponent<Player>().IsOwner && NetworkManager.Singleton) return false;

		if (currentSoul >= amount) {
			currentSoul -= amount;
			OnSoulChanged?.Invoke(currentSoul, maxSoul);
			return true; // Successfully casted a spell
		}
		return false; // Not enough soul
	}

	private void Die() {
		OnPlayerDeath?.Invoke();
		// Handle death logic (animations, respawn, etc.)
		var gameOver = GameObject.Find("GameOverContainer").GetComponent<GameOver>();
		var arenaManager = GameObject.Find("ArenaManager").GetComponent<ArenaManager>();

		GetComponent<SpriteRenderer>().enabled = false;
		GetComponent<PlayerInput>().enabled = false;
		transform.position = arenaManager.waveManager.deathAnchor.position;
		
		if (NetworkManager.Singleton)
		{
			DieServerRpc();

		} else {
			gameOver.SetGameOver();
		}
	}

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();
		
    	isDead.OnValueChanged += OnDeadStateChanged;
	}

	private void OnDeadStateChanged(bool oldValue, bool newValue)
	{
		Debug.Log("Death state: " + newValue);
		if (!newValue) // revived
		{
			var waveManager = GameObject.Find("ArenaManager").GetComponent<WaveManager>();
			waveManager.ReviveAllDeadPlayersClientRpc();
		} else
		{
			DieServerRpc();
		}
	}

	bool CheckAllPlayersAreDead() {
		foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
		{
			var player = client.PlayerObject.GetComponent<PlayerStats>();

			if (player.isDead.Value == false)
				return false;
		}

		GameOverServerRpc();
		return true;
	}

	[ServerRpc]
	void DieServerRpc()
	{	
		isDead.Value = true;
		OnDeathClientRpc();	
	}

	[ClientRpc]
	void OnDeathClientRpc()
	{
		if (!IsOwner) return;
		
		Debug.Log("Died: " + isDead.Value);
		
		var arenaManager = GameObject.Find("ArenaManager").GetComponent<ArenaManager>();

		arenaManager.SetCameraTarget(arenaManager.otherPlayer.transform);
		GetComponent<SpriteRenderer>().enabled = false;
		GetComponent<PlayerInput>().enabled = false;
		transform.position = arenaManager.waveManager.deathAnchor.position;

		if (CheckAllPlayersAreDead())
		{
			Debug.Log("Game Over. All players dead.");
		} else {
			Debug.Log("Player(s) still alive.");
		}
		
	}

	[ServerRpc(RequireOwnership = false)]
	void GameOverServerRpc()
	{
		SendGameOverClientRpc();
	}

	[ClientRpc]
	void SendGameOverClientRpc()
	{
		var gameOver = GameObject.Find("GameOverContainer").GetComponent<GameOver>();
		gameOver.SetGameOver();
	}

}
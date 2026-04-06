using System;
using UnityEngine;
using Unity.Netcode;

public class PlayerStats : MonoBehaviour {
	[Header("Health")]
	[SerializeField] private int maxHp = 5;
	[SerializeField] private int currentHp;

	[Header("Soul")]
	[SerializeField] private int maxSoul = 3;
	[SerializeField] private int currentSoul;

	// Events that your UI scripts will subscribe to
	public event Action<int, int> OnHealthChanged; // Sends (currentHp, maxHp)
	public event Action<int, int> OnSoulChanged;   // Sends (currentSoul, maxSoul)
	public static event System.Action OnPlayerDeath;

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
        FindFirstObjectByType<HealthSoulUI>().Initialize(this);
    }


	public int GetCurrentHp() { return currentHp; }
	public int GetMaxHp() {  return maxHp; }
	public int GetCurrentSoul() {  return currentSoul; }
	public int GetMaxSoul() {  return maxSoul; }

	public void TakeDamage(int damageAmount) {
		currentHp -= damageAmount;
		currentHp = Mathf.Clamp(currentHp, 0, maxHp);

		// Fire the event so the UI knows to update a health mask/heart container
		OnHealthChanged?.Invoke(currentHp, maxHp);

		if (currentHp <= 0) {
			Die();
		}
	}

	public void Heal(int healAmount) {
		currentHp += healAmount;
		currentHp = Mathf.Clamp(currentHp, 0, maxHp);
		OnHealthChanged?.Invoke(currentHp, maxHp);
	}

	public void AddSoul(int soulAmount) {
		currentSoul += soulAmount;
		currentSoul = Mathf.Clamp(currentSoul, 0, maxSoul);
		OnSoulChanged?.Invoke(currentSoul, maxSoul);
	}

	public bool TryConsumeSoul(int amount) {
		if (currentSoul >= amount) {
			currentSoul -= amount;
			OnSoulChanged?.Invoke(currentSoul, maxSoul);
			return true; // Successfully casted a spell
		}
		return false; // Not enough soul
	}

	private void Die() {
		PlayerStats.OnPlayerDeath?.Invoke();
		// Handle death logic (animations, respawn, etc.)
		if (NetworkManager.Singleton)
		{
			var arenaManager = GameObject.Find("ArenaManager").GetComponent<ArenaManager>();
			arenaManager.SetCameraToOwner(true);
		}
	}
}
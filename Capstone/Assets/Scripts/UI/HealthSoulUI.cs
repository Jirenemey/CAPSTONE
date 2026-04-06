using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class HealthSoulUI : MonoBehaviour
{
	public Image soulFill;
	[SerializeField] GameObject MaskPrefab;
	[SerializeField] Sprite emptyMask, filledMask;
	[SerializeField] GameObject maskContainer;

	private List<GameObject> spawnedMasks = new List<GameObject>();
	PlayerStats localPlayer;
	public void Initialize(PlayerStats player) {
		localPlayer = player;

		// Subscribe to events
		localPlayer.OnHealthChanged += UpdateHealth;
		localPlayer.OnSoulChanged += UpdateSoul;

		// Initial Setup
		SetupMasks(player.GetMaxHp()); // Assuming 5 is max HP, or get from player
		UpdateHealth(player.GetCurrentHp(), player.GetMaxHp());
		UpdateSoul(player.GetCurrentSoul(), player.GetMaxSoul());
	}

	private void SetupMasks(int maxHp) {
		// Clear existing
		foreach (var m in spawnedMasks) Destroy(m);
		spawnedMasks.Clear();

		// setup with all empty masks
		for (int i = 0; i < maxHp; i++) {
			GameObject mask = Instantiate(MaskPrefab, maskContainer.transform);
			spawnedMasks.Add(mask);
			mask.GetComponent<Image>().sprite = emptyMask;
		}
	}

	public void UpdateHealth(int currentHp, int maxHp) {
		// assume mxHP is the same and was setup before
		int i = 0;
		while(i < currentHp) {
			spawnedMasks[i].GetComponent<Image>().sprite = filledMask;
			i++;
		}
		while(i < maxHp) {
			spawnedMasks[i].GetComponent<Image>().sprite = emptyMask;
			i++;
		}

		//if (spawnedMasks.Count != maxHp) {
		//	// recrete all masks
		//	spawnedMasks.ForEach(m => Destroy(m));

		//	for(int i = 0;i < maxHp; i++) {
		//		Sprite img = i <= currentHp ? filledMask : emptyMask;
		//		GameObject mask = Instantiate(MaskPrefab);
		//		mask.GetComponent<Image>().sprite = img;
		//	}
		//}
		//for (int i = 0; i < spawnedMasks.Count; i++) {
		//	Sprite img = i <= currentHp ? filledMask : emptyMask;
		//	if(!spawnedMasks[i]) SetupMasks(maxHp);
		//	spawnedMasks[i].GetComponent<Image>().sprite = img;
		//}
	}

	public void UpdateSoul(int currentSoul, int maxSoul) {
		soulFill.fillAmount = (float)currentSoul / maxSoul;
	}
}

using System.Collections.Generic;
using UnityEngine;

public class DamageTicker : MonoBehaviour {
	public float damage;
	public float tickInterval;

	private List<IDamageable> targetsInRange = new List<IDamageable>();
	private Dictionary<IDamageable, float> nextTickTime = new Dictionary<IDamageable, float>();

	private void Update() {
		for (int i = targetsInRange.Count - 1; i >= 0; i--) {
			var target = targetsInRange[i];
			if (Time.time >= nextTickTime[target]) {
				target.TakeDamage(damage);
				nextTickTime[target] = Time.time + tickInterval;
			}
		}
	}

	private void OnTriggerEnter2D(Collider2D other) {
		if (other.TryGetComponent<IDamageable>(out var target)) {
			if (!targetsInRange.Contains(target)) {
				targetsInRange.Add(target);
				nextTickTime[target] = Time.time; // Damage immediately on enter
			}
		}
	}

	private void OnTriggerExit2D(Collider2D other) {
		if (other.TryGetComponent<IDamageable>(out var target)) {
			targetsInRange.Remove(target);
			nextTickTime.Remove(target);
		}
	}
}
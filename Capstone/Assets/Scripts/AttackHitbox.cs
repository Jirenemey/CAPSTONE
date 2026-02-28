using UnityEngine;

public class AttackHitbox : MonoBehaviour {
	[SerializeField] LayerMask enemyLayer;
	[SerializeField] float damage = 10f;

	private void OnTriggerEnter2D(Collider2D other) {
		// Check if the object is on the enemy layer
		if ((enemyLayer.value & (1 << other.gameObject.layer)) == 0) return;
		print("hit");
		// Try to get a health/damageable component and apply damage
		if (other.TryGetComponent<IDamageable>(out var target)) {
			target.TakeDamage(damage);
		}

		Debug.Log($"Hit: {other.gameObject.name}");
	}
}

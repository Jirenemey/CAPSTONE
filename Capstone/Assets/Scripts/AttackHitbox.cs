using UnityEngine;

public class AttackHitbox : MonoBehaviour {
	[SerializeField] LayerMask enemyLayer;
	[SerializeField] float damage = 10f;
    private bool active = false;

    public void Activate()
    {
        active = true;
        gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        active = false;
        gameObject.SetActive(false);
    }

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

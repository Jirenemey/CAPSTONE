using UnityEngine;

public class AttackHitbox : MonoBehaviour {
	[SerializeField] LayerMask enemyLayer;
	[SerializeField] float damage = 10f;
	[SerializeField] int soulGain = 1;
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
        if(other.gameObject.TryGetComponent<Bouncable>(out Bouncable bouncable)) {
            //push backwards
            transform.parent.GetComponent<Player>().BounceBack();
		}

		if ((enemyLayer.value & (1 << other.gameObject.layer)) == 0) return;
		// Try to get a health/damageable component and apply damage
		if (other.TryGetComponent<IDamageable>(out var target)) {
			target.TakeDamage(damage);
		}

		// Check if the hit object contains soul and grant it to the player
		if (other.TryGetComponent<ContainsSoul>(out var containsSoul)) {
			Player player = transform.parent.GetComponent<Player>();
			if (player != null) {
				PlayerStats playerStats = player.playerStats;
				if (playerStats != null) {
					playerStats.AddSoul(soulGain);
					Debug.Log($"Player gained {soulGain} soul from {other.gameObject.name}");
				}
			}
		}

		Debug.Log($"Hit: {other.gameObject.name}");
	}
}

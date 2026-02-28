using UnityEngine;

public class Projectile : MonoBehaviour {
	[SerializeField] float speed = 15f;
	[SerializeField] float damage = 20f;
	[SerializeField] float lifetime = 3f; // destroy if it never hits a wall

	float direction;

	public void Init(float dir) {
		direction = dir;
		Destroy(gameObject, lifetime);
	}

	void Update() {
		transform.Translate(Vector2.right * direction * speed * Time.deltaTime);
	}

	private void OnTriggerEnter2D(Collider2D other) {
		// Hit wall — destroy projectile
		if (other.gameObject.layer == LayerMask.NameToLayer("Ground")) {
			Destroy(gameObject);
			return;
		}

		// Hit enemy — deal damage but keep going
		if (other.TryGetComponent<IDamageable>(out var target)) {
			target.TakeDamage(damage);
		}
	}
}

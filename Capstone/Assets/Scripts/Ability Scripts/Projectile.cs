using UnityEngine;

public class Projectile : MonoBehaviour {
	[SerializeField] float speed;
	[SerializeField] float damage;
	[SerializeField] float lifetime; // destroy if it never hits a wall
	[SerializeField] int numberOfTicks;

	float direction;

	public void Init(float dir) {
		direction = dir;

		DamageTicker ticker = gameObject.AddComponent<DamageTicker>();
		ticker.damage = damage;
		ticker.tickInterval = lifetime / numberOfTicks;
		AudioManager.instance.PlaySFX("Fireball");


		Destroy(gameObject, lifetime);
	}

	void Update() {
		transform.Translate(Vector2.right * direction * speed * Time.deltaTime);
	}

	private void OnTriggerEnter2D(Collider2D other) {
		// Hit wall — destroy projectile
		if (other.gameObject.TryGetComponent<Wall>(out Wall wall) || other.gameObject.TryGetComponent<Ground>(out Ground ground)) {
			Debug.Log("Projectile hit the wall/ground");
			Destroy(gameObject);
			return;
		}

		// Hit enemy — deal damage but keep going
		//if (other.TryGetComponent<IDamageable>(out var target)) {
		//	target.TakeDamage(damage);
		//}
	}
}

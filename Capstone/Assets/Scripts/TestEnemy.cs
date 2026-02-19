using UnityEngine;

public class TestEnemy : MonoBehaviour, IDamageable {
	float health = 100f;
	SpriteRenderer sr;

	void Start() {
		sr = GetComponent<SpriteRenderer>();
	}
	public void TakeDamage(float amount) {
		health -= amount;
		Debug.Log($"{gameObject.name} took {amount} damage. HP: {health}");
		sr.color = Color.red;
		if (health <= 0) Die();
	}

	void Die() {
		//Destroy(gameObject);
		print("die lol");
	}

}

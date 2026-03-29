using UnityEngine;
using TMPro;
public class TestEnemy : MonoBehaviour, IDamageable, ICanGiveSoul {
	public event System.Action OnDeath;
	float health = 100f;
	float flashDuration = 0.5f;
	SpriteRenderer sr;
	[SerializeField] TMP_Text ui_hp;

	void Start() {
		sr = GetComponent<SpriteRenderer>();
	}
	public void TakeDamage(float amount) {
		health -= amount;
		ui_hp.text = ""+health;
		//Debug.Log($"{gameObject.name} took {amount} damage. HP: {health}");
		StartCoroutine(FlashRed());
		if (health <= 0) Die();
	}

	void Die() {
		//Destroy(gameObject);
		print("die lol");
		OnDeath?.Invoke();
	}

	System.Collections.IEnumerator FlashRed() {
		Color originalColor = Color.white;
		sr.color = Color.red;
		yield return new WaitForSeconds(flashDuration);
		sr.color = originalColor;
	}

	public bool GetSoulVal() {
		if(health > 0f) {
			return true;
		} else {
			return false;
		}
	}
}

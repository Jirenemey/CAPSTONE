using UnityEngine;

public class FlyingCreature : MonoBehaviour, IDamageable
{
    [SerializeField] float minVel = 2.0f;
	[SerializeField] float maxVel = 5.0f;
	float hp = 20f;
    float vel;
    Rigidbody2D rb;
	public Vector2 initialDirection = Vector2.right;

	public event System.Action OnDeath;

	void Start(){
        if(!rb) rb = GetComponent<Rigidbody2D>();
		vel = Random.Range(minVel, maxVel);

		rb.linearVelocity = initialDirection * vel;
	}

	void FixedUpdate(){
		rb.linearVelocity= Vector2.ClampMagnitude(rb.linearVelocity, maxVel);
	}

	public void TakeDamage(float amount) {
		hp -= amount;
		if(hp < 0) {
			OnDeath?.Invoke();
			Destroy(gameObject);
		}
	}
}

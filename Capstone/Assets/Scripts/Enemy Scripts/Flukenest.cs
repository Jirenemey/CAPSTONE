using UnityEngine;

public class Flukenest : MonoBehaviour, IDamageable {
    Transform[] birthPoints;
	[SerializeField] float delayBetweenSpawns = 5.0f;
	float currentTime = 0.0f;
	[SerializeField] GameObject workPrefab;
	[SerializeField] float hp = 200;

	public event System.Action OnDeath;

	void Start(){
        birthPoints = GetComponentsInChildren<Transform>();
    }

    void Update(){
		currentTime += Time.deltaTime;
		if(currentTime >= delayBetweenSpawns) {
			SpawnNewWorm();
			currentTime = 0.0f;
		}
    }

    void SpawnNewWorm() {
		Transform randomObj = birthPoints[Random.Range(0, birthPoints.Length)];
		Vector2 dir = RandomVectorWithinCone(randomObj, 120f);
		//Transform wormTransform = new Transform();
		//wormTransform.position = randomObj.position;
		//wormTransform.localRotation = dir;

		float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
		//wormTransform.localRotation = Quaternion.Euler(0, 0, angle);


		Instantiate(workPrefab, randomObj.position, Quaternion.Euler(0, 0, angle), transform);
	}

	public static Vector2 RandomVectorWithinCone(Transform obj, float coneAngleDegrees = 120f) {
	float coneAngleRadians = coneAngleDegrees * Mathf.Deg2Rad;
	float halfAngle = coneAngleRadians / 2f;
	Vector3 up = obj.up;

	float angle = Random.Range(-halfAngle, halfAngle);

	Vector2 randomDir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

	Quaternion rotation = Quaternion.Euler(0, 0, 0);
	Vector2 worldDir = rotation * randomDir;

	return worldDir.normalized;
}

	public void TakeDamage(float amount) {
		hp -= amount;
		if (hp <= 0f) {
			OnDeath?.Invoke();
			Destroy(gameObject);
		}
	}
}

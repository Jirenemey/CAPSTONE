using System.Collections;
using UnityEngine;

public class Spikes : MonoBehaviour
{
	Transform originalPos;
	Transform finalPos;
	float riseDuration = 0.5f;

	void Awake() {
		originalPos = transform;
		float halfHight = GetComponent<SpriteRenderer>().size.y / 2;
		finalPos = transform;
		finalPos.position = new Vector3(transform.position.x, transform.position.y + halfHight, transform.position.z);
	}

	private void OnEnable() {
		StartCoroutine(RiseToFinalPosition());
	}
	private void OnDisable() {
		StartCoroutine(RevertToOriginalPosition());
	}

	IEnumerator RiseToFinalPosition() {
		float elapsedTime = 0f;
		Vector3 startPosition = originalPos.position;
		Vector3 endPosition = finalPos.position;

		while (elapsedTime < riseDuration) {
			float t = elapsedTime / riseDuration;
			transform.position = Vector3.Lerp(startPosition, endPosition, t);
			elapsedTime += Time.deltaTime;
			yield return null;
		}

		transform.position = endPosition;
	}
	IEnumerator RevertToOriginalPosition() {
		float elapsedTime = 0f;
		Vector3 startPosition = finalPos.position;
		Vector3 endPosition = originalPos.position;

		while (elapsedTime < riseDuration) {
			float t = elapsedTime / riseDuration;
			transform.position = Vector3.Lerp(startPosition, endPosition, t);
			elapsedTime += Time.deltaTime;
			yield return null;
		}

		transform.position = endPosition;
	}

	void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.TryGetComponent<Player>(out Player player)) {
            // kill player then respawn them
            player.Respawn();
        }
    }

}

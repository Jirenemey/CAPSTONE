using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class HowlingWraiths : MonoBehaviour
{
    public float duration;
    public float dmg;
	public int numberOfTicks;

    void Start(){
		DamageTicker ticker = gameObject.AddComponent<DamageTicker>();

		ticker.damage = dmg;
		ticker.tickInterval = duration / numberOfTicks;
		PlaySound();

		Destroy(gameObject, duration);
	}

	private void PlaySound() {
		AudioManager.instance.PlaySFX("HollowScream");
	}
}

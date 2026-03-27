using System.Collections.Generic;
using UnityEngine;

public class HowlingWraiths : MonoBehaviour
{
    public float duration;
    public float dmg;
	public int numberOfTicks;

    void Start(){
		DamageTicker ticker = gameObject.AddComponent<DamageTicker>();

		ticker.damage = dmg;
		ticker.tickInterval = duration / numberOfTicks;

		Destroy(gameObject, duration);
	}
}

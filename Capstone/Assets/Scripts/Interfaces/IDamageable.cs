using System;

public interface IDamageable {
	void TakeDamage(float amount);
	event System.Action OnDeath;
}
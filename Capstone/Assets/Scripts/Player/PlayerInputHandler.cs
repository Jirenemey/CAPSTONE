using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour {
	[Header("Input Actions")]
	[SerializeField] InputActionReference moveAction;
	[SerializeField] InputActionReference jumpAction;
	[SerializeField] InputActionReference dashAction;
	[SerializeField] InputActionReference attackAction;
	[SerializeField] InputActionReference quickCastAction;

	// Public properties for other scripts to read
	public Vector2 MovementInput { get; private set; }
	public bool JumpTriggered { get; private set; }
	public bool JumpReleased { get; private set; }
	public bool DashTriggered { get; private set; }
	public bool AttackTriggered { get; private set; }
	public bool QuickCastTriggered { get; private set; }

	private void OnEnable() {
		moveAction.action.Enable();

		jumpAction.action.started += ctx => JumpTriggered = true;
		jumpAction.action.canceled += ctx => JumpReleased = true;
		jumpAction.action.Enable();

		dashAction.action.started += ctx => DashTriggered = true;
		dashAction.action.Enable();

		attackAction.action.started += ctx => AttackTriggered = true;
		attackAction.action.Enable();

		quickCastAction.action.started += ctx => QuickCastTriggered = true;
		quickCastAction.action.Enable();
	}

	private void OnDisable() {
		moveAction.action.Disable();
		jumpAction.action.Disable();
		dashAction.action.Disable();
		attackAction.action.Disable();
		quickCastAction.action.Disable();
	}

	private void Update() {
		MovementInput = moveAction.action.ReadValue<Vector2>();
	}

	// Call this at the end of the frame in your main controller to reset triggers
	public void ConsumeTriggers() {
		JumpTriggered = false;
		JumpReleased = false;
		DashTriggered = false;
		AttackTriggered = false;
		QuickCastTriggered = false;
	}
}
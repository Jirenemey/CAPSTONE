using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour {
	[Header("Input Actions")]
	[SerializeField] InputActionReference moveAction;
	[SerializeField] InputActionReference jumpAction;
	[SerializeField] InputActionReference dashAction;
	[SerializeField] InputActionReference attackAction;
	[SerializeField] InputActionReference quickCastAction;
	[SerializeField] InputActionReference focusAction;

	[Header("Input Settings")]
	[SerializeField, Range(0f, 0.5f)] private float moveDeadZone = 0.25f;

	// Public properties for other scripts to read
	public Vector2 MovementInput { get; private set; }
	public bool JumpTriggered { get; private set; }
	public bool JumpReleased { get; private set; }
	public bool DashTriggered { get; private set; }
	public bool AttackTriggered { get; private set; }
	public bool QuickCastTriggered { get; private set; }
	public bool FocusHeld { get; private set; }

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

		focusAction.action.started += ctx => FocusHeld = true;
		focusAction.action.canceled += ctx => FocusHeld = false;
		focusAction.action.Enable();
	}
	private void OnDisable() {
		moveAction.action.Disable();
		jumpAction.action.Disable();
		dashAction.action.Disable();
		attackAction.action.Disable();
		quickCastAction.action.Disable();
		focusAction.action.Disable();
	}

	private void Update() {
		Vector2 rawInput = moveAction.action.ReadValue<Vector2>();
		MovementInput = new Vector2(
			Mathf.Abs(rawInput.x) < moveDeadZone ? 0f : rawInput.x,
			Mathf.Abs(rawInput.y) < moveDeadZone ? 0f : rawInput.y
		);
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
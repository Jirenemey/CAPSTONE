using System;
using System.Collections.Generic;
using UnityEngine;

public enum NodeState {
	Success,    // Node execution succeeded
	Failure,    // Node execution failed
	Running     // Node is running (executes over multiple frames)
}

public abstract class BTNode {
	public abstract NodeState Run();
}

public class Sequence : BTNode {
	List<BTNode> children;  // List of child nodes
	int currentIndex = 0;   // Index of currently executing child node

	public Sequence(List<BTNode> nodes) {
		children = nodes;
	}

	public override NodeState Run() {
		while (currentIndex < children.Count) {
			var result = children[currentIndex].Run();

			// To do: If the result is Failure, make the entire sequence fail. At this point, the index should be reset to 0.            
			if (result == NodeState.Failure) // If any child fails, the entire sequence fails
			{
				return NodeState.Failure;
			}

			// To do: If the result is Running, make the sequence return Running.
			if (result == NodeState.Running) // If Running, maintain current index and wait
			{
				return NodeState.Running;
			}

			// To do: If the result is Success, increase the current index.
			currentIndex++;
		}

		// All children succeeded
		currentIndex = 0; // Reset index
		return NodeState.Success;
	}
}

public class Selector : BTNode {
	List<BTNode> children;  // List of child nodes
	int currentIndex = 0;   // Index of currently executing child node

	public Selector(List<BTNode> nodes) {
		children = nodes;
	}

	public override NodeState Run() {
		// To do: Complete this Run function to perform the functionality of a Selector node.
		//        Refer to the Run function of the Sequence node and follow the rules below.
		// - If the result is Success, the selector as a whole succeeds and the current index should be reset.
		// - If the result is Running, keep the current index unchanged and return Running.
		// - If the result is Failure, move on to the next child.
		// - If all children fail, reset the current index and return Failure.
		while (currentIndex < children.Count) {
			var result = children[currentIndex].Run();

			if (result == NodeState.Running) {
				return NodeState.Running;
			}
			if (result == NodeState.Success) {
				currentIndex = 0;
				return NodeState.Success;
			}
			currentIndex++;
		}
		currentIndex = 0;
		return NodeState.Failure;
	}
}

public class WaitTask : BTNode {
	float waitTime;
	float startTime;
	bool started = false;

	public WaitTask(float seconds) {
		waitTime = seconds;
	}

	public override NodeState Run() {
		// To do: Implement a Wait node that returns Running until the specified waitTime has elapsed, 
		//        then returns Success.
		if (!started) {
			startTime = Time.time;
			started = true;
		}
		float elapsed = Time.time - startTime;
		//Debug.Log("Elapsed: " + elapsed + "\twaitTime: " + waitTime + "\t startTime: " + startTime);
		if (elapsed < waitTime) {
			return NodeState.Running;
		}
		started = false;
		return NodeState.Success;
	}
}

public class Condition : BTNode {
	private Func<NodeState> condition;
	public Condition(Func<NodeState> condition) {
		this.condition = condition;
	}
	public override NodeState Run() {
		return condition();
	}

}

public class Action : BTNode {
	private System.Func<NodeState> action;
	public Action(System.Func<NodeState> action) {
		this.action = action;
	}
	public override NodeState Run() {
		return action();
	}
}
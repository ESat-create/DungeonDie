using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AdvancedPlayerMovement : MonoBehaviour
{
	private const float RightHand = 90.0f;
	private const float LeftHand = -RightHand;
	private const float approximationThreshold = 0.025f;

	[SerializeField] private float gridSize = 3.0f;
	public AudioClip steps;

	[Header("Walk speed settings")]
	[SerializeField] private float walkSpeed = 1.0f;
	[SerializeField] private float turnSpeed = 5.0f;

	[Header("Walking animation curve")]
	[SerializeField] private AnimationCurve walkSpeedCurve;

	[Header("Walking head bob curve")]
	[SerializeField] private AnimationCurve walkHeadBobCurve;

	[Header("Run speed settings")]
	[SerializeField] private float runningSpeed = 1.5f;

	[Header("Maximum step height")]
	[SerializeField] private float maximumStepHeight = 2.0f;

	[Header("Event when the path is blocked")]
	[SerializeField] private UnityEvent blockedEvent;

	[Header("Event when the player takes a step")]
	[SerializeField] private UnityEvent stepEvent;

	[Header("Event when the player is turning")]
	[SerializeField] private UnityEvent turnEvent;

	// Animation target values.
	private Vector3 moveTowardsPosition;
	private Quaternion rotateFromDirection;

	
	// Animation source values.
	private Vector3 moveFromPosition;
	private Quaternion rotateTowardsDirection;

	// Animation progress
	private float rotationTime = 0.0f;
	private float curveTime = 0.0f;

	private float stepTime = 0.0f;
	private float stepTimeCounter = 0.0f;

	//Current settings
	private AnimationCurve currentAnimationCurve;
	private AnimationCurve currentHeadBobCurve;
	private float currentSpeed;

	void Start()
	{
		moveTowardsPosition = transform.position;
		rotateTowardsDirection = transform.rotation;
		currentAnimationCurve = walkSpeedCurve;
		currentHeadBobCurve = walkHeadBobCurve;
		currentSpeed = walkSpeed;
		stepTime = 1.0f / gridSize;
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.W))
			MoveForward();
		if (Input.GetKeyDown(KeyCode.A))
			StrafeLeft();
		if (Input.GetKeyDown(KeyCode.S))
			MoveBackward();
		if (Input.GetKeyDown(KeyCode.D))
			StrafeRight();
		if (Input.GetKeyDown(KeyCode.E))
			TurnRight();
		if (Input.GetKeyDown(KeyCode.Q))
			TurnLeft();
		if (IsMoving())
		{
			AnimateMovement();
		}

		if (IsRotating())
		{
			AnimateRotation();
		}
	}

	private void AnimateRotation()
	{
		rotationTime += Time.deltaTime;
		transform.rotation = Quaternion.Slerp(rotateFromDirection, rotateTowardsDirection, rotationTime * turnSpeed);
		CompensateRoundingErrors();
	}

	private void AnimateMovement()
	{
		curveTime += Time.deltaTime * currentSpeed;

		stepTimeCounter += Time.deltaTime * currentSpeed;

		if (stepTimeCounter > stepTime)
		{
			stepTimeCounter = 0.0f;
			stepEvent?.Invoke();
		}

		var currentPositionValue = currentAnimationCurve.Evaluate(curveTime);
		var currentHeadBobValue = currentHeadBobCurve.Evaluate(curveTime * gridSize);

		var targetHeading = Vector3.Normalize(HeightInvariantVector(moveTowardsPosition) - HeightInvariantVector(moveFromPosition));
		var newPosition = moveFromPosition + (targetHeading * (currentPositionValue * gridSize));
		newPosition.y = maximumStepHeight;

		RaycastHit hit;
		Ray downRay = new Ray(newPosition, -Vector3.up);
		//newPosition.y = currentHeadBobValue;
		// Cast a ray straight downwards.
		if (Physics.Raycast(downRay, out hit))
		{
			newPosition.y = (maximumStepHeight - hit.distance) + currentHeadBobValue;
		}
		else
		{
			newPosition.y = currentHeadBobValue;
		}

		transform.position = newPosition;
		CompensateRoundingErrors();
	}

	private void CompensateRoundingErrors()
	{
		// Bear in mind that floating point numbers are inaccurate by design. 
		// The == operator performs a fuzy compare which means that we are only approximatly near the target value.
		// We may not entirely reached the value yet or we may have slightly overshot it already (both within the margin of error).
		if (transform.rotation == rotateTowardsDirection)
		{
			// To compensate rounding errors we explictly set the transform to our desired rotation.
			transform.rotation = rotateTowardsDirection;
		}

		//mask out the head bobbing
		var currentPosition = HeightInvariantVector(transform.position);
		var target = HeightInvariantVector(moveTowardsPosition);

		if (currentPosition == target)
		{
			// To compensate rounding errors we explictly set the transform to our desired rotation.
			currentPosition = HeightInvariantVector(moveTowardsPosition);
			currentPosition.y = transform.position.y;

			transform.position = currentPosition;
			curveTime = 0.0f;
			stepTimeCounter = 0.0f;
		}

	}

	public void MoveForward()
	{
		RaycastHit hit;
		if(!Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, gridSize))
			CollisonCheckedMovement(CalculateForwardPosition());
	}

	public void MoveBackward()
	{
		RaycastHit hit;
		if (!Physics.Raycast(transform.position, transform.TransformDirection(Vector3.back), out hit, gridSize))
			CollisonCheckedMovement(-CalculateForwardPosition());
	}

	public void StrafeRight()
	{
		RaycastHit hit;
		if (!Physics.Raycast(transform.position, transform.TransformDirection(Vector3.right), out hit, gridSize))
			CollisonCheckedMovement(CalculateStrafePosition());
	}

	public void StrafeLeft()
	{
		RaycastHit hit;
		if (!Physics.Raycast(transform.position, transform.TransformDirection(Vector3.left), out hit, gridSize))
			CollisonCheckedMovement(-CalculateStrafePosition());
	}

	private void CollisonCheckedMovement(Vector3 movementDirection)
	{
		
		if (IsStationary())
		{
			Vector3 targetPosition = moveTowardsPosition + movementDirection;
			moveFromPosition = transform.position;
			moveTowardsPosition = targetPosition;
			AudioManager.instance.PlaySound(steps);
			//if (!FreeSpace(movementDirection))
			//{
			//	moveFromPosition = transform.position;
			//	moveTowardsPosition = targetPosition;
			//}
			//else
			//{
			//	blockedEvent?.Invoke();
			//}
		}
	}

	// should be refactored into an new class
	private bool FreeSpace(Vector3 movementDirection)
	{
		// this is pretty lousy way to perform collision checks, its just here for demonstration purposes.
		// Hint: layers are much faster then tags ;-)
		//Vector3 delta = targetPosition - moveTowardsPosition;
		//delta *= .6f;
		//Collider[] intersectingColliders = Physics.OverlapBox(moveTowardsPosition + delta, new Vector3((gridSize / 2.0f) - .1f, 1.0f, (gridSize / 2.0f) - .1f), gameObject.transform.rotation);
		//Collider[] filteredColliders = System.Array.FindAll(intersectingColliders, collider => collider.CompareTag("Level"));
		//return filteredColliders.Length == 0;
		RaycastHit hit;
		return Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, gridSize);
	}

	public void TurnRight()
	{
		turnEvent?.Invoke();
		TurnEulerDegrees(RightHand);
	}

	public void TurnLeft()
	{
		turnEvent?.Invoke();
		TurnEulerDegrees(LeftHand);
	}

	private void TurnEulerDegrees(in float eulerDirectionDelta)
	{
		if (!IsRotating())
		{
			rotateFromDirection = transform.rotation;
			rotateTowardsDirection *= Quaternion.Euler(0.0f, eulerDirectionDelta, 0.0f);
			rotationTime = 0.0f;
		}
	}

	public bool IsStationary()
	{
		return !(IsMoving() || IsRotating());
	}

	private bool IsMoving()
	{
		var current = HeightInvariantVector(transform.position);
		var target = HeightInvariantVector(moveTowardsPosition);
		return current != target;
	}

	private bool IsRotating()
	{
		return transform.rotation != rotateTowardsDirection;
	}

	private Vector3 HeightInvariantVector(Vector3 inVector)
	{
		return new Vector3(inVector.x, 0.0f, inVector.z);
	}

	private Vector3 CalculateForwardPosition()
	{
		return transform.forward * gridSize;
	}

	private Vector3 CalculateStrafePosition()
	{
		return transform.right * gridSize;
	}
}

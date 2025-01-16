using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	public float gridSize = 2.0f;
	public float moveSpeed = 5.0f;
	public float rotateSpeed = 360.0f;

	private Vector3 pos;
	private Transform tr;
	private Quaternion rot;
	private Quaternion rotateTowardsDirection;

	private float degree;
	private float down;
	private float angle;
	private bool browsing = false;
	public bool inBattle = false;

	[Header("Movement Keys")]
	public KeyCode forwardKey = KeyCode.W;
	public KeyCode downKey = KeyCode.S;
	[Space(1)]
	public KeyCode rightKey = KeyCode.D;
	public KeyCode leftKey = KeyCode.A;
	[Space(1)]
	public KeyCode turnLeftKey = KeyCode.Q;
	public KeyCode turnRightKey = KeyCode.E;


	void Start()
	{
		rotateTowardsDirection = transform.rotation;
		pos = transform.position;
		tr = transform;
		rot = transform.rotation;
	}

	void Update()
	{
		bool canMove = (tr.position == pos) ;
		if (canMove)
		{
			if (Input.GetKey(forwardKey))
			{
				if (!RaycastHit("forward"))
					pos += transform.forward * gridSize;
			}
			else if (Input.GetKey(downKey))
			{
				if (!RaycastHit("back"))
					pos += -transform.forward * gridSize;
			}
			else if (Input.GetKey(rightKey))
			{
				if (!RaycastHit("right"))
					pos += transform.right * gridSize;
			}
			else if (Input.GetKey(leftKey))
			{
				if (!RaycastHit("left"))
					pos += -transform.right * gridSize;
			}
			else if (Input.GetKey(turnLeftKey))
			{
				//degree = Mathf.Repeat(degree - 90f, 360f);
				rotateTowardsDirection *= Quaternion.Euler(0.0f, -90f, 0.0f);
			}
			else if (Input.GetKey(turnRightKey))
			{
				//degree = Mathf.Repeat(degree + 90f, 360f);
				rotateTowardsDirection *= Quaternion.Euler(0.0f, 90f, 0.0f);
			}
		}
		
		transform.position = Vector3.MoveTowards(transform.position, pos, Time.deltaTime * moveSpeed);
		transform.rotation = Quaternion.Slerp(transform.rotation, rotateTowardsDirection, Time.deltaTime * rotateSpeed);
		//angle = Mathf.MoveTowardsAngle(transform.rotation.y, degree, Time.deltaTime * 20f);
		//transform.eulerAngles = new Vector3(transform.eulerAngles.x, angle, transform.eulerAngles.z);
	}
	public bool RaycastHit(string direction)
	{
		RaycastHit hit;
		switch(direction)
		{
			default:
				return Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, gridSize);
			case "back":
				return Physics.Raycast(transform.position, transform.TransformDirection(Vector3.back), out hit, gridSize);
			case "right":
				return Physics.Raycast(transform.position, transform.TransformDirection(Vector3.right), out hit, gridSize);
			case "left":
				return Physics.Raycast(transform.position, transform.TransformDirection(Vector3.left), out hit, gridSize);
		}
	}
}

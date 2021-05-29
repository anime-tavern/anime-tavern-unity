using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddConstantVelocity : MonoBehaviour
{

	// Start is called before the first frame update
	void Start()
	{

	}

	// Update is called once per frame
	void FixedUpdate()
	{
		Vector3 finalForce = new Vector3();
		float speed = 0.5f;

		if (Input.GetKey(KeyCode.A))
		{
			finalForce.x += -speed;
		}

		if (Input.GetKey(KeyCode.D))
		{
			finalForce.x += speed;
		}

		if (Input.GetKey(KeyCode.S))
		{
			finalForce.z += -speed;
		}

		if (Input.GetKey(KeyCode.W))
		{
			finalForce.z += speed;
		}

		Rigidbody rb = GetComponent<Rigidbody>();
		rb.velocity = Vector3.ClampMagnitude(rb.velocity += finalForce, 3);
	}
}

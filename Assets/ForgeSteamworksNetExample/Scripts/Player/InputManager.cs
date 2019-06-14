using UnityEngine;

namespace ForgeSteamworksNETExample.Player
{
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(CapsuleCollider))]
	public class InputManager : MonoBehaviour
	{
		public float MoveAmount { get; private set; }

		private float horizontal;

		private float vertical;

		private Camera cam;

		private Rigidbody rb;

		private Animator baseAnimator;

		private Vector3 moveDirection;

		private int animatorVertical;

		[SerializeField]
		private float moveSpeed = 6f;

		[SerializeField]
		private float rotationSpeed = 40f;

		private void Start()
		{
			rb = GetComponent<Rigidbody>();
			baseAnimator = GetComponent<Animator>();
			cam = GetComponentInChildren<Camera>();
			animatorVertical = Animator.StringToHash("vertical");
		}

		private void Update()
		{
			horizontal = Input.GetAxis("Horizontal");
			vertical = Input.GetAxis("Vertical");
			MoveAmount = Mathf.Clamp01(Mathf.Abs(horizontal) + Mathf.Abs(vertical));
		}

		private void FixedUpdate()
		{
			CameraRelativeMovement();
		}

		private void LateUpdate()
		{
			baseAnimator.SetFloat(animatorVertical, MoveAmount);
		}

		private void CameraRelativeMovement()
		{
			// Forward vector relative to the camera along the xz plane
			var forward = cam.transform.TransformDirection(Vector3.forward);
			forward.y = 0;
			forward = forward.normalized;

			// Right vector relative to the camera always orthogonal to the forward vector
			//var right = new Vector3(forward.z, 0, -forward.x);
			var right = cam.transform.TransformDirection(Vector3.right);

			var inputVector = horizontal * right + vertical * forward;

			if (inputVector.magnitude > 1)
				inputVector.Normalize();

			var moveVelocity = inputVector * moveSpeed;
			moveVelocity.y = rb.velocity.y;

			if (inputVector != Vector3.zero)
			{
				rb.rotation = Quaternion.Slerp(rb.rotation, Quaternion.LookRotation(inputVector), Time.deltaTime * rotationSpeed);
			}

			rb.velocity = moveVelocity;
		}
	}
}

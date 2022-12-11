using UnityEngine;

public class BulletMovement : MonoBehaviour
{
	private float _angleRad, _bulletVelocity, _gravityValue;
	private Vector3 _gravityVector, _velocityVector, _normal;
	private Transform _planet;
	private bool _withRigidBody, _lookingRight, _startParabolicMovement;

	//FinalWithRigidBody
	public void InitValues(Vector3 initialPosition, float velocity, float angle, float gravity)
    {
		_withRigidBody = true;

		//Initialize Position, velocity vector and gravity vector
		_startParabolicMovement = false;
		this.transform.position = initialPosition;
		_gravityVector = Vector3.up * velocity;
		_angleRad = angle * Mathf.PI / 180;

		_velocityVector = new Vector3(
			velocity * Mathf.Cos(_angleRad),
			velocity * Mathf.Sin(_angleRad),
			0f);

		_gravityVector = Vector3.down * gravity;
		_startParabolicMovement = true;
	}

	//FinalWithoutRigidBody
	public void InitValues(Transform initialPosition, bool lookingRight, float bulletVelocity, Transform planet, float gravityValue)
	{
		_withRigidBody = false;

		//Initialize Position, velocity vector and gravity vector
		_startParabolicMovement = false;
		this.transform.position = initialPosition.position;
		_gravityValue = gravityValue;
		_planet = planet;
		_startParabolicMovement = true;
		_lookingRight = lookingRight;
		_bulletVelocity = bulletVelocity;
	}

	private void ManageVectors()
	{
		_gravityVector = ((Vector2)_planet.position - (Vector2)transform.position).normalized * _gravityValue;
		_normal = ((Vector2)transform.position - (Vector2)_planet.transform.position).normalized;
		_velocityVector = Vector3.Cross(_normal, (_lookingRight ? 1 : -1) * Vector3.forward) * _bulletVelocity;
	}

	private void ManageMovement()
	{
		//Add both vectors
		//velocityVector acts as the vector distance traveled each frame
		//Calculate the distance traveled in each frame with the formula d = v * t
		_velocityVector += _gravityVector * Time.deltaTime;

		//Add the distance traveled to the position
		transform.position += _velocityVector * Time.deltaTime;
	}

	void Update()
    {
		if(_startParabolicMovement == true)
		{
			if(_withRigidBody == false)
			{
				ManageVectors();
			}

			ManageMovement();
		}
	}
}

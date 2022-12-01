using UnityEngine;

public class BulletMovement : MonoBehaviour
{
	private float _angleRad;
	private Vector3 _gravityVector, _velocityVector;
	private bool _startParabolicMovement;
	private float _gravityValue;
	private Transform _planet;
	private bool withRigidBody;

	public void InitValues(Vector3 initialPosition, float velocity, float angle, float gravity)
    {
		withRigidBody = true;

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

	public void InitValues(Transform initialPosition, Vector3 velocityVector, Transform planet, float gravityValue)
	{
		withRigidBody = false;

		//Initialize Position, velocity vector and gravity vector
		_startParabolicMovement = false;
		this.transform.position = initialPosition.position;
		_velocityVector = velocityVector;
		_gravityValue = gravityValue;
		_planet = planet;
		_startParabolicMovement = true;
	}

	void Update()
    {
		if(_startParabolicMovement == true)
		{
			if(withRigidBody == false) _gravityVector = ((Vector2)_planet.position - (Vector2)transform.position).normalized * _gravityValue;

			//Add both vectors
			//velocityVector acts as the vector distance traveled each frame
			//Calculate the distance traveled in each frame with the formula d = v * t
			_velocityVector += _gravityVector * Time.deltaTime;

			//Add the distance traveled to the position
			transform.position += _velocityVector * Time.deltaTime;
		}
	}
}

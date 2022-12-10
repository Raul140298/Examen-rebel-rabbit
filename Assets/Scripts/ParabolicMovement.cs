using UnityEngine;

public class ParabolicMovement : MonoBehaviour
{
	[SerializeField] private float _velocity, _angle, _gravity;
	[SerializeField] private Vector3 initialPosition;
	private bool _startParabolicMovement;
	private float _angleRad;
	private Vector3 _gravityVector, _velocityVector;

	void Awake()
	{
		InitValues();
	}

	private void InitValues()
	{
		//Initialize Position, velocity vector and gravity vector
		_startParabolicMovement = false;
		this.transform.position = initialPosition;
		_angleRad = _angle * Mathf.PI / 180;

		_velocityVector = new Vector3(
			_velocity * Mathf.Cos(_angleRad),
			_velocity * Mathf.Sin(_angleRad),
			0f);

		_gravityVector = Vector3.down * _gravity;
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Return))
		{
			InitValues();
		}

		if (_startParabolicMovement == false)
		{
			if (Input.GetKeyDown(KeyCode.Space))
			{
				_startParabolicMovement = true;
			}
		}
		else
		{
			//Add both vectors
			//velocityVector acts as the vector distance traveled each frame
			//Calculate the distance traveled in each frame with the formula d = v * t
			_velocityVector += _gravityVector * Time.deltaTime;

			//Add the distance traveled to the position
			transform.position += _velocityVector * Time.deltaTime;
		}
	}
}

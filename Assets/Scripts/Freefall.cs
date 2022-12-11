using UnityEngine;

public class Freefall : MonoBehaviour
{
    [SerializeField] private float _gravityValue, _angle;
    [SerializeField] private Vector3 initialPosition;
    private bool _startFreefall;
    private Vector3 _distance, _gravityVector;
    private float _angleRad;

    void Awake()
    {
        InitValues();
	}

    private void InitValues()
    {
        //Initialize Position and gravity vector
		_startFreefall = false;
		this.transform.position = initialPosition;
		_distance = Vector3.zero;

		_angleRad = _angle * Mathf.PI / 180;

		_gravityVector = new Vector3(
			_gravityValue * Mathf.Cos(_angleRad),
			_gravityValue * Mathf.Sin(_angleRad),
			0f);
	}

    private void ManageMovement()
    {
		//Calculate the distance traveled in each frame with the formula d = v * t
		_distance += _gravityVector * Time.deltaTime;

		//Add the distance traveled to the position
		transform.position += _distance * Time.deltaTime;
	}

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return))
        {
			InitValues();
		}

        if(_startFreefall == false)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _startFreefall = true;
            }
        }
        else
        {
            ManageMovement();
		}
    }
}

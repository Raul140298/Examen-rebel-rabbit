using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TotalController2 : MonoBehaviour
{
	[Header("Player")]
	[SerializeField] private State _state;
	[SerializeField] private float _jumpForce, _rollDuration, _velocity;
	private Vector3 _velocityVector, _jumpVector;
	private Vector3 _normal;
	private bool _lookingRight;
	private Animator _animator;
	private SpriteRenderer _spriteRenderer;
	private CircleCollider2D _rollCollider;
	private BoxCollider2D _neutralCollider;

	[Header("Gravity")]
	[SerializeField] private GameObject _planet;
	[SerializeField] private float _gravityValue;
	private float _gravityAngle;
	private bool _onGround;
	private Vector3  _gravityVector;

	[Header("Gun")]
	[SerializeField] private float _bulletLifeTime;
	[SerializeField] private float _timeBetweenShot;
	[SerializeField] private float _bulletVelocity;
	[SerializeField] private float _gravity;
	[SerializeField] private GameObject _bullet, _gunR, _gunL;
	[SerializeField] private int _bulletCount;
	private List<GameObject> _bullets;

	//Input
	private float _move;
	private bool _jump, _roll, _shooting, _loading;
	

	void Awake()
	{
		_animator = this.GetComponent<Animator>();
		_spriteRenderer = this.GetComponent<SpriteRenderer>();
		_rollCollider = this.GetComponent<CircleCollider2D>();
		_neutralCollider = this.GetComponent<BoxCollider2D>();
		_state = State.STATE_IDLE;
		_neutralCollider.enabled = true;
		_rollCollider.enabled = false;
		_onGround = true;
		_shooting = false;
		_loading = false;
		_gunL.SetActive(false);
		_gunR.SetActive(false);
		_lookingRight = true;
		_gravityAngle = -90f;
		_jumpVector = Vector3.zero;

		//Initialize bullet variables
		_bulletCount = 0;
		_bullets = new List<GameObject>();

		//Add some bullets to satisfy the life time and shot between bullets
		//and round to the nearest integer
		int amountBullets = (int)Mathf.Ceil(_bulletLifeTime / _timeBetweenShot);

		for (int i = 0; i < amountBullets; i++)
		{
			GameObject bullet = Instantiate(_bullet);
			bullet.SetActive(false);
			_bullets.Add(bullet);
		}
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.gameObject.tag == "Ground")
		{
			_onGround = true;
			_jumpVector = Vector3.zero;
		}
	}

	private void OnCollisionExit2D(Collision2D collision)
	{
		if (collision.gameObject.tag == "Ground")
		{
			_onGround = false;
		}
	}

	private void manageMovement()
	{
		if (_move == 0)
		{
			_velocityVector = Vector3.zero;
		}
		if (_move > 0)
		{
			_lookingRight = true;
			_spriteRenderer.flipX = false;
			_velocityVector = Vector3.Cross(_normal, Vector3.forward) * _velocity;
		}
		if (_move < 0)
		{
			_lookingRight = false;
			_spriteRenderer.flipX = true;
			_velocityVector = Vector3.Cross(_normal, -1 * Vector3.forward) * _velocity;
		}

		if (_onGround == true && _jump)
		{
			_jumpVector += _normal * _jumpForce;
		}
	}

	private void manageGravity()
	{
		//Calculate the distance traveled in each frame with the formula d = v * t
		_gravityVector = ((Vector2)_planet.transform.position - (Vector2)transform.position).normalized * _gravityValue;
	}

	private void manageRotation()
	{
		//Get the angle between planet and player
		Vector3 dir = (Vector2)_planet.transform.position - (Vector2)transform.position;
		dir = _planet.transform.InverseTransformDirection(dir);
		_gravityAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

		this.transform.eulerAngles = new Vector3(
		this.transform.eulerAngles.x,
		this.transform.eulerAngles.y,
		_gravityAngle + 90f);
	}

	private void manageGun()
	{
		//Can't shot during roll
		if (_state != State.STATE_ROLL)
		{
			if (_shooting == false && Input.GetKeyDown(KeyCode.LeftShift))
			{
				if (_lookingRight == true)
				{
					_gunR.SetActive(true);
				}
				else
				{
					_gunL.SetActive(true);
				}
				_shooting = true;
				if (_loading == false) StartCoroutine(ManageShot());
			}
			if (_shooting == true)
			{
				if (_lookingRight == true)
				{
					_gunL.SetActive(false);
					_gunR.SetActive(true);
				}
				else
				{
					_gunR.SetActive(false);
					_gunL.SetActive(true);
				}
				if (Input.GetKeyUp(KeyCode.LeftShift))
				{
					_gunR.SetActive(false);
					_gunL.SetActive(false);
					_shooting = false;
				}
			}
		}
		else
		{
			_gunR.SetActive(false);
			_gunL.SetActive(false);
		}
	}

	IEnumerator PlayerRoll()
	{
		//Shrink his collider as his body
		_rollCollider.enabled = true;
		_neutralCollider.enabled = false;
		_animator.Play("Player_roll");

		//After the roll, have to check if the player is in the air and moving
		yield return new WaitForSeconds(_rollDuration);

		if (_onGround == false)
		{
			_state = State.STATE_JUMP;
		}
		else
		{
			if (_move != 0) _state = State.STATE_RUN;
			else _state = State.STATE_IDLE;
		}

		//Reset his collider	
		_neutralCollider.enabled = true;
		_rollCollider.enabled = false;
	}

	IEnumerator ManageShot()
	{
		if (_loading == false && _shooting == true)
		{
			if (_loading == false) StartCoroutine(Shot());
			_loading = true;
		}

		yield return new WaitForSeconds(_timeBetweenShot);
		_loading = false;
		if (_shooting == true) StartCoroutine(ManageShot());
	}

	IEnumerator Shot()
	{
		if (_bulletCount >= _bullets.Count) _bulletCount = 0;
		if (_bulletCount <= -1) _bulletCount = _bullets.Count - 1;

		Vector3 bulletDirection;
		if (_lookingRight == true) bulletDirection = Vector3.Cross(_normal, Vector3.forward) * _bulletVelocity;
		else bulletDirection = Vector3.Cross(_normal, -1 * Vector3.forward) * _bulletVelocity;

		//Get the bullet fired
		GameObject bulletAux = _bullets[_bulletCount];

		//Next bullet in the list
		_bulletCount++;

		if (_shooting == true && _state != State.STATE_ROLL)
		{
			Transform gunPosition = _lookingRight == true ? _gunR.transform.GetChild(0) : _gunL.transform.GetChild(0);

			//Active Bullet and set velocity
			bulletAux.gameObject.SetActive(true);
			bulletAux.GetComponent<BulletMovement>().InitValues(
				gunPosition,
				bulletDirection,
				_planet.transform,
				_gravityValue);

			//Disable Bullet after a while
			yield return new WaitForSeconds(_bulletLifeTime);
			bulletAux.gameObject.SetActive(false);
		}
		else
		{
			_bulletCount--;
		}

	}

	void Update()
	{
		//Capture input
		_move = Input.GetAxisRaw("Horizontal");
		_jump = Input.GetKeyDown(KeyCode.Space);
		_roll = Input.GetKeyDown(KeyCode.LeftControl);

		//Get the normal between planet and player
		_normal = ((Vector2)transform.position - (Vector2)_planet.transform.position).normalized;

		manageGun();
		manageRotation();
		manageGravity();
		manageMovement();

		//Add gravity
		if(_onGround == false) _jumpVector += _gravityVector * Time.deltaTime;

		//Add the distance traveled to the position
		transform.position += (_velocityVector + _jumpVector) * Time.deltaTime;


		switch (_state)
		{
			case State.STATE_IDLE:
				if (_roll == true)
				{
					_state = State.STATE_ROLL;
				}
				else if (_jump == true)
				{
					if (_onGround == true)
					{
						//Check if it is after a roll
						_onGround = false;
						_state = State.STATE_JUMP;
					}
				}
				else if (_move != 0)
				{
					_state = State.STATE_RUN;
				}
				_animator.Play("Player_idle");
				break;

			case State.STATE_JUMP:
				if (_onGround == true)
				{
					if (_move != 0) _state = State.STATE_RUN;
					else _state = State.STATE_IDLE;
				}
				else
				{
					if (_roll == true)
					{
						_state = State.STATE_ROLL;
					}
					else
					{
						_animator.Play("Player_jump");
					}
				}
				break;

			case State.STATE_ROLL:
				StartCoroutine(PlayerRoll());
				break;

			case State.STATE_RUN:
				if (_roll == true)
				{
					_state = State.STATE_ROLL;
				}
				else if (_jump == true)
				{
					if (_onGround == true)
					{
						//Check if it is after a roll
						_onGround = false;
						_state = State.STATE_JUMP;
					}
				}
				else if (_move == 0)
				{
					_state = State.STATE_IDLE;
				}
				_animator.Play("Player_run");
				break;

			default:
				break;
		}
	}
}
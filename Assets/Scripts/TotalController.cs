using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TotalController : MonoBehaviour
{
    private Rigidbody2D _rigidbody;
	private Animator _animator;
	private SpriteRenderer _spriteRenderer;
	private CircleCollider2D _rollCollider;
	private BoxCollider2D _neutralCollider;
	private float _move;
	private bool _jump, _roll, _shooting, _loading;
	private int _bulletCount;
	private List<GameObject> _bullets;
	[SerializeField] private float _bulletLifeTime;
	[SerializeField] private float _timeBetweenShot;
	[SerializeField] private float _bulletVelocity;
	[SerializeField] private GameObject _bullet, _gun, _planet;
	[SerializeField] private State _state;
	[SerializeField] private float _velocity, _jumpForce, _rollDuration;
	[SerializeField] private bool _onGround;

	void Awake()
    {
        _rigidbody = this.GetComponent<Rigidbody2D>();
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
		_gun.SetActive(false);

		//Initialize the variables
		_bulletCount = 0;
		_bullets = new List<GameObject>();

		//Add some bullets to satisfy the life time and shot between bullets
		//and round to the nearest integer
		int amountBullets = (int)Mathf.Ceil(_bulletLifeTime / _timeBetweenShot);

		for (int i = 0; i < amountBullets; i++)
		{
			GameObject bullet = Instantiate(_bullet);
			bullet.transform.parent = _planet.transform;
			bullet.SetActive(false);
			_bullets.Add(bullet);
		}
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if(collision.gameObject.tag == "Ground")
		{
			_onGround = true;
		}
	}

	private void OnCollisionExit2D(Collision2D collision)
	{
		if (collision.gameObject.tag == "Ground")
		{
			_onGround = false;
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

		if(_onGround == false)
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

	private void manageMovement()
	{
		if (_move > 0)
		{
			_spriteRenderer.flipX = false;
			_gun.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
			_gun.transform.position = new Vector3(0.318f, _gun.transform.position.y, _gun.transform.position.z);
		}
		if (_move < 0)
		{
			_spriteRenderer.flipX = true;
			_gun.transform.rotation = new Quaternion(0f, 180f, 0f, 0f);
			_gun.transform.position = new Vector3(-0.318f, _gun.transform.position.y, _gun.transform.position.z);
		}

		if (_onGround == true && _jump)
		{
			_rigidbody.AddForce(Vector2.up * _jumpForce);
		}
	}

	IEnumerator manageShot()
	{
		if (_loading == false && _shooting == true)
		{
			if (_loading == false) StartCoroutine(Shot());
			_loading = true;
		}

		yield return new WaitForSeconds(_timeBetweenShot);
		_loading = false;
		if (_shooting == true) StartCoroutine(manageShot());
	}

	IEnumerator Shot()
	{
		Vector3 bulletDirection = Vector3.zero;
		if (_spriteRenderer.flipX == true) bulletDirection = Vector3.left;
		if (_spriteRenderer.flipX == false) bulletDirection = Vector3.right;

		//Get the bullet fired
		GameObject bulletAux = _bullets[_bulletCount];

		//Next bullet in the list
		_bulletCount++;
		if (_bulletCount == _bullets.Count) _bulletCount = 0;

		//Set to Gun position
		bulletAux.transform.position = _gun.transform.position + bulletDirection * 0.28f; //Offset of the gun

		if (_shooting == true)
		{
			//Active Bullet and set velocity
			bulletAux.gameObject.SetActive(true);
			bulletAux.GetComponent<Rigidbody2D>().velocity = bulletDirection * _bulletVelocity;

			//Disable Bullet after a while
			yield return new WaitForSeconds(_bulletLifeTime);
			bulletAux.gameObject.SetActive(false);
		}
		else
		{
			_bulletCount--;
			if (_bulletCount == -1) _bulletCount = _bullets.Count;
		}
		
	}

	void Update()
    {
		//Capture input
		_move = Input.GetAxisRaw("Horizontal");
		_jump = Input.GetKeyDown(KeyCode.Space);
		_roll = Input.GetKeyDown(KeyCode.LeftControl);

		//Can't shot during roll
		if(_state != State.STATE_ROLL)
		{
			if (_shooting == false && Input.GetKeyDown(KeyCode.LeftShift))
			{
				_gun.SetActive(true);
				_shooting = true;
				if(_loading == false) StartCoroutine(manageShot());
			}
			if (_shooting == true && Input.GetKeyUp(KeyCode.LeftShift))
			{
				_gun.SetActive(false);
				_shooting = false;
			}
		}

		manageMovement();

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
				if(_onGround == true)
				{
					if (_move != 0) _state = State.STATE_RUN;
					else _state = State.STATE_IDLE;
				}
				else
				{
					if(_roll == true)
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
					if(_onGround == true)
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

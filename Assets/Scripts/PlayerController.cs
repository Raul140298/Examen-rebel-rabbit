using System.Collections;
using UnityEngine;

enum State
{
	//Four states for each animation
	STATE_IDLE,
	STATE_JUMP,
	STATE_ROLL,
	STATE_RUN
};

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D _rigidbody;
	private Animator _animator;
	private SpriteRenderer _spriteRenderer;
	private CircleCollider2D _rollCollider;
	private BoxCollider2D _neutralCollider;
	private float _move;
	private bool _jump, _roll;
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

	private void ManageMovement()
	{
		if (_move > 0)
		{
			_spriteRenderer.flipX = false;
		}
		if (_move < 0)
		{
			_spriteRenderer.flipX = true;
		}

		if (_onGround == true && _jump)
		{
			_rigidbody.AddForce(Vector2.up * _jumpForce);
		}

		//The roll shouldn't be able to propel it into the air
		_rigidbody.velocity = new Vector2(_move * (_velocity + (_state == State.STATE_ROLL && _onGround == true ? 2f : 0f)), _rigidbody.velocity.y);
	}

	void Update()
    {
		//Capture input
		_move = Input.GetAxisRaw("Horizontal");
		_jump = Input.GetKeyDown(KeyCode.Space);
		_roll = Input.GetKeyDown(KeyCode.LeftShift);

		ManageMovement();

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

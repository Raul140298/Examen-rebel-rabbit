using System.Collections;
using UnityEngine;

public class PlayerFSM : MonoBehaviour
{
	private PlayerState _state;
	private Rigidbody2D _rigidbody;
	private Animator _animator;
	private SpriteRenderer _spriteRenderer;
	[SerializeField] private float _velocity, _rollVelocity, _jumpForce, _rollDuration;
	[SerializeField] private bool _onGround;

	void Awake()
	{
		_rigidbody = this.GetComponent<Rigidbody2D>();
		_animator = this.GetComponent<Animator>();
		_spriteRenderer = this.GetComponent<SpriteRenderer>();
	}

	public void Start()
	{
		_state = new IdleState();
		_state.Enter(this);
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.gameObject.tag == "Ground")
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

	public virtual void HandleInput()
	{
		PlayerState state = _state.HandleInput(this);
		if(state != null)
		{
			//Can we delete a static variable?
			//Garbage collector will erase instances
			_state = state;
			_state.Enter(this);
		}
	}

	public void Update()
	{
		HandleInput();
		_state.Update(this);
	}

	public void ManageDirectionLooking()
	{
		if (Input.GetAxisRaw("Horizontal") > 0)
		{
			this._spriteRenderer.flipX = false;
		}

		if (Input.GetAxisRaw("Horizontal") < 0)
		{
			this._spriteRenderer.flipX = true;
		}
	}

	IEnumerator PlayerRoll()
	{
		//After the roll, have to check if the player is in the air and moving
		yield return new WaitForSeconds(this._rollDuration);

		if (_onGround == true)
		{
			if (Input.GetAxisRaw("Horizontal") != 0) this._state = new RunState();
			else this._state = new IdleState();
		}
		else
		{
			this._state = new JumpState();
		}
		_state.Enter(this);
	}

	public class PlayerState
	{
		//public static IdleState idle;
		//public static RunState run;
		//public static JumpState jump;
		//public static RollState roll;

		public virtual PlayerState HandleInput(PlayerFSM player)
		{
			return null;
		}

		public virtual void Update (PlayerFSM player){}

		public virtual void Enter(PlayerFSM player){}
	}

	public class IdleState : PlayerState
	{
		public override PlayerState HandleInput(PlayerFSM player)
		{
			if (Input.GetKeyDown(KeyCode.Space) && player._onGround == true)
			{
				return new JumpState();
			}
			else if (Input.GetKeyDown(KeyCode.LeftShift))
			{
				return new RollState();
			}
			else if (Input.GetAxisRaw("Horizontal") != 0)
			{
				return new RunState();
			}
			else
			{
				return null;
			}
		}

		public override void Update(PlayerFSM player)
		{
			player.ManageDirectionLooking();
			player._rigidbody.velocity = new Vector2(0f, player._rigidbody.velocity.y);
		}

		public override void Enter(PlayerFSM player)
		{
			player._animator.Play("Player_idle");
		}
	}

	public class RunState : PlayerState
	{
		public override PlayerState HandleInput(PlayerFSM player)
		{
			if (Input.GetKeyDown(KeyCode.Space) && player._onGround == true)
			{
				return new JumpState();
			}
			else if (Input.GetKeyDown(KeyCode.LeftShift))
			{
				return new RollState();
			}
			else if (Input.GetAxisRaw("Horizontal") == 0)
			{
				return new IdleState();
			}
			else
			{
				return null;
			}
		}

		public override void Update(PlayerFSM player)
		{
			float _move = Input.GetAxisRaw("Horizontal");

			player.ManageDirectionLooking();

			player._rigidbody.velocity = new Vector2(_move * player._velocity, player._rigidbody.velocity.y);
		}

		public override void Enter(PlayerFSM player)
		{
			player._animator.Play("Player_run");
		}
	}

	public class JumpState : PlayerState
	{
		public override PlayerState HandleInput(PlayerFSM player)
		{
			if (Input.GetKeyDown(KeyCode.LeftShift))
			{
				return new RollState();
			}
			else
			{
				return null;
			}
		}

		public override void Update(PlayerFSM player)
		{
			float _move = Input.GetAxisRaw("Horizontal");

			player.ManageDirectionLooking();

			player._rigidbody.velocity = new Vector2(_move * player._velocity, player._rigidbody.velocity.y);

			if (player._onGround == true && Input.GetKey(KeyCode.Space) == false)
			{
				if (_move != 0)
				{
					player._state = new RunState();
				}
				else
				{
					player._state = new IdleState();
				}
				player._state.Enter(player);
			}
		}

		public override void Enter(PlayerFSM player)
		{
			player._animator.Play("Player_jump");
			if(player._onGround == true) player._rigidbody.AddForce(Vector2.up * player._jumpForce);
		}
	}

	public class RollState : PlayerState
	{
		public override PlayerState HandleInput(PlayerFSM player)
		{
			if (Input.GetKeyDown(KeyCode.Space) && player._onGround == true)
			{
				player._rigidbody.AddForce(Vector2.up * player._jumpForce);

			}
			return null;
		}

		public override void Update(PlayerFSM player)
		{

		}

		public override void Enter(PlayerFSM player)
		{
			player._animator.Play("Player_roll");
			player._rigidbody.velocity = new Vector2((player._spriteRenderer.flipX ? -1 : 1) * player._rollVelocity, player._rigidbody.velocity.y);
			player.StartCoroutine(player.PlayerRoll());
		}
	}
}

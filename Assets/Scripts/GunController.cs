using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class GunController : MonoBehaviour
{
	private int _bulletCount;
	private List<GameObject> _bullets;
	[SerializeField] private float _bulletLifeTime;
	[SerializeField] private float _timeBetweenShot;
	[SerializeField] private float _bulletVelocity;
	[SerializeField] private GameObject _bullet;

	private void Awake()
	{
		//Initialize the variables
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

	void Start()
    {
		//Have to check that the number of bullets satisfies the life time and shot between bullets
		Assert.IsTrue(_bullets.Count >= _bulletLifeTime / _timeBetweenShot, "Pooling must have more bullets");

		StartCoroutine(LoadBullets());
    }

	IEnumerator LoadBullets()
	{
		yield return new WaitForSeconds(_timeBetweenShot);
		StartCoroutine(Shot());
		StartCoroutine(LoadBullets());
	}

	IEnumerator Shot()
	{
		if (_bulletCount == _bullets.Count) _bulletCount = 0;

		//Get the bullet fired
		GameObject bulletAux = _bullets[_bulletCount];

		//Next bullet in the list
		_bulletCount++;
		
		//Set to Gun position
		bulletAux.transform.position = this.transform.position + Vector3.right; //Offset of the gun

		//Active Bullet and set velocity
		bulletAux.gameObject.SetActive(true);
		bulletAux.GetComponent<Rigidbody2D>().velocity = Vector2.right * _bulletVelocity;

		//Disable Bullet after a while
		yield return new WaitForSeconds(_bulletLifeTime);
		bulletAux.gameObject.SetActive(false);
	}
}

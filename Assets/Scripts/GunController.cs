using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class GunController : MonoBehaviour
{
	private List<GameObject> _bullets;
	[SerializeField] private float _bulletLifeTime;
	[SerializeField] private float _timeBetweenShot;
	[SerializeField] private float _bulletVelocity;
	[SerializeField] private GameObject _bullet;

	private void Awake()
	{
		//Initialize the variables
		_bullets = new List<GameObject>();
	}

	void Start()
    {
		//Add some bullets to satisfy the life time and shot between bullets
		//and round to the nearest integer
		int amountBullets = (int)Mathf.Ceil(_bulletLifeTime / _timeBetweenShot);
		AddBulletsToPool(amountBullets);

		//Have to check that the number of bullets satisfies the life time and shot between bullets
		Assert.IsTrue(_bullets.Count >= _bulletLifeTime / _timeBetweenShot, "Pooling must have more bullets");

		StartCoroutine(LoadBullets());
    }

	private void AddBulletsToPool(int amountBullets)
	{
		for (int i = 0; i < amountBullets; i++)
		{
			GameObject bullet = Instantiate(_bullet);
			bullet.SetActive(false);
			_bullets.Add(bullet);
		}
	}

	private GameObject RequestBullet()
	{
		int i;

		for (i = 0; i < _bullets.Count; i++)
		{
			if(!_bullets[i].activeSelf)
			{
				break;
			}
		}

		//If the initial pool has been exceeded then
		if (i == _bullets.Count)
		{
			//Have to add as many bullets as the pool needs
			int amountBullets = (int)Mathf.Ceil(_bulletLifeTime / _timeBetweenShot) - _bullets.Count;
			AddBulletsToPool(amountBullets);
		}

		_bullets[i].SetActive(true);
		return _bullets[i];
	}

	IEnumerator LoadBullets()
	{
		yield return new WaitForSeconds(_timeBetweenShot);
		StartCoroutine(Shot());
		StartCoroutine(LoadBullets());
	}

	IEnumerator Shot()
	{
		//Get the bullet fired
		GameObject bulletAux = RequestBullet();
		
		//Set to Gun position
		bulletAux.transform.position = this.transform.position + Vector3.right; //Offset of the gun

		//Active Bullet and set velocity
		bulletAux.GetComponent<Rigidbody2D>().velocity = Vector2.right * _bulletVelocity;

		//Disable Bullet after a while
		yield return new WaitForSeconds(_bulletLifeTime);
		bulletAux.gameObject.SetActive(false);
	}
}

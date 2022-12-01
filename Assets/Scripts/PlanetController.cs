using UnityEngine;

public class PlanetController : MonoBehaviour
{
	[SerializeField] private float speedRot;

	void Update()
	{
		//Simulates the movement of the character around the planet
		transform.Rotate(0.0f, 0.0f, Input.GetAxisRaw("Horizontal") * speedRot);
	}
}
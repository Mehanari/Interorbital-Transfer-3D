using System.Collections.Generic;
using UnityEngine;

namespace BoatAndRafts.Src
{
	public class Game : MonoBehaviour
	{
		[SerializeField] private List<IntRouteMovement2D> rafts = new();
		[SerializeField] private IntMovement boat;

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Space))
			{
				MoveRafts();
			}

			if (Input.GetKeyDown(KeyCode.A))
			{
				boat.Move(Direction2D.Left);
				MoveRafts();
			}
			else if(Input.GetKeyDown(KeyCode.D))
			{
				boat.Move(Direction2D.Right);
				MoveRafts();
			}
			else if(Input.GetKeyDown(KeyCode.W))
			{
				boat.Move(Direction2D.Up);
				MoveRafts();
			}
			else if (Input.GetKeyDown(KeyCode.S))
			{
				boat.Move(Direction2D.Down);
				MoveRafts();
			}
		}

		private void MoveRafts()
		{
			for (int i = 0; i < rafts.Count; i++)
			{
				rafts[i].Move();
			}
		}
	}
}
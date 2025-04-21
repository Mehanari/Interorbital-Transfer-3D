using System;

namespace Src.ControlGeneration
{
	/// <summary>
	/// The purpose of this class is to select a pool of specimen from a given specimen array.
	/// The selection process is called tournament selection and it consists of two steps:
	/// 1) Randomly (with uniform distribution) select k specimen from the array.
	/// 2) From those k specimen select the one with the highest fit value.
	/// Those two steps can be repeated as many times, as you need to form a result pool.
	/// Specimen in the result pool may repeat and the result pool can be bigger than the given array.
	/// </summary>
	public class TournamentSelector
	{
		private readonly int _tournamentSize;

		public TournamentSelector(int tournamentSize)
		{
			_tournamentSize = tournamentSize;
		}

		public Specimen[] SelectPool(Specimen[] input, int poolSize)
		{
			var pool = new Specimen[poolSize];
			for (int i = 0; i < poolSize; i++)
			{
				pool[i] = Select(input);
			}

			return pool;
		}

		private Specimen Select(Specimen[] input)
		{
			var rnd = new Random();
			var tournament = new Specimen[_tournamentSize];
			for (int i = 0; i < _tournamentSize; i++)
			{
				tournament[i] = input[rnd.Next(0, _tournamentSize)];
			}
			Array.Sort(tournament, new SpecimenComparer());
			return tournament[0];
		}
	}
}
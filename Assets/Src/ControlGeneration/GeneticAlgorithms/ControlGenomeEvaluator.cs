using Src.GeneticAlgorithms;

namespace Src.ControlGeneration.GeneticAlgorithms
{
	public class ControlGenomeEvaluator : IGenomeEvaluator
	{
		private ControlPrecisionEvaluator _precisionEvaluator;
		private int _polynomialDegree;

		public ControlGenomeEvaluator(ControlPrecisionEvaluator precisionEvaluator, int polynomialDegree)
		{
			_precisionEvaluator = precisionEvaluator;
			_polynomialDegree = polynomialDegree;
		}

		public double Evaluate(double[] genome)
		{
			return _precisionEvaluator.EvaluateControl(GenomeConverter.FromGenome(genome, _polynomialDegree));
		}
	}
}
using MehaMath.Math.Components;

namespace Src.NeuralNetworkExperiments
{
	public class Neuron
	{
		private Vector _weights;

		public Neuron(Vector initialWeights)
		{
			_weights = initialWeights;
		}

		public double Activation(Vector input)
		{
			var sum = input.WeightedSum(_weights);
			return sum;
		}
	}
}
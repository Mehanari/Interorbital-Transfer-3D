using System;
using System.Collections.Generic;

namespace MehaMath.Math.Components
{
    /// <summary>
    /// A vector of functions that take a Vector of numbers as an input and produce a double as an output.
    /// </summary>
    public class FuncVector
    {
        private readonly List<Func<Vector, double>> _functions = new();

        public int Count => _functions.Count;

        public FuncVector(params Func<Vector, double>[] funcs)
        {
            _functions = new List<Func<Vector, double>>(funcs);
        }

        public Func<Vector, double> this[int i]
        {
            get => _functions[i];
            set => _functions[i] = value;
        }

        public Vector Calculate(Vector input)
        {
            var output = new Vector(_functions.Count);
            for (int i = 0; i < _functions.Count; i++)
            {
                output[i] = _functions[i](input);
            }

            return output;
        }
    }
}
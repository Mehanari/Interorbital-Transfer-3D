using UnityEngine;

namespace MehaMath.VisualisationTools.Plotting
{
    public class MeshPlotParameters : PlotParameters
    {
        public MeshFilter MeshFilter { get; set; }
        /// <summary>
        /// Samples count on x axis
        /// </summary>
        public int XDots { get; set; }
        /// <summary>
        /// Samples count on y axis
        /// </summary>
        public int YDots { get; set; }
        /// <summary>
        /// Function values
        /// </summary>
        public float[,] Z { get; set; }
        
        public override void Destroy()
        {
            Object.Destroy(MeshFilter.gameObject);
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace MehaMath.VisualisationTools.Plotting
{
    public class DotPlotParameters : PlotParameters
    {
        public List<GameObject> Dots { get; set; }
        
        public override void Destroy()
        {
            foreach (var dot in Dots)
            {
                Object.Destroy(dot);
            }
            Dots.Clear();
        }
    }
}
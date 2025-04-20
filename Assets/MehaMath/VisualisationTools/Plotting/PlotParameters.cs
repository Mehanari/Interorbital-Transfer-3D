namespace MehaMath.VisualisationTools.Plotting
{
    public abstract class PlotParameters
    {
        public string Name { get; set; }
        public abstract void Destroy();
    }
}
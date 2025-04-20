using System.IO;
using Newtonsoft.Json;

namespace MehaMath.Math.OptimalControlProblems
{
    /// <summary>
    /// Class for saving control to a JSON text file, as well for reading control from a JSON file.
    /// </summary>
    public class ControlJsonIO
    {
        public void Save(Control control, string filePath)
        {
            var jsonString = control.ToJson();
            File.WriteAllText(filePath, jsonString);
        }

        public LerpControl ReadDiscrete(string filePath)
        {
            var json = File.ReadAllText(filePath);
            var controlData = JsonConvert.DeserializeObject<DiscreteControlData>(json);
            var control = new LerpControl(controlData.Time, controlData.ControlSamples);
            return control;
        }
    }
}
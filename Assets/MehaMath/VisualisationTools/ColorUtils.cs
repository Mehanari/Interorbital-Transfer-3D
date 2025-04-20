using UnityEngine;

namespace MehaMath.VisualisationTools
{
    public static class ColorUtils
    {
        public static Color HeatToColor(float heat, float min, float max)
        {
            var t = Mathf.InverseLerp(min, max, heat);
            return Color.HSVToRGB(0.66f * (1 - t), 1, 1);            
        }
    }
}
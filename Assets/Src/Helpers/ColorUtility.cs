using UnityEngine;

namespace Src.Helpers
{
	public static class ColorUtility
	{
		public static Color GetRandomBrightColor()
		{
			// Use HSV to ensure bright colors (high value, high saturation)
			float hue = Random.value; // Random hue (0 to 1)
			float saturation = Random.Range(0.7f, 1f); // High saturation for vibrancy
			float value = Random.Range(0.7f, 1f); // High value for brightness
        
			// Convert HSV to RGB
			return Color.HSVToRGB(hue, saturation, value);
		}
	}
}
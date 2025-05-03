using UnityEditor;
using UnityEngine;

namespace RaftAndWhales.Editor
{
	[CustomEditor(typeof(MonoBehaviour), editorForChildClasses: true)]
	public class MonoBehaviourTooltipDrawer : UnityEditor.Editor
	{
		string tooltip;

		private void OnEnable()
		{
			var attributes = target.GetType().GetCustomAttributes(inherit: false);
			foreach(var attr in attributes)
			{
				if(attr is ClassTooltipAttribute tooltip)
				{
					this.tooltip = tooltip.description;
				}
			}
		}
		public override void OnInspectorGUI()
		{
			var tooltipStyle = new GUIStyle(EditorStyles.label);
			tooltipStyle.wordWrap = true;
			tooltipStyle.richText = true;
			tooltipStyle.padding = new RectOffset(5, 5, 5, 5);
			EditorGUILayout.LabelField(tooltip, tooltipStyle);
			base.OnInspectorGUI();
		}
	}
}
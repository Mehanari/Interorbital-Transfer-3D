using UnityEngine;

namespace MehaMath.VisualisationTools
{
    public class Arrow : MonoBehaviour
    {
        [Header("Parameters")]
        [SerializeField] private float headLength;
        [SerializeField] private float length;
        [SerializeField] private Color color;
        [SerializeField] private float angleRad;
        [Header("Components")]
        [SerializeField] private GameObject pivot;
        [SerializeField] private GameObject line;
        [SerializeField] private GameObject headPivot;
        [SerializeField] private Transform headTip;
        [SerializeField] private SpriteRenderer lineRenderer;
        [SerializeField] private SpriteRenderer headRenderer;   
        
        public void SetAngleRadians(float angle)
        {
            SetAngleDegrees(angle * Mathf.Rad2Deg);
        }
        
        public void SetAngleDegrees(float angle)
        {
            pivot.transform.localRotation = Quaternion.Euler(0, 0, 90 + angle);
        }
        
        public void SetLength(float length)
        {
            length -= headLength;
            var lineScale = line.transform.localScale;
            lineScale.y = length;
            line.transform.localScale = lineScale;
            var linePosition = line.transform.localPosition;
            linePosition.y = -length / 2;
            line.transform.localPosition = linePosition;
            var headPosition = headPivot.transform.localPosition;
            headPosition.y = -length;
            headPivot.transform.localPosition = headPosition;
        }
        
        public void SetColor(Color color)
        {
            lineRenderer.color = color;
            headRenderer.color = color;
        }

        public void AttachToTip(GameObject attachment)
        {
            attachment.transform.position = headTip.position;
            attachment.transform.parent = headTip;
        }

        private void OnValidate()
        {
            if (length < 0)
            {
                length = 0;
            }
            SetLength(length);
            SetAngleRadians(angleRad);
            SetColor(color);
        }
    }
}

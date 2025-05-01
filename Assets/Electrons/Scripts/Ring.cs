using UnityEngine;

namespace Electrons.Scripts
{
    public class Ring : MonoBehaviour
    {
        [SerializeField] private SpriteMask mask;
        [SerializeField] private SpriteRenderer circle;

        public void SetColor(Color color)
        {
            circle.color = color;
        }
        
        public void SetOuterRadius(float radius)
        {
            circle.transform.localScale = new Vector3(radius, radius, radius);
        }

        public void SetInnerRadius(float radius)
        {
            mask.transform.localScale = new Vector3(radius, radius, radius);
        }
    }
}

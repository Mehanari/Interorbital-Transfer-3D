using System.Collections.Generic;
using BoatAndRafts.Src;
using UnityEngine;

namespace CircleTravelling.Src
{
    /// <summary>
    /// I need this class to ensure that all other components can receive a collection of travellers with the same order.
    /// The order is important, because this is what I'm trying to optimize here.
    /// </summary>
    [ClassTooltip("Collects Traveller components from children that are active.")]
    public class TravellersCollection : MonoBehaviour
    {
        public IReadOnlyCollection<Traveller> Travellers => GetTravellers();

        private List<Traveller> GetTravellers()
        {
            var childrenCount = transform.childCount;
            var travellers = new List<Traveller>();
            for (int i = 0; i < childrenCount; i++)
            {
                var child = transform.GetChild(i);
                if (child.gameObject.activeSelf && child.TryGetComponent<Traveller>(out var traveller))
                {
                    travellers.Add(traveller);
                }
            }

            return travellers;
        }
    }
}

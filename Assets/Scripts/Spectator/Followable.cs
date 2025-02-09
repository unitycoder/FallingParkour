using System;
using UnityEngine;

namespace PropHunt.Spectator
{
    /// <summary>
    /// An object that can be followed by a spectator player
    /// </summary>
    public class Followable : MonoBehaviour, IComparable<Followable>
    {
        /// <summary>
        /// Start each followable object with a random GUID id
        /// </summary>
        private Guid id = Guid.NewGuid();

        /// <summary>
        /// Get identifier for this followable object
        /// </summary>
        public Guid Id => id;

        /// <summary>
        /// GameObject to ignore collider of when following this object
        /// </summary>
        public GameObject ignoreCollider;

        public int CompareTo(Followable other)
        {
            return id.CompareTo(other.id);
        }
    }
}

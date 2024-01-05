using UnityEngine;

namespace TheKiwiCoder
{
    /// <summary>
    /// Scriptable object for holding a global blackboard for behaviour trees to shared and use.
    /// This should contain common fields and values that behavior trees sharing this can read.
    /// </summary>
    [CreateAssetMenu()]
    public class BlackboardSO : ScriptableObject
    {
        [SerializeField] private Blackboard blackboard;

        public Blackboard Blackboard => blackboard;
    }
}

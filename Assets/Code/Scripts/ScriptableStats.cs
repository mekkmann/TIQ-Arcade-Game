using UnityEngine;

[CreateAssetMenu(fileName = "ScriptableStats", menuName = "Scriptable Objects/ScriptableStats")]
public class ScriptableStats : ScriptableObject
{
    [Tooltip("Capacity to gain fall speed (gravity)")]
    public float FallAcceleration = 100f;
}

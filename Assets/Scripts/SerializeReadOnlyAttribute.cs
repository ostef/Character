using UnityEngine;

/// <summary>
/// Shows a serialized field in the editor as disabled
/// Does not serialize a field, so use SerializeField for private fields
/// </summary>
public class SerializeReadOnlyAttribute : PropertyAttribute {}

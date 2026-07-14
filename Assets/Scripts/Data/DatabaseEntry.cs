using UnityEngine;

public class DatabaseEntry : ScriptableObject, IDatabaseEntry
{
    [SerializeField] private string id;
    public string Id => id;

#if UNITY_EDITOR

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(id))
        {
            id = System.Guid.NewGuid().ToString();
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }

#endif
}

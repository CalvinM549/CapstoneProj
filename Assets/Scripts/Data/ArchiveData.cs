using UnityEngine;

[CreateAssetMenu(fileName = "ArchiveEntry", menuName = "Archives/NewArchiveEntry", order = 1)]
public class ArchiveData : DatabaseEntry
{
    public string Title;
    [TextArea] public string Text;
}

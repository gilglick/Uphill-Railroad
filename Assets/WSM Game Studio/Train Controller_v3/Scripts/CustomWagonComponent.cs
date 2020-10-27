using UnityEngine;

[System.Serializable]
public struct CustomWagonComponent
{
    public GameObject prefab;
    public string customName;
    public SpawnPosition[] positions;
}
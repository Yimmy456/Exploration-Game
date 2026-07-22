using System;
using UnityEngine;

[Serializable]
public class Relic
{
    public string RelicID;
    public string RelicName;
    public string RelicDescription;
    public bool IsCollected;
    public string RelicThumbnailAddress;
    public Vector3 Position;
}

[Serializable]
public class RelicArray
{
    public Relic[] data;
}
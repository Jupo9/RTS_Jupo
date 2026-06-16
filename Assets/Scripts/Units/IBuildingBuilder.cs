using UnityEngine;

public interface IBuildingBuilder
{
    public GameObject Build(BuildingSO building, Vector3 targetLocation);
}

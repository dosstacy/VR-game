using UnityEngine;

public enum CableId
{
    None,
    CableA,
    CableB,
    CableC,
    CableD,
    CableE
}

public class Cable : MonoBehaviour
{
    public CableId id = CableId.None;
}

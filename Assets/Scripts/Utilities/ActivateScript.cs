using UnityEngine;

public class ActivateScript : MonoBehaviour
{
    [SerializeField] private MonoBehaviour scriptToActivate;

    private void Start()
    {
        if (scriptToActivate != null)
        {
            scriptToActivate.enabled = true;
        }
    }
}

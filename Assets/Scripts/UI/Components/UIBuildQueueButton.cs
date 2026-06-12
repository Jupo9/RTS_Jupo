using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

class UIBuildQueueButton : MonoBehaviour, IUIElement<AbstractUnitSO, UnityAction>
{
    [SerializeField] private Image icon;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        Disable();
    }

    public void EnableFor(AbstractUnitSO item, UnityAction callback)
    {
        button.onClick.RemoveAllListeners();
        button.interactable = true;
        button.onClick.AddListener(callback);
        icon.gameObject.SetActive(true);
        icon.sprite = item.Icon;

    }

    public void Disable()
    {
        button.interactable = false;
        button.onClick.RemoveAllListeners();
        icon.gameObject.SetActive(false);
    }
}

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIActionButton : MonoBehaviour, IUIElement<BaseCommand, UnityAction>
{
    [SerializeField] private Image icon;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        Disable();
    }

    public void EnableFor(BaseCommand action, UnityAction onClick)
    {
        button.onClick.RemoveAllListeners();
        SetIcon(action.Icon);
        button.interactable = !action.IsLocked(new CommandContext(null, new RaycastHit(), 0));
        button.onClick.AddListener(onClick);
    }

    public void Disable()
    {
        SetIcon(null);
        button.interactable = false;
        button.onClick.RemoveAllListeners();
    }

    private void SetIcon(Sprite icon)
    {
        if (icon == null)
        {
            this.icon.enabled = false;
        }
        else
        {
            this.icon.sprite = icon;
            this.icon.enabled = true;
        }
    }
}

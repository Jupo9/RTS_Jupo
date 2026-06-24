using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIActionButton : MonoBehaviour, IUIElement<BaseCommand, UnityAction>, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image icon;
    [SerializeField] private Tooltip tooltip;

    private bool isActive;
    private RectTransform rectTransform;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        rectTransform = GetComponent<RectTransform>();
        Disable();
    }

    public void EnableFor(BaseCommand command, UnityAction onClick)
    {
        button.onClick.RemoveAllListeners();
        SetIcon(command.Icon);
        button.interactable = !command.IsLocked(new CommandContext(null, new RaycastHit(), 0));
        button.onClick.AddListener(onClick);
        isActive = true;

        if (tooltip != null)
        {
            tooltip.SetText(GetTooltipText(command));
        }
    }

    public void Disable()
    {
        SetIcon(null);
        button.interactable = false;
        button.onClick.RemoveAllListeners();
        isActive = false;

        if (tooltip != null)
        {
            tooltip.Hide();
        }
        CancelInvoke();
    }

    public void OnPointerEnter(PointerEventData _)
    {
        if (isActive)
        {
            Invoke(nameof(ShowTooltip), 0.5f);
        }
    }

    public void OnPointerExit(PointerEventData _)
    {
        if (tooltip != null)
        {
            tooltip.Hide();
        }
        CancelInvoke();
    }

    private void ShowTooltip()
    {
        if (tooltip != null)
        {
            tooltip.Show();
            tooltip.RectTransform.position = new Vector2(
                rectTransform.position.x + rectTransform.rect.width / 2f,
                rectTransform.position.y + rectTransform.rect.height / 2f
                );
        }
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

    private string GetTooltipText(BaseCommand command)
    {
        string tooltipText = command.name + "\n";

        SupplyCostSO supplyCost = null;

        if (command is BuildUnitCommand unitCommand)
        {
            supplyCost = unitCommand.Unit.Cost;
        }
        else if (command is BuildBuildingCommand buildingCommand)
        {
            supplyCost = buildingCommand.Building.Cost;
        }

        if (supplyCost != null)
        {
            if (supplyCost.Minerals > 0)
            {
                tooltipText += $"{supplyCost.Minerals} Minerals.";
            }
            if (supplyCost.Gas > 0)
            {
                tooltipText += $"{supplyCost.Gas} Gas.";
            }
        }

        return tooltipText;
    }
}

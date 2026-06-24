using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class ActionsUI : MonoBehaviour, IUIElement<HashSet<AbstractCommandable>>
{
    [SerializeField] private UIActionButton[] actionButtons;
    private HashSet<AbstractCommandable> selectedUnits = new(12);

    public void EnableFor(HashSet<AbstractCommandable> selectedUnits)
    {
        RefreshButtons(selectedUnits);
    }

    public void Disable()
    {
        foreach (UIActionButton button in actionButtons)
        {
            button.Disable();
        }
    }

    private void RefreshButtons(HashSet<AbstractCommandable> selectedUnits)
    {
        HashSet<BaseCommand> availableCommands = new(9);

        foreach (AbstractCommandable commandable in selectedUnits)
        {
            if (commandable.AvailableCommands != null)
            {
                availableCommands.AddRange(commandable.AvailableCommands);
            }
        }

        for (int i = 0; i < actionButtons.Length; i++)
        {
            BaseCommand actionForSlot = availableCommands.Where(action => action.Slot == i).FirstOrDefault();

            if (actionForSlot != null)
            {
                actionButtons[i].EnableFor(actionForSlot, HandleClick(actionForSlot));
            }
            else
            {
                actionButtons[i].Disable();
            }
        }
    }

    private UnityAction HandleClick(BaseCommand action)
    {
        return () => Bus<CommandSelectedEvent>.Raise(new CommandSelectedEvent(action));
    }
}

using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/GatherSuppliesEventChannel")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "GatherSuppliesEventChannel", message: "[Self] gathers [Amount] [Supplies]", category: "Events", id: "dd5e42d7ab9bd18403bf86b9ed090d41")]
public sealed partial class GatherSuppliesEventChannel : EventChannel<GameObject, int, SupplySO> { }


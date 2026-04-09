using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseBuilding : AbstractCommandable
{
    private Queue<UnitSO> buildingQueue = new (MAX_QUEUE_SIZE);
    private const int MAX_QUEUE_SIZE = 5;

    public void BuildUnit(UnitSO unit)
    {
        if (buildingQueue.Count == MAX_QUEUE_SIZE)
        {
            Debug.LogError("Building queue is full!");
            return;
        }

        buildingQueue.Enqueue(unit);

        if (buildingQueue.Count == 1)
        { 
            StartCoroutine(DoBuildUnits());
        }
    }

    private IEnumerator DoBuildUnits()
    {
        while (buildingQueue.Count > 0)
        {
            UnitSO unit = buildingQueue.Peek();

            Debug.Log("Start Building");
            yield return new WaitForSeconds(unit.BuildTime);

            Instantiate(unit.Prefab, transform.position, Quaternion.identity);
            buildingQueue.Dequeue();
        }

    }
}

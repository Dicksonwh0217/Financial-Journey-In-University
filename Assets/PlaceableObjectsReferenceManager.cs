using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceableObjectsReferenceManager : MonoBehaviour
{
    public PlaceableObjectsManager placeableObjectsManager;

    public void Place(Item item, Vector3Int pos)
    {
        if (placeableObjectsManager == null)
        {
            Debug.LogWarning("No placeableObjectsManager reference detected");
            return;
        }
        placeableObjectsManager.Place(item, pos);
    }

    internal void PickUp(Vector3Int gridPosition)
    {

        if (placeableObjectsManager == null)
        {
            Debug.LogWarning("No placeableObjectsManager reference detected");
            return;
        }

        placeableObjectsManager.PickUp(gridPosition);
    }

    public bool Check(Vector3Int pos)
    {
        if (placeableObjectsManager == null)
        {
            Debug.LogWarning("No placeableObjectsManager reference detected");
            return false;
        }

        return placeableObjectsManager.Check(pos);
    }
}

using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerPanelSpace : NetworkBehaviour
{
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        // get the parent
        // var parentTransform = transform.parent;
        SetSiblingIndex_ServerRpc(0);
        
    }
    
    [ClientRpc]
    private void SetSiblingIndex_ClientRpc(int index)
    {
        transform.SetSiblingIndex(index);
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void SetSiblingIndex_ServerRpc(int index)
    {
        transform.SetSiblingIndex(index);
    }

    public override void OnNetworkObjectParentChanged(NetworkObject parentNetworkObject)
    {
        // NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(parentNetworkObject.NetworkObjectId, out var objToPickup);
        // if (objToPickup == null || objToPickup.transform.parent != null) return; // object already picked up, server authority says no
        //
        //
        // // change the position of the panel to be relative to the parent
        // var parentTransform = objToPickup.transform;
        // // get the child of the parent that is the panel
        // transform.SetSiblingIndex(0);
        //
        // Debug.LogError("Setting sibling index to 0");
    }
}

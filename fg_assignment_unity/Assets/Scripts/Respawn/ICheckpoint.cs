using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public interface ICheckpoint {
    public Vector3 SpawnWorldPosition { get; } 
}

public abstract class Checkpoint : MonoBehaviour, ICheckpoint {    
    [SerializeField] protected Vector3 spawnLocalPosition;
    public static Vector3 CurrentSpawnWorldPosition;

    public Vector3 SpawnWorldPosition {
        get { return transform.TransformPoint(spawnLocalPosition); }
    }

    public void SetCurrentSpawn() {
        CurrentSpawnWorldPosition = transform.TransformPoint(spawnLocalPosition);
    }

    public static void Respawn(Transform body) {
        body.position = CurrentSpawnWorldPosition;        
    }

    #if UNITY_EDITOR

    private void OnDrawGizmos() {
        Gizmos.color = (CurrentSpawnWorldPosition == SpawnWorldPosition) ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.TransformPoint(spawnLocalPosition), 0.5f);        
    }
    
    #endif    
}

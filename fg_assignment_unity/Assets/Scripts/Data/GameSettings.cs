using Lander;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Settings/Game")]
public class GameSettings : ScriptableObject
{
    [Header("Main")]
    public CameraController CameraPrefab;
    public Player PlayerPrefab;
    public InputController InputPrefab;
    
    [Header("UI")]
    public PauseUI PauseUIPrefab;
    
    [Header("Level")]
    public LevelData[] LevelDataPrefab;        
}

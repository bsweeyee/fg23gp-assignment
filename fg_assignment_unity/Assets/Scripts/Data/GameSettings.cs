using Lander;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Settings/Game")]
public class GameSettings : ScriptableObject {
    [Header("Main")]
    public Game GamePrefab;
    public CameraController CameraPrefab;
    public Player PlayerPrefab;
    public InputController InputPrefab;
    public LevelController LevelPrefab;

    [Header("UI")]
    public PauseUI PauseUIPrefab;
    public LevelCompleteUI LevelCompleteUIPrefab;

    [Header("Level")]
    public float LevelWidth;
    public float LevelLength;
    public LevelData[] LevelData;

    [Header("Physics layers")]
    public LayerMask PlayerLayer;
    public LayerMask ObstacleLayer;
    public LayerMask TriggerLayer;
}

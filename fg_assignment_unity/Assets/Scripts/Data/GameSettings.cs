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

    [FormerlySerializedAs("LevelDataPrefab")] [Header("Level")]
    public LevelData[] LevelData;
}

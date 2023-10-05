using Lander;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Init : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod]
    public static void InitGame() {
        var gameSettings = Resources.Load<GameSettings>("GameSettings");
        var gamePrefab = gameSettings.GamePrefab;
        var game = Instantiate(gamePrefab);
        if(SceneManager.GetActiveScene().name == "Entrypoint") {
            // we generate all the necessary prefabs only in init scene
            var camera = Instantiate(gameSettings.CameraPrefab);
            var player = Instantiate(gameSettings.PlayerPrefab);
            var input = Instantiate(gameSettings.InputPrefab);
            var level = Instantiate(gameSettings.LevelPrefab);

            var pauseUI = Instantiate(gameSettings.PauseUIPrefab);
        }

        game.Initialize(gameSettings);
        game.CurrentState = Game.PLAY_STATE;
    }
}

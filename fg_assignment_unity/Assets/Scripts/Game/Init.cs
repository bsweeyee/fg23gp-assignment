using Lander;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Init : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod]
    public static void InitGame() {
        var gamePrefab = Resources.Load<Game>("Game");
        var game = Instantiate(gamePrefab);
        game.Initialize();
        game.CurrentState = Game.PLAY_STATE;
    }
}

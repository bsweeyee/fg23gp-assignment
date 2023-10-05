using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Runtime.ExceptionServices;
using Lander;
using System.Linq;

namespace Lander {
    #if UNITY_EDITOR
    [CustomEditor(typeof(PlatformGenerator))]
    public class PlatformGeneratorEditor : Editor
    {
        protected virtual void OnSceneGUI() {
            PlatformGenerator generator = (PlatformGenerator)target;

            EditorGUI.BeginChangeCheck();
            if (generator.TileMap == null) return;
            if (generator.OccupiedTilePositions == null) return;

            // var blocks = generator.GetWorldSpaceBlockPositions(generator.TileMap, generator.OccupiedTilePositions, generator.AxisDirection);
            var sp = generator.LocalSpaceBlocks;

            Handles.color = Color.red;
            Gizmos.color = Color.red;

            if(!Application.isPlaying) {
                for(int i=0; i< sp.Count; i++) {
                    Vector3 newPos = Handles.PositionHandle(sp[i].LocalSpawnPoint, Quaternion.identity);
                    if (newPos != sp[i].LocalSpawnPoint) {
                        var b = sp[i];
                        b.LocalSpawnPoint = newPos;
                        sp[i] = b;
                    }
                }
            }
            else {
                for(int i = 0; i < generator.Platforms.Count; i++) {
                    var spawnWorldPosition = generator.Platforms[i].SpawnWorldPosition;
                    if (Handles.Button(spawnWorldPosition, Quaternion.identity, 0.5f, 0.5f, Handles.SphereHandleCap)) {
                        if (Application.isPlaying) {
                            var player = Game.instance.CurrentState.Entities.First(x => (x as Player) != null) as Player;
                            if (player != null) {
                                Checkpoint.CurrentSpawnWorldPosition = spawnWorldPosition;
                                Checkpoint.Respawn(player.transform);
                            }
                        }
                    }
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                if(!Application.isPlaying) {
                    Undo.RecordObject(generator, "Change Local Position");
                    var so = new SerializedObject(target);
                    for (var i=0; i<sp.Count; i++) {
                        var element = so.FindProperty("localSpaceBlocks").GetArrayElementAtIndex(i).FindPropertyRelative("LocalSpawnPoint");
                        element.vector3Value = sp[i].LocalSpawnPoint;
                    }
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(so.targetObject);
                }
                // generator.SpawnPoints = sp;
            }
        }
    }
    #endif
}


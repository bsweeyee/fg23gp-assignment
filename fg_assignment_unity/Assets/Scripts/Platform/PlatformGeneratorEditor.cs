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

            var blocks = generator.GetWorldSpaceBlockPositions(generator.TileMap, generator.OccupiedTilePositions, generator.AxisDirection);
            var sp = generator.SpawnPoints;

            foreach(var block in blocks) {
                var idx = sp.FindIndex( x => x.Centre == block.Centre);
                if (idx < 0) {
                    sp.Add(block);
                }
            }

            for(int i=0; i< sp.Count; i++) {
                Vector3 newPos = Handles.PositionHandle(sp[i].WorldSpawnPoint, Quaternion.identity);
                if (newPos != sp[i].WorldSpawnPoint) {
                    var b = sp[i];
                    b.WorldSpawnPoint = newPos;
                    sp[i] = b;
                }
                Handles.color = Color.red;
                if (Handles.Button(newPos + Vector3.down.normalized * 1, Quaternion.identity, 0.5f, 0.5f, Handles.SphereHandleCap)) {
                    if (Application.isPlaying) {
                        var player = Game.instance.Entities.First(x => (x as Player) != null) as Player;
                        Checkpoint.CurrentSpawnWorldPosition = newPos;
                        Checkpoint.Respawn(player.transform);
                    }
                }
            }

            // remove any blocks that are no longer available
            for(var i = sp.Count - 1; i >= 0; i--) {
                int idx = blocks.FindIndex( x=> sp[i].Centre == x.Centre);
                if (idx < 0) {
                    sp.RemoveAt(i);
                }
            }


            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(generator, "Change Local Position");

                var so = new SerializedObject(target);
                for (var i=0; i<sp.Count; i++) {
                    var element = so.FindProperty("spawnPoints").GetArrayElementAtIndex(i).FindPropertyRelative("WorldSpawnPoint");
                    element.vector3Value = sp[i].WorldSpawnPoint;
                }
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(so.targetObject);
                // generator.SpawnPoints = sp;
            }
        }
    }
    #endif
}


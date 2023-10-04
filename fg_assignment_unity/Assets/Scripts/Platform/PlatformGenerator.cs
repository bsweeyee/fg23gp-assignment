using Lander;
using Lander.GameState;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Lander {

    [ExecuteInEditMode]
    public class PlatformGenerator : MonoBehaviour, IPlayStateEntity {
        [Serializable]
        public struct TileBlockData {
            public Vector3 Centre;
            public Vector3 Size;
            public Vector3 WorldSpawnPoint;

            public TileBlockData(Vector3 centre, Vector3 size) {
                this.Centre = centre;
                this.Size = size;
                this.WorldSpawnPoint = centre;
            }
        }

        public enum EAxisDirection {
            X = 0,
            Y = 1
        }

        [SerializeField] private EAxisDirection axisDirection;

        private Grid grid;
        private Tilemap tMap;
        private TilemapRenderer tMapRenderer;

        private List<Vector3Int> occupiedTilePositions;
        private List<Platform> platforms;
        [HideInInspector][SerializeField] private List<TileBlockData> spawnPoints;

        public Tilemap TileMap {
            get {
                return tMap;
            }
        }

        public List<Vector3Int> OccupiedTilePositions {
            get {
                return occupiedTilePositions;
            }
        }

        public EAxisDirection AxisDirection {
            get {
                return axisDirection;
            }
        }

        public List<TileBlockData> SpawnPoints {
            get {
                return spawnPoints;
            }
            set {
                spawnPoints = value;
            }
        }

        public bool IsEarlyInitialized { get; private set; }

        public bool IsLateInitialized { get; private set; }

        public void EarlyInitialize(Game game) {
            if (IsEarlyInitialized) return;

            grid = GetComponentInChildren<Grid>();
            tMap = GetComponentInChildren<Tilemap>();
            tMapRenderer = GetComponentInChildren<TilemapRenderer>();

            occupiedTilePositions = new List<Vector3Int>();
            platforms = new List<Platform>();

            foreach(var position in tMap.cellBounds.allPositionsWithin) {
                if(tMap.HasTile(position)) {
                    occupiedTilePositions.Add(position);
                }
            }

            var wsBlocks = GetWorldSpaceBlockPositions(tMap, occupiedTilePositions, axisDirection);
            foreach(var block in wsBlocks) {
                var go = new GameObject("Platform");
                go.transform.parent = tMap.transform;
                go.transform.position = block.Centre;

                var bx = go.AddComponent<BoxCollider2D>();
                var platform = go.AddComponent<Platform>();

                bx.size = block.Size;

                var spawn = spawnPoints.Find(x => x.Centre == block.Centre);
                platform.SetLocalSpawnPoint(platform.transform.InverseTransformPoint(spawn.WorldSpawnPoint));
                platform.TileBlockData = block;

                platforms.Add(platform);
            }

            IsEarlyInitialized = true;
        }

        public void LateInitialize(Game game) {
            if (IsLateInitialized) return;

            IsLateInitialized = true;
        }

        public void OnEnter(Game game, IBaseGameState previous) {

        }
        public void OnExit(Game game, IBaseGameState current) {

        }
        public void OnTick(Game game, float dt) {

        }
        public void OnFixedTick(Game game, float dt) {

        }

        public List<TileBlockData> GetWorldSpaceBlockPositions(Tilemap tm, List<Vector3Int> tiles, EAxisDirection mainAxis = 0) {
            List<TileBlockData> worldSpaceBlockCentre = new List<TileBlockData>();
            if (tiles.Count <= 0) return worldSpaceBlockCentre;

            var sortByMainAxis = (mainAxis == 0) ? tiles.OrderBy(x => x.x).OrderBy(x => x.y).ToArray() : tiles.OrderBy(x => x.y).OrderBy(x => x.x).ToArray();
            int prevMainAxis = (mainAxis == 0) ? sortByMainAxis[0].y : sortByMainAxis[0].x;
            int prevOtherAxis = (mainAxis == 0) ? sortByMainAxis[0].x : sortByMainAxis[0].y;
            var center = Vector3.zero;
            Vector3Int start = sortByMainAxis[0], end = sortByMainAxis[0];
            var st = tm.GetTile(start);
            var et = tm.GetTile(end);
            var length = 1;

            for(int i=1; i < sortByMainAxis.Length; i++) {
                var v = sortByMainAxis[i];
                var main = (mainAxis == 0) ? v.y : v.x;
                var other = (mainAxis == 0) ? v.x : v.y;

                if(other - 1 != prevOtherAxis) {
                    st = tm.GetTile(start);
                    et = tm.GetTile(end);

                    if (st == null || et == null) { continue; }

                    if (GetCenter(tm, st as Tile, et as Tile, start, end, out center)) {
                        var size = ((et as Tile).sprite.bounds.size + (st as Tile).sprite.bounds.size) / 2;
                        if (mainAxis == 0) size.x *= length;
                        else size.y *= length;
                        TileBlockData data = new TileBlockData(center, size);
                        worldSpaceBlockCentre.Add( data );
                    }
                    start = v;
                    length = 0;
                }

                if(main != prevMainAxis) {
                    start = v;
                }
                prevMainAxis = (mainAxis == 0) ? v.y : v.x;
                prevOtherAxis = (mainAxis == 0) ? v.x : v.y;
                end = v;
                length++;
            }

            st = tm.GetTile(start);
            et = tm.GetTile(end);

            if (st == null || et == null) { return worldSpaceBlockCentre; }

            if (GetCenter(tm, st as Tile, et as Tile, start, end, out center)) {
                var size = ((et as Tile).sprite.bounds.size + (st as Tile).sprite.bounds.size) / 2;
                if (mainAxis == 0) size.x *= length;
                else size.y *= length;
                TileBlockData data = new TileBlockData(center, size);
                worldSpaceBlockCentre.Add( data );
            }

            return worldSpaceBlockCentre;
        }

        private bool GetCenter(Tilemap tm, Tile startTile, Tile endTile, Vector3Int start, Vector3Int end, out Vector3 centre) {
            centre = (tm.CellToWorld(end) + endTile.sprite.bounds.size / 2 + tm.CellToWorld(start) + startTile.sprite.bounds.size / 2) / 2;

            return true;
        }

        #if UNITY_EDITOR
        private void OnTileMapTileChanged(Tilemap tm, Tilemap.SyncTile[] tiles) {
            foreach(var tile in tiles) {
                if(tile.tileData.sprite != null) {
                    occupiedTilePositions.Add(tile.position);
                }
                else {
                    occupiedTilePositions.Remove(tile.position);
                }
            }
        }

        private void OnEnable() {
            Tilemap.tilemapTileChanged += OnTileMapTileChanged;
            // Tilemap.tilemapPositionsChanged += OnTileMapPositionsChanged;
        }

        private void OnDrawGizmos() {
            if (occupiedTilePositions == null) occupiedTilePositions = new List<Vector3Int>();
            if (tMapRenderer == null) tMapRenderer = GetComponentInChildren<TilemapRenderer>();
            if (spawnPoints == null) spawnPoints = new List<TileBlockData>();
            if(tMap == null) {
                tMap = GetComponentInChildren<Tilemap>();
                foreach(var position in tMap.cellBounds.allPositionsWithin) {
                    if(tMap.HasTile(position)) {
                        occupiedTilePositions.Add(position);
                    }
                }
            }

            foreach (var position in occupiedTilePositions)
            {
                var t = tMap.GetTile(position);
                if (t == null) continue;
                var tile = tMap.GetTile(position) as Tile;

                var worldPos = tMap.CellToWorld(position);
                var size = tile.sprite.bounds.size;

                Gizmos.color = new Color(Color.green.r, Color.green.g, Color.green.b, 0.1f);
                Gizmos.DrawCube(worldPos + size / 2, size);
            }


            var blocks = GetWorldSpaceBlockPositions(tMap, occupiedTilePositions, axisDirection);

            foreach(var block in blocks) {
                var newBlock = block;
                Gizmos.color = Color.black;
                Gizmos.DrawSphere(block.Centre, 0.25f);
            }


            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(tMapRenderer.bounds.center, tMapRenderer.bounds.size);

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(tMapRenderer.bounds.center, tMapRenderer.chunkCullingBounds);
        }
    #endif
    }
}


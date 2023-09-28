using Lander;
using Lander.GameState;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteInEditMode]
public class PlatformController : MonoBehaviour, IEntities {
    private Tilemap tMap;
    private TilemapRenderer tMapRenderer;
    private BoxCollider2D boxCollider;

    private List<Vector3Int> occupiedTilePositions;
    public void Initialize(Game game) {
        tMap = GetComponentInChildren<Tilemap>();
        tMapRenderer = GetComponentInChildren<TilemapRenderer>();

        occupiedTilePositions = new List<Vector3Int>();
        foreach(var position in tMap.cellBounds.allPositionsWithin) {
            if(tMap.HasTile(position)) {
                occupiedTilePositions.Add(position);
            }
        }
    }
    public void OnEnter(Game game, IBaseGameState previous, IBaseGameState current) {

    }
    public void OnExit(Game game, IBaseGameState previous, IBaseGameState current) {

    }
    public void OnTick(Game game, float dt) {

    }
    public void OnFixedTick(Game game, float dt) {

    }

    private List<Vector3> GetWorldSpaceBlockPositions(Tilemap tm, List<Vector3Int> tiles) {
        List<Vector3> worldSpaceBlockCentre = new List<Vector3>();

        var sortedByY = tiles.OrderBy(x => x.x).OrderBy(x => x.y).ToArray();
        int prevY = sortedByY[0].y;
        int prevX = sortedByY[0].x;
        Vector3Int start = sortedByY[0], end = sortedByY[0];

        for(int i=1; i < sortedByY.Length; i++) {
            var v = sortedByY[i];
            // Debug.Log("v: " + v.y + ", " + prevY + ", " + v);
            if(v.y != prevY) {
                Debug.Log(end + ", " + start);
                if(v.x - 1 != prevX) {
                    var center = (tm.CellToWorld(end) + tm.CellToWorld(start)) / 2;
                    worldSpaceBlockCentre.Add( center );
                }
                prevY = v.y;
                start = v;
            }
            end = v;
        }

        return worldSpaceBlockCentre;
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
        if (tMapRenderer == null) tMapRenderer = GetComponent<TilemapRenderer>();
        if(tMap == null) {
            tMap = GetComponent<Tilemap>();
            foreach(var position in tMap.cellBounds.allPositionsWithin) {
                if(tMap.HasTile(position)) {
                    occupiedTilePositions.Add(position);
                }
            }
        }

        foreach (var position in occupiedTilePositions)
        {
            var tile = tMap.GetTile(position) as Tile;

            var worldPos = tMap.CellToWorld(position);
            var size = tile.sprite.bounds.size;

            Gizmos.color = new Color(Color.blue.r, Color.blue.g, Color.blue.b, 0.5f);
            Gizmos.DrawCube(worldPos + size / 2, size);
        }

        var centres = GetWorldSpaceBlockPositions(tMap, occupiedTilePositions);
        foreach(var centre in centres) {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(centre, 0.1f);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(tMapRenderer.bounds.center, tMapRenderer.bounds.size);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(tMapRenderer.bounds.center, tMapRenderer.chunkCullingBounds);
    }
#endif
}

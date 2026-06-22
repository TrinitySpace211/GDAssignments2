using System.Collections.Generic;
using EditorAttributes;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour {

    public int dungeonWidth = 100;
    public int dungeonHeight = 100;
    public float tileSize = 2f;

    public int minRoomSize = 10;
    public int maxRoomSize = 25;

    public int maxRooms = 20;

    [Header("Light Controller Reference")]
    [SerializeField] private LightController lightController;

    [Header("Runtime Filled")]
    [SerializeField] private int roomsAmount = 0;

    [Header("Prefabs")]
    [SerializeField] private List<GameObject> floorPrefabs;
    [SerializeField] private List<GameObject> roofPrefabs;
    [SerializeField] private List<GameObject> wallPrefab;
    [SerializeField] private GameObject pillarPrefab;
    [SerializeField] private GameObject doorwayPrefab;
    [SerializeField] private GameObject torchPrefab;

    private DungeonNode rootNode;

    private List<GameObject> availableFloorTiles = new();

    private void Start() {
        GenerateDungeon();
    }

    private void Update() {
        if (PlayerInputHandler.Instance.SpaceTriggered && !Player.Instance.InDungeon) {
            GenerateDungeon();
            PlayerInputHandler.Instance.SetSpaceTriggered(false);
        }

        if (PlayerInputHandler.Instance.EnterTriggered && !Player.Instance.InDungeon) {
            Player.Instance.EnterDungeon(availableFloorTiles);
            PlayerInputHandler.Instance.SetEnterTriggered(false);
        }
    }

    [Button("Generate Dungeon", 20f)]
    private void GenerateDungeon() {
        roomsAmount = 0;

        ClearDungeonEditor();
        GenerateDungeonLayout();
        SpawnDungeon();
    }

    //Generiert einen Dungeon aus Böden und spawned dann erst die Prefabs.
    //Wenn im Grid an der Position X und Y kein Boden ist "grid[x, y] != 1", 
    //dann wird geprüft ob eine Wand platziert werden soll.
    //Ein Padding ist integriert, damit Wände richtig generiert werden auf X = 0 und Y = 0.
    //Das Grid wird einfach um den Padding Wert (1) in X und Y Richtung verschoben.
    private void SpawnDungeon() {
        int padding = 1;
        int gridWidth = dungeonWidth + (padding * 2);
        int gridHeight = dungeonHeight + (padding * 2);
        int[,] grid = new int[gridWidth, gridHeight];
        CornerType[,] cornerGrid = new CornerType[gridWidth + 1, gridHeight + 1];

        FillGridWithRoomsAndCorridors(rootNode, grid, padding);
        MarkCorners(grid, cornerGrid, padding);

        for (int x = 0; x < gridWidth; x++) {
            for (int y = 0; y < gridHeight; y++) {
                Vector3 position = new Vector3(x, 0, y);

                if (grid[x, y] == (int)TileType.Room || grid[x, y] == (int)TileType.Corridor) {
                    //Spawne Böden und Dächer
                    GameObject floorTileInstance = Instantiate(floorPrefabs[Random.Range(0, floorPrefabs.Count)], position * tileSize, Quaternion.identity, transform);
                    Instantiate(roofPrefabs[Random.Range(0, roofPrefabs.Count)], position * tileSize + new Vector3(0, 5.2f, 0), Quaternion.identity, transform);

                    availableFloorTiles.Add(floorTileInstance);

                    //Platziere Wände
                    SpawnWallsInRooms(x, y, grid, gridWidth, gridHeight, position);
                    SpawnWallsInCorridors(x, y, grid, gridWidth, gridHeight, position);
                    SpawnDoorways(x, y, grid, position);
                }
            }
        }
        SpawnPillars(cornerGrid);
    }

    //Clearen für den Button
    private void ClearDungeonEditor() {
        for (int i = transform.childCount - 1; i >= 0; i--) {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }

    //Füllt den Grid mit Böden "grid[x,y] = 1" unter Beachtung des Paddings
    private void FillGridWithRoomsAndCorridors(DungeonNode node, int[,] grid, int padding) {
        if (node == null) return;

        if (node.IsLeaf()) {
            for (int x = node.room.x; x < node.room.xMax; x++)
                for (int y = node.room.y; y < node.room.yMax; y++) {
                    grid[x + padding, y + padding] = (int)TileType.Room;
                }
        }

        foreach (var corridor in node.corridors) {
            for (int x = corridor.x; x < corridor.xMax; x++)
                for (int y = corridor.y; y < corridor.yMax; y++) {
                    if (grid[x + padding, y + padding] == (int)TileType.None) grid[x + padding, y + padding] = (int)TileType.Corridor;
                }
        }

        FillGridWithRoomsAndCorridors(node.left, grid, padding);
        FillGridWithRoomsAndCorridors(node.right, grid, padding);
    }

    private void MarkCorners(int[,] grid, CornerType[,] cornerGrid, int padding) {
        int width = grid.GetLength(0);
        int height = grid.GetLength(1);

        for (int x = 0; x < width - 1; x++) {
            for (int y = 0; y < height - 1; y++) {

                int floorCount = 0;
                if (grid[x, y] != (int)TileType.None) floorCount++; // Down/Left
                if (grid[x + 1, y] != (int)TileType.None) floorCount++; // Down/Right
                if (grid[x, y + 1] != (int)TileType.None) floorCount++; // Up/Left
                if (grid[x + 1, y + 1] != (int)TileType.None) floorCount++; // Up/Right

                if (floorCount == 1 || floorCount == 3) {
                    cornerGrid[x + padding, y + padding] = CornerType.Pillar;
                }
            }
        }
    }

    #region Prefab Spawns
    //Schaut sich an den X und Y Koordinaten, die 8 umherliegenden Werte im Grid an.
    //Wenn es einen Boden gibt, dann soll es auch eine Wand an den X und Y Koordinaten geben.
    //* * *
    //* X *
    //* * *
    private void SpawnWallsInRooms(int x, int y, int[,] grid, int width, int height, Vector3 position) {
        if (grid[x, y] != (int)TileType.Room) return;

        if (x >= 0 && x < width && y >= 0 && y < height) {
            bool north = IsEmpty(x, y + 1, grid);
            bool west = IsEmpty(x - 1, y, grid);
            bool south = IsEmpty(x, y - 1, grid);
            bool east = IsEmpty(x + 1, y, grid);

            if (south) {//Down
                Instantiate(wallPrefab[Random.Range(0, wallPrefab.Count)], position * tileSize, Quaternion.Euler(0, 0, 0), transform);
            }

            if (west) {//Left
                Instantiate(wallPrefab[Random.Range(0, wallPrefab.Count)], position * tileSize, Quaternion.Euler(0, 90, 0), transform);
            }

            if (north) {//Up
                Instantiate(wallPrefab[Random.Range(0, wallPrefab.Count)], position * tileSize, Quaternion.Euler(0, 180, 0), transform);
            }

            if (east) {//Right
                Instantiate(wallPrefab[Random.Range(0, wallPrefab.Count)], position * tileSize, Quaternion.Euler(0, 270, 0), transform);
            }
        }
    }

    private void SpawnWallsInCorridors(int x, int y, int[,] grid, int width, int height, Vector3 position) {
        if (grid[x, y] != (int)TileType.Corridor) return;

        if (x >= 0 && x < width && y >= 0 && y < height) {
            bool north = IsEmpty(x, y + 1, grid);
            bool west = IsEmpty(x - 1, y, grid);
            bool south = IsEmpty(x, y - 1, grid);
            bool east = IsEmpty(x + 1, y, grid);

            if (south) {//Down
                Instantiate(wallPrefab[Random.Range(0, wallPrefab.Count)], position * tileSize, Quaternion.Euler(0, 0, 0), transform);
            }

            if (west) {//Left
                Instantiate(wallPrefab[Random.Range(0, wallPrefab.Count)], position * tileSize, Quaternion.Euler(0, 90, 0), transform);
            }

            if (north) {//Up
                Instantiate(wallPrefab[Random.Range(0, wallPrefab.Count)], position * tileSize, Quaternion.Euler(0, 180, 0), transform);
            }

            if (east) {//Right
                Instantiate(wallPrefab[Random.Range(0, wallPrefab.Count)], position * tileSize, Quaternion.Euler(0, 270, 0), transform);
            }
        }
    }

    private void SpawnPillars(CornerType[,] cornerGrid) {
        for (int x = 0; x < cornerGrid.GetLength(0); x++) {
            for (int y = 0; y < cornerGrid.GetLength(1); y++) {
                if (cornerGrid[x, y] != CornerType.Pillar)
                    continue;

                Vector3 position = CornerToWorldPosition(x, y);

                if (cornerGrid[x, y] == CornerType.Pillar) {
                    Instantiate(pillarPrefab, position * tileSize, Quaternion.identity, transform);
                }
            }
        }
    }

    private void SpawnDoorways(int x, int y, int[,] grid, Vector3 position) {
        if (grid[x, y] != (int)TileType.Room) return;

        bool north = IsCorridor(x, y + 1, grid);
        bool east = IsCorridor(x + 1, y, grid);
        bool south = IsCorridor(x, y - 1, grid);
        bool west = IsCorridor(x - 1, y, grid);

        float torchOffset = 0.3f;

        if (south) {//In South a Corridor
            Vector3 doorPos = position + new Vector3(0, 0, -0.5f);
            Quaternion rotation = Quaternion.Euler(0, 0, 0);

            Instantiate(doorwayPrefab, doorPos * tileSize, rotation, transform);
            GameObject lightGameObject1 = Instantiate(torchPrefab, (doorPos + new Vector3(-0.5f, 0, 0)) * tileSize + new Vector3(0, 3, torchOffset), rotation, transform);
            GameObject lightGameObject2 = Instantiate(torchPrefab, (doorPos + new Vector3(0.5f, 0, 0)) * tileSize + new Vector3(0, 3, torchOffset), rotation, transform);

            Light light1 = lightGameObject1.GetComponentInChildren<Light>();
            Light light2 = lightGameObject2.GetComponentInChildren<Light>();

            lightController.RegisterLight(light1);
            lightController.RegisterLight(light2);
        }

        if (west) {//In West a Corridor
            Vector3 doorPos = position + new Vector3(-0.5f, 0, 0);
            Quaternion rotation = Quaternion.Euler(0, 90, 0);

            Instantiate(doorwayPrefab, doorPos * tileSize, rotation, transform);
            GameObject lightGameObject1 = Instantiate(torchPrefab, (doorPos + new Vector3(0, 0, -0.5f)) * tileSize + new Vector3(torchOffset, 3, 0), rotation, transform);
            GameObject lightGameObject2 = Instantiate(torchPrefab, (doorPos + new Vector3(0, 0, 0.5f)) * tileSize + new Vector3(torchOffset, 3, 0), rotation, transform);

            Light light1 = lightGameObject1.GetComponentInChildren<Light>();
            Light light2 = lightGameObject2.GetComponentInChildren<Light>();
            lightController.RegisterLight(light1);
            lightController.RegisterLight(light2);
        }

        if (north) {//In North a Corridor
            Vector3 doorPos = position + new Vector3(0, 0, 0.5f);
            Quaternion rotation = Quaternion.Euler(0, 180, 0);

            Instantiate(doorwayPrefab, doorPos * tileSize, rotation, transform);
            GameObject lightGameObject1 = Instantiate(torchPrefab, (doorPos + new Vector3(-0.5f, 0, 0)) * tileSize + new Vector3(0, 3, -torchOffset), rotation, transform);
            GameObject lightGameObject2 = Instantiate(torchPrefab, (doorPos + new Vector3(0.5f, 0, 0)) * tileSize + new Vector3(0, 3, -torchOffset), rotation, transform);

            Light light1 = lightGameObject1.GetComponentInChildren<Light>();
            Light light2 = lightGameObject2.GetComponentInChildren<Light>();
            lightController.RegisterLight(light1);
            lightController.RegisterLight(light2);
        }

        if (east) {//In East a Corridor
            Vector3 doorPos = position + new Vector3(0.5f, 0, 0);
            Quaternion rotation = Quaternion.Euler(0, 270, 0);

            Instantiate(doorwayPrefab, doorPos * tileSize, rotation, transform);
            GameObject lightGameObject1 = Instantiate(torchPrefab, (doorPos + new Vector3(0, 0, -0.5f)) * tileSize + new Vector3(-torchOffset, 3, 0), rotation, transform);
            GameObject lightGameObject2 = Instantiate(torchPrefab, (doorPos + new Vector3(0, 0, 0.5f)) * tileSize + new Vector3(-torchOffset, 3, 0), rotation, transform);

            Light light1 = lightGameObject1.GetComponentInChildren<Light>();
            Light light2 = lightGameObject2.GetComponentInChildren<Light>();
            lightController.RegisterLight(light1);
            lightController.RegisterLight(light2);
        }
    }
    #endregion

    #region Checks
    private bool IsEmpty(int x, int y, int[,] grid) {
        if (x < 0 || y < 0 ||
            x >= grid.GetLength(0) ||
            y >= grid.GetLength(1))
            return true;

        return grid[x, y] == (int)TileType.None;
    }

    private bool IsCorridor(int x, int y, int[,] grid) {
        if (x < 0 || y < 0 ||
            x >= grid.GetLength(0) ||
            y >= grid.GetLength(1))
            return true;

        return grid[x, y] == (int)TileType.Corridor;
    }

    private Vector3 CornerToWorldPosition(int x, int y) {
        return new Vector3(
            x - 0.5f,
            0,
            y - 0.5f);
    }
    #endregion

    #region Grid Generation
    /// <summary>
    /// Binary Space Partitioning - based Dungeon Generator
    /// </summary>
    private void GenerateDungeonLayout() {
        rootNode = new DungeonNode(new RectInt(0, 0, dungeonWidth, dungeonHeight));
        SplitNode(rootNode);
        CreateRooms(rootNode);
    }

    //Splitten der Nodes bis maximale Raumgröße erreicht oder der Split zu klein ist
    private void SplitNode(DungeonNode node) {
        //Abbruchbedingung der Rekursion. Solange teilen bis der Raum unter die maxRoomSize kommt.
        if (node.area.width <= maxRoomSize && node.area.height <= maxRoomSize) return;

        bool splitHorizontal = Random.value > 0.5f;

        //Wenn ein Raum breiter ist als seine Höhe * 2, dann lieber vertikal teilen
        if (node.area.width > node.area.height * 2) splitHorizontal = false;
        else if (node.area.height > node.area.width * 2) splitHorizontal = true;

        int maxSplit = (splitHorizontal ? node.area.height : node.area.width) - minRoomSize;
        if (maxSplit <= minRoomSize) return; //minRoomSize unterschritten. Zu klein zum teilen

        int splitPoint = Random.Range(minRoomSize, maxSplit);

        if (splitHorizontal) {
            node.left = new DungeonNode(new RectInt(node.area.x, node.area.y, node.area.width, splitPoint));
            node.right = new DungeonNode(new RectInt(node.area.x, node.area.y + splitPoint, node.area.width, node.area.height - splitPoint));
        } else {
            node.left = new DungeonNode(new RectInt(node.area.x, node.area.y, splitPoint, node.area.height));
            node.right = new DungeonNode(new RectInt(node.area.x + splitPoint, node.area.y, node.area.width - splitPoint, node.area.height));
        }

        //Weiter teilen
        SplitNode(node.left);
        SplitNode(node.right);
    }

    //Erstellt Räume mit zufälliger Breite und Länge für jeden Node 
    //bis zum maximalen gesetzten Raum Limit oder bis alle Blätter durchgelaufen sind.
    private void CreateRooms(DungeonNode node) {
        if (roomsAmount < maxRooms) {
            if (node.IsLeaf()) {
                int roomWidth = Random.Range(minRoomSize, node.area.width);
                int roomHeight = Random.Range(minRoomSize, node.area.height);

                int roomX = node.area.x + Random.Range(0, node.area.width - roomWidth);
                int roomY = node.area.y + Random.Range(0, node.area.height - roomHeight);

                node.room = new RectInt(roomX, roomY, roomWidth, roomHeight);

                roomsAmount++;
            } else {
                if (node.left != null) CreateRooms(node.left);
                if (node.right != null) CreateRooms(node.right);

                ConnectNodes(node.left, node.right);
            }
        } else {
            return;
        }
    }

    //Verbindet Räume mit Korridoren
    private void ConnectNodes(DungeonNode left, DungeonNode right) {
        Vector2Int leftCenter = GetRoomCenter(left);
        Vector2Int rightCenter = GetRoomCenter(right);

        //Erstellt einen L-förmigen Korridor "_|", statt Diagonale "/"
        if (Random.value > 0.5f) {
            CreateHorizontalCorridor(leftCenter.x, rightCenter.x, leftCenter.y, left);
            CreateVerticalCorridor(leftCenter.y, rightCenter.y, rightCenter.x, left);
        } else {//"|_"-Korridor
            CreateVerticalCorridor(leftCenter.y, rightCenter.y, leftCenter.x, left);
            CreateHorizontalCorridor(leftCenter.x, rightCenter.x, rightCenter.y, left);
        }
    }

    private Vector2Int GetRoomCenter(DungeonNode node) {
        if (node.IsLeaf()) {
            return new Vector2Int((int)node.room.center.x, (int)node.room.center.y);
        } else {
            return GetRoomCenter(node.left);
        }
    }

    private void CreateHorizontalCorridor(int x1, int x2, int y, DungeonNode node) {
        int start = Mathf.Min(x1, x2);
        int end = Mathf.Max(x1, x2);
        node.corridors.Add(new RectInt(start, y, end - start + 1, 1));
    }

    private void CreateVerticalCorridor(int y1, int y2, int x, DungeonNode node) {
        int start = Mathf.Min(y1, y2);
        int end = Mathf.Max(y1, y2);
        node.corridors.Add(new RectInt(x, start, 1, end - start + 1));
    }
    #endregion
}

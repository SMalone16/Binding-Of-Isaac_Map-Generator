using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum EdgeDirection
{
    Up,
    Down,
    Left,
    Right
}

public class Room : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    private Dictionary<EdgeDirection, List<Door>> doorsByDirection;
    private bool collidersBuilt;
    private bool isStartRoom;
    private bool hasCachedBounds;
    private Rect cachedMovementBounds;
    private EnemyController enemyInstance;
    private bool shouldSpawnEnemy;

    private void Awake()
    {
        doorsByDirection = new Dictionary<EdgeDirection, List<Door>>();
    }

    public void SetupRoom(Cell currentCell, RoomScriptable room)
    {
        spriteRenderer.sprite = room.roomVariations[Random.Range(0, room.roomVariations.Length)];

        shouldSpawnEnemy = currentCell.roomType == RoomType.Regular;
        doorsByDirection.Clear();
        collidersBuilt = false;
        hasCachedBounds = false;

        if (currentCell.roomType == RoomType.Secret)
        {
            BuildWallColliders();
            return;
        }

        var floorplan = MapGenerator.instance.getFloorPlan;
        var cellList = MapGenerator.instance.getSpawnedCells;

        switch (currentCell.roomShape)
        {
            case RoomShape.OneByOne:
                SetupOneByOne(currentCell, floorplan, cellList);
                break;

            case RoomShape.OneByTwo:
                SetupOneByTwo(currentCell, floorplan, cellList);
                break;

            case RoomShape.TwoByOne:
                SetupTwoByOne(currentCell, floorplan, cellList);
                break;

            case RoomShape.TwoByTwo:
                SetupTwoByTwo(currentCell, floorplan, cellList);
                break;

            case RoomShape.LShape:
                SetupLShapeRoom(currentCell, floorplan, cellList);
                break;
        }

        BuildWallColliders();
    }

    public Vector2 GetSpawnPoint()
    {
        return transform.position;
    }

    public void MarkAsStartRoom()
    {
        isStartRoom = true;
        InstructionOverlay.Show();
    }

    public void OnPlayerEntered(PlayerController player)
    {
        if (isStartRoom)
        {
            InstructionOverlay.Show();
        }
        else
        {
            InstructionOverlay.Hide();
        }

        HandleEnemySpawn(player);
    }

    public void OnPlayerExited()
    {
        if (isStartRoom)
        {
            InstructionOverlay.Hide();
        }

        if (enemyInstance != null)
        {
            enemyInstance.SetActive(false);
        }
    }

    public void RefreshPlayerReference(PlayerController player)
    {
        if (enemyInstance != null)
        {
            enemyInstance.Configure(player.transform, GetMovementBounds());
        }

        if (isStartRoom)
        {
            InstructionOverlay.Show();
        }
    }

    public void SetupOneByOne(Cell cell, int[] floorplan, List<Cell> cellList)
    {
        var currentCell = cell.cellList[0];

        TryPlaceDoor(currentCell, new Vector2(0, 1.75f), EdgeDirection.Up, floorplan, cellList, cell);
        TryPlaceDoor(currentCell, new Vector2(0, -1.75f), EdgeDirection.Down, floorplan, cellList, cell);
        TryPlaceDoor(currentCell, new Vector2(-4.25f, 0), EdgeDirection.Left, floorplan, cellList, cell);
        TryPlaceDoor(currentCell, new Vector2(4.25f, 0), EdgeDirection.Right, floorplan, cellList, cell);
    }

    public void SetupOneByTwo(Cell cell, int[] floorplan, List<Cell> cellList)
    {
        var cellA = cell.cellList[0];
        var cellB = cell.cellList[1];

        TryPlaceDoor(cellA, new Vector2(0f, 4f), EdgeDirection.Up, floorplan, cellList, cell);
        TryPlaceDoor(cellA, new Vector2(-4.25f, 2.6125f), EdgeDirection.Left, floorplan, cellList, cell);
        TryPlaceDoor(cellA, new Vector2(4.25f, 2.6125f), EdgeDirection.Right, floorplan, cellList, cell);

        TryPlaceDoor(cellB, new Vector2(0f, -4f), EdgeDirection.Down, floorplan, cellList, cell);
        TryPlaceDoor(cellB, new Vector2(-4.25f, -2.6125f), EdgeDirection.Left, floorplan, cellList, cell);
        TryPlaceDoor(cellB, new Vector2(4.25f, -2.6125f), EdgeDirection.Right, floorplan, cellList, cell);
    }

    public void SetupTwoByOne(Cell cell, int[] floorplan, List<Cell> cellList)
    {
        var cellA = cell.cellList[0];
        var cellB = cell.cellList[1];

        TryPlaceDoor(cellA, new Vector2(-5f, 1.5f), EdgeDirection.Up, floorplan, cellList, cell);
        TryPlaceDoor(cellA, new Vector2(-9.75f, 0f), EdgeDirection.Left, floorplan, cellList, cell);
        TryPlaceDoor(cellA, new Vector2(-5f, -1.5f), EdgeDirection.Down, floorplan, cellList, cell);

        TryPlaceDoor(cellB, new Vector2(5f, 1.5f), EdgeDirection.Up, floorplan, cellList, cell);
        TryPlaceDoor(cellB, new Vector2(5f, -1.5f), EdgeDirection.Down, floorplan, cellList, cell);
        TryPlaceDoor(cellB, new Vector2(9.75f, 0f), EdgeDirection.Right, floorplan, cellList, cell);
    }

    public void SetupTwoByTwo(Cell cell, int[] floorplan, List<Cell> cellList)
    {
        var cellA = cell.cellList[0];
        var cellB = cell.cellList[1];
        var cellC = cell.cellList[2];
        var cellD = cell.cellList[3];

        TryPlaceDoor(cellA, new Vector2(-5.3125f, 4.5f), EdgeDirection.Up, floorplan, cellList, cell);
        TryPlaceDoor(cellB, new Vector2(5.3125f, 4.5f), EdgeDirection.Up, floorplan, cellList, cell);

        TryPlaceDoor(cellA, new Vector2(-9.75f, 2.6125f), EdgeDirection.Left, floorplan, cellList, cell);
        TryPlaceDoor(cellC, new Vector2(-9.75f, -2.6125f), EdgeDirection.Left, floorplan, cellList, cell);

        TryPlaceDoor(cellC, new Vector2(-5.3125f, -4.5f), EdgeDirection.Down, floorplan, cellList, cell);
        TryPlaceDoor(cellD, new Vector2(5.3125f, -4.5f), EdgeDirection.Down, floorplan, cellList, cell);

        TryPlaceDoor(cellB, new Vector2(9.75f, 2.6125f), EdgeDirection.Right, floorplan, cellList, cell);
        TryPlaceDoor(cellD, new Vector2(9.75f, -2.6125f), EdgeDirection.Right, floorplan, cellList, cell);
    }

    public void SetupLShapeRoom(Cell cell, int[] floorplan, List<Cell> cellList)
    {
        var cellA = cell.cellList[0];
        var cellB = cell.cellList[1];
        var cellC = cell.cellList[2];

        if (cellA + 1 == cellB && cellA + 10 == cellC)
        {
            TryPlaceDoor(cellA, new Vector2(-5.3125f, 4.5f), EdgeDirection.Up, floorplan, cellList, cell);
            TryPlaceDoor(cellA, new Vector2(-9.75f, 2.6125f), EdgeDirection.Left, floorplan, cellList, cell);

            TryPlaceDoor(cellB, new Vector2(5.3125f, 4.5f), EdgeDirection.Up, floorplan, cellList, cell);
            TryPlaceDoor(cellB, new Vector2(9.75f, 2.6375f), EdgeDirection.Right, floorplan, cellList, cell);
            TryPlaceDoor(cellB, new Vector2(5.3125f, 1f), EdgeDirection.Down, floorplan, cellList, cell);

            TryPlaceDoor(cellC, new Vector2(-5.3125f, -4.5f), EdgeDirection.Down, floorplan, cellList, cell);
            TryPlaceDoor(cellC, new Vector2(-1f, -2.6375f), EdgeDirection.Right, floorplan, cellList, cell);
            TryPlaceDoor(cellC, new Vector2(-9.75f, -2.6125f), EdgeDirection.Left, floorplan, cellList, cell);
        }
        else if (cellA + 1 == cellB && cellB + 10 == cellC)
        {
            TryPlaceDoor(cellA, new Vector2(-5.3125f, 4.5f), EdgeDirection.Up, floorplan, cellList, cell);
            TryPlaceDoor(cellA, new Vector2(-9.75f, 2.6125f), EdgeDirection.Left, floorplan, cellList, cell);
            TryPlaceDoor(cellA, new Vector2(-5.3125f, 1f), EdgeDirection.Down, floorplan, cellList, cell);

            TryPlaceDoor(cellB, new Vector2(5.3125f, 4.5f), EdgeDirection.Up, floorplan, cellList, cell);
            TryPlaceDoor(cellB, new Vector2(9.75f, 2.6375f), EdgeDirection.Right, floorplan, cellList, cell);

            TryPlaceDoor(cellC, new Vector2(5.3125f, -4.5f), EdgeDirection.Down, floorplan, cellList, cell);
            TryPlaceDoor(cellC, new Vector2(9.75f, -2.6375f), EdgeDirection.Right, floorplan, cellList, cell);
            TryPlaceDoor(cellC, new Vector2(1, -2.6125f), EdgeDirection.Left, floorplan, cellList, cell);
        }
        else if (cellA + 10 == cellB)
        {
            TryPlaceDoor(cellA, new Vector2(-5.3125f, 4.5f), EdgeDirection.Up, floorplan, cellList, cell);
            TryPlaceDoor(cellA, new Vector2(-9.75f, 2.6125f), EdgeDirection.Left, floorplan, cellList, cell);
            TryPlaceDoor(cellA, new Vector2(-1f, 2.6125f), EdgeDirection.Right, floorplan, cellList, cell);

            TryPlaceDoor(cellB, new Vector2(-5.3125f, -4.5f), EdgeDirection.Down, floorplan, cellList, cell);
            TryPlaceDoor(cellB, new Vector2(-9.75f, -2.6125f), EdgeDirection.Left, floorplan, cellList, cell);

            TryPlaceDoor(cellC, new Vector2(5.3125f, -1), EdgeDirection.Up, floorplan, cellList, cell);
            TryPlaceDoor(cellC, new Vector2(5.3125f, -4.5f), EdgeDirection.Down, floorplan, cellList, cell);
            TryPlaceDoor(cellC, new Vector2(9.75f, -2.6375f), EdgeDirection.Right, floorplan, cellList, cell);
        }
        else if (cellA + 10 == cellC)
        {
            TryPlaceDoor(cellA, new Vector2(5.3125f, 4.5f), EdgeDirection.Up, floorplan, cellList, cell);
            TryPlaceDoor(cellA, new Vector2(1, 2.6125f), EdgeDirection.Left, floorplan, cellList, cell);
            TryPlaceDoor(cellA, new Vector2(9.75f, 2.6125f), EdgeDirection.Right, floorplan, cellList, cell);

            TryPlaceDoor(cellB, new Vector2(-5.3125f, -1f), EdgeDirection.Up, floorplan, cellList, cell);
            TryPlaceDoor(cellB, new Vector2(-5.3125f, -4.5f), EdgeDirection.Down, floorplan, cellList, cell);
            TryPlaceDoor(cellB, new Vector2(-9.75f, -2.6125f), EdgeDirection.Left, floorplan, cellList, cell);

            TryPlaceDoor(cellC, new Vector2(5.3125f, -4.5f), EdgeDirection.Down, floorplan, cellList, cell);
            TryPlaceDoor(cellC, new Vector2(9.75f, -2.6375f), EdgeDirection.Right, floorplan, cellList, cell);
        }
    }

    private void TryPlaceDoor(int fromIndex, Vector2 positionOffset, EdgeDirection direction, int[] floorplan, List<Cell> cellList, Cell currentCell)
    {
        int neighbourIndex = fromIndex + GetOffset(direction);

        if (neighbourIndex < 0 || neighbourIndex >= floorplan.Length)
            return;

        if (floorplan[neighbourIndex] != 1)
            return;

        var foundCell = cellList.FirstOrDefault(x => x.cellList.Contains(neighbourIndex));

        if (foundCell.roomType == RoomType.Secret)
            return;

        var door = Instantiate(RoomManager.instance.doorPrefab, transform);
        door.transform.position = (Vector2)transform.position + positionOffset;

        RegisterDoor(door, direction, fromIndex, neighbourIndex);
        SetupDoor(door, direction, currentCell.roomType == RoomType.Regular ? foundCell.roomType : currentCell.roomType);
    }

    private void RegisterDoor(Door door, EdgeDirection direction, int fromIndex, int neighbourIndex)
    {
        if (!doorsByDirection.TryGetValue(direction, out var list))
        {
            list = new List<Door>();
            doorsByDirection.Add(direction, list);
        }

        list.Add(door);
        door.SetDirection(direction);
        door.SetConnection(fromIndex, neighbourIndex);
    }

    private void SetupDoor(Door door, EdgeDirection direction, RoomType roomType)
    {
        var doorTypes = GetDoorOptions(roomType);

        switch (direction)
        {
            case EdgeDirection.Up:
                door.SetDoorSprite(doorTypes.upDoor);
                break;

            case EdgeDirection.Down:
                door.SetDoorSprite(doorTypes.downDoor);
                break;

            case EdgeDirection.Left:
                door.SetDoorSprite(doorTypes.leftDoor);
                break;

            case EdgeDirection.Right:
                door.SetDoorSprite(doorTypes.rightDoor);
                break;
        }
    }

    private DoorScriptable GetDoorOptions(RoomType roomType)
    {
        return RoomManager.instance.doors.FirstOrDefault(x => x.roomType == roomType);
    }

    private int GetOffset(EdgeDirection direction)
    {
        switch (direction)
        {
            case EdgeDirection.Up:
                return -10;

            case EdgeDirection.Down:
                return 10;

            case EdgeDirection.Right:
                return 1;

            case EdgeDirection.Left:
                return -1;
        }

        return 0;
    }

    private void HandleEnemySpawn(PlayerController player)
    {
        if (!shouldSpawnEnemy)
            return;

        var bounds = GetMovementBounds();

        if (enemyInstance == null)
        {
            enemyInstance = EnemyController.Create();
            enemyInstance.transform.SetParent(transform);
            enemyInstance.transform.position = bounds.center;
        }

        enemyInstance.Configure(player.transform, bounds);
    }

    private Rect GetMovementBounds()
    {
        if (!hasCachedBounds)
        {
            var bounds = spriteRenderer.bounds;
            const float margin = 0.6f;
            var width = Mathf.Max(0.5f, bounds.size.x - margin * 2f);
            var height = Mathf.Max(0.5f, bounds.size.y - margin * 2f);
            cachedMovementBounds = new Rect(bounds.min.x + margin, bounds.min.y + margin, width, height);
            hasCachedBounds = true;
        }

        return cachedMovementBounds;
    }

    private void BuildWallColliders()
    {
        if (collidersBuilt)
            return;

        collidersBuilt = true;

        var worldBounds = spriteRenderer.bounds;
        var left = worldBounds.min.x - transform.position.x;
        var right = worldBounds.max.x - transform.position.x;
        var bottom = worldBounds.min.y - transform.position.y;
        var top = worldBounds.max.y - transform.position.y;
        const float thickness = 0.6f;

        BuildHorizontalWall(EdgeDirection.Up, top - thickness * 0.5f, left, right, thickness);
        BuildHorizontalWall(EdgeDirection.Down, bottom + thickness * 0.5f, left, right, thickness);
        BuildVerticalWall(EdgeDirection.Left, left + thickness * 0.5f, bottom, top, thickness);
        BuildVerticalWall(EdgeDirection.Right, right - thickness * 0.5f, bottom, top, thickness);
    }

    private void BuildHorizontalWall(EdgeDirection direction, float y, float left, float right, float thickness)
    {
        CreateWallSegments(direction, y, left, right, thickness, true);
    }

    private void BuildVerticalWall(EdgeDirection direction, float x, float bottom, float top, float thickness)
    {
        CreateWallSegments(direction, x, bottom, top, thickness, false);
    }

    private void CreateWallSegments(EdgeDirection direction, float constant, float min, float max, float thickness, bool horizontal)
    {
        var gaps = new List<(float start, float end)>();
        if (doorsByDirection.TryGetValue(direction, out var doorList))
        {
            foreach (var door in doorList)
            {
                var local = door.transform.localPosition;
                var size = door.GetSize();
                var half = (horizontal ? size.x : size.y) * 0.5f + 0.25f;
                var center = horizontal ? local.x : local.y;

                var gapStart = Mathf.Max(min, center - half);
                var gapEnd = Mathf.Min(max, center + half);
                if (gapEnd <= min || gapStart >= max)
                    continue;

                gaps.Add((gapStart, gapEnd));
            }
        }

        gaps.Sort((a, b) => a.start.CompareTo(b.start));

        var cursor = min;
        foreach (var gap in gaps)
        {
            if (gap.start > cursor)
            {
                CreateCollider(cursor, Mathf.Min(gap.start, max), constant, thickness, horizontal);
            }

            cursor = Mathf.Max(cursor, gap.end);
            if (cursor >= max)
                break;
        }

        if (cursor < max)
        {
            CreateCollider(cursor, max, constant, thickness, horizontal);
        }
    }

    private void CreateCollider(float start, float end, float constant, float thickness, bool horizontal)
    {
        var length = end - start;
        if (length <= 0.05f)
            return;

        var collider = gameObject.AddComponent<BoxCollider2D>();
        if (horizontal)
        {
            collider.size = new Vector2(length, thickness);
            collider.offset = new Vector2((start + end) * 0.5f, constant);
        }
        else
        {
            collider.size = new Vector2(thickness, length);
            collider.offset = new Vector2(constant, (start + end) * 0.5f);
        }
    }
}

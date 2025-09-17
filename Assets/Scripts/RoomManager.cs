using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    private List<Room> createdRooms;
    private Dictionary<int, Room> roomsByCellIndex;
    private Room currentRoom;

    [Header("Offset Variables")]
    public float offsetX;
    public float offsetY;

    [Header("Prefab References")]
    public Room roomPrefab;
    public Door doorPrefab;

    [Header("Scriptable Object References")]
    public DoorScriptable[] doors;
    public RoomScriptable[] rooms;

    public static RoomManager instance;

    private void Awake()
    {
        instance = this;
        createdRooms = new List<Room>();
        roomsByCellIndex = new Dictionary<int, Room>();
    }

    public void SetupRooms(List<Cell> spawnedCells)
    {
        for(int i = createdRooms.Count - 1; i >= 0; i--)
        {
            Destroy(createdRooms[i].gameObject);
        }

        createdRooms.Clear();
        roomsByCellIndex.Clear();
        currentRoom = null;

        foreach(var currentCell in spawnedCells)
        {
            var foundRoom = rooms.FirstOrDefault(x => x.roomShape == currentCell.roomShape && x.roomType == currentCell.roomType && DoesTileMatchCell(x.occupiedTiles, currentCell));

            var currentPosition = currentCell.transform.position;

            var convertedPosition = new Vector2(currentPosition.x * offsetX, currentPosition.y * offsetY);

            var spawnedRoom = Instantiate(roomPrefab, convertedPosition, Quaternion.identity);

            spawnedRoom.SetupRoom(currentCell, foundRoom);

            foreach (var index in currentCell.cellList)
            {
                roomsByCellIndex[index] = spawnedRoom;
            }

            createdRooms.Add(spawnedRoom);
        }
    }

    public bool TryGetRoom(int cellIndex, out Room room)
    {
        return roomsByCellIndex.TryGetValue(cellIndex, out room);
    }

    public Vector2 GetRoomCenter(int cellIndex)
    {
        return roomsByCellIndex.TryGetValue(cellIndex, out var room) ? room.GetSpawnPoint() : Vector2.zero;
    }

    public void NotifyPlayerEntered(int cellIndex, PlayerController player)
    {
        if (!roomsByCellIndex.TryGetValue(cellIndex, out var room))
            return;

        if (currentRoom == room)
        {
            if (room != null)
            {
                room.RefreshPlayerReference(player);
            }
            return;
        }

        currentRoom?.OnPlayerExited();
        currentRoom = room;
        currentRoom.OnPlayerEntered(player);
    }

    public void MarkStartRoom(int cellIndex)
    {
        if (roomsByCellIndex.TryGetValue(cellIndex, out var room))
        {
            room.MarkAsStartRoom();
        }
    }

    private bool DoesTileMatchCell(int[] occupiedTiles, Cell cell)
    {
        if(occupiedTiles.Length != cell.cellList.Count)
            return false;

        int minIndex = cell.cellList.Min();
        List<int> normalizedCell = new List<int>();

        foreach(int index in cell.cellList)
        {
            int dx = (index % 10) - (minIndex % 10);
            int dy = (index / 10) - (minIndex / 10);

            normalizedCell.Add(dy * 10 + dx);
        }

        normalizedCell.Sort();
        int[] sortedOccupied = (int[])occupiedTiles.Clone();
        Array.Sort(sortedOccupied);

        return normalizedCell.SequenceEqual(sortedOccupied);
    }
}

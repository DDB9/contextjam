using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public int TargetRoom;
    public float CameraTransitionSpeed = 4f;

    [SerializeField]
    private List<int> RoomHistory = new List<int>();

    private void Start()
    {
        RoomHistory.Add(0);
    }
    private void Update()
    {
        // Set target room for the camera to adjust position.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TargetRoom++;
            if (TargetRoom >= DungeonManager.Instance.Rooms.Count) TargetRoom = 0;
            RoomHistory.Add(TargetRoom);
        }
        // return to previous room. Might wanna implement list history.
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            if (RoomHistory.Count <= 0) TargetRoom = 0;
            else if (RoomHistory[RoomHistory.Count - 1] == 0) TargetRoom = 0;
            else
            {
                RoomHistory.RemoveAt(RoomHistory.Count - 1);
                TargetRoom = RoomHistory[RoomHistory.Count - 1];
            }
        }

        DetermineTargetRoom();
    }

    /// <summary>
    /// Scans all the rooms to determine what room to move the camera to.
    /// </summary>
    private void DetermineTargetRoom()
    {
        foreach (GameObject room in DungeonManager.Instance.Rooms)
        {
            if (room.GetComponent<DungeonRoom>().RoomNumber == TargetRoom)
            {
                PlaceCamera(room);
                break;
            }
        }
    }

    /// <summary>
    /// Move the camera to target room's position.
    /// </summary>
    /// <param name="pTargetRoom">Room to move camera to.</param>
    private void PlaceCamera(GameObject pTargetRoom)
    {
        GetComponent<Camera>().transform.position = Vector2.Lerp(GetComponent<Camera>().transform.position,
                                                                 pTargetRoom.transform.position,
                                                                 Time.deltaTime * CameraTransitionSpeed);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Vector2 cameraZoom = new Vector2();
    public int TargetRoom;
    public float CameraTransitionSpeed = 4f;
    private Transform player;

    [SerializeField]
    private List<int> RoomHistory = new List<int>();

    private void Start()
    {
        RoomHistory.Add(0);
        player = GameObject.Find("Player").transform;
    }

    private void Update()
    {
        // Set target room for the camera to adjust position.
        if (/*Input.GetKeyDown(KeyCode.Space)*/ false)
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
        CameraOrientation();

        float angle = Mathf.Abs(transform.eulerAngles.z);
        if (angle <= 180f)
        {
            float difference = 1f - (Mathf.Abs(transform.eulerAngles.z - 90f) / 90f);
            GetComponent<Camera>().orthographicSize = Mathf.Lerp(cameraZoom.x + 1f, cameraZoom.y + 1f, difference);
        }
        else
        {
            //float difference = 1f - (Mathf.Abs(transform.eulerAngles.z)
            float difference = 1f - (Mathf.Abs((transform.eulerAngles.z - 180f) - 90f) / 90f);
            GetComponent<Camera>().orthographicSize = Mathf.Lerp(cameraZoom.x + 1f, cameraZoom.y + 1f, difference);
        }
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
        GetComponent<Camera>().transform.position = Vector3.Lerp(GetComponent<Camera>().transform.position, pTargetRoom.transform.position + new Vector3(0,0,-5), Time.deltaTime * CameraTransitionSpeed);
    }

    private void CameraOrientation()
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, player.rotation, Time.deltaTime * CameraTransitionSpeed);
    }
}

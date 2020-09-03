using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cursor : MonoBehaviour
{
    // Start is called before the first frame update
    private void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        RaycastHit hit = new RaycastHit();
        Ray ray = GameManager.Instance._Camera.CameraComp.ScreenPointToRay(Input.mousePosition);
        LayerMask mask = LayerMask.GetMask("CursorCollider");
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
        {
            transform.position = hit.point;
        }
    }
}

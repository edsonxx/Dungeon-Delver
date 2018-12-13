using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{

    [Header("Set Dynamically")]
    public int x;
    public int y;
    public int tileNum;

    private BoxCollider bColl; // a
    void Awake()
    {
        bColl = GetComponent<BoxCollider>();
    }

    public void SetTile(int eX, int eY, int eTileNum = -1)
    {
        x = eX;
        y = eY;
        transform.localPosition = new Vector3(x, y, 0);
        gameObject.name = x.ToString("D3") + "x" + y.ToString("D3");
        if (eTileNum == -1)
        {
            eTileNum = TileCamera.GET_MAP(x, y);
        }
        else { TileCamera.SET_MAP(x, y, eTileNum);
        }

        tileNum = eTileNum;
        GetComponent<SpriteRenderer>().sprite = TileCamera.SPRITES[tileNum];

        SetCollider();
    }

    // Arrange the collider for this tile
    void SetCollider()
    {
        // Collider info is pulled from DelverCollisions.txt
        bColl.enabled = true;
        char c = TileCamera.COLLISIONS[tileNum]; // c
        switch (c)
        {
            case 'S': // Whole
                bColl.center = Vector3.zero;
                bColl.size = Vector3.one;
                break;
            case 'W': // Top
                bColl.center = new Vector3(0, 0.25f, 0);
                bColl.size = new Vector3(1, 0.5f, 1);
                break;
            case 'A': // Left
                bColl.center = new Vector3(-0.25f, 0, 0);
                bColl.size = new Vector3(0.5f, 1, 1);
                break;
            case 'D': // Right
                bColl.center = new Vector3(0.25f, 0, 0);
                bColl.size = new Vector3(0.5f, 1, 1);
                break;

            case 'Q': // Top, Left
                bColl.center = new Vector3(-0.25f, 0.25f, 0);
                bColl.size = new Vector3(0.5f, 0.5f, 1);
                break;
            case 'E': // Top, Right
                bColl.center = new Vector3(0.25f, 0.25f, 0);
                bColl.size = new Vector3(0.5f, 0.5f, 1);
                break;
            case 'Z': // Bottom, left
                bColl.center = new Vector3(-0.25f, -0.25f, 0);
                bColl.size = new Vector3(0.5f, 0.5f, 1);
                break;
            case 'X': // Bottom
                bColl.center = new Vector3(0, -0.25f, 0);
                bColl.size = new Vector3(1, 0.5f, 1);
                break;
            case 'C': // Bottom, Right
                bColl.center = new Vector3(0.25f, -0.25f, 0);
                bColl.size = new Vector3(0.5f, 0.5f, 1);
                break;
            // ^^^^^^^^-------- These are optional --------


            default: // Anything else: _, |, etc. // e
                bColl.enabled = false;
                break;
        }
    }
}


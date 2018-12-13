using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFacingMover
{ // a
    int GetFacing(); // b
    bool moving { get; } // c
    float GetSpeed();
    float gridMult { get; } // d
    Vector2 roomPos { get; set; } // e
    Vector2 roomNum { get; set; }
    Vector2 GetRoomPosOnGrid(float mult = -1); // f
}




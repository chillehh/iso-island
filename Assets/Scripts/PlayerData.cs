using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public static PlayerData instance;
    public Vector3Int position;

    private void Awake() {
        if (instance == null) { instance = this; }
    }
}

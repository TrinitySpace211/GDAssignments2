using System.Collections.Generic;
using UnityEngine;

public enum TileType {
    None = 0,
    Room = 1,
    Corridor = 2,
}

public enum CornerType {
    None,
    Pillar,
}

public class DungeonNode {
    public RectInt area;

    public DungeonNode left;
    public DungeonNode right;

    public RectInt room;

    public List<RectInt> corridors = new();

    public DungeonNode(RectInt area) {
        this.area = area;
    }

    public bool IsLeaf() {
        return left == null && right == null;
    }
}

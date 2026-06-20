using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "ItemSO", menuName = "Items/ItemSO")]
public class ItemSO : ScriptableObject {
    public enum Direction {
        Down,
        Left,
        Up,
        Right,
    }

    public string nameString;
    public Transform model;
    public Transform icon;
    public Transform drop;

    public float maxStack;

    public int width;
    public int height;

    public static Direction GetNextDirection(Direction direction) {
        switch (direction) {
            default:
            case Direction.Down: return Direction.Left;
            case Direction.Left: return Direction.Up;
            case Direction.Up: return Direction.Right;
            case Direction.Right: return Direction.Down;
        }
    }

    public static Vector2Int GetDirectionForwardVector(Direction direction) {
        switch (direction) {
            default:
            case Direction.Down: return Vector2Int.down;
            case Direction.Left: return Vector2Int.left;
            case Direction.Up: return Vector2Int.up;
            case Direction.Right: return Vector2Int.right;
        }
    }

    public static Direction GetDirection(Vector2Int from, Vector2Int to) {
        if (from.x < to.x) {
            return Direction.Right;
        } else {
            if (from.x > to.x) {
                return Direction.Left;
            } else {
                if (from.y < to.y) {
                    return Direction.Up;
                } else {
                    return Direction.Down;
                }
            }
        }
    }

    public int GetRotationAngle(Direction direction) {
        switch (direction) {
            default:
            case Direction.Down: return 0;
            case Direction.Left: return 90;
            case Direction.Up: return 180;
            case Direction.Right: return 270;
        }
    }

    public Vector2Int GetRotationOffset(Direction direction) {
        switch (direction) {
            default:
            case Direction.Down: return new Vector2Int(0, 0);
            case Direction.Left: return new Vector2Int(0, width);
            case Direction.Up: return new Vector2Int(width, height);
            case Direction.Right: return new Vector2Int(height, 0);
        }
    }

    public List<Vector2Int> GetGridPositionList(Vector2Int offset, Direction dir) {
        List<Vector2Int> gridPositionList = new List<Vector2Int>();
        switch (dir) {
            default:
            case Direction.Down:
            case Direction.Up:
                for (int x = 0; x < width; x++) {
                    for (int y = 0; y < height; y++) {
                        gridPositionList.Add(offset + new Vector2Int(x, y));
                    }
                }
                break;
            case Direction.Left:
            case Direction.Right:
                for (int x = 0; x < height; x++) {
                    for (int y = 0; y < width; y++) {
                        gridPositionList.Add(offset + new Vector2Int(x, y));
                    }
                }
                break;
        }
        return gridPositionList;
    }


}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AutoSolver
{
    private static Queue<char[]> _autoMoves = new Queue<char[]>();

    public static int CalculateTotalMoves(int disks)
    {
        float totalMoves = Mathf.Pow(2, disks) - 1;
        return (int)totalMoves;
    }

    public static void GenerateAutoMoves(int disks)
    {
        _autoMoves.Clear();
        GameSolver(disks, 'A', 'C', 'B');

        Debug.Log("Moves Generated: " + _autoMoves.Count);
    }

    public static int GetAutoMovesCount()
    {
        return _autoMoves.Count;
    }

    public static char[] DequeueAutoMoves()
    {
        return _autoMoves.Dequeue();
    }

    private static void GameSolver(int disks, char towerA, char towerC, char towerB)
    {
        if (disks == 1)
        {
            char[] moves = { towerA, towerC };
            _autoMoves.Enqueue(moves);

            return;
        }

        GameSolver(disks - 1, towerA, towerB, towerC);

        char[] moves1 = { towerA, towerC };
        _autoMoves.Enqueue(moves1);

        GameSolver(disks - 1, towerB, towerC, towerA);
    }
}

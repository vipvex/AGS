using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class Pathfinding : MonoBehaviour
{
    /*
    public static int gridSize;


    public static List<Hex> FindPath(Hex startHex, Hex targetHex)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();

        if (startHex == null || targetHex == null || startHex == targetHex)
        {
            return null;
        }

        Heap<Hex> openSet = new Heap<Hex>(gridSize);
        HashSet<Hex> closedSet = new HashSet<Hex>();
        openSet.Add(startHex);

        while (openSet.Count > 0)
        {
            Hex currentHex = openSet.RemoveFirst();
            closedSet.Add(currentHex);

            if (currentHex == targetHex)
            {
                sw.Stop();
                //print("Path Found: " + sw.ElapsedMilliseconds + " ms");
                return RetracePath(startHex, targetHex);
            }

            foreach (Hex neighbour in currentHex.neighbors)
            {
                if (closedSet.Contains(neighbour))
                {
                    continue;
                }

                int newMovementCostToNeighbour = currentHex.gCost + (int)Vector3.Distance(neighbour.worldPos, targetHex.worldPos);
                if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = (int)Vector3.Distance(neighbour.worldPos, targetHex.worldPos); // might break things
                    neighbour.parent = currentHex;

                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                    else
                        openSet.UpdateItem(neighbour);
                }
            }
        }

        sw.Stop();
        print("Path Found: " + sw.ElapsedMilliseconds + " ms");
        return null;
    }


    public static List<Hex> RetracePath(Hex startHex, Hex endHex)
    {
        List<Hex> path = new List<Hex>();
        Hex currentHex = endHex;

        while (currentHex != startHex)
        {
            path.Add(currentHex);
            currentHex = currentHex.parent;
        }
        path.Reverse();

        return path;
    }


    public static List<Hex> GetWalkablePath(Hex startHex, int range)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();

        if (startHex == null || range <= 1)
        {
            UnityEngine.Debug.Log("Invalid Path");
            return null;
        }

        Heap<Hex> openSet = new Heap<Hex>(gridSize);
        HashSet<Hex> closedSet = new HashSet<Hex>();
        openSet.Add(startHex);

        while (openSet.Count > 0)
        {
            Hex currentHex = openSet.RemoveFirst();
            closedSet.Add(currentHex);

            foreach (Hex neighbour in currentHex.neighbors)
            {
                if (closedSet.Contains(neighbour))
                {
                    continue;
                }

                // if the new cost is less than the old cost replace
                int newMovementCostToNeighbour = currentHex.gCost + (int)Vector3.Distance(neighbour.worldPos, startHex.worldPos);
                if (newMovementCostToNeighbour > range || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = (int)Vector3.Distance(neighbour.worldPos, startHex.worldPos); // might break things
                    neighbour.parent = currentHex;

                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                    else
                        openSet.UpdateItem(neighbour);
                }
            }
        }

        List<Hex> area = new List<Hex>();
        for (int i = 0; i < openSet.items.Length; i++)
        {
            area.Add(openSet.items[i]);
        }

        sw.Stop();
        print("Walable area found in: " + sw.ElapsedMilliseconds + " ms");

        return area;

    }
    */
}

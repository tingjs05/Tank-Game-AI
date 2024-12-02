using System.Collections.Generic;
using UnityEngine;

namespace AI.ObstacleDetection
{
    public class Direction
    {
        private Vector3[] _directions;
        public Vector3[] directions
        {
            get { return _directions; }
        }

        public Direction()
        {
            // calculate all directions
            float[] availableValues = new float[] { -1f, 0f, 1f };
            List<Vector3> directionsList = new List<Vector3>();

            for (int x = 0; x < availableValues.Length; x++)
            {
                for (int y = 0; y < availableValues.Length; y++)
                {
                    for (int z = 0; z < availableValues.Length; z++)
                    {
                        directionsList.Add(new Vector3(availableValues[x], availableValues[y], availableValues[z]).normalized);
                    }
                }
            }

            // do not include (0, 0, 0) as a direction
            directionsList.Remove(Vector3.zero);
            // convert to array
            _directions = directionsList.ToArray();
        }
    }
}

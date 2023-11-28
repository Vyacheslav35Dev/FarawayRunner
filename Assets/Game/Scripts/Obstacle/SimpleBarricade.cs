using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class SimpleBarricade : Obstacle
{
    private const int k_MinObstacleCount = 1;
    private const int k_MaxObstacleCount = 2;
    private const int k_LeftMostLaneIndex = -1;
    private const int k_RightMostLaneIndex = 1;

    public override async UniTask Spawn(TrackSegment segment, float t)
    {
        int count = Random.Range(k_MinObstacleCount, k_MaxObstacleCount + 1);
        int startLane = Random.Range(k_LeftMostLaneIndex, k_RightMostLaneIndex + 1);

        Vector3 position;
        Quaternion rotation;
        segment.GetPointAt(t, out position, out rotation);

        for(int i = 0; i < count; ++i)
        {
            int lane = startLane + i;
            lane = lane > k_RightMostLaneIndex ? k_LeftMostLaneIndex : lane;

            
            var obj = await Addressables.InstantiateAsync(gameObject.name, position, rotation);
            
            if (obj == null)
                Debug.Log(gameObject.name);
            else
            {
                obj.transform.position += obj.transform.right * lane * segment.manager.laneOffset;
                obj.transform.SetParent(segment.objectRoot, true);
                
                Vector3 oldPos = obj.transform.position;
                obj.transform.position += Vector3.back;
                obj.transform.position = oldPos;
            }
        }
    }
}

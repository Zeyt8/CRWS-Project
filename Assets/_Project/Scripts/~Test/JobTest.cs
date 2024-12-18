using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using UnityEngine.Jobs;
using Unity.Mathematics;

public class JobTest : MonoBehaviour
{
    int dataCount = (int)1e6;

    void Start()
    {
        var a = new int3[dataCount];
        var time = Time.realtimeSinceStartup;
        for (int i = 0; i < dataCount; ++i) a[i] = new int3(i, i, i);
        Debug.Log("顺序直接赋值" + dataCount + "个用时" + (Time.realtimeSinceStartup - time) + "秒");

        var b = new NativeArray<int3>(dataCount, Allocator.TempJob);
        JobHandle orderHandle = new CountInOrder() { data = b }.Schedule(dataCount, 64);
        time = Time.realtimeSinceStartup;
        orderHandle.Complete();
        Debug.Log("并行直接赋值" + dataCount + "个用时" + (Time.realtimeSinceStartup - time) + "秒");
    }

    [BurstCompile]
    struct CountInOrder : IJobParallelFor
    {
        public NativeArray<int3> data;

        public void Execute(int i)
        {
            data[i] = new int3(i, i, i);
        }
    }
}

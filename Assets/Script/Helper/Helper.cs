using UnityEngine;

public static class Helper
{
    public static class Vector3Converter
    {
        public static Vector3 ToUnityVector3(System.Numerics.Vector3 systemVector)
        {
            return new Vector3(systemVector.X, systemVector.Y, systemVector.Z);
        }

        public static System.Numerics.Vector3 ToSystemVector3(UnityEngine.Vector3 unityVector)
        {
            return new System.Numerics.Vector3(unityVector.x, unityVector.y, unityVector.z);
        }
    }
}

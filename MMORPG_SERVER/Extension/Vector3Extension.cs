using global::System.Numerics;

namespace MMORPG_SERVER.Extension
{
    public static class Vector3Extensions
    {
        /// <summary>
        /// 计算向量的长度（magnitude）
        /// </summary>
        public static float Magnitude(this Vector3 vector)
        {
            return MathF.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);
        }
    }
}

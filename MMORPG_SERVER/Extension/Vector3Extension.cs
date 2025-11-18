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

        /// <summary>
        /// 计算向量的平方长度（性能更好，适用于比较）
        /// </summary>
        public static float SqrMagnitude(this Vector3 vector)
        {
            return vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z;
        }

        /// <summary>
        /// 向量归一化
        /// </summary>
        public static Vector3 Normalized(this Vector3 vector)
        {
            float magnitude = vector.Magnitude();
            if (magnitude > 1E-05f) // 避免除以0
                return vector / magnitude;
            return Vector3.Zero;
        }

        /// <summary>
        /// 计算两个向量之间的角度（度数）
        /// </summary>
        public static float Angle(this Vector3 from, float target)
        {
            // 公式：θ = arccos((a·b) / (|a||b|))
            var to = new Vector3(0, target, 0);
            float denominator = MathF.Sqrt(from.SqrMagnitude() * to.SqrMagnitude());
            if (denominator < 1E-15f) // 避免除以0
                return 0f;

            float dot = Vector3.Dot(from, to) / denominator;
            dot = Math.Clamp(dot, -1f, 1f); // 确保在有效范围内
            return MathF.Acos(dot) * (180f / MathF.PI); // 弧度转角度
        }

        /// <summary>
        /// 计算两点之间的距离
        /// </summary>
        public static float Distance(this Vector3 a, Vector3 b)
        {
            return (a - b).Magnitude();
        }
    }
}

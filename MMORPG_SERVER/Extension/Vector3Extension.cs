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
        /// 判断目标是否在观察者的指定水平角度范围内
        /// </summary>
        /// <param name="targetRelativeVec">目标相对于观察者的位置向量（目标位置 - 观察者位置）</param>
        /// <param name="observerRotationY">观察者Y轴旋转角（角度制，0~360°，遵循右手坐标系：0°=Z轴正前，顺时针Y值递增）</param>
        /// <param name="angleLimit">总攻击/视野角度（如60°、90°，必须>0且≤180°）</param>
        /// <returns>是否在指定角度范围内</returns>
        /// <exception cref="ArgumentOutOfRangeException">角度限制非法时抛出</exception>
        public static bool IsInAngleRange(
            Vector3 targetRelativeVec,
            float observerRotationY,
            float angleLimit)
        {
            // 1. 非法角度限制校验
            if (angleLimit <= 0f || angleLimit > 200f)
                throw new ArgumentOutOfRangeException(nameof(angleLimit), "角度限制必须大于0且不超过200°");

            // 2. 处理目标与观察者重合的情况（相对向量长度接近0）
            float horizontalDistance = (float)Math.Sqrt(
                targetRelativeVec.X * targetRelativeVec.X +
                targetRelativeVec.Z * targetRelativeVec.Z
            );
            if (horizontalDistance < 1e-10f)
                return true;

            // 3. 计算目标的水平单位向量（忽略Y轴，归一化）
            Vector3 horizontalDir = new Vector3(
                targetRelativeVec.X / horizontalDistance,
                0f,
                targetRelativeVec.Z / horizontalDistance
            );

            // 4. 计算观察者的水平正前方向量（基于Y轴旋转角）
            double yawRadians = observerRotationY * Math.PI / 180.0; // 角度→弧度（原代码错误：60.0改为180.0）
            Vector3 observerForward = new Vector3(
                (float)Math.Sin(yawRadians),
                0f,
                (float)Math.Cos(yawRadians)
            );
            // 理论上sin²+cos²=1，无需归一化，保险起见处理极小值
            float forwardLength = (float)Math.Sqrt(observerForward.X * observerForward.X + observerForward.Z * observerForward.Z);
            if (forwardLength > 1e-10f)
            {
                observerForward = new Vector3(
                    observerForward.X / forwardLength,
                    0f,
                    observerForward.Z / forwardLength
                );
            }

            // 5. 点积计算夹角：cos(θ) = 单位向量点积，θ为两向量夹角
            float dotProduct = horizontalDir.X * observerForward.X + horizontalDir.Z * observerForward.Z;

            // 6. 计算角度限制的半角余弦值（总角度的一半，如60°→30°）
            double halfAngleRadians = angleLimit * Math.PI / 360.0;
            float minDotProduct = (float)Math.Cos(halfAngleRadians);

            // 7. 点积≥半角余弦值 → 夹角≤半角 → 在总角度范围内
            return dotProduct >= minDotProduct - 1e-6f; // 减1e-6f处理浮点精度误差
        }

        //根据旋转Y值计算实体前方方向向量
        public static Vector3 CalculateForwardDirection(float yawAngle)
        {
            float rad = (float)(yawAngle * Math.PI / 180.0); // 度转弧度
            float x = (float)Math.Sin(rad);
            float z = (float)Math.Cos(rad);
            float y = 0f;

            // 自定义归一化（若需）
            float magnitude = (float)Math.Sqrt(x * x + y * y + z * z);
            Vector3 forwardDir = new Vector3(x / magnitude, y / magnitude, z / magnitude);

            return forwardDir;
        }

        /// <summary>
        /// 检测单个敌人是否在技能区域内
        /// </summary>
        /// <param name="playerPos">玩家位置（x/z为平面坐标）</param>
        /// <param name="playerYRot">玩家旋转Y值（度）</param>
        /// <param name="enemyPos">敌人位置（x/z为平面坐标）</param>
        /// <returns>是否在区域内</returns>
        public static bool IsEnemyInSkillArea(Vector3 playerPos, float playerYRot, Vector3 enemyPos)
        {
            // 1. 计算玩家前方向量（平面向量，忽略Y轴）
            // 旋转Y值转换为弧度
            float rotRadian = playerYRot * (float)Math.PI / 180f;
            // 正Z轴为0度，顺时针旋转Y角后的前方向量（x=sinθ，z=cosθ，右手坐标系）
            Vector3 playerForward = new Vector3(
                (float)Math.Sin(rotRadian),
                0,
                (float)Math.Cos(rotRadian)
            );
            // 归一化前方向量（避免旋转计算误差导致长度不为1）
            float forwardLength = playerForward.Length();
            if (forwardLength > 0.0001f)
            {
                playerForward.X /= forwardLength;
                playerForward.Z /= forwardLength;
            }

            // 2. 计算敌人相对于玩家的平面向量（忽略Y轴高度差）
            Vector3 enemyRelative = new Vector3(
                enemyPos.X - playerPos.X,
                0,
                enemyPos.Z - playerPos.Z
            );

            // 3. 先判断敌人到玩家的平面距离是否超过15米（平方比较，优化性能）
            float distanceSquared = enemyRelative.LengthSquared();
            if (distanceSquared > 15 * 15)
            {
                return false;
            }

            // 4. 计算敌人相对向量在玩家前方向量上的投影长度（即“前方距离”）
            float forwardDot = Vector3.Dot(enemyRelative, playerForward);
            // 投影长度需>0（在前方）且<=15米（在长度范围内）
            if (forwardDot <= 0 || forwardDot > 15)
            {
                return false;
            }

            // 5. 计算敌人相对向量在垂直于玩家朝向方向上的分量长度（即“左右偏移”）
            // 垂直向量：玩家前方向量逆时针转90度（x=-z，z=x）
            Vector3 playerRight = new Vector3(-playerForward.Z, 0, playerForward.X);
            float rightDot = Vector3.Dot(enemyRelative, playerRight);
            // 绝对值需<=2.5米（在宽度范围内）
            if (Math.Abs(rightDot) > 2.5)
            {
                return false;
            }

            // 所有条件满足，敌人在技能区域内
            return true;
        }

        public static Vector3 ToVector3(this int[] pos)
        {
            return new Vector3()
            {
                X = pos[0],
                Y = pos[1],
                Z = pos[2]
            };
        }
    }
}

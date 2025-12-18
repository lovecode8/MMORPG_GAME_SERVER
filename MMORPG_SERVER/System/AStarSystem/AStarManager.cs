using MMORPG_SERVER.Tool;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static Mysqlx.Expect.Open.Types.Condition.Types;

namespace MMORPG_SERVER.System.AStarSystem
{
    public class AStarManager : Singleton<AStarManager>
    {
        private AStarManager() { }

        //所有三角形
        private List<Triangle> _allTriangles = new();

        //三角形字典 -- Key为边，value为具体的三角形列表，用于根据已知的边查找三角形
        private Dictionary<string, List<int>> _edgeToTriangle = new();

        // 采样配置（可根据场景调整）
        private readonly float _searchRadius = 50f; // 最大搜索半径（米）
        private readonly int _sampleStep = 10; // 每层采样步数
        private readonly int _sampleLayer = 10; // 采样层数（半径分层）

        public void Start()
        {
            LoadNavMeshData();
            BuildEdgeToTriangle();
        }

        //加载NavMesh数据
        private void LoadNavMeshData()
        {
            string path = "D:\\game\\MMORPG_GAME_SERVER\\MMORPG_SERVER\\Data\\Json\\NavMeshData.json";
            string context = File.ReadAllText(path);
            _allTriangles = JsonConvert.DeserializeObject<NavMeshDataRoot>(context).Triangles;
            Log.Information($"[AstarManager] 成功加载：共{_allTriangles.Count}个三角形");
        }

        //构建边对应三角形字典
        private void BuildEdgeToTriangle()
        {
            //遍历所有三角形
            for (int i = 0; i < _allTriangles.Count; i++)
            {
                var triangle = _allTriangles[i];
                AddEdgeToTriangle(triangle.Vertices[0], triangle.Vertices[1], i);
                AddEdgeToTriangle(triangle.Vertices[1], triangle.Vertices[2], i);
                AddEdgeToTriangle(triangle.Vertices[0], triangle.Vertices[2], i);
            }
        }

        //增加边对应三角形数据
        private void AddEdgeToTriangle(Vector3 point1, Vector3 point2, int triangleIndex)
        {
            //获取一条表（即两个点）的唯一索引Key
            var key = GetEdgeKey(point1, point2);

            if (!_edgeToTriangle.ContainsKey(key))
            {
                _edgeToTriangle[key] = new();
            }

            if (!_edgeToTriangle[key].Contains(triangleIndex))
            {
                _edgeToTriangle[key].Add(triangleIndex);
            }
        }

        /// <summary>
        /// 核心A*算法
        /// </summary>
        /// <param name="startPos">起始点位置</param>
        /// <param name="endPos">终点位置</param>
        /// <returns></returns>
        public async Task<List<Vector3>> GetAStarPath(Vector3 startPos, Vector3 endPos)
        {
            //获取起始点和终点所在的三角形索引
            int startIndex = GetTriangleIndexByPos(startPos);
            int endIndex = GetTriangleIndexByPos(endPos);

            //如果找不到起点或终点所在的三角形，则找距离它最近的三角形
            if (startIndex == -1 || endIndex == -1)
            {
                if(startIndex == -1)
                {
                    startIndex = GetTriangleIndexByPos(FindNearestReachablePoint(startPos));
                    //Log.Information($"起点不可到达：{startPos}");
                }
                if(endIndex == -1)
                {
                    endIndex = GetTriangleIndexByPos(FindNearestReachablePoint(endPos));
                    //Log.Information($"终点不可到达：{endPos}");
                }
            }

            //如果起点和终点重合，直接返回
            if (startIndex == endIndex)
            {
                return new() { startPos, endPos };
            }

            //A*算法用到的数据结构

            //从起点到该点的消耗字典
            Dictionary<int, float> gCost = new Dictionary<int, float>();
            //从该点到终点的消耗字典
            Dictionary<int, float> hCost = new Dictionary<int, float>();
            //父节点字典
            Dictionary<int, int> parentDict = new Dictionary<int, int>();
            //关闭列表--已遍历节点表
            HashSet<int> closeSet = new HashSet<int>();
            //开发列表--待遍历节点表
            PriorityQueue<int, float> openSet = new PriorityQueue<int, float>();

            //加入起点
            gCost[startIndex] = 0;
            hCost[startIndex] = GetHCost(startPos, endPos);
            openSet.Enqueue(startIndex, gCost[startIndex] + hCost[startIndex]);

            //异步计算路径点
            var ans = await Task.Run(() =>
            {
                while (openSet.Count > 0)
                {
                    //获取当前点所在三角形的索引
                    int currentIndex = openSet.Dequeue();

                    //当前点索引等于字典索引--找到路径，构建路径点后平滑
                    if (currentIndex == endIndex)
                    {
                        var ans = SmoothPath
                            (ReConstructPath(parentDict, startIndex, endIndex), startPos, endPos);
                        return ans;
                    }

                    //当前点已经计算，加入关闭列表，避免重复计算
                    closeSet.Add(currentIndex);

                    //获取当前点的所有邻接点
                    var neighborTriangleIndexList = GetNeighborTriangle(currentIndex);

                    //遍历所有邻接点，计算gCost和hCost
                    foreach (var neighborIndex in neighborTriangleIndexList)
                    {
                        if (closeSet.Contains(neighborIndex)) continue;

                        //计算该邻接点所在三角形的中心位置
                        Vector3 neighborCenter = _allTriangles[neighborIndex].Center;

                        //计算邻接点的gCost
                        //gCost是起点到邻接点的花费，也就是起点到当前点加上当前点到邻接点的花费
                        float neighborGCost = gCost[currentIndex] +
                            Vector3.Distance(_allTriangles[currentIndex].Center, neighborCenter);

                        //如果邻接点未被计算过或者花费更小，则更新数据
                        if (!gCost.ContainsKey(neighborIndex) || neighborGCost < gCost[neighborIndex])
                        {
                            gCost[neighborIndex] = neighborGCost;
                            hCost[neighborIndex] = GetHCost(neighborCenter, _allTriangles[endIndex].Center);
                            parentDict[neighborIndex] = currentIndex;

                            openSet.Enqueue(neighborIndex, gCost[neighborIndex] + hCost[neighborIndex]);
                        }
                    }
                }
                Log.Information("找不到路径");
                return null;
            });
            return ans;
        }

        /// <summary>
        /// 获取点所在的三角形
        /// </summary>
        /// <param name="point">点的位置信息</param>
        /// <returns></returns>
        public int GetTriangleIndexByPos(Vector3 point)
        {
            for (int i = 0; i < _allTriangles.Count; i++)
            {
                if (isPointInTriangle(point, _allTriangles[i].Vertices))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// 判断一个点是否在三角形中
        /// </summary>
        /// <param name="point">点的位置信息</param>
        /// <param name="vertices">三角形的三条边位置信息</param>
        /// <returns></returns>
        private bool isPointInTriangle(Vector3 point, Vector3[] vertices)
        {
            if (vertices == null || vertices.Length != 3)
                return false;

            Vector2 p = new Vector2(point.X, point.Z);
            Vector2 v0 = new Vector2(vertices[0].X, vertices[0].Z);
            Vector2 v1 = new Vector2(vertices[1].X, vertices[1].Z);
            Vector2 v2 = new Vector2(vertices[2].X, vertices[2].Z);

            float s1 = v2.Y - v0.Y;
            float s2 = v2.X - v0.X;
            float s3 = v1.Y - v0.Y;
            float s4 = p.Y - v0.Y;

            // 分母计算 + 非零保护（避免除以0）
            float denominator = s3 * s2 - (v1.X - v0.X) * s1;
            if (Math.Abs(denominator) < 0.0001f)
                return false; // 三角形退化为线，无意义

            float w1 = (v0.X * s1 + s4 * s2 - p.X * s1) / denominator;
            float w2 = (s4 - w1 * s3) / s1;

            // 误差容忍范围（0.1，适配大型游戏场景）
            const float tolerance = 0.1f;
            return (w1 >= -tolerance && w2 >= -tolerance && (w1 + w2) <= 1 + tolerance);
        }

        /// <summary>
        /// 获取该点到终点的消耗值（即该点和终点的直线距离）
        /// </summary>
        /// <param name="current">当前节点位置</param>
        /// <param name="end">终点位置</param>
        /// <returns></returns>
        private float GetHCost(Vector3 current, Vector3 end)
        {
            return Vector3.Distance(current, end);
        }

        /// <summary>
        /// 回溯路径
        /// </summary>
        /// <param name="parentDict">父节点字典</param>
        /// <param name="startIndex">起始点索引</param>
        /// <param name="endIndex">终点索引</param>
        /// <returns></returns>
        private List<Vector3> ReConstructPath(Dictionary<int, int> parentDict, int startIndex, int endIndex)
        {
            List<Vector3> ans = new List<Vector3>();
            int currentIndex = endIndex;

            while (currentIndex != startIndex)
            {
                ans.Add(_allTriangles[currentIndex].Center);
                currentIndex = parentDict[currentIndex];
            }

            ans.Add(_allTriangles[startIndex].Center);
            ans.Reverse();

            return ans;
        }

        /// <summary>
        /// 查找指定点附近最近的可达点（在NavMesh三角形内）
        /// </summary>
        /// <param name="originPos">原始不可达点</param>
        /// <returns>最近的可达点（无则返回原始点）</returns>
        private Vector3 FindNearestReachablePoint(Vector3 originPos)
        {
            // 存储采样点和距离，用于找最近点
            Dictionary<Vector3, float> samplePoints = new Dictionary<Vector3, float>();

            // 分层采样（从近到远，提高效率）
            for (int layer = 1; layer <= _sampleLayer; layer++)
            {
                float currentRadius = _searchRadius * (layer / (float)_sampleLayer);
                float stepAngle = 360f / _sampleStep;

                // 1. 平面（X/Z）圆形采样
                for (int step = 0; step < _sampleStep; step++)
                {
                    float angle = step * stepAngle * (float)Math.PI / 180f;
                    float offsetX = currentRadius * (float)Math.Cos(angle);
                    float offsetZ = currentRadius * (float)Math.Sin(angle);

                    // 生成采样点（固定Y轴为原始点Y，或适配三角形Y）
                    Vector3 samplePos = new Vector3(
                        originPos.X + offsetX,
                        originPos.Y,
                        originPos.Z + offsetZ
                    );

                    // 检查是否可达
                    if (GetTriangleIndexByPos(samplePos) != -1)
                    {
                        float distance = Vector3.Distance(originPos, samplePos);
                        samplePoints[samplePos] = distance;
                    }

                    // 2. Y轴上下采样（适配地形高度）
                    Vector3 samplePosYUp = samplePos with { Y = originPos.Y + 0.5f * layer };
                    Vector3 samplePosYDown = samplePos with { Y = originPos.Y - 0.5f * layer };
                    if (GetTriangleIndexByPos(samplePosYUp) != -1)
                    {
                        float distance = Vector3.Distance(originPos, samplePosYUp);
                        samplePoints[samplePosYUp] = distance;
                    }
                    if (GetTriangleIndexByPos(samplePosYDown) != -1)
                    {
                        float distance = Vector3.Distance(originPos, samplePosYDown);
                        samplePoints[samplePosYDown] = distance;
                    }
                }

                // 3. 十字形采样（上下左右）
                Vector3[] crossOffsets = new[]
                {
                    new Vector3(currentRadius, 0, 0),   // X+
                    new Vector3(-currentRadius, 0, 0),  // X-
                    new Vector3(0, 0, currentRadius),   // Z+
                    new Vector3(0, 0, -currentRadius)   // Z-
                };
                foreach (var offset in crossOffsets)
                {
                    Vector3 samplePos = originPos + offset;
                    if (GetTriangleIndexByPos(samplePos) != -1)
                    {
                        float distance = Vector3.Distance(originPos, samplePos);
                        samplePoints[samplePos] = distance;
                    }
                }

                // 找到当前层的最近点，直接返回（无需继续采样）
                if (samplePoints.Count > 0)
                {
                    break;
                }
            }

            // 无采样点则返回原始点，否则返回最近点
            if (samplePoints.Count == 0)
            {
                return originPos;
            }
            return samplePoints.OrderBy(kv => kv.Value).First().Key;
        }

        //获取指定节点三角形的相邻三角形的索引值
        private List<int> GetNeighborTriangle(int currentIndex)
        {
            HashSet<int> triangles = new HashSet<int>();
            var vertices = _allTriangles[currentIndex].Vertices;
            foreach (var key in new[]
            {
                GetEdgeKey(vertices[0], vertices[1]),
                GetEdgeKey(vertices[0], vertices[2]),
                GetEdgeKey(vertices[1], vertices[2])
            })
            {
                if (_edgeToTriangle.TryGetValue(key, out var list))
                {
                    foreach (var tarangleIndex in list)
                    {
                        if (tarangleIndex != currentIndex)
                        {
                            triangles.Add(tarangleIndex);
                        }
                    }
                }
            }

            return new List<int>(triangles);
        }

        //根据三角形顶点信息获取Key
        private string GetEdgeKey(Vector3 point1, Vector3 point2)
        {
            var p1_x = Math.Round(point1.X, 2);
            var p1_y = Math.Round(point1.Y, 2);
            var p1_z = Math.Round(point1.Z, 2);
            var p2_x = Math.Round(point2.X, 2);
            var p2_y = Math.Round(point2.Y, 2);
            var p2_z = Math.Round(point2.Z, 2);

            if (p1_x + p1_z > p2_x + p2_z)
            {
                return $"{p1_x}_{p1_y}_{p1_z}|{p2_x}_{p2_y}_{p2_z}";
            }
            else
            {
                return $"{p2_x}_{p2_y}_{p2_z}|{p1_x}_{p1_y}_{p1_z}";
            }
        }

        /// <summary>
        /// 路径平滑方法：剔除冗余拐点，减少路径拐弯（仅保留必要节点）
        /// </summary>
        /// <param name="rawPath">A星生成的原始路径（三角形中心列表）</param>
        /// <param name="startPos">路径起点</param>
        /// <param name="endPos">路径终点</param>
        /// <returns>平滑后的路径点列表</returns>
        public List<Vector3> SmoothPath(List<Vector3> rawPath, Vector3 startPos, Vector3 endPos)
        {
            // 空值/短路径直接返回
            if (rawPath == null)
                return rawPath ?? new List<Vector3> { startPos, endPos };

            // 步骤1：替换原始路径首尾为真实起点/终点（剔除三角形中心偏差）
            List<Vector3> path = new List<Vector3> { startPos };
            path.AddRange(rawPath.Skip(1).Take(rawPath.Count - 2));
            path.Add(endPos);

            // 步骤2：核心拐点剔除（从起点开始，跳过可直线通行的中间点）
            List<Vector3> smoothPath = new List<Vector3> { path[0] };
            int lastValidIdx = 0; // 上一个保留的节点索引

            // 遍历路径，只保留"必须拐弯"的节点
            for (int i = 1; i < path.Count - 1; i++)
            {
                // 检查上一个有效节点到下一个节点是否可直线通行
                if (!IsLineWalkable(path[lastValidIdx], path[i + 1]))
                {
                    smoothPath.Add(path[i]);
                    lastValidIdx = i; // 更新上一个有效节点
                }
            }

            // 步骤3：添加终点，完成平滑
            smoothPath.Add(endPos);

            foreach(var p in smoothPath)
            {
                Log.Information(p.ToString());
            }
            return smoothPath;
        }

        /// <summary>
        /// 辅助方法：判断两点间直线是否可通行（采样验证是否全在NavMesh内）
        /// </summary>
        private bool IsLineWalkable(Vector3 start, Vector3 end)
        {
            // 两点距离过近，直接判定可通行
            if (Vector3.Distance(start, end) < 0.1f) return true;

            // 采样直线上的点（可根据场景调整采样数，8个足够平衡精度和性能）
            int sampleCount = 8;
            for (int i = 1; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                // 线性插值生成采样点
                Vector3 samplePoint = new Vector3(
                    start.X + (end.X - start.X) * t,
                    start.Y + (end.Y - start.Y) * t,
                    start.Z + (end.Z - start.Z) * t
                );
                // 采样点不在任何三角形内 → 直线不可通行
                if (GetTriangleIndexByPos(samplePoint) == -1)
                    return false;
            }
            return true;
        }
    }
}

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

        //三角形字典 -- 边的Key对应三角形索引表
        private Dictionary<string, List<int>> _edgeToTriangle = new();

        public void Start()
        {
            LoadNavMeshData();
            BuildEdgeToTriangle();
            GetAStarPath(new Vector3(24, -2.75f, 98), new Vector3(32, -2.75f, 103));
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
            for(int i = 0; i < _allTriangles.Count; i++)
            {
                var triangle = _allTriangles[i];
                AddEdgeToTriangle(triangle.Vertices[0], triangle.Vertices[1], i);
                AddEdgeToTriangle(triangle.Vertices[1], triangle.Vertices[2], i);
                AddEdgeToTriangle(triangle.Vertices[0], triangle.Vertices[2], i);
            }
        }

        //增加边对应三角形
        private void AddEdgeToTriangle(Vector3 point1, Vector3 point2, int triangleIndex)
        {
            string key;

            if(point1.X + point1.Z > point2.X + point2.Z)
            {
                key = $"{point1.X}_{point1.Y}_{point1.Z}|{point2.X}_{point2.Y}_{point2.Z}";
            }
            else
            {
                key = $"{point2.X}_{point2.Y}_{point2.Z}|{point1.X}_{point1.Y}_{point1.Z}";
            }

            if (!_edgeToTriangle.ContainsKey(key))
            {
                _edgeToTriangle[key] = new();
            }

            if (!_edgeToTriangle[key].Contains(triangleIndex))
            {
                _edgeToTriangle[key].Add(triangleIndex);
            }
        }

        //核心算法--获取路径
        public List<Vector3> GetAStarPath(Vector3 startPos, Vector3 endPos)
        {
            int startIndex = GetTriangleIndexByPos(startPos);
            int endIndex = GetTriangleIndexByPos(endPos);

            if(startIndex == -1 || endIndex == -1)
            {
                Log.Information("起点或终点不可抵达");
                return null;
            }

            if(startIndex == endIndex)
            {
                return new() { startPos, endPos };
            }

            //数据结构
            //从起点到该点的消耗
            Dictionary<int, float> gCost = new Dictionary<int, float>();
            //从该点到终点的消耗
            Dictionary<int, float> hCost = new Dictionary<int, float>();
            //父节点字典
            Dictionary<int, int> parentDict = new Dictionary<int, int>();
            //已遍历节点表
            HashSet<int> closeSet = new HashSet<int>();
            //待遍历节点表
            PriorityQueue<int, float> openSet = new PriorityQueue<int, float>();

            gCost[startIndex] = 0;
            hCost[startIndex] = GetHCost(startPos, endPos);
            openSet.Enqueue(startIndex, gCost[startIndex] + hCost[startIndex]);

            while(openSet.Count > 0)
            {
                int currentIndex = openSet.Dequeue();

                if(currentIndex == endIndex)
                {
                    return ReConstructPath(parentDict, startIndex, endIndex);
                }

                closeSet.Add(currentIndex);

                var neighborTriangleIndexList = GetNeighborTriangle(currentIndex);

                foreach(var neighborIndex in neighborTriangleIndexList)
                {
                    if (closeSet.Contains(neighborIndex)) continue;

                    Vector3 neighborCenter = _allTriangles[currentIndex].Center;

                    float neighborGCost = gCost[currentIndex] +
                        Vector3.Distance(_allTriangles[currentIndex].Center, neighborCenter);

                    if (!gCost.ContainsKey(neighborIndex) || neighborGCost < gCost[neighborIndex])
                    {
                        gCost[neighborIndex] = neighborGCost;
                        hCost[neighborIndex] = GetHCost(neighborCenter, _allTriangles[endIndex].Center) * 3;
                        parentDict[neighborIndex] = currentIndex;

                        openSet.Enqueue(neighborIndex, gCost[neighborIndex] + hCost[neighborIndex]);
                    }
                }
            }

            Log.Information("找不到路径");
            return null;
        }

        //获取点所在的三角形
        private int GetTriangleIndexByPos(Vector3 point)
        {
            for(int i = 0; i < _allTriangles.Count; i++)
            {
                if(isPointInTriangle(point, _allTriangles[i].Vertices))
                {
                    return i;
                }
            }
            return -1;
        }

        //判断点是否在三角形中
        private bool isPointInTriangle(Vector3 point, Vector3[] vertices)
        {
            // 转换为二维（X/Z）进行判断
            Vector2 p = new Vector2(point.X, point.Z);
            Vector2 v0 = new Vector2(vertices[0].X, vertices[0].Z);
            Vector2 v1 = new Vector2(vertices[1].X, vertices[1].Z);
            Vector2 v2 = new Vector2(vertices[2].X, vertices[2].Z);

            // 计算重心坐标（判断点是否在三角形内部）
            float s1 = v2.Y - v0.Y;
            float s2 = v2.X - v0.X;
            float s3 = v1.Y - v0.Y;
            float s4 = p.Y - v0.Y;

            float w1 = (v0.X * s1 + s4 * s2 - p.X * s1) / (s3 * s2 - (v1.X - v0.X) * s1);
            float w2 = (s4 - w1 * s3) / s1;

            // 重心坐标在0~1之间 → 点在三角形内
            return (w1 >= -0.01f && w2 >= -0.01f && (w1 + w2) <= 1.01f); // 浮点误差容忍
        }

        //获取该点到终点的消耗值
        private float GetHCost(Vector3 current, Vector3 end)
        {
            return Vector3.Distance(current, end);
        }

        //回溯路径
        private List<Vector3> ReConstructPath(Dictionary<int, int> parentDict, int startIndex, int endIndex)
        {
            List<Vector3> ans = new List<Vector3>();
            int currentIndex = endIndex;
            
            while(currentIndex != startIndex)
            {
                ans.Add(_allTriangles[currentIndex].Center);
                currentIndex = parentDict[currentIndex];
            }

            ans.Add(_allTriangles[startIndex].Center);
            ans.Reverse();

            Log.Information($"路径生成成功：共{ans.Count}个点");

            foreach(var pos in ans)
            {
                Log.Information(pos.ToString());
            }

            return ans;
        }

        //获取指定节点三角形的相邻三角形的索引值
        private List<int> GetNeighborTriangle(int currentIndex)
        {
            HashSet<int> triangles = new HashSet<int>();
            var vertices = _allTriangles[currentIndex].Vertices;
            foreach(var key in new[]
            {
                GetEdgeKey(vertices[0], vertices[1]),
                GetEdgeKey(vertices[0], vertices[2]),
                GetEdgeKey(vertices[1], vertices[2])
            })
            {
                if(_edgeToTriangle.TryGetValue(key, out var list))
                {
                    foreach(var tarangleIndex in list)
                    {
                        if(tarangleIndex != currentIndex)
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
            if (point1.X + point1.Z > point2.X + point2.Z)
            {
                return $"{point1.X}_{point1.Y}_{point1.Z}|{point2.X}_{point2.Y}_{point2.Z}";
            }
            else
            {
                return $"{point2.X}_{point2.Y}_{point2.Z}|{point1.X}_{point1.Y}_{point1.Z}";
            }
        }
    }
}

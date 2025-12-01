using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_SERVER.System.AStarSystem
{
    //NavMesh导出的三角形数据
    public class Triangle
    {
        public Vector3[] Vertices;

        public Vector3 Center;

        public int AreaMask;
    }

    public class NavMeshDataRoot
    {
        public List<Triangle> Triangles { get; set; }
    }
}

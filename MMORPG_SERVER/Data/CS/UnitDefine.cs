
namespace MMORPG_SERVER.Data.CS
{
    //
    // Auto Generated Code By excel2json
    // https://neil3d.gitee.io/coding/excel2json.html
    // 1. 每个 Sheet 形成一个 Struct 定义, Sheet 的名称作为 Struct 的名称
    // 2. 表格约定：第一行是变量名称，第二行是变量类型

    // Generate From D:\game\MMORPG_GAME_SERVER\MMORPG_SERVER\Data\Excel\UnitDefine.xlsx.xlsx

    public class UnitDefine
    {
        public int ID; // 角色ID
        public string Name; // 角色名称
        public string Type; // 角色类型
        public string Tool; // 角色武器
        public string Skill; // 角色技能
        public string Path; // 模型路径
        public int Hp; // 初始血量
        public int Mp; // 初始蓝量
        public int[] AttackDemage; // 三段攻击的伤害
    }


    // End of Auto Generated Code

}

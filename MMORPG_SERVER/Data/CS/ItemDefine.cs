using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_SERVER.Data.CS
{
    //
    // Auto Generated Code By excel2json
    // https://neil3d.gitee.io/coding/excel2json.html
    // 1. 每个 Sheet 形成一个 Struct 定义, Sheet 的名称作为 Struct 的名称
    // 2. 表格约定：第一行是变量名称，第二行是变量类型

    // Generate From D:\game\MMORPG_GAME_SERVER\MMORPG_SERVER\Data\Excel\ItemDefine.xlsx.xlsx

    public class ItemDefine
    {
        public int ID; // 物品id
        public string Name; // 物品名称
        public string Description; // 物品描述
        public int ItemType; // 物品类型
        public int EquipType; // 装备类型
        public int ConsumableType; // 消耗品类型
        public int Hp; // 对hp的增加值
        public int Mp; // 对mp的增加值
        public int MaxHp; // 对最大生命值的增加值
        public int MaxMp; // 对最大蓝量的增加值
        public int Atk; // 对攻击力的增加值
        public int Def; // 防御力的增加值
        public int Addition; // 对其他属性的增加值
        public string SpritePath; // 图片路径
        public bool CanbeDrop; // 是否可以被丢弃
        public int UnitId; // 对应实体id
        public int Price; //售价
        public int ItemQuality; //物品价值
    }


    // End of Auto Generated Code



}

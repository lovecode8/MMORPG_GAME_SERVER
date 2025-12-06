using Extension;
using MMORPG_SERVER.Extension;
using MMORPG_SERVER.System.EntitySystem;
using MMORPG_SERVER.System.MonsterSystem;
using MMORPG_SERVER.System.PlayerSystem;
using MMORPG_SERVER.Tool;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_SERVER.System.MapSystem
{
    //游戏场景管理器
    public class MapManager : Singleton<MapManager>
    {
        private MapManager() { }

        private Random _random = new();

        //新实体进入游戏
        public void EntityEnter(Entity entity)
        {
            Log.Information($"[MapManager] 实体进入场景：{entity._entityType} {entity._entityId}");

            //向已在场景中的玩家广播
            EntityDataList entityDatalist = new EntityDataList();
            entityDatalist.EntityDataType = EntityDataType.NewEntityEnter;
            entityDatalist.EntityDatas.Add(ConstructEntityData(entity));

            PlayerManager.Instance.Broadcast(entityDatalist, entity);

            if(entity._entityType == EntityType.Player)
            {
                //向该玩家发送场景中的实体数据
                entityDatalist.EntityDataType = EntityDataType.SyncAllEntity;
                entityDatalist.EntityDatas.Clear();

                foreach(Entity e in EntityManager.Instance.GetEntityDict().Values)
                {
                    //是自己不同步
                    if (e._entityId == entity._entityId) continue;
                    entityDatalist.EntityDatas.Add(ConstructEntityData(e));
                }

                (entity as Player)?._user._netChannel.SendAsync(entityDatalist);
            }
        }

        //实体离开游戏
        public void EntityLeave(Entity entity)
        {
            Log.Information($"[MapManager]实体{entity._entityId}离开游戏");

            EntityLeaveResponse entityLeaveResponse = new EntityLeaveResponse();
            entityLeaveResponse.EntityData = ConstructEntityData(entity);
            PlayerManager.Instance.Broadcast(entityLeaveResponse, entity);
        }

        //构造网络实体数据
        private EntityData ConstructEntityData(Entity entity)
        {
            EntityData entityData = new EntityData();
            entityData.EntityId = entity._entityId;
            entityData.UnitId = entity._unitDefine.ID;
            entityData.EntityType = (int)entity._entityType;
            entityData.Transform = new NetTransform();
            entityData.Transform.Position = new NetVector3(entity._position.ToNetVector3());
            entityData.Transform.Rotation = new NetVector3()
            {
                X = 0,
                Y = entity._rotationY,
                Z = 0
            };

            //如果是物品则随机分配旋转值，避免重叠
            if(entityData.EntityType == (int)EntityType.Item)
            {
                entityData.Transform.Rotation.X = _random.Next(200, 360);
                entityData.Transform.Rotation.Y = _random.Next(0, 200);
                entityData.Transform.Rotation.Z = _random.Next(-50, 50);
            }

            else if (entity is Player player)
            {
                entityData.Name = (entity as Player)?._user._dbUser.UserName;
                entityData.Hp = player._dbCharacter.Hp;
                entityData.Mp = player._dbCharacter.Mp;
                entityData.MaxHpAddition = player._dbCharacter.MaxHpAddition;
                entityData.MaxMpAddition = player._dbCharacter.MaxMpAddition;
                entityData.Gold = player._dbCharacter.Gold;
                entityData.Level = player._dbCharacter.Level;
            }

            else if(entity is Monster monster)
            {
                entityData.Hp = monster._hp; 
            }
                return entityData;
        }
    }
}

using MMORPG_SERVER.Database.Data;
using MMORPG_SERVER.Manager;
using MMORPG_SERVER.System.GuildSystem;
using System.Runtime.CompilerServices;

namespace MMORPG_SERVER.Extension
{
    public static class MysqlExtension
    {
        public static NetCharacter ToNetCharacter(this DbCharacter dbCharacter)
        {
            NetTransform netTransform = new NetTransform();
            netTransform.Position = new NetVector3() 
            { X = dbCharacter.posX, Y = dbCharacter.posY, Z = dbCharacter.posZ };
            netTransform.Rotation = new NetVector3()
            { X = dbCharacter.rotX, Y = dbCharacter.posY, Z = dbCharacter.posZ };

            return new NetCharacter()
            {
                UserId = dbCharacter.UserId,
                UnitId = dbCharacter.UnitId,
                Name = dbCharacter.Name,
                Hp = dbCharacter.Hp,
                Mp = dbCharacter.Mp,
                MaxHpAddition = dbCharacter.MaxHpAddition,
                MaxMpAddition = dbCharacter.MaxMpAddition,
                Transform = netTransform,
                Level = dbCharacter.Level,
                Gold = dbCharacter.Gold,
                Exp = dbCharacter.Exp,
                InteractedUnitId = dbCharacter.InteractedUnitId
            };
        }

        public static DbCharacter ToDbCharacter(this NetCharacter netCharacter)
        {
            return new DbCharacter()
            {
                UserId = netCharacter.UserId,
                UnitId = netCharacter.UnitId,
                Name = netCharacter.Name,
                Hp = netCharacter.Hp,
                Mp = netCharacter.Mp,
                Level = netCharacter.Level,
                posX = netCharacter.Transform.Position.X,
                posY = netCharacter.Transform.Position.Y,
                posZ = netCharacter.Transform.Position.Z,
                rotX = netCharacter.Transform.Rotation.X,
                rotY = netCharacter.Transform.Rotation.Y,
                rotZ = netCharacter.Transform.Rotation.Z,
                Gold = netCharacter.Gold,
                Exp = netCharacter.Exp,
                InteractedUnitId = netCharacter.InteractedUnitId
            };
        }

        //GuildInfo转DbGuild
        public static DbGuild ToDbGuild(this GuildInfo guildInfo)
        {
            return new DbGuild()
            {
                guildName = guildInfo.GuildName,
                ownerName = guildInfo.OwnerName,
                count = guildInfo.Count,
                slogan = guildInfo.GuildSlogan,
                iconIndex = guildInfo.IconIndex,
                needEnterCheck = guildInfo.NeedEnterCheck ? 1 : 0
            };
        }

        //DbGuild转Guild
        public static Guild ToGuild(this DbGuild dbGuild)
        {
            return new Guild()
            {
                guildName = dbGuild.guildName,
                ownerName = dbGuild.ownerName,
                count = dbGuild.count,
                slogan = dbGuild.slogan,
                iconIndex = dbGuild.iconIndex,
                needEnterCheck = dbGuild.needEnterCheck == 1 ? true : false,
                applicationList = new(),
                memberList = new()
            };
        }

        //Guild转DbGuild
        public static DbGuild ToDbGuild(this Guild guild)
        {
            return new DbGuild()
            {
                guildName = guild.guildName,
                ownerName = guild.ownerName,
                count = guild.count,
                slogan = guild.slogan,
                iconIndex = guild.iconIndex,
                needEnterCheck = guild.needEnterCheck? 1 : 0
            };
        }

        //DbTask转BaseTask
        public static BaseTask ToBaseTask(this DbTask dbTask)
        {
            var taskDefine = DataManager.Instance.GetTaskDefine(dbTask.taskId);

            return new BaseTask()
            {
                TaskId = dbTask.taskId,
                CurrentCount = dbTask.currentCount,
                TaskContent = taskDefine.Content,
                TargetCount = taskDefine.TargetCount
            };
        }

        //BaseTask转DBTask
        public static DbTask ToDbTask(this BaseTask baseTask, int userId)
        {
            return new DbTask()
            {
                taskId = baseTask.TaskId,
                ownerId = userId,
                currentCount = baseTask.CurrentCount
            };
        }
    }
}

using Extension;
using MMORPG_SERVER.Common.Network;
using MMORPG_SERVER.Manager;
using MMORPG_SERVER.Network;
using MMORPG_SERVER.System.EntitySystem;
using MMORPG_SERVER.System.NpcSystem;
using MMORPG_SERVER.System.TaskSystem;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_SERVER.Service
{
    //处理对话相关消息
    public class TalkService : ServiceBase<TalkService>
    {
        //开始对话请求
        public void OnHandle(object sender, TalkWithNpcRequest talkWithNpcRequest)
        {
            UpdateManager.Instance.AddTask(async () =>
            {
                var channel = sender as NetChannel;
                var npcId = talkWithNpcRequest.NpcEntityId;
                var npcEntity = EntityManager.Instance.GetEntity(npcId);
                var playerPos = channel._user._player._position;
                var response = new TalkWithNpcResponse();
                Log.Information($"收到对话请求：{channel._user._userId}");

                if(npcEntity != null && npcEntity._entityType == EntityType.NPC)
                {
                    var npcAi = (npcEntity as Npc)._npcAi;
                    //超过距离或者已经对话过的不交互
                    if(Vector3.DistanceSquared(playerPos, npcEntity._position) > 50f ||
                    channel._user._player.IsInteractedWithNpc(npcAi._npc._unitDefine.ID))
                    {
                        response.IsSuccessfulTalk = false;
                        channel.SendAsync(response);
                    }
                    else
                    {
                        await npcAi.WaitNpcWalkToPlayer(channel._user._userId, playerPos);
                        var talkDefine = DataManager.Instance.GetTalkDefine(npcEntity._unitDefine.CommunicationId);
                        response.IsSuccessfulTalk = true;
                        response.ContentList.AddRange(talkDefine.Content);
                        channel.SendAsync(response);
                        channel._user._player.AddInteractedNpc(npcAi._npc._unitDefine.ID);
                    }
                }
            });
        }

        //结束对话请求
        public void OnHandle(object sender, EndTalkRequest endTalkRequest)
        {
            UpdateManager.Instance.AddTask(() =>
            {
                var channel = sender as NetChannel;
                var userId = channel._user._userId;
                var npcId = endTalkRequest.NpcEntityId;
                Log.Information($"收到结束对话请求：{userId}结束{npcId}");
                var npc = NpcManager.Instance.GetNpc(npcId);
                var response = new EndTalkResponse();
                if(npc != null)
                {
                    //信息不对，结束失败
                    if(npc._npcAi._userId == -1 || npc._npcAi._userId != userId)
                    {
                        response.IsSuccessfulEndTalk = false;
                        channel.SendAsync(response);
                    }
                    else
                    {
                        npc._npcAi._userId = -1;
                        npc._npcAi._currentTarget = Vector3.Zero;
                        var taskList = new List<BaseTask>();
                        foreach(var taskId in npc._unitDefine.TaskId)
                        {
                            var baseTask = DataManager.Instance.GetTaskDefine(taskId).ToBaseTask();
                            taskList.Add(baseTask);
                            TaskManager.Instance.AddTask(userId, baseTask);
                        }
                        response.IsSuccessfulEndTalk = true;
                        response.TaskList.AddRange(taskList);
                        channel.SendAsync(response);

                        //更新任务（如果结束对村长的对话则更新任务）
                        if(npc._unitDefine.ID == 12)
                        {
                            TaskManager.Instance.UpdateTask(userId, 1, 1);
                        }
                    }
                }
            });
        }
    }
}

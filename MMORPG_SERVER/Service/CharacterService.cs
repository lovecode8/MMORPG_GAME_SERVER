using MMORPG_SERVER.Common.Network;
using MMORPG_SERVER.Database;
using MMORPG_SERVER.Database.Data;
using MMORPG_SERVER.Extension;
using MMORPG_SERVER.Manager;
using MMORPG_SERVER.Network;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMORPG_SERVER.Service
{
    public class CharacterService : ServiceBase<CharacterService>
    {
        //创建新角色
        public void OnHandle(object sender, CreateCharacterRequest createCharacterRequest)
        {
            UpdateManager.Instance.AddTask(() =>
            {
                Log.Information($"[CharacterService]创建新角色：{createCharacterRequest.Character.UserId}");
                NetChannel? channel = sender as NetChannel;
                if(createCharacterRequest.Character != null)
                {
                    int res = MysqlManager.Instance._freeSql.
                        Insert<DbCharacter>(createCharacterRequest.Character.ToDbCharacter()).
                        ExecuteAffrows();
                    if(res != 0)
                    {
                        Log.Information("[CharacterService]创建角色成功");
                    }
                    else
                    {
                        Log.Information("[CharacterService] 创建角色失败：数据库错误");
                    }
                }
            });
        }

        //查询用户玩家信息
        public void OnHandle(object sender, LoadCharacterRequest loadCharacterRequest)
        {
            UpdateManager.Instance.AddTask(async () =>
            {
                Log.Information($"[CharacterService]查询用户角色信息：{loadCharacterRequest.UserId}");
                NetChannel? channel = sender as NetChannel;
                var dbCharacter = await MysqlManager.Instance._freeSql.Select<DbCharacter>().
                                    Where(c => c.UserId == loadCharacterRequest.UserId).
                                    FirstAsync();

                if (dbCharacter == null)
                {
                    channel?.SendAsync(new LoadCharacterResponse() 
                                { Result = LoadCharacterResult.LoadFailed });
                    Log.Information($"[CharacterService] 查询结果：没有角色信息");
                    return;
                }

                channel?.SendAsync(new LoadCharacterResponse() 
                            {Character = dbCharacter.ToNetCharacter(), Result = LoadCharacterResult.LoadSuccess });
                Log.Information($"[CharacterService] 查询结果：有角色信息");
            });
        }
    }
}

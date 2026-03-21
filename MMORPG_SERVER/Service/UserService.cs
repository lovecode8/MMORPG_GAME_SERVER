using MMORPG_SERVER.Common.Network;
using MMORPG_SERVER.Database;
using MMORPG_SERVER.Database.Data;
using MMORPG_SERVER.Extension;
using MMORPG_SERVER.Manager;
using MMORPG_SERVER.Network;
using MMORPG_SERVER.System.UserSystem;
using Serilog;

namespace MMORPG_SERVER.Service
{
    //用户服务--处理登录注册等逻辑
    public class UserService : ServiceBase<UserService>
    {
        //处理登录请求
        public void OnHandle(object sender, LoginRequest loginRequest)
        {
            UpdateManager.Instance.AddTask(async () =>
            {
                var channel = sender as NetChannel;
                Log.Information($"[UserService] 收到玩家登录请求：userName：{loginRequest.UserName}");
                
                if(channel?._user != null)
                {
                    channel?.SendAsync(new LoginResponse() { Result = LoginResult.UserAlreadyOnLoad });
                    Log.Information("[UserService] 登录失败：账号已登录");
                    return;
                }

                else if(UserManager.Instance.GetUserByName(loginRequest.UserName) != null)
                {
                    channel?.SendAsync(new LoginResponse() { Result = LoginResult.OnLoadByOther });
                    Log.Information("[UserService] 登录失败：账号在别处登录");
                    return;
                }

                //查找用户信息
                var dbUser = await MysqlManager.Instance._freeSql.Select<DBUser>()
                    .Where(u => u.UserName == loginRequest.UserName)
                    .Where(u => u.Password == loginRequest.Password)
                    .FirstAsync();

                if(dbUser == null)
                {
                    channel?.SendAsync(new LoginResponse()
                    { 
                        Result = LoginResult.UserNameOrPasswordWrong 
                    });
                    Log.Information($"[UserService] 登录失败：用户名或密码错误");
                    return;
                }

                channel?.SendAsync(new LoginResponse() 
                { 
                    Result = LoginResult.Success, 
                    UserId = dbUser.UserId
                });

                Log.Information($"[UserService] 登录成功：{loginRequest.UserName}登录成功");

                //TODO登录成功逻辑 √
                channel?.SetUser(UserManager.Instance.NewUser(channel, dbUser));
            });
        }

        //处理注册请求
        public void OnHandle(object sender, RegisterRequest registerRequest)
        {
            UpdateManager.Instance.AddTask(async () =>
            {
                var channel = sender as NetChannel;
                Log.Information($"[UserService] 收到玩家注册请求：userName：{registerRequest.UserName}");
                
                var user = await MysqlManager.Instance._freeSql.Select<DBUser>()
                   .Where(u => u.UserName == registerRequest.UserName)
                   .FirstAsync();
                
                if(user != null)
                {
                    channel?.SendAsync(new RegisterResponse() { Result = RegisterResult.RepeatUserName });
                    Log.Information($"[UserService] 注册失败：用户名已被占用");
                    return;
                }

                //增加数据库数据
                var dbUser = new DBUser(registerRequest.UserName, registerRequest.Password);
                int insertCount = MysqlManager.Instance._freeSql.Insert<DBUser>(dbUser).ExecuteAffrows();
                
                if (insertCount <= 0)
                {
                    channel?.SendAsync(new RegisterResponse() { Result = RegisterResult.DatabaseWrong });
                    Log.Information($"[UserService] 注册失败：数据库错误");
                    return;
                }

                channel?.SendAsync(new RegisterResponse() { Result = RegisterResult.Success });
                Log.Information($"[UserService] 注册成功：{registerRequest.UserName}注册成功");

            });
        }

        //处理获取玩家设置信息的消息
        public void OnHandle(object sender, LoadSettingRequest loadSettingRequest)
        {
            UpdateManager.Instance.AddTask(() =>
            {
                var channel = sender as NetChannel;
                var userId = channel?._user._userId;
                var dbSetting = MysqlManager.Instance._freeSql.Select<DbSetting>().
                    Where(s => s.OwnerId == userId).First();
                channel?.SendAsync(new LoadSettingResponse()
                {
                    Setting = dbSetting?.ToPlayerSetting() ?? null
                });
            });
        }

        //处理保存玩家设置信息的消息
        public void OnHandle(object sender, SyncSettingRequest syncSettingRequest)
        {
            UpdateManager.Instance.AddTask(() =>
            {
                var channel = sender as NetChannel;
                int userId = channel._user._userId;
                var playerSetting = syncSettingRequest.Setting;

                var dbSetting = MysqlManager.Instance._freeSql.Select<DbSetting>().
                    Where(s => s.OwnerId == userId).First();

                if (dbSetting == null)
                {
                    MysqlManager.Instance._freeSql.Insert<DbSetting>(new DbSetting()
                    {
                        OwnerId = userId,
                        IsMusicPlay = (short)(playerSetting.IsPlayMusic ? 1 : 0),
                        IsEffectPlay = (short)(playerSetting.IsPlayEffect ? 1 : 0),
                        MusicVolume = (int)playerSetting.MusicVolume,
                        EffectVolume = (int)playerSetting.EffectVolume
                    }).ExecuteAffrows();
                }
                else
                {
                    MysqlManager.Instance._freeSql.Update<DbSetting>().
                        Where(s => s.OwnerId == userId).
                        Set(s => s.IsMusicPlay, (short)(playerSetting.IsPlayMusic ? 1 : 0)).
                        Set(s => s.IsEffectPlay, (short)(playerSetting.IsPlayEffect ? 1 : 0)).
                        Set(s => s.MusicVolume, (int)playerSetting.MusicVolume).
                        Set(s => s.EffectVolume, (int)playerSetting.EffectVolume).
                        ExecuteAffrows();
                }
            });
        }

        //处理玩家退出游戏消息
        public void OnHandle(object sender, LeaveGameRequest leaveGameRequest)
        {
            UpdateManager.Instance.AddTask(() =>
            {
                var channel = sender as NetChannel;
                channel?.Close(leaveGameRequest.IsCloseConnection);
            });
        }

        //处理玩家退出登录事件
        public void OnConnectionClosed(NetChannel sender)
        {
            UpdateManager.Instance.AddTask(() =>
            {
                if(sender._user == null)
                {
                    return;
                }

                UserManager.Instance.RemoveUser(sender._user._dbUser.UserName);
                Log.Information($"[UserService]用户连接断开：{sender._user._userId}");
            });
        }
    }
}

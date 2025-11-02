using FreeSql;
using MMORPG_SERVER.Tool;
using Serilog;


namespace MMORPG_SERVER.Database
{
    public class MysqlManager : Singleton<MysqlManager>
    {
        public IFreeSql _freeSql;

        private MysqlManager() { }

        public void Start()
        {
            _freeSql = new FreeSqlBuilder()
                .UseConnectionString(DataType.MySql,
                $"server={MysqlConfig.server};port={MysqlConfig.port};" +
                $"user={MysqlConfig.user};" +
                $"password={MysqlConfig.password};database={MysqlConfig.database}")
                .UseAutoSyncStructure(false)
                .Build();

            Log.Information("[MysqlManager] 数据库连接成功！");
        }
    }
}

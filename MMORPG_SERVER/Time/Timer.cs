

namespace MMORPG_SERVER.Time
{
    //服务器时间类
    public static class Timer
    {
        //服务器开启时间
        public static long startTime;

        //服务器运行时间
        public static float time { get; private set; }

        //上一次更新的时间
        public static long lastTime;

        //这次更新距离上次更新的时间间隔
        public static float deltaTime { get; private set; }

        //时间更新
        public static void Tick()
        {
            var nowTime = DateTimeOffset.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;

            if (startTime == 0) startTime = nowTime;
            time = (nowTime - startTime) * 0.001f;

            if (lastTime == 0) lastTime = nowTime;
            deltaTime = (nowTime - lastTime) * 0.001f;

            lastTime = nowTime;
            //Log.Information($"[TimeTick] nowTime = {nowTime}; lastTime = {lastTime}; deltaTime = {deltaTime};");
        }
    }
}

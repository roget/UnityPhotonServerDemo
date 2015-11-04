using Photon.SocketServer;
using ExitGames.Logging;
using ExitGames.Logging.Log4Net;
using log4net;
using log4net.Config;
using LogManager = ExitGames.Logging.LogManager;
using System.IO;

namespace LRServer
{
    public class LRServerApplication : ApplicationBase
    {
        protected static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        protected override PeerBase CreatePeer(InitRequest initRequest)
        {
             //建立连线并回传给Phonton Server
            return new LRServerPeer(initRequest.Protocol, initRequest.PhotonPeer);
        }

        protected override void Setup()
        {
            //初始化Game Server

            LogManager.SetLoggerFactory(Log4NetLoggerFactory.Instance);
            GlobalContext.Properties["LogFileName"] = ApplicationName + System.DateTime.Now.ToString("yyyy-MM-dd");
            XmlConfigurator.ConfigureAndWatch(new FileInfo(Path.Combine(BinaryPath, "log4net.config")));
        }

        protected override void TearDown()
        {
            //关闭Game Server并释放资源
        }
    }
}

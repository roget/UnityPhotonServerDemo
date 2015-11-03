using Photon.SocketServer;

namespace LRServer
{
    public class LRServerApplication : ApplicationBase
    {
        protected override PeerBase CreatePeer(InitRequest initRequest)
        {
             //建立连线并回传给Phonton Server
            return new LRServerPeer(initRequest.Protocol, initRequest.PhotonPeer);
        }

        protected override void Setup()
        {
            //初始化Game Server
        }

        protected override void TearDown()
        {
            //关闭Game Server并释放资源
        }
    }
}

using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using ExitGames.Concurrency.Fibers;
using LRProtocol;

namespace LRClient
{
    class LRClient : IPhotonPeerListener
    {
        public bool ServerConnected;
        public bool OnLogin;
        PhotonPeer peer;
        private readonly IFiber fiber;

        public LRClient()
        {
            ServerConnected = false;
            OnLogin = false;
            this.peer = new PhotonPeer(this, ConnectionProtocol.Udp);

            this.fiber = new PoolFiber();
            this.fiber.Start();
        }

        static void Main(string[] args)
        {
            new LRClient().Run();
        }

        public void Run()
        {
            bool isPeerConnected = peer.Connect("localhost:5055", "LRServer");
            if (isPeerConnected)
            {
                do
                {
                    peer.Service();
                    if (ServerConnected && !OnLogin)
                    {
                        Console.WriteLine("\n请输入用户名：");
                        string memberID = Console.ReadLine();
                        Console.WriteLine("\n请输入用户密码：");
                        string memberPW = Console.ReadLine();
                        var parameter = new Dictionary<byte, object>() { { (byte)LoginParameterCode.MEMBER_ID, memberID.Trim() }, { (byte)LoginParameterCode.MEMBER_PW, memberPW.Trim() } };
                        this.fiber.Enqueue(() => this.peer.OpCustom((byte)OperationCode.LOGIN, parameter, true));
                        OnLogin = true;
                    }
                    System.Threading.Thread.Sleep(500);
                }
                while (true);
            }
            else
            {
                Console.WriteLine("Unknown hostname!");
            }
        }

        public void DebugReturn(DebugLevel level, string message)
        {
            //打印回传的Debug信息
            Console.WriteLine("Debug message : " + message);
        }

        public void OnEvent(EventData eventData)
        {
            //取得Server传过来的事件
        }

        public void OnOperationResponse(OperationResponse operationResponse)
        {
            //取得Server传过来的命令回传
            switch(operationResponse.OperationCode)
            {
                case (byte)OperationCode.LOGIN:
                    {
                        switch(operationResponse.ReturnCode)
                        {
                            case (short)ErrorCode.OK:
                                {
                                    int Ret = Convert.ToInt16(operationResponse.Parameters[(byte)LoginResponseCode.RET]);
                                    string memberID = Convert.ToString(operationResponse.Parameters[(byte)LoginResponseCode.MEMBER_ID]);
                                    string memberPW = Convert.ToString(operationResponse.Parameters[(byte)LoginResponseCode.MEMBER_PW]);
                                    string Nickname = Convert.ToString(operationResponse.Parameters[(byte)LoginResponseCode.NICK_NAME]);
                                    Console.WriteLine(String.Format("Login Success \nRet={0}\nmemberID={1}\nmemberPW={2}\nNickname={3}", Ret, memberID, memberPW, Nickname));
                                }
                                break;
                            case (short)ErrorCode.INVALID_OPERATION:
                                {
                                    Console.WriteLine(operationResponse.DebugMessage);
                                    OnLogin = false;
                                }
                                break;
                            case (short)ErrorCode.INVALID_PARAMETER:
                                {
                                    Console.WriteLine(operationResponse.DebugMessage);
                                }
                                break;
                            default:
                                {
                                    Console.WriteLine(String.Format("不明的ReturnCode：{0}", operationResponse.ReturnCode));
                                }
                                break;
                        }
                    }
                    break;
                default:
                    {
                        Console.WriteLine(String.Format("不明的OperationCode：{0}", operationResponse.OperationCode));
                    }
                    break;
            }
        }

        public void OnStatusChanged(StatusCode statusCode)
        {
            //连线状态变更的通知
            Console.WriteLine("PeerStatusCallback:" + statusCode.ToString());
            switch (statusCode)
            {
                case StatusCode.Connect:
                    ServerConnected = true;
                    break;
                case StatusCode.Disconnect:
                    ServerConnected = false;
                    break;
            }
        }
    }
}

﻿using System.Collections.Generic;
using Photon.SocketServer;
using PhotonHostRuntimeInterfaces;
using ExitGames.Concurrency.Fibers;
using LRProtocol;
using ExitGames.Logging;

namespace LRServer
{
    public class LRServerPeer : PeerBase
    {
        private readonly IFiber fiber;
        protected static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        public LRServerPeer(IRpcProtocol rpcProtocol, IPhotonPeer nativePeer) : base(rpcProtocol, nativePeer)
        {
            this.fiber = new PoolFiber();
            this.fiber.Start();
        }

        protected override void OnDisconnect(PhotonHostRuntimeInterfaces.DisconnectReason reasonCode, string reasonDetail)
        {
            //失去连接时要处理的事项，例如释放资源
        }

        protected override void OnOperationRequest(OperationRequest operationRequest, SendParameters sendParameters)
        {
            //或是Debug模式将传过来的资料全部打印出来以方便体验
            if(Log.IsDebugEnabled)
            {
                Log.Debug("OnOperationRequest取得的资料Key：值");
                foreach(KeyValuePair<byte,object> item in operationRequest.Parameters)
                {
                    Log.DebugFormat(string.Format("{0},{1}", item.Key, item.Value.ToString()));
                }
            }

            //取得Client端传过来的要求并加以处理
            switch(operationRequest.OperationCode)
            {
                case (byte)OperationCode.LOGIN:
                    {
                        if (operationRequest.Parameters.Count<2)
                        {
                            OperationResponse response = new OperationResponse(operationRequest.OperationCode) { ReturnCode = (short)ErrorCode.INVALID_PARAMETER, DebugMessage = "登录失败！" };
                            this.fiber.Enqueue(() => SendOperationResponse(response, new SendParameters()));
                        }
                        else
                        {
                            string memberID = operationRequest.Parameters[(byte)LoginParameterCode.MEMBER_ID].ToString();
                            string memberPW = operationRequest.Parameters[(byte)LoginParameterCode.MEMBER_PW].ToString();
                            if (memberID == "test" && memberPW == "1234")
                            {
                                int Ret = 1;
                                var parameter = new Dictionary<byte, object> { { (byte)LoginResponseCode.RET, Ret }, { (byte)LoginResponseCode.MEMBER_ID, memberID }, { (byte)LoginResponseCode.MEMBER_PW, memberPW }, { (byte)LoginResponseCode.NICK_NAME, "LuoRui" } };
                                OperationResponse response = new OperationResponse(operationRequest.OperationCode, parameter) { ReturnCode = (short)ErrorCode.OK, DebugMessage = "登录成功！" };
                                this.fiber.Enqueue(() => SendOperationResponse(response, new SendParameters()));
                            }
                            else
                            {
                                OperationResponse response = new OperationResponse(operationRequest.OperationCode) { ReturnCode = (short)ErrorCode.INVALID_OPERATION, DebugMessage = "账号或密码错误！" };
                                this.fiber.Enqueue(() => SendOperationResponse(response, new SendParameters()));
                            }
                        }
                    }
                    break;
            }
        }
    }
}

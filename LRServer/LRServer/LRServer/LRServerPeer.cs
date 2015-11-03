using System.Collections.Generic;
using Photon.SocketServer;
using PhotonHostRuntimeInterfaces;
using ExitGames.Concurrency.Fibers;
using LRProtocol;

namespace LRServer
{
    public class LRServerPeer : PeerBase
    {
        private readonly IFiber fiber;

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
            //取得Client端传过来的要求并加以处理
            switch(operationRequest.OperationCode)
            {
                case (byte)OperationCode.LOGIN:
                    {
                        if (operationRequest.Parameters.Count<2)
                        {
                            OperationResponse response = new OperationResponse(operationRequest.OperationCode) { ReturnCode = (short)ErrorCode.INVALID_PARAMETER, DebugMessage = "Login Fail" };
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
                                OperationResponse response = new OperationResponse(operationRequest.OperationCode, parameter) { ReturnCode = (short)ErrorCode.OK, DebugMessage = "Login Success" };
                                this.fiber.Enqueue(() => SendOperationResponse(response, new SendParameters()));
                            }
                            else
                            {
                                OperationResponse response = new OperationResponse(operationRequest.OperationCode) { ReturnCode = (short)ErrorCode.INVALID_OPERATION, DebugMessage = "Wrong id or password" };
                                this.fiber.Enqueue(() => SendOperationResponse(response, new SendParameters()));
                            }
                        }
                    }
                    break;
            }
        }
    }
}

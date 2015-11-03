using UnityEngine;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using System;
using System.Security;
using LRProtocol;

public class PhotonClient : MonoBehaviour,IPhotonPeerListener
{
    public string ServerAddress = "localhost:5055";
    protected string ServerApplication = "LRServer";
    protected PhotonPeer peer;
    public bool ServerConnected;
    public string memberID = "";
    public String memberPW = "";
    public bool LoginStatus;
    public string getMemberID = "";
    public string getMemberPW = "";
    public string getNickname = "";
    public int getRet = 0;
    public string LoginResult = "";

    // Use this for initialization
    void Start()
    {
        this.ServerConnected = false;
        this.LoginStatus = false;
        this.peer = new PhotonPeer(this, ConnectionProtocol.Udp);
        this.Connect();
    }

    internal virtual void Connect()
    {
        try
        {
            this.peer.Connect(ServerAddress, ServerApplication);
        }
        catch(SecurityException se)
        {
            this.DebugReturn(0, "Connection Failed:" + se.ToString());
        }
    }

    // Update is called once per frame
    void Update()
    {
        this.peer.Service();
    }

    public void DebugReturn(DebugLevel level, string message)
    {
        Debug.Log(message);
    }

    public void OnEvent(EventData eventData)
    {
        throw new NotImplementedException();
    }

    public void OnOperationResponse(OperationResponse operationResponse)
    {
        this.DebugReturn(0, String.Format("OperationResult:{0}", operationResponse.OperationCode.ToString()));
        switch(operationResponse.OperationCode)
        {
            case (byte)OperationCode.LOGIN:
                {
                    if(operationResponse.ReturnCode==0)
                    {
                        getRet = Convert.ToInt32(operationResponse.Parameters[(byte)LoginResponseCode.RET]);
                        getMemberID = Convert.ToString(operationResponse.Parameters[(byte)LoginResponseCode.MEMBER_ID]);
                        getMemberPW = Convert.ToString(operationResponse.Parameters[(byte)LoginResponseCode.MEMBER_PW]);
                        getNickname = Convert.ToString(operationResponse.Parameters[(byte)LoginResponseCode.NICK_NAME]);
                        LoginStatus = true;
                    }
                    else
                    {
                        LoginResult = operationResponse.DebugMessage;
                        LoginStatus = false;
                    }
                }
                break;
        }
    }

    public void OnStatusChanged(StatusCode statusCode)
    {
        this.DebugReturn(0, String.Format("PeerStatusCallBack", statusCode));
        switch(statusCode)
        {
            case StatusCode.Connect:
                this.ServerConnected = true;
                break;
            case StatusCode.Disconnect:
                this.ServerConnected = false;
                break;
        }
    }

    void OnGUI()
    {
        GUI.Label(new Rect(30f,10f,400f,20f),"LRServer Unity3D Test");
        if (this.ServerConnected)
        {
            GUI.Label(new Rect(30f, 30f, 400f, 20f), "Connected");
            GUI.Label(new Rect(30f, 60f, 80f, 20f), "MemberId");
            memberID = GUI.TextField(new Rect(110f, 60f, 100f, 20f), memberID, 10);
            GUI.Label(new Rect(30f, 90f, 80f, 20f), "MemberPW");
            memberPW = GUI.TextField(new Rect(110f, 90f, 100f, 20f), memberPW, 10);
            if(GUI.Button(new Rect(30f,120f,100f,24f),"Login"))
            {
                var parameter = new Dictionary<byte, object> { { (byte)LoginParameterCode.MEMBER_ID, memberID }, { (byte)LoginParameterCode.MEMBER_PW, memberPW } };
                this.peer.OpCustom((byte)OperationCode.LOGIN, parameter, true);
            }
            if(LoginStatus)
            {
                GUI.Label(new Rect(30f, 150f, 400f, 20f), "Your MemberID:" + getMemberID);
                GUI.Label(new Rect(30f, 170f, 400f, 20f), "Your MemberPW:" + getMemberPW);
                GUI.Label(new Rect(30f, 190f, 400f, 20f), "Your Nickname:" + getNickname);
                GUI.Label(new Rect(30f, 210f, 400f, 20f), "Ret:" + getRet.ToString());
            }
            else
            {
                GUI.Label(new Rect(30f, 150f, 400f, 20f), "Please Login");
                GUI.Label(new Rect(30f, 170f, 400f, 20f), LoginResult);
            }
        }
        else
        {
            GUI.Label(new Rect(30f, 30, 400f, 20f), "Disonnected");
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UDP : MonoBehaviour
{
    // Start is called before the first frame update
    UdpClient udpClient;
    IPEndPoint targetPoint;

    /// <summary>
    /// 端口设定，主机端口为9001，发送指令端口为9002，接收命令端口为9003
    /// </summary>
    public int port = 9001;

    bool udp_send_flag = false;
    bool udp_recv_flag = false;

    public Action<string> ComAction;
    /// <summary>
    /// 是否循环指令
    /// </summary>
    public string loopCom = "loopTrue";
    public void Start_UDPSender(int self_port, int target_port)
    {
        if (udp_send_flag == true)
            return;
        udpClient = new UdpClient(self_port);

        targetPoint = new IPEndPoint(IPAddress.Parse("255.255.255.255"), target_port);

        udp_send_flag = true;
    }
    Thread thrRecv;
    private UdpClient UDPrecv;
    private IPEndPoint endpoint;
    private byte[] recvBuf;
    private Thread recvThread;
    public void Start_UDPReceive(int recv_port)
    {
        if (udp_recv_flag == true)
            return;

        UDPrecv = new UdpClient(new IPEndPoint(IPAddress.Any, recv_port));
        endpoint = new IPEndPoint(IPAddress.Any, 0);
        recvThread = new Thread(new ThreadStart(RecvThread));
        recvThread.IsBackground = true;
        recvThread.Start();

        udp_recv_flag = true;


    }


    public void Close_UDPSender()
    {
        if (udp_send_flag == false)
            return;
        udpClient.Close();
        udp_send_flag = false;
    }

    public void Close_UDPReceive()
    {
        if (udp_recv_flag == false)
            return;
        recvThread.Interrupt();
        recvThread.Abort();
        udp_recv_flag = false;
    }

    public void Write_UDPSender(string strdata)
    {
        if (udp_send_flag == false)
            return;
        byte[] sendData = Encoding.Default.GetBytes(strdata);
        udpClient.Send(sendData, sendData.Length, targetPoint);
    }


    public string Read_UDPReceive()
    {
        returnstr = String.Copy(recvdata);
        if (old)
        {
            old = false;
            recvdata = "";
            return returnstr;
        }
        else
            return "";
    }
    bool old = false;
    string returnstr;
    string recvdata;
    private bool messageReceive;
    private void ReceiveCallback(IAsyncResult ar)
    {
        recvBuf = UDPrecv.EndReceive(ar, ref endpoint);
        recvdata = Encoding.Default.GetString(recvBuf);
       // Debug.Log("ReceiveCallback " + recvdata);
        if (ComAction != null) ComAction(recvdata);
        Write_UDPSender(recvdata);
         old = true;
        messageReceive = true;
    }
    private void RecvThread()
    {
        messageReceive = true;

        while (true)
        {
            try
            {
                if (messageReceive)
                {
                    UDPrecv.BeginReceive(new AsyncCallback(ReceiveCallback), null);
                    messageReceive = false;
                }
            }
            catch (Exception e)
            {

            }
        }
    }

    public void StartMessage()
    {
        //Debug.LogError("该主机通信端口为 " + port);
        Start_UDPReceive(port);

        if (port == 9001)//发送数据9002为主机端口,接收的端口为9003
        {
            Start_UDPSender(port + 1, 9003);
            //一秒发送一次命令
            StartCoroutine(WaiTime(1f, (() =>
            {
                Write_UDPSender(loopCom);
            })));
        }
    }
    // Start is called before the first frame update
    void Start()
    {
       

       
    }
      
    private IEnumerator WaiTime(float time,Action action)
    {
        while (true)
        {
            yield return new WaitForSeconds(time);
            if (action != null) action();
        }
    }
    private void OnDestroy()
    {
        Close_UDPReceive();
        Close_UDPSender();
    }
}

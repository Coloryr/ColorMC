//using System.Net;
//using System.Net.Sockets;

//namespace ColorMC.Core.Net;

//public class SocketProxy
//{
//    public class Sockets
//    {
//        public Socket Tcp1 { get; set; }
//        public Socket Tcp2 { get; set; }
//    }

//    private readonly int _srcProt;
//    private readonly string _srcIP;
//    private readonly int _dstPort;
//    private readonly string _dstIp;

//    private bool _isRun;

//    private Socket serverSocket;
//    private readonly List<Sockets> _conns = new();

//    public SocketProxy(string srcIP, int localProt, string dstIP, int dstPort)
//    {
//        _srcIP = srcIP;
//        _srcProt = localProt;
//        _dstIp = dstIP;
//        _dstPort = dstPort;
//    }

//    public void Stop()
//    {
//        _isRun = false;

//        serverSocket.Close();
//        serverSocket.Dispose();

//        foreach (var item in _conns)
//        {
//            try
//            {
//                item.Tcp1.Close();
//                item.Tcp2.Close();
//                item.Tcp1.Dispose();
//                item.Tcp2.Dispose();
//            }
//            catch
//            {

//            }
//        }
//    }

//    public void Run()
//    {
//        _isRun = true;

//        var ip = IPAddress.Parse(_srcIP);
//        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
//        serverSocket.Bind(new IPEndPoint(ip, _srcProt));
//        serverSocket.Listen();
//        serverSocket.BeginAccept(new AsyncCallback(Listen), serverSocket);
//    }

//    //监听客户端连接
//    private void Listen(IAsyncResult obj)
//    {
//        try
//        {
//            var serverSocket = (obj.AsyncState as Socket)!;
//            var ip = IPAddress.Parse(_dstIp);

//            var tcp1 = serverSocket.EndAccept(obj);
//            var tcp2 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
//            tcp2.Connect(new IPEndPoint(ip, _dstPort));
//            var temp = new Sockets
//            {
//                Tcp1 = tcp2,
//                Tcp2 = tcp1
//            };
//            //目标主机返回数据
//            ThreadPool.QueueUserWorkItem(new WaitCallback(SwapMsg), temp);
//            //中间主机请求数据
//            ThreadPool.QueueUserWorkItem(new WaitCallback(SwapMsg), new Sockets
//            {
//                Tcp1 = tcp1,
//                Tcp2 = tcp2
//            });

//            _conns.Add(temp);

//            serverSocket.BeginAccept(new AsyncCallback(Listen), serverSocket);
//        }
//        catch (Exception e)
//        {
//            ColorMCCore.Error("socket proxy", e, false);
//        }
//    }
//    ///两个 tcp 连接 交换数据，一发一收
//    private void SwapMsg(object? obj)
//    {
//        var mSocket = (obj as Sockets)!;
//        while (_isRun)
//        {
//            try
//            {
//                byte[] result = new byte[1024];
//                int num = mSocket.Tcp2.Receive(result, result.Length, SocketFlags.None);
//                if (num == 0) //接受空包关闭连接
//                {
//                    if (mSocket.Tcp1.Connected)
//                    {
//                        mSocket.Tcp1.Close();
//                    }
//                    if (mSocket.Tcp2.Connected)
//                    {
//                        mSocket.Tcp2.Close();
//                    }
//                    break;
//                }
//                mSocket.Tcp1.Send(result, num, SocketFlags.None);
//            }
//            catch
//            {
//                if (mSocket.Tcp1.Connected)
//                {
//                    mSocket.Tcp1.Close();
//                }
//                if (mSocket.Tcp2.Connected)
//                {
//                    mSocket.Tcp2.Close();
//                }
//                break;
//            }
//        }
//    }

//}
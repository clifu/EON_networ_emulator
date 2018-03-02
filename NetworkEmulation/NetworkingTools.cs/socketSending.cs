using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace NetworkingTools
{
    public class SocketSending
    {
        private  IPAddress ipAddress;
        private  IPEndPoint remoteEP;
        private  Socket sendSocket;
        private object thislock = new object();


        public Socket ConnectToEndPoint(string IP)
        {
            try
            {
                lock(thislock)
                {
                    ipAddress = IPAddress.Parse(IP);
                    remoteEP = new IPEndPoint(ipAddress, 11000);

                    // Create a TCP/IP socket.  
                    sendSocket = new Socket(ipAddress.AddressFamily,
                        SocketType.Stream, ProtocolType.Tcp);
                    sendSocket.Connect((EndPoint)remoteEP);
                    return sendSocket;
                }
            }
            catch (SocketException)
            {
                return null;
            }
            catch (Exception)
            {
                return null;
            }

        }

        public void SendingPackage(Socket socket, string msg)
        {
            try
            {
                byte[] byteData = Encoding.ASCII.GetBytes(msg);

                socket.Send(byteData);
            }
            catch (SocketException)
            {

            }
            catch (Exception)
            {

            }
        }
        public void SendingPackageBytes(Socket socket, byte[] msg)
        {
            try
            {
                // byte[] byteData = msg;
                
                
                    socket.Send(msg);
                
            }
            catch (SocketException)
            {

            }
            catch (Exception)
            {

            }
        }
    }
}

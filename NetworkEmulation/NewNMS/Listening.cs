using NetworkingTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace NewNMS
{

    public class Listening 
    {

        private object _syncRoot = new object();

        public byte[] ProcessRecivedByteMessage(Socket client, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                byte[] buffer = new byte[64];
                byte[] package;
                client.ReceiveTimeout = 4000;
                int bytesRead = client.Receive(buffer);

                do
                {
                    package = new byte[64];
                    Array.Copy(buffer, package, bytesRead);

                    bytesRead = 0;


                } while (bytesRead > 0);
                return package.ToArray();

            }
            catch (IOException)
            {

                return null;
            }
            catch (SocketException)
            {

                return null;
            }
            catch (Exception ex)
            {

                return null;
            }
            finally
            {

            }
        }


    }
}

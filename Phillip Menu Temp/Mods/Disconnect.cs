using System;
using System.Collections.Generic;
using System.Text;
using Photon.Pun;

namespace Phillip_Menu_Temp.Mods
{
    internal class Disconnect
    {
        public static void DisconnectButton()
        {
            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.Disconnect();
            }
        }
    }
}

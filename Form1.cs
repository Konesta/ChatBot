using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Media;
using System.Diagnostics;

namespace SongBot
{
    public partial class Form1 : Form
    {
        #region Variables
        private static string userName = "songbot3000";
        private static string password = "oauth:6s88590fzalttfixt1b92s6xjo3x74";

        IrcClient irc = new IrcClient("irc.chat.twitch.tv", 6667, userName, password);
        NetworkStream serverStream = default(NetworkStream);
        string readData = "";
        string channelToJoin = "sirpumpkn";
        Thread chatThread;
        #endregion
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            irc.joinRoom(channelToJoin);
            chatThread = new Thread(getMessage);
            chatThread.Start();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            irc.leaveRoom();
            serverStream.Dispose();
            Environment.Exit(0);
        }

        private void getMessage()
        {
            serverStream = irc.tcpClient.GetStream();
            int buffsize = 0;
            byte[] inStream = new byte[10025];
            buffsize = irc.tcpClient.ReceiveBufferSize;
            while (true)
            {
                try
                {
                    readData = irc.readMessage();
                    msg();
                }
                catch(Exception e)
                {
                    //
                }
            }
        }

        /*
         * This function does the handling of chat, i.e commands and stuff
         */ 
        private void msg()
        {
            if (this.InvokeRequired)
                this.Invoke(new MethodInvoker(msg));
            else
            {
                string[] messageSeparator = new string[] { "#" + channelToJoin + " :" };
                string[] senderSeparator = new string[] { ":","!"};

                if (readData.Contains("PRIVMSG"))
                {
                    string username = readData.Split(senderSeparator, StringSplitOptions.None)[1];
                    string message = readData.Split(messageSeparator, StringSplitOptions.None)[1];
                    chatBox.Text = chatBox.Text + username + " : " + message + Environment.NewLine;
                }
                if (readData.Contains("PING"))
                    irc.PingResponse();
            }
        }

    }

    class IrcClient
    {
        private string userName;
        private string channel;

        public TcpClient tcpClient;
        private StreamReader inputStream;
        private StreamWriter outputStream;

        public IrcClient(string ip, int port, string userName, string password)
        {
            tcpClient = new TcpClient(ip, port);
            inputStream = new StreamReader(tcpClient.GetStream());
            outputStream = new StreamWriter(tcpClient.GetStream());

            outputStream.WriteLine("PASS " + password);
            outputStream.WriteLine("NICK " + userName);
            outputStream.WriteLine("USER " + userName + " 8 * :" + userName);
            outputStream.WriteLine("CAP REQ :twitch.tv/membership");
            outputStream.WriteLine("CAP REQ :twitch.tv/commands");
            outputStream.Flush();
        }

        public void joinRoom(string channel)
        {
            this.channel = channel;
            outputStream.WriteLine("JOIN #" + channel);
            outputStream.Flush();
        }

        public void leaveRoom()
        {
            outputStream.Close();
            inputStream.Close();
        }

        public void sendIrcMessage(string message)
        {
            outputStream.WriteLine(message);
            outputStream.Flush();
        }

        public void sendChatMessage(string message)
        {
            sendIrcMessage(":" + userName + "!" + userName + "@" + userName + ".tmi.twitch.tv PRIVMSG #" + channel + " :" + message);
        }

        public void PingResponse()
        {
            sendIrcMessage("PONG tmi.twitch.tv\r\n");
        }

        public string readMessage()
        {
            string message = "";
            message = inputStream.ReadLine();
            return message;
        }
    }
}

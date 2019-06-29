using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace Kółko_i_Krzyżyk
{


    public partial class Form2 : Form
    {

        public string Port
        {
            get
            {
                return textBox1.Text; //komuniacja z glownym oknem wyslanie portu
            }
        }

        public string Host
        {
            get
            {
                return textBox2.Text; //komuniacja z glownym oknem wyslanie ip
            }
        }



        public Form2()
        {
            InitializeComponent();

            IPAddress[] localIP = Dns.GetHostAddresses(Dns.GetHostName()); //pobranie adresu lokalnego

            foreach (IPAddress address in localIP)
            {
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    textBox2.Text = address.ToString();
                }
            }

        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true; //pole port - tylko cyfry
            }


        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                (e.KeyChar != '.'))
            {
                e.Handled = true; //pole ip cyfry i znak '.'
            }
        }
    }
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
///                                                 gra - Kółko i Krzyżyk                                            ///
///                                                 autor - Krzysztof Czekański                                      ///
///                                                 17-06-2019                                                       ///
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


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
using System.Threading;

namespace Kółko_i_Krzyżyk
{
    public partial class Form1 : Form
    {
        
        bool czysc = false; //czy mozna wyczyscic plansze
        public bool koniec_gry = false; // czy koniec gry
        bool online = false; //onlie / oflinie
        public System.Windows.Forms.Button[,] pole = new Button[15, 15]; //tabela przyciskow - plansza
        public string[,] punkty_O = new string[15, 15]; //tabele punktow
        public string[,] punkty_X = new string[15, 15];
        string host, port; //IP i POrt

        // zmienne do komunikacji w sieci
        private TcpClient client;
        public StreamReader STR;
        public StreamWriter STW;
        public string recieve;
        public String TextToSend;
        TcpListener listener;

        //zmienne pomocnicze
        int x;
        int y;
        string pomoc_x;
        string pomoc_y;

        //wspolrzedne przycisku do podswietlenia po wykonaniu ruchu
        public int zaswiec_X;
        public int zaswiec_Y;

        


        // funkcja obliczajaca punkty do tamblic punkty_O i punkty_X
        void punkty()
        {
            //zerowanie
            int a, b, c, d, x, y, k, pun, w, poz;
            a = c = d = x = y = k = pun = w = poz = 0;
            string s;
            string s1 = "";
            s = "X";
            //petla X i O
            for (d = 1; d <= 2; d++)
            {
                if (d == 2) s = "O";
                for (a = 0; a < 15; a++)
                {
                    for (b = 0; b < 15; b++)
                    {
                        pun = 0;
                        if (pole[a, b].Text == "") //jesli pole jest puste
                        {
                            x = -2;
                            y = 1;
                            for (k = 1; k < 5; k++) //petla 5 kolumn
                            {
                                if (k < 4) x++;
                                if (k > 3) y = 0;
                                poz = 1;
                                w = 0;
                                c = 0;
                                do
                                {
                                    c = c + 1;
                                    if ((a + c * x) >= 0 && (b + c * y) >= 0 && (a + c * x) < 15 && (b + c * y) < 15) //sprawdzenie czy wspolrzedne ni sa poza zakresem tabeli
                                    {
                                        s1 = pole[a + (c * x), b + (c * y)].Text; //pobranie znaku z planszy
                                    }
                                    else
                                        s1 = "*"; // znak poza tabela
                                    if (s1 == "") w++; //kolejny wiersz
                                    if (s1 == s) poz++; //jesli jest ten sam znak zwieksz poziom
                                }
                                while (s1 == s);

                                c = 0;

                                do
                                {
                                    c = c + 1;
                                    if ((a - c * x) >= 0 && (b - c * y) >= 0 && (a - c * x) < 15 && (b - c * y) < 15)
                                    {
                                        s1 = pole[a - c * x, b - c * y].Text;
                                    }
                                    else
                                        s1 = "*";
                                    if (s1 == "") w++;
                                    if (s1 == s) poz++;


                                }
                                while (s1 == s);

                                //naliczanie punktow na podstawie ktorych jest wykoywany ruch komputera
                                if (poz > 4) pun = pun + 100;
                                if (poz == 4) pun = pun + w * w * 8;
                                if (poz == 3) pun = pun + w * w * 2;
                                if (poz == 2) pun = pun = pun + w / 2;
                            }
                            if (s == "X") punkty_X[a, b] = pun.ToString(); //zapisanie punkotw dla X
                            if (s == "O") punkty_O[a, b] = pun.ToString(); //dla O 


                        }
                    }

                }
            }
        }



//ruch komputera
void ruch()
{
    //zerowanie
    int x, o, q, kx, wx, ko, wo;
    x = o = q = kx = wx = ko = wo = 0;
    for (int i = 0; i < 15; i++)
    {
        //wyszukiwanie w tabelach punktyX i punktyY
        for (int j = 0; j < 15; j++)
        {
            try
            {
                if (Convert.ToInt16(punkty_X[i, j]) > o)
                {                                           //dla O - blad w kodzie musialem zamienic
                    o = Convert.ToInt16(punkty_X[i, j]); //najwieksza wartosc
                    ko = j;                             //kolumna
                    wo = i;                             //wiersz
                }

                if (Convert.ToInt16(punkty_O[i, j]) > x)
                {                                       //dla X                            
                    x = Convert.ToInt16(punkty_O[i, j]); //wartosc
                    kx = j;                                 //kolumna
                    wx = i;                                 //wiersz
                }
            }
            catch
            {
            }
        }
    }

    //warunki postawienia znaku ... czy blokuj O czy probuj wygrac X
    q = 1;
    if ((x < 5) && (o > 0)) q = 1;
    if (x > o) q = 0;
    if ((x > o) && (x > 4)) q = 0;
    if (o > 99) q = 1;
    pole[wo * q + wx * (1 - q), ko * q + kx * (1 - q)].ForeColor = Color.Red;     //kolor ... migniecie znaku
    pole[wo * q + wx * (1 - q), ko * q + kx * (1 - q)].Text = "X";                  //wstawienie znaku
    punkty_O[wo * q + wx * (1 - q), ko * q + kx * (1 - q)] = "";                    //zaznaczenie pola w tabelach z punktami
    punkty_X[wo * q + wx * (1 - q), ko * q + kx * (1 - q)] = "";
    try
    {
        backgroundWorker4.RunWorkerAsync();  //proces w tle wykonujace mrugniecie znaku
    }
    catch
    {
        pole[wo * q + wx * (1 - q), ko * q + kx * (1 - q)].ForeColor = Color.Black; //jesli by cos nie pylko ... wroc na czarny znak
    }
    zaswiec_X = wo * q + wx * (1 - q); //gdzie mrugnie
    zaswiec_Y = ko * q + kx * (1 - q);
    


}








        //wyczysc :)
        void wyczysc_plansze()
        {
            for (int i = 0; i < 15; i++)
                for (int j = 0; j < 15; j++)
                {
                    pole[i, j].Text = "";
                    pole[i, j].BackColor = Color.LightBlue;
                }
            label7.Text = ">";
            label8.Text = "";
            panel2.Visible = false;
            koniec_gry = false;

        }


        //sprawdzanie czy ktos nie wygral
        void sprawdz()
        {
            int a, b, c, d, g, k, x, y;
            string s; 
                string s1="";

            s = "X";

            for (d = 1; d <= 2; d++) //dwa obiegi petli X i 0
            {
                if (d == 2) s = "O";
                for (a = 0; a < 15; a++)
                {
                    for (b = 0; b < 15; b++)
                    {
                        if (pole[a, b].Text == s)
                        {
                            x = -2;
                            y = 1;
                            for (k = 1; k < 5; k++)
                            {
                                if (k < 4) x++;
                                if (k > 3) y = 0;
                                c = 1;
                                do
                                {
                                    if ((a + c * x) >= 0 && (b + c * y) >= 0 && (a + c * x) < 15 && (b + c * y) < 15)
                                    {
                                        s1 = pole[a + c * x, b + c * y].Text;
                                        if (s1 == s) c++; //zliczanie pol w lini ... jesli 5 to wygrana

                                    }
                                    else
                                        s1 = "*";



                                }
                                while (s1 == s);
                                if (c > 4)
                                {
                                    for (g = 0; g < 5; g++) //petla kolorujaca wygrana linie
                                    {
                                        label17.Text = pole[a + g * x, b + g * y].Text;
                                        if (label12.Text == pole[a + g * x, b + g * y].Text)
                                            pole[a + g * x, b + g * y].BackColor = Color.Green; //jesli twoj znak to zielony
                                        else
                                            pole[a + g * x, b + g * y].BackColor = Color.Red; //przeciwnik czerowny

                                            
                                    }
                                    panel2.Visible = true; //panel konca gry 
                                    koniec_gry = true;          //koniec                          
                                }
                            }
                        }
                    }
                }
            }
            

        }



        private void pole_zmiana_textu(object sender, EventArgs e) //zdarzenie dla przycisku ... wprowadzenie znaku
        {
            
            Button btn = sender as Button;
            if (btn.Text == "X" || btn.Text == "O") //jesli pojawi sie znka
            {
                int a, b, c, d, g, k, x, y;
                string s;
                string s1 = "";

                s = "X";

                for (d = 1; d <= 2; d++) //analogicznie jak funkcja sprawdz
                {
                    if (d == 2) s = "O";
                    for (a = 0; a < 15; a++)
                    {
                        for (b = 0; b < 15; b++)
                        {
                            if (pole[a, b].Text == s)
                            {
                                x = -2;
                                y = 1;
                                for (k = 1; k < 5; k++)
                                {
                                    if (k < 4) x++;
                                    if (k > 3) y = 0;
                                    c = 1;
                                    do
                                    {
                                        if ((a + c * x) >= 0 && (b + c * y) >= 0 && (a + c * x) < 15 && (b + c * y) < 15)
                                        {
                                            s1 = pole[a + c * x, b + c * y].Text;
                                            if (s1 == s) c++;

                                        }
                                        else
                                            s1 = "*";



                                    }
                                    while (s1 == s);
                                    if (c > 4)
                                    {
                                        for (g = 0; g < 5; g++)
                                        {
                                            if (label12.Text == pole[a + g * x, b + g * y].Text)
                                                pole[a + g * x, b + g * y].BackColor = Color.Green;
                                            else
                                                pole[a + g * x, b + g * y].BackColor = Color.Red;
                                            label17.Text = pole[a + g * x, b + g * y].Text;
                                        }
                                        koniec_gry = true;
                                    }
                                }
                            }
                        }
                    }
                }
                if (koniec_gry)
                    richTextBox1.Text += "[Info] Wygrywa gracz - "+label17.Text+"\n"; //wyswietlenie komnikaty w oknie chatu
            }
            if (!czysc)
            {
                if (koniec_gry && label17.Text == "O")
                {
                    label18.Text = (Convert.ToInt32(label18.Text) + 1).ToString(); //naliczenie punktow dla O
                    czysc = true;
                }
                if (koniec_gry && label17.Text == "X")
                {
                    label19.Text = (Convert.ToInt32(label19.Text) + 1).ToString(); //punkty X
                    czysc = true;
                }
            }
        }


        private void pole_click(object sender, MouseEventArgs e) //obsluga klikniecia myszka w pole gry
        {
            if (Cursor != Cursors.WaitCursor) //jesli akurat komputer nie wykonje ruchu
            {
                sprawdz(); //sprawdz czy ktos nie wygral
                Button btn = sender as Button;
                if (!koniec_gry && btn.Text == "")  //blokowanie podczas konca gry i pol ze znkaiem X O
                {
                    if (online)  //podczas gry online
                    {
                        if (label12.Text == "O" && label7.Text == ">") //gracz O
                        {

                            btn.Text = label12.Text; //wstawienie znkau
                            TextToSend = btn.Name; //wyslanie przeciwnikowi gdzie wstawiono znak
                            backgroundWorker2.RunWorkerAsync(); //uruchomienie procesu odpowiedzialengo za wysyanie 
                            label7.Text = ""; //zmiania wskaznika kogo kolej
                            label8.Text = ">";
                        }

                        if (label12.Text == "X" && label8.Text == ">") //jak wyzej dla X
                        {
                            btn.Text = label12.Text;
                            TextToSend = btn.Name;
                            backgroundWorker2.RunWorkerAsync();
                            label7.Text = ">";
                            label8.Text = "";
                        }
                    }
                    else
                    { //gra offline
                        if (label12.Text == "O" && label7.Text == ">") //dla O
                        {
                            btn.Text = label12.Text;
                            label7.Text = "";
                            label8.Text = ">";
                            bool czy_x = true;
                            pomoc_x = pomoc_y = "";
                            for (int i = 0; i < btn.Name.Length; i++) //petla wyciagajaca wspolrzedne przycisku z jego nazwy {X:Y]
                            {
                                if (btn.Name[i] != ':')
                                    if (czy_x)
                                        pomoc_x += btn.Name[i];
                                    else
                                        pomoc_y += btn.Name[i];
                                else
                                    czy_x = false;
                            }
                            x = Convert.ToInt32(pomoc_x); //wspolrzedne w postaci int
                            y = Convert.ToInt32(pomoc_y);
                            punkty_O[x, y] = "";          //zaznaczenie pola w tablicach punkty
                            punkty_X[x, y] = "";
                        }

                        if (label12.Text == "X" && label8.Text == ">" && radioButton1.Checked) //dla znaku X gdy gra gracz
                        {
                            btn.Text = label12.Text;
                            label7.Text = ">";
                            label8.Text = "";
                        }
                        if (radioButton2.Checked) //gdy komputer
                        {
                            Cursor = Cursors.WaitCursor; //ruchy komputera
                            punkty();
                            ruch();
                            Cursor = Cursors.Arrow;

                            label7.Text = ">";
                            label8.Text = "";
                            label12.Text = "X";
                        }
                        if (label12.Text == "X")
                            label12.Text = "O";
                        else
                            label12.Text = "X";
                    }
                    sprawdz();
                }
            }
        }

        void nowa_plansza() //tworzenie nowej planszy z z przyciskow 15x15
        {
            panel1.Height = panel1.Width = 605;
            for (int i = 0; i < 15; i++)
                for (int j = 0; j < 15; j++)
                {
                    pole[i, j] = new  Button();
                    pole[i, j].Height = 40;
                    pole[i, j].Width = 40;
                    pole[i, j].Top = i * 40;
                    pole[i, j].Left = j * 40;
                    pole[i,j].Name = i+":"+j;
                    pole[i, j].Font = new Font(pole[i,j].Font.FontFamily, 20);
                    pole[i, j].MouseDown += new MouseEventHandler(pole_click);
                    pole[i, j].TextChanged += new System.EventHandler(pole_zmiana_textu);
                    pole[i, j].BackColor = Color.LightBlue;
                    panel1.Controls.Add(pole[i,j]);

                    
                }
        }


        public Form1() //glowne okno
        {
            InitializeComponent();
            nowa_plansza();
            radioButton2.Checked = true;
        }

        private void klientSerwerToolStripMenuItem_Click(object sender, EventArgs e) //menu - wywolanie okna z ustaiweniem servera / klienta
        {
            using (Form2 frm = new Form2())
            {
                DialogResult odp = new DialogResult(); //okno jako dialogowe
                odp = frm.ShowDialog();
                if (odp == DialogResult.OK) //jesli OK - przyciks start
                {
                    port = frm.Port; //port 
                    host = frm.Host; //ip

                    try
                    {
                        listener = new TcpListener(IPAddress.Any, int.Parse(port)); //tworzenie serwera
                        listener.Start(); //uruchomienie
                        backgroundWorker3.WorkerSupportsCancellation = true; //mozliwoesc zatrzymania procesu w tle
                        backgroundWorker3.RunWorkerAsync(); //uruchomienie procesu odpowiedzialengo za oczekiwanie na klienta
                        label12.Text = "O"; //ustawiamy wsztstko dla servera
                        label14.Text = "Serwer";
                        wyczysc_plansze();
                        label5.Text = host;
                        label6.Text = port;
                        pictureBox1.BackColor = Color.Green; //kontrolka zielona server
                        pictureBox2.BackColor = Color.Red; //czrwoan klient - jeszcze czekamy
                        richTextBox1.Text = "[Info] Uruchomiono serwer ...\n[Info] IP: " + host + "\tPort: " + port +
                                           "\n[Info] Oczekiwanie na przeciwnika ...\n"; //info na chacie
                        label18.Text = "0"; //zerujemy punkty
                        label19.Text = "0";
                        czysc = false;
                    }
                    catch
                    {
                        MessageBox.Show("Nieprawidłowy adres IP lub/i Port. \n Wprowadź poprawne dane.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); //cos poszlo nie tak 
                    }
                    
                }
                if (odp == DialogResult.Ignore) //przycisk polacz
                {
                    port = frm.Port;
                    host = frm.Host;




                    client = new TcpClient(); //tworzymy klienta
                    try
                    {
                        IPEndPoint IpEnd = new IPEndPoint(IPAddress.Parse(host), int.Parse(port)); //parsujemy IP port

                        try
                        {
                            client.Connect(IpEnd); //probujemy nawiazac polaczenie

                            if (client.Connected) //jesli polaczono
                            {

                                STW = new StreamWriter(client.GetStream()); //zmiena wysyajaca
                                STR = new StreamReader(client.GetStream()); //zmienna odbierajaca
                                STW.AutoFlush = true;
                                backgroundWorker1.RunWorkerAsync(); //proces nasluchjacy
                                backgroundWorker2.WorkerSupportsCancellation = true; //proces wysyajacy ustawiamy na mozliwy do zatrzymania
                                pictureBox1.BackColor = Color.Green; //kolorujemy kontroli
                                pictureBox2.BackColor = Color.Green;
                                label12.Text = "X"; //i ustawiamy wszystkie napisy
                                online = true;
                                button2.Enabled = true;
                                label14.Text = "Klient";
                                label5.Text = host;
                                label6.Text = port;
                                wyczysc_plansze();
                                richTextBox1.Text = "[Info] Połączyłeś się z serwerem ...\n[Info] IP: " + host + "\tPort: " + port +
                                                    "\n[Info] Nowa gra rozpoczęta.\n\n";
                                label18.Text = "0"; //zerujemy pkt
                                label19.Text = "0";
                                czysc = false;
                                radioButton1.Enabled = radioButton2.Enabled = false; //blokujemy ozliwosc wyboru pc / gracz

                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message.ToString()); //cos poszlo nie tak komunikat
                        }
                    }
                    catch 
                    {
                        MessageBox.Show("Nieprawidłowy adres IP lub/i Port. \n Wprowadź poprawne dane.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); //bledny adres IP
                    }
                }

                if (odp == DialogResult.No) //zatrzymanie serwera - rozlaczenie klienta 
                {
                    if (online) //mozliwe jesli jestemy onllinie
                    {
                        port = frm.Port; //przywracamy ustaiwneia planszy jak w grze offlinie
                        host = frm.Host;
                        label14.Text = "---"; //status
                        label5.Text = ""; //ip i port
                        label6.Text = "";
                        pictureBox1.BackColor = Color.Red;  //kontrolki na czerowno
                        pictureBox2.BackColor = Color.Red;
                        if (label14.Text == "Serwer") //jesli jestesmy serwerem to zatrzymujemy server
                            listener.Stop();
                        client.Close();
                        online = false; //offlinie
                        button2.Enabled = false; //wylaczamy mozliwosc wyslij wiadomosc na czacie
                        wyczysc_plansze();
                        label18.Text = "0"; //zerujemy pkt
                        label19.Text = "0";
                        czysc = false;
                        radioButton1.Enabled = radioButton2.Enabled = true; //wlaczamy opcje zmien pc / gracz
                    }
                }
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e) //proces obslugujacy nasluchiwanie 
        {
            while (client.Connected) //jesli polaczono
            {
                try
                {
                    recieve = STR.ReadLine(); //zmienna odczytujaca

                    if (recieve[0] == '>') //jesli pierwszy znak w wiadomosci to > - wiadomosc od gracza
                    {
                        richTextBox1.Text +=">" + recieve+"\n"; //wyswietl na czacie komunikat 
                    }
                    if (recieve[0] != '>')
                    {
                        if (recieve == "NOWA") //jesli wyslano NOWA - przeciwnik klikna nowa gra po zakonczeniu gry 
                        {
                            for (int i = 0; i < 15; i++)  //czyscimy wszystko
                                for (int j = 0; j < 15; j++)
                                {
                                    pole[i, j].Text = "";
                                    pole[i, j].BackColor = Color.LightBlue;
                                }
                            label7.Text = ">";
                            label8.Text = "";
                            panel2.Visible = false;
                            koniec_gry = false;
                            czysc = false;
                        }
                        else //jeli cos innego to obslugujemy wyswietlenie ruchu przeciwnika
                        {
                            bool czy_x = true;
                            pomoc_x = pomoc_y = "";
                            for (int i = 0; i < recieve.Length; i++) //wspolrzedne w tablicy 
                            {
                                if (recieve[i] != ':')
                                    if (czy_x)
                                        pomoc_x += recieve[i];
                                    else
                                        pomoc_y += recieve[i];
                                else
                                    czy_x = false;
                            }
                            x = Convert.ToInt32(pomoc_x);
                            y = Convert.ToInt32(pomoc_y);
                            zaswiec_X = x;
                            zaswiec_Y = y;
                            pole[x,y].ForeColor = Color.Red; //mrugniecie znaku ktory zostanie wstawniony
                            if (label12.Text == "X")
                            {
                                pole[x, y].Text = "O";
                                label8.Text = ">";
                                label7.Text = "";
                            }
                            if (label12.Text == "O")
                            {
                                pole[x, y].Text = "X";
                                label8.Text = "";
                                label7.Text = ">";
                            }

                            try
                            {
                                backgroundWorker4.RunWorkerAsync(); //proces w tle obslugujacy mrugniecie
                            }
                            catch
                            {
                                pole[x, y].ForeColor = Color.Black; //jesli nie pyklo to wroc na czarny
                            }
                        }

                        
                    }
                    recieve = ""; //zmienna do odczytu wyczyszczona
                    
                }
                catch  //jesli zerwanno polaczenie 
                {
                    richTextBox1.Text += "\n[Info] Połączenie zostało utracone!!!\n"; //komunikat
                    client.Close(); //przejsie w tryb offlinie kontrolki , etykietki itp
                    label14.Text = "---";
                    label5.Text = "";
                    label6.Text = "";
                    pictureBox1.BackColor = Color.Red;
                    pictureBox2.BackColor = Color.Red;
                    if (label14.Text == "Serwer") 
                    listener.Stop();
                    client.Close();
                    online = false;
                    button2.Enabled = false;
                    wyczysc_plansze();
                    label18.Text = "0";
                    label19.Text = "0";
                    radioButton1.Enabled = radioButton2.Enabled = true;
                }
            }
        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e) //proces w tle wysylajacy 
        {
            if (client.Connected) //jesli polaczono
            {
                STW.WriteLine(TextToSend); //wyslij wiadomosc
            }
            else
            {
             //   MessageBox.Show("Sending failed");
                label18.Text = "0";
                label19.Text = "0";
            }
            backgroundWorker2.CancelAsync(); //zatrzymaj proce
        }

        private void backgroundWorker3_DoWork(object sender, DoWorkEventArgs e) //proces oczekiwania na klienta - przeciwnika
        {
            try
            {
                client = listener.AcceptTcpClient(); //zaakceptowanie klienta przez serwer 
                STR = new StreamReader(client.GetStream()); //zmienna odbierajaca
                STW = new StreamWriter(client.GetStream()); //zmienna wysylajaca
                STW.AutoFlush = true;
                backgroundWorker1.RunWorkerAsync(); //uruchom proces nasluchiwania
                backgroundWorker2.WorkerSupportsCancellation = true; //proces wywylania ustawiony jako mozliwy do zatrzymania
                pictureBox2.BackColor = Color.Green; //kontrolka klient zielona
                online = true; //online
                button2.Enabled = true; //przygotuj wszystkie etykientki i wiadomosci do gry online
                wyczysc_plansze();
                label12.Text = "O";
                richTextBox1.Text += "[Info] Przeciwnik dołączył do gry.\n\n";
                label18.Text = "0";
                label19.Text = "0";
                radioButton1.Enabled = radioButton2.Enabled = false;
            }
            catch { }
            backgroundWorker3.CancelAsync(); //zatrzymaj proces
        }

        void nowa_gra() //funkcaj nowa gra po zakonczeniu gry 
        {
            for (int i = 0; i < 15; i++) //czyscimy
                for (int j = 0; j < 15; j++)
                {
                    pole[i, j].Text = "";
                    pole[i, j].BackColor = Color.LightBlue;
                    pole[i,j].ForeColor = Color.Black;
                }
            label7.Text = ">";
            label8.Text = "";
            if (online) //jesli online
            {
                TextToSend = "NOWA"; //wysylamy wiadomosc NOWA - przeciwnik sam wyczysci plansze
                backgroundWorker2.RunWorkerAsync();
            }
            panel2.Visible = false;
            koniec_gry = false;
            if (!online) //jesli offlinie 
                label12.Text = "O";
            czysc = false;
        }

        private void button1_Click(object sender, EventArgs e) //przycisk nowa gra
        {
            nowa_gra();
        }


        private void button2_Click(object sender, EventArgs e) //przycisk wyslij 
        {
            if (textBox1.Text != "") //blokowanie pustych wiadomosci
            {
                TextToSend = "> " + textBox1.Text; //dodanie znkau > na poczatek
                backgroundWorker2.RunWorkerAsync(); //proces wysylajacy
                richTextBox1.Text += "Ja: " + textBox1.Text + "\n"; //dodanie do swojego czatu
                textBox1.Text = ""; //wycysczenie pola wyslij
            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == Convert.ToChar(Keys.Return)) //jesli klikniemy enter to wyslij wiadomosc
            {
                e.Handled = true;
                button2.PerformClick();
            }
        }

        private void richTextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true; //blokowanie pisania w oknie czatu
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) //obsluga zdarzenia zamknij okno
        {
            DialogResult dialogResult = MessageBox.Show("Zamknąć program?", "Zamknij", MessageBoxButtons.YesNo,MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                e.Cancel = false;
            }
            else if (dialogResult == DialogResult.No)
            {
                e.Cancel = true;
            }
        }

        private void zamkniToolStripMenuItem_Click(object sender, EventArgs e) //zamknij w oknie menu gornego
        {
            System.Windows.Forms.Application.Exit();
        }

        private void nowaGraToolStripMenuItem_Click(object sender, EventArgs e) //meu gorne nowa gra
        {
            if (online) //jesli w trybie online komunikat
            {
                MessageBox.Show("Jesteś w trakcie gry online.\nNie możesz rozpocząć gry od nowa.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
            else //oflnie wyczysc 
            {
                label18.Text = "0";
                label19.Text = "0";
                nowa_gra();
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e) //zmiana miedzy gracz a pc ... wyczysc wszystko
        {
            nowa_gra();
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            nowa_gra();
        }

        private void wyczyśćToolStripMenuItem_Click(object sender, EventArgs e) //menu podreczne wyczysc czat
        {
            richTextBox1.Text="";
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e) //czat ... wyswietlanie zawsze ostatniej lini 
        {
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.ScrollToCaret(); 
        }

        private void autorToolStripMenuItem_Click(object sender, EventArgs e) //info o programie wyswietlenie okna info
        {
            using (Form3 frm = new Form3())
            {
                DialogResult odp = new DialogResult();
                odp = frm.ShowDialog();
            }
        }

        private void backgroundWorker4_DoWork(object sender, DoWorkEventArgs e) //proces odpowiedzialny za miganie wstawionego znaku
        {
             System.Threading.Thread.Sleep(200); //migniecie 0,2s
             pole[zaswiec_X, zaswiec_Y].ForeColor = Color.Black; //powrot na czarny kolor
            backgroundWorker4.CancelAsync(); //zatrzymanie procesu
        }


    }
}

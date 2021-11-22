using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;
using System;
using System.Collections.Generic;

public class HT : PhysicsGame
{
    private PlatformCharacter p1;
    private IntMeter pistelaskuri;
    private ScoreList topLista = new ScoreList(10, false, 0);
    private List<char> kerattavat = new List<char>{ '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H' };
    private DoubleMeter alaspainlaskuri;
    private Timer aikalaskuri;
    private double suunta = 200.0;

    public override void Begin()
    {
        Alkuvalikko();
        Level.Background.Image = LoadImage("alkukuva.png");
        Label ohjeet = new Label("Mitä pelissä pitää tehdä?");
        ohjeet.Y = Level.Bottom + 150;
        Add(ohjeet);
        Label ohjeet2 = new Label("Kerää mahdollisimman monta punaista palloa ennen ajan loppumista! Bonuspalloista saa enemmän pisteitä!");
        ohjeet2.Y = Level.Bottom + 120;
        Add(ohjeet2);
    }


    /// <summary>
    /// Aliohjelma, joka lisää peliin alkunäytön alkuvalikon
    /// </summary>
    private void Alkuvalikko()
    {
        string[] vaihtoehdot = { "Aloita peli", "Lopeta" };
        MultiSelectWindow alkuvalikko = new MultiSelectWindow("Pelin alkuvalikko", vaihtoehdot);
        alkuvalikko.Color = Color.Black;
        PushButton[] nappula = alkuvalikko.Buttons;
        alkuvalikko.SetButtonColor(Color.Green);
        alkuvalikko.SetButtonTextColor(Color.Gold);
        nappula[1].Color = Color.Red;
        nappula[1].TextColor = Color.Black;
        Add(alkuvalikko);

        alkuvalikko.AddItemHandler(0, AloitaPeli);
        alkuvalikko.AddItemHandler(1, Exit);
    }


    /// <summary>
    /// Aliohjelma, joka aloittaa pelin ja luo painovoiman sekä asettaa kameran.
    /// </summary>
    private void AloitaPeli()
    {
        ClearGameObjects();
        ClearControls();

        Gravity = new Vector(0, -1500.0);

        LuoKentta();
        LuoOhjaimet();
        LuoPistelaskuri();
        LuoAikaLaskuri();
        aikalaskuri.Start();

        Camera.Follow(p1);
        Camera.StayInLevel = true;
        Level.Background.Image = LoadImage("ohj1Pelitausta");

        topLista = DataStorage.TryLoad<ScoreList>(topLista, "pisteet.xml");
    }


    /// <summary>
    /// Aliohjelma, joka luo kentän ja kutsuu kenttätiedostoa
    /// </summary>
    private void LuoKentta()
    {
        TileMap kentta = TileMap.FromLevelAsset("kentta1.txt");
        kentta.SetTileMethod('#', Este);
        kentta.SetTileMethod('S', LuoPelaaja);
        kentta.SetTileMethod('M', Maali);
        Sotke(kerattavat);
        List<char> kaytettavat = kerattavat.GetRange(0, 10);
        int i = 0;
        while (i < kaytettavat.Count)
        {
            kentta.SetTileMethod(kaytettavat[i], Keraaminen);
            i++;
        }
        kentta.SetTileMethod('x', SuperKeraaminen);
        kentta.Execute(40, 40);
        Level.CreateBottomBorder();
        Level.CreateLeftBorder();
        Level.CreateRightBorder();

    }


    /// <summary>
    /// Aliohjelmalla sotketaan kerattavat-listan alkioiden järjestystä
    /// </summary>
    /// <param name="kerattavat">sotkettava lista</param>
    private static void Sotke(List<char> kerattavat)
    {
        Random rand = new Random();
        for (int viimeisenPaikka = kerattavat.Count - 1; viimeisenPaikka > 0; viimeisenPaikka--)
        {
            int arvottuPaikka = rand.Next(viimeisenPaikka + 1);
            char tmp = kerattavat[arvottuPaikka];
            kerattavat[arvottuPaikka] = kerattavat[viimeisenPaikka];
            kerattavat[viimeisenPaikka] = tmp;
        }
    }


    /// <summary>
    /// Aliohjelma, joka luo pelaajan kentälle
    /// </summary>
    /// <param name="paikka">paikka, johon pelaaja luodaan</param>
    /// <param name="leveys">Pelaajan leveys</param>
    /// <param name="korkeus">Pelaajan korkeus</param>
    private void LuoPelaaja(Vector paikka, double leveys, double korkeus)
    {
        p1 = new PlatformCharacter(1.2 * leveys, 1.2 * korkeus, Shape.Circle);
        p1.Position = paikka;
        p1.Color = Color.Black;
        p1.Mass = 0.1;
        p1.Tag = "pelaaja";
        p1.Image = LoadImage("pelaaja.png");
        Add(p1);
    }


    /// <summary>
    /// Aliohjelma, joka tekee esteitä peliin
    /// </summary>
    /// <param name="paikka">esteen paikka</param>
    /// <param name="leveys">esteeen leveys</param>
    /// <param name="korkeus">esteen korkeus</param>
    private void Este(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject este = PhysicsObject.CreateStaticObject(leveys, korkeus, Shape.Rectangle);
        este.Position = paikka;
        este.Color = Color.Brown;
        este.Tag = "este";
        Add(este);
    }


    /// <summary>
    /// Aliohjelma, joka luo maaliobjektin
    /// </summary>
    /// <param name="paikka">maalin paikka</param>
    /// <param name="leveys">maalin leveys</param>
    /// <param name="korkeus">maalin korkeus</param>
    private void Maali(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject maali = PhysicsObject.CreateStaticObject(leveys, korkeus, Shape.Triangle);
        maali.Position = paikka;
        maali.Color = Color.Yellow;
        maali.Tag = "maali";
        maali.Image = LoadImage("MaalinKuva.png");
        Add(maali);
        AddCollisionHandler(p1, maali, Maaliintulo);
    }


    /// <summary>
    /// Aliohjelma, joka lisää kerättäviä tavaroita peliin
    /// </summary>
    /// <param name="paikka">paikka, johon tavara lisätään</param>
    /// <param name="leveys">tavaran leveys</param>
    /// <param name="korkeus">tavaran korkeus</param>
    private void Keraaminen(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject kerattava = new PhysicsObject (0.5*leveys, 0.5*korkeus, Shape.Circle);
        kerattava.Position = paikka;
        kerattava.Color = Color.Red;
        kerattava.Tag = "esine";
        kerattava.Mass = 0.1;
        kerattava.Image = LoadImage("KerattavanKuva.png");
        Add(kerattava);
        AddCollisionHandler(p1, kerattava, Kerays);
    }


    /// <summary>
    /// Luodaan "superkerättävä", josta saa enemmän pisteitä.
    /// </summary>
    /// <param name="paikka"></param>
    /// <param name="leveys"></param>
    /// <param name="korkeus"></param>
    private void SuperKeraaminen(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject SuperKerattava = PhysicsObject.CreateStaticObject(leveys, korkeus, Shape.Circle);
        SuperKerattava.Position = paikka;
        SuperKerattava.Color = Color.LightBlue;
        SuperKerattava.Tag = "Super";
        SuperKerattava.Mass = 0.1;
        SuperKerattava.Image = LoadImage("SuperKerattavanKuva.png");
        Add(SuperKerattava);
        AddCollisionHandler(p1, SuperKerattava, SuperKerays);
    }


    /// <summary>
    /// Aliohjelma, joka lisää peliin ohjaimet
    /// </summary>
    private void LuoOhjaimet()
    {
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Tab, ButtonState.Pressed, ShowControlHelp, "Näytä näppäinohjeet");
        Keyboard.Listen(Key.F1, ButtonState.Pressed, AloitaPeli, "Aloita uusi peli");
        Keyboard.Listen(Key.Space, ButtonState.Down, LiikuYlos, "Liiku ylös", p1, 100.0);
        Keyboard.Listen(Key.Right, ButtonState.Down, Liiku, "Liiku oikealle", p1, suunta);
        Keyboard.Listen(Key.Left, ButtonState.Down, Liiku, "Liiku vasemmalle", p1, -suunta);
        
    }


    /// <summary>
    /// Aliohjelma, jolla pelaaja saadaan liikkumaan
    /// </summary>
    /// <param name="p1">pelaaja joka liikkuu</param>
    /// <param name="suunta">pelaajan liikkumissuunta ja -nopeus</param>
    private void Liiku(PlatformCharacter p1, double suunta)
    {
        p1.Walk(suunta);
    }


    /// <summary>
    /// Aliohjelma, jolla pelaaja saadaan hyppäämään
    /// </summary>
    /// <param name="p1">pelaaja joka liikkuu</param>
    /// <param name="nopeus">nopeus, jolla pelaaja hyppää</param>
    private void LiikuYlos(PlatformCharacter p1, double nopeus)
    {
        p1.Jump(nopeus);
    }


    /// <summary>
    /// Aliohjelmalla määritetään mitä tapahtuu kun pelaaja kerää kerättävän esineen.
    /// </summary>
    /// <param name="pelaaja">pelaaja joka kerää esineen</param>
    /// <param name="kohde">kerättävä esine</param>
    private void Kerays(PhysicsObject pelaaja, PhysicsObject kohde)
    {
        kohde.Destroy();
        pistelaskuri.Value += 1;
    }


    /// <summary>
    /// Aliohjelmalla määritetään mitä tapahtuu kun pelaaja kerää kerättävän esineen.
    /// </summary>
    /// <param name="pelaaja">pelaaja joka kerää superesineen</param>
    /// <param name="kohde">kerättävä superesine</param>
    private void SuperKerays(PhysicsObject pelaaja, PhysicsObject kohde)
    {
        kohde.Destroy();
        pistelaskuri.Value += 5;
    }


    /// <summary>
    /// Aliohjelma, jossa määritetään mitä tapahtuu maaliin tultaessa
    /// </summary>
    /// <param name="pelaaja">pelaaja joka tulee maaliin</param>
    /// <param name="maali">maali</param>
    private void Maaliintulo(PhysicsObject pelaaja, PhysicsObject maali)
    {
        maali.Destroy();
        aikalaskuri.Stop();
        HighScoreWindow topIkkuna = new HighScoreWindow(
                     "Parhaat pisteet",
                     "Onneksi olkoon, pääsit listalle pisteillä " + pistelaskuri + "! Syötä nimesi:",
                     topLista, pistelaskuri);
        topIkkuna.Width = 2000.0;
        topIkkuna.Color = Color.Gold;
        topIkkuna.Closed += TallennaPisteet;
        Loppuvalikko();
        Add(topIkkuna);
    }


    /// <summary>
    /// Aliohjelma, jolla luodaan pistelaskuri peliin
    /// </summary>
    private void LuoPistelaskuri()
    {
        pistelaskuri = new IntMeter(0);

        Label pistenaytto = new Label();
        pistenaytto.X = Screen.Right - 100;
        pistenaytto.Y = Screen.Top - 150;
        pistenaytto.TextColor = Color.Green;
        pistenaytto.Color = Color.Black;
        pistenaytto.Title = "Pisteet: ";

        pistenaytto.BindTo(pistelaskuri);
        Add(pistenaytto);
    }


    /// <summary>
    /// Luodaan peliin aikalaskuri
    /// </summary>
    private void LuoAikaLaskuri()
    {
        alaspainlaskuri = new DoubleMeter(60);

        aikalaskuri = new Timer();
        aikalaskuri.Interval = 0.1;
        aikalaskuri.Timeout += LaskeAlas;

        Label aikanaytto = new Label();
        aikanaytto.X = Screen.Right - 100;
        aikanaytto.Y = Screen.Top - 130;
        aikanaytto.Title = "Aikaa jäljellä: ";
        aikanaytto.TextColor = Color.Green;
        aikanaytto.Color = Color.Black;
        aikanaytto.DecimalPlaces = 2;
        aikanaytto.BindTo(alaspainlaskuri);
        Add(aikanaytto);
    }


    /// <summary>
    /// Aliohjelma, jolla luodaan peliin loppuvalikko.
    /// </summary>
    private void Loppuvalikko()
    {
        string[] vaihtoehdot = { "Uusi peli", "Alkunäyttöön", "Näytä Ohjaimet", "Lopeta peli" };
        MultiSelectWindow loppuvalikko = new MultiSelectWindow("Voitit pelin! Keräsit " + pistelaskuri + "pistettä !", vaihtoehdot);
        PushButton[] nappula = loppuvalikko.Buttons;
        loppuvalikko.Color = Color.Gold;
        loppuvalikko.SetButtonColor(Color.Black);
        loppuvalikko.SetButtonTextColor(Color.Gold);
        nappula[3].Color = Color.Red;
        nappula[3].TextColor = Color.Black;
        Add(loppuvalikko);

        loppuvalikko.AddItemHandler(0, AloitaPeli);
        loppuvalikko.AddItemHandler(1, Begin);
        loppuvalikko.AddItemHandler(2, ShowControlHelp);
        loppuvalikko.AddItemHandler(3, Exit);
    }


    /// <summary>
    /// Asetetaan aikalaskuri laskemaan alaspäin 60 sekunnista
    /// </summary>
    private void LaskeAlas()
    {
        alaspainlaskuri.Value -= 0.1;

        if (alaspainlaskuri.Value <= 0)
        {
            MessageDisplay.Add("Aika loppui!!");
            aikalaskuri.Stop();
            suunta = 0.0;
            EpaOnnistuminen();
        }
    }


    /// <summary>
    /// Aliohjelma, joka määrittää mitä tapahtuu jos pelin tavoite ei täyty.
    /// </summary>
    private void EpaOnnistuminen()
    {
        string[] vaihtoehdot = { "Uusi peli","Alkunäyttöön", "Näytä Ohjaimet", "Lopeta peli" };
        MultiSelectWindow epaonnistuminen = new MultiSelectWindow("Hävisit pelin! Et saavuttanut maalia ajoissa!", vaihtoehdot);
        PushButton[] nappula = epaonnistuminen.Buttons;
        epaonnistuminen.Color = Color.Gold;
        epaonnistuminen.SetButtonColor(Color.Black);
        epaonnistuminen.SetButtonTextColor(Color.Gold);
        nappula[3].Color = Color.Red;
        nappula[3].TextColor = Color.Black;
        Add(epaonnistuminen);

        epaonnistuminen.AddItemHandler(0, AloitaPeli);
        epaonnistuminen.AddItemHandler(1, Begin);
        epaonnistuminen.AddItemHandler(1, ShowControlHelp);
        epaonnistuminen.AddItemHandler(2, Exit);
    }


    /// <summary>
    /// Aliohjelma, joka tallentaa pisteet tiedostoon.
    /// </summary>
    /// <param name="sender"></param>
    private void TallennaPisteet(Window sender)
    {
        DataStorage.Save<ScoreList>(topLista, "pisteet.xml"); 
    }

}


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
    
    public override void Begin()
    {
        ClearGameObjects();
        ClearControls();

        Gravity = new Vector(0, -1500.0);

        LuoKentta();
        LuoOhjaimet();
        LuoPistelaskuri();

        Camera.Follow(p1);
        Camera.StayInLevel = true;
        Level.Background.Image = LoadImage("ohj1Pelitausta");

        string[] vaihtoehdot = { "Aloita peli","Näytä Ohjaimet", "Parhaat pisteet", "Lopeta" };
        MultiSelectWindow alkuvalikko = new MultiSelectWindow("Pelin alkuvalikko", vaihtoehdot);
        Add(alkuvalikko);

        //alkuvalikko.AddItemHandler(0, Aloita);
        alkuvalikko.AddItemHandler(1, ShowControlHelp);
        //alkuvalikko.AddItemHandler(1, ParhaatPisteet);
        alkuvalikko.AddItemHandler(3, Exit);
    }




    /// <summary>
    /// Aliohjelma, joka luo kentän ja kutsuu kenttätiedostoa
    /// </summary>
    private void LuoKentta()
    {
        TileMap kentta = TileMap.FromLevelAsset("kentta1.txt");
        kentta.SetTileMethod('#', Este);
        kentta.SetTileMethod('*', Keruu);
        kentta.SetTileMethod('S', LuoPelaaja);
        kentta.SetTileMethod('M', Maali);
        kentta.Execute(40, 40);
        Level.CreateBottomBorder();
        Level.CreateLeftBorder();
        Level.CreateRightBorder();
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
        Add(maali);
        AddCollisionHandler(p1, maali, maaliintulo);
    }


    /// <summary>
    /// Aliohjelma, joka lisää kerättäviä tavaroita peliin
    /// </summary>
    /// <param name="paikka">paikka, johon tavara lisätään</param>
    /// <param name="leveys">tavaran leveys</param>
    /// <param name="korkeus">tavaran korkeus</param>
    private void Keruu(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject keruu = PhysicsObject.CreateStaticObject(leveys, korkeus, Shape.Circle);
        keruu.Position = paikka;
        keruu.Color = Color.Red;
        keruu.Tag = "keruu";
        Add(keruu);
        AddCollisionHandler(p1, keruu, Kerays);
    }


    /// <summary>
    /// Aliohjelma, joka lisää peliin ohjaimet
    /// </summary>
    private void LuoOhjaimet()
    {
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Space, ButtonState.Down, LiikuYlos, "Liiku ylös", p1, 100.0);
        Keyboard.Listen(Key.Right, ButtonState.Down, Liiku, "Liiku oikealle", p1, 200.0);
        Keyboard.Listen(Key.Left, ButtonState.Down, Liiku, "Liiku vasemmalle", p1, -200.0);
        Keyboard.Listen(Key.Tab, ButtonState.Pressed, ShowControlHelp, "Näytä näppäinohjeet");
        Keyboard.Listen(Key.F1, ButtonState.Pressed, Begin, "Aloita uusi peli");
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


    private void Kerays(PhysicsObject pelaaja, PhysicsObject kohde)
    {
        kohde.Destroy();
        pistelaskuri.Value += 1;
    }


    /// <summary>
    /// Aliohjelma, jossa määritetään mitä tapahtuu maaliin tultaessa
    /// </summary>
    /// <param name="pelaaja">pelaaja joka tulee maaliin</param>
    /// <param name="maali">maali</param>
    private void maaliintulo(PhysicsObject pelaaja, PhysicsObject maali)
    {
        maali.Destroy();
        string[] vaihtoehdot = { "Uusi peli", "Näytä Ohjaimet", "Parhaat pisteet", "Lopeta peli" };
        MultiSelectWindow loppuvalikko = new MultiSelectWindow("Voitit pelin! Keräsit " + pistelaskuri + "pistettä!", vaihtoehdot);
        PushButton[] nappula = loppuvalikko.Buttons;
        loppuvalikko.Color = Color.Gold;
        loppuvalikko.SetButtonColor(Color.Black);
        loppuvalikko.SetButtonTextColor(Color.Gold);
        nappula[3].Color = Color.Red;
        Add(loppuvalikko); 

        loppuvalikko.AddItemHandler(0, Begin);
        loppuvalikko.AddItemHandler(1, ShowControlHelp);
        ///alkuvalikko.AddItemHandler(1, ParhaatPisteet);
        loppuvalikko.AddItemHandler(3, Exit);
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

}


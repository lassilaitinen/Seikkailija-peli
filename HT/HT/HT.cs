using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;
using System;
using System.Collections.Generic;

public class HT : PhysicsGame
{
    private PlatformCharacter p1;


    public override void Begin()
    {
        ClearGameObjects();
        ClearControls();

        Gravity = new Vector(0, -1500.0);

        LuoKentta();
        LuoOhjaimet();

        Camera.Follow(p1);
        Camera.ZoomFactor = 1.2;
        Camera.StayInLevel = true;
        Level.Background.Image = LoadImage("ohj1Pelitausta");
    }


    private void LuoKentta()
    {
        TileMap kentta = TileMap.FromLevelAsset("kentta1.txt");
        kentta.SetTileMethod('#', Este);
        //kentta.SetTileMethod('*', Kolikko);
        kentta.SetTileMethod('S', LuoPelaaja);
        kentta.SetTileMethod('M', Maali);
        kentta.Execute(40, 40);
        Level.CreateBottomBorder();
        Level.CreateLeftBorder();
        Level.CreateRightBorder();


        //AddCollisionHandler<PlatformCharacter, PhysicsObject>(p1, Maaliintulo);


    }

    //void Maaliintulo(PlatformCharacter p1, PhysicsObject maali)
    //{
    //MessageDisplay.Add("Voitit pelin!!");

    //}

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

    private void Este(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject este = PhysicsObject.CreateStaticObject(leveys, korkeus, Shape.Rectangle);
        este.Position = paikka;
        este.Color = Color.Brown;
        este.Tag = "este";
        Add(este);
    }

    private void Maali(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject maali = PhysicsObject.CreateStaticObject(leveys, korkeus, Shape.Triangle);
        maali.Position = paikka;
        maali.Color = Color.Yellow;
        maali.Tag = "maali";
        Add(maali);
    }



    private void LuoOhjaimet()
    {
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Space, ButtonState.Down, LiikuYlos, "Liiku ylös", p1, 100.0);
        Keyboard.Listen(Key.Right, ButtonState.Down, Liiku, "Liiku oikealle", p1, 200.0);
        Keyboard.Listen(Key.Left, ButtonState.Down, Liiku, "Liiku vasemmalle", p1, -200.0);
        Keyboard.Listen(Key.Tab, ButtonState.Pressed, ShowControlHelp, "Näytä näppäinohjeet");
        Keyboard.Listen(Key.F1, ButtonState.Pressed, Begin, "Aloita uusi peli");
    }

    private void Liiku(PlatformCharacter p1, double suunta)
    {
        p1.Walk(suunta);
    }

    private void LiikuYlos(PlatformCharacter p1, double nopeus)
    {
        p1.Jump(nopeus);
    }

}


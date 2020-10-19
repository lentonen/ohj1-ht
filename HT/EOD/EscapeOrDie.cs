using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;

public class EscapeOrDie : PhysicsGame
{

    private PlatformCharacter pelaaja;
    private PhysicsObject liikkuvaTaso;
    private PhysicsObject[] liikkuvatTasot;
    private int avainLoytynyt;
    private DoubleMeter matkaLaskuri;
    private IntMeter hypyt;




    public override void Begin()
    {
        LuoKentta();
        LisaaNappaimet();      
    }


    /// <summary>
    /// Luodaan kenttä tilemap:n avulla
    /// </summary>
    private void LuoKentta()
    {
        Gravity = new Vector(0, -1000);
        TileMap kentta = TileMap.FromLevelAsset("kentta1.txt");
        kentta.SetTileMethod('#', LisaaTaso);
        kentta.SetTileMethod('*', LisaaEliksiiri);
        kentta.SetTileMethod('P', LisaaPelaaja);
        kentta.SetTileMethod('O', LisaaOvi);
        kentta.SetTileMethod('A', LisaaAvain);
        kentta.SetTileMethod('N', LisaaNappi);
        kentta.SetTileMethod('E', LisaaPiikit);
        kentta.Execute(20, 20);
        Level.CreateBorders();
        Level.Background.CreateGradient(Color.Black, Color.White);

        liikkuvatTasot = new PhysicsObject[4];
        liikkuvatTasot[0] = LisaaLiikkuvaTaso(new Vector(-200,-150), 50, 20, 0.5 * Math.PI, Vector.UnitX);
        liikkuvatTasot[1] = LisaaLiikkuvaTaso(new Vector(-100, -150), 50, 20, -0.5 * Math.PI, Vector.UnitX);
        liikkuvatTasot[2] = LisaaLiikkuvaTaso(new Vector(-200, -150), 50, 20, 0.5 * Math.PI, Vector.UnitY);
        liikkuvatTasot[3] = LisaaLiikkuvaTaso(new Vector(-100, -150), 50, 20, -0.5 * Math.PI, Vector.UnitY);

        LuoMatkaLaskuri();
        HyppyLaskuri();

        Camera.Follow(pelaaja);
        Camera.ZoomFactor = 5;
        Camera.StayInLevel = true;
    }


    /// <summary>
    /// Lisää pelikentän tasoja haluttuun paikkaan.
    /// </summary>
    /// <param name="paikka">Paikka johon taso lisätään</param>
    /// <param name="leveys">Tason leveys</param>
    /// <param name="korkeus">Tason korkeus</param>
    private void LisaaTaso(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject taso = PhysicsObject.CreateStaticObject(leveys, korkeus);
        taso.Position = paikka;
        taso.Color = Color.DarkOrange;
        Add(taso);
    }


    private PhysicsObject LisaaLiikkuvaTaso(Vector paikka, double leveys, double korkeus, double vaihe, Vector suunta)
    {
        liikkuvaTaso = new PhysicsObject(leveys, korkeus);
        liikkuvaTaso.IgnoresGravity = true;
        liikkuvaTaso.MakeStatic();
        liikkuvaTaso.Position = paikka;
        liikkuvaTaso.Color = Color.Black;
        liikkuvaTaso.Height = 5;
        Add(liikkuvaTaso);
        liikkuvaTaso.Oscillate(suunta, 30, 2, vaihe);
        return liikkuvaTaso;
    }

    /// <summary>
    /// Luo hidastuseliksiirin
    /// </summary>
    /// <param name="paikka">Paikka johon eliksiiri lisätään</param>
    /// <param name="leveys">Eliksiiripurkin leveys</param>
    /// <param name="korkeus">Eliksiiripurkin korkeus</param>
    private void LisaaEliksiiri(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject eliksiiri = PhysicsObject.CreateStaticObject(leveys, korkeus);
        eliksiiri.IgnoresCollisionResponse = true;
        eliksiiri.Position = paikka;
        eliksiiri.Image = LoadImage("eliksiiri.png");
        eliksiiri.Tag = "eliksiiri";
        Add(eliksiiri);
    }

    private void LisaaPiikit(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject piikki = PhysicsObject.CreateStaticObject(leveys, korkeus);
        piikki.IgnoresCollisionResponse = true;
        piikki.Position = paikka;
        piikki.Image = LoadImage("piikit.png");
        piikki.Tag = "piikit";
        Add(piikki);
    }


    /// <summary>
    /// Luo oven, johon kenttä päättyy
    /// </summary>
    /// <param name="paikka">Oven paikka</param>
    /// <param name="leveys">Oven leveys</param>
    /// <param name="korkeus">Oven korkeus</param>
    private void LisaaOvi(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject ovi = PhysicsObject.CreateStaticObject(leveys, korkeus);
        ovi.IgnoresCollisionResponse = true;
        ovi.Position = paikka;
        ovi.Image = LoadImage("ovi.png");
        ovi.Tag = "ovi";
        Add(ovi);
    }

    
    private void LisaaAvain(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject avain = PhysicsObject.CreateStaticObject(leveys, korkeus);
        avain.IgnoresCollisionResponse = true;
        avain.Position = paikka;
        avain.Image = LoadImage("avain.png");
        avain.Tag = "avain";
        avain.Height = 10;
        Add(avain);
    }


    /// <summary>
    /// Luo pelaajan
    /// </summary>
    /// <param name="paikka">Pelaajan paikka</param>
    /// <param name="leveys">Pelaajan leveys</param>
    /// <param name="korkeus">Pelaajan korkeus</param>
    private void LisaaPelaaja(Vector paikka, double leveys, double korkeus)
    {
        
        pelaaja = new PlatformCharacter(leveys, korkeus);
        pelaaja.Position = paikka;
        pelaaja.Height = 15;
        pelaaja.Mass = 4.0;
        pelaaja.Image = LoadImage("pelaaja.png");
        AddCollisionHandler(pelaaja, "eliksiiri", TormaaEliksiiriin);
        AddCollisionHandler(pelaaja, "ovi", PelaajaOvella);
        AddCollisionHandler(pelaaja, "avain", PelaajaAvain);
        AddCollisionHandler(pelaaja, "piikit", PelaajaPiikit);
        Add(pelaaja);
    }


    private void LisaaNappi(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject nappi = PhysicsObject.CreateStaticObject(leveys, korkeus);
        nappi.IgnoresCollisionResponse = true;
        nappi.Position = paikka;
        nappi.Image = LoadImage("nappi.png");
        nappi.Tag = "nappi";
        nappi.Height = 5;
        nappi.Width = 5;
        Add(nappi);
    }



    /// <summary>
    /// Lisää peliin näppäimet
    /// </summary>
    private void LisaaNappaimet()
    {
        double nopeus = 200;
        double hyppynopeus = 400;

        Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "Näytä ohjeet");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");

        Keyboard.Listen(Key.Left, ButtonState.Down, Liikuta, "Liikkuu vasemmalle", pelaaja, -nopeus);
        Keyboard.Listen(Key.Right, ButtonState.Down, Liikuta, "Liikkuu vasemmalle", pelaaja, nopeus);
        Keyboard.Listen(Key.Up, ButtonState.Pressed, Hyppaa, "Pelaaja hyppää", pelaaja, hyppynopeus);

        /*ControllerOne.Listen(Button.Back, ButtonState.Pressed, Exit, "Poistu pelistä");

        ControllerOne.Listen(Button.DPadLeft, ButtonState.Down, Liikuta, "Pelaaja liikkuu vasemmalle", pelaaja, -nopeus);
        ControllerOne.Listen(Button.DPadRight, ButtonState.Down, Liikuta, "Pelaaja liikkuu oikealle", pelaaja, nopeus);
        ControllerOne.Listen(Button.A, ButtonState.Pressed, Hyppaa, "Pelaaja hyppää", pelaaja, hyppynopeus);*/

        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
    }

    
    /// <summary>
    /// Aliohjelma jonka avulla pelaajaa voidaan liikuttaa
    /// </summary>
    /// <param name="hahmo">Objekti jota halutaan liikuttaa</param>
    /// <param name="nopeus">Liikuttelunopeus</param>
    private void Liikuta(PlatformCharacter hahmo, double nopeus)
    {
        hahmo.Walk(nopeus);
    }


    /// <summary>
    /// Aliohjelma jonka avulla pelaaja voi hypätä
    /// </summary>
    /// <param name="hahmo">Objekti jonka halutaan hyppäävän</param>
    /// <param name="hyppyVoima">Hypyn voimakkuus</param>
    private void Hyppaa(PlatformCharacter hahmo, double hyppyVoima)
    {
        hahmo.Jump(hyppyVoima);
        hypyt.Value += 1;
    }


    /// <summary>
    /// Aliohjelma kun törmätään hidastuseliksiiriin
    /// </summary>
    /// <param name="hahmo">Törmääjä</param>
    /// <param name="eliksiiri">Törmäyksen kohde</param>
    private void TormaaEliksiiriin(PhysicsObject hahmo, PhysicsObject eliksiiri)
    {
        MessageDisplay.Add("Keräsit hidastuseliksiirin!");
        eliksiiri.Destroy();
    }


    private void PelaajaOvella(PhysicsObject pelaaja, PhysicsObject ovi)
    {
        if (avainLoytynyt == 1) MessageDisplay.Add("Amazing");
        else MessageDisplay.Add("Käyppä etsimässä avain");

    }


    private void PelaajaAvain(PhysicsObject pelaaja, PhysicsObject avain)
    {
        MessageDisplay.Add("Löysit avaimen!");
        avainLoytynyt += 1;
        avain.Destroy();
    }

    private void PelaajaPiikit(PhysicsObject pelaaja, PhysicsObject piikit)
    {
        MessageDisplay.Add("Kuolit piikkeihin");
        ClearAll();
        LuoKentta();
        LisaaNappaimet();
    }

    private void LuoMatkaLaskuri()
    {
        matkaLaskuri = new DoubleMeter(0);

        Timer aikaLaskurinTriggeri = new Timer();
        aikaLaskurinTriggeri.Interval = 0.05;
        aikaLaskurinTriggeri.Timeout += LaskeJosNappiPohjassa;
        aikaLaskurinTriggeri.Start();

        Label aikaNaytto = new Label();
        aikaNaytto.X = Screen.Right - 100;
        aikaNaytto.Y = Screen.Top - 150;
        aikaNaytto.TextColor = Color.White;
        aikaNaytto.DecimalPlaces = 1;
        aikaNaytto.BindTo(matkaLaskuri);
        aikaNaytto.DoubleFormatString = "Liikuttu matka {0:N1}m";
        Add(aikaNaytto);
    }

    private void LaskeJosNappiPohjassa()
    {
        if (Keyboard.GetKeyState(Key.Left) == ButtonState.Down) matkaLaskuri.Value += 0.1;
        if (Keyboard.GetKeyState(Key.Right) == ButtonState.Down) matkaLaskuri.Value += 0.1;
    }

    private void HyppyLaskuri()
    {
        hypyt = new IntMeter(0);

        Label hypytNaytto = new Label();
        hypytNaytto.X = Screen.Right - 157;
        hypytNaytto.Y = Screen.Top - 100;
        hypytNaytto.TextColor = Color.White;
        hypytNaytto.Title = "Hypyt";

        hypytNaytto.BindTo(hypyt);
        Add(hypytNaytto);
       
       // if (Keyboard.GetKeyState(Key.Up) == ButtonState.Pressed) hypyt.Value += 1;
    }


}


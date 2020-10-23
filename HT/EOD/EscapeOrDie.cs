using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Input;

public class EscapeOrDie : PhysicsGame
{
    // Pelaajaan liittyvät
    private PlatformCharacter pelaaja;          // PlatformCharacter-tyyppinen pelaaja
    private int elamatAlussa = 3;               // Pelaajan elämät alussa
    private int avainLoytynyt;                  // Muuttuja joka reagoi avainmen löytymiseen
    private int kenttaNro = 1;
    
    
    //Laskureihin liittyvät
    private DoubleMeter matkaLaskuri;           // Pelaajan liikkumaa matkaa laskeva laskuri
    private IntMeter hypyt;                     // Pelaajan hyppyjä laskeva laskuri
    private IntMeter eliksiirit;                // Pelaajan keräämiä eliksiirejä laskeva laskuri
    private double kuolemanLuku;                // Luku joka lasketaan eliksiirien, hyppyjen ja matkan perusteella
    private double kuolemanLuvunRaja = 20;      // Raja-arvo, jonka ylittäminen aiheuttaa pelaajan tuhoutumisen kentän lopussa. 
    private MessageDisplay kuolemanNaytto;      // Näyttö joka näyttää pelaajan selviytymisen laskureista laskettujen arvojen perusteella.

    // Valikkoon liittyvät
    private Label[] valikko;                    // Label-taulukko valikon napeista
    private Label nappiAloita;                  // Nappi joka käynnistää pelin. Attribuuttina koska käytetään parissa eri kohdassa.
    private Font teksti;                        // Fontti jota käytetään laskureissa ja valikon tarinassa.

    // Pelikenttään liittyvät
    private PhysicsObject liikkuvaTaso;         // Pelikentällä liikkuvat tasot

    // Äänet
    SoundEffect hyppyAani = LoadSoundEffect("hyppy");
    SoundEffect eliksiiriKeraysAani = LoadSoundEffect("eliksiirinkerays");
    SoundEffect evilNauru = LoadSoundEffect("evilNauru");

    // Fontit
    




    /// <summary>
    /// Escape Or Die- peli
    /// </summary>
    public override void Begin()
    {
        Alkuvalikko(); 
    }


    /// <summary>
    /// Luo alkuvalikon, josta voidaan käynnistää peli ja lukea pelin tarina
    /// </summary>
    private void Alkuvalikko()
    {
        ClearAll();
        Font valikonFontti = LoadFont("valikkoFontti.ttf");
        MediaPlayer.Play("taustamusa");
        MediaPlayer.IsRepeating = true;
        teksti = LoadFont("teksti.ttf");
        Level.Background.CreateGradient(Color.Black, Color.White);
        valikko = new Label[3];
        
        
        nappiAloita = new Label("Aloita Peli");         // Luo näppäimen "Aloita Peli"
        nappiAloita.Font = valikonFontti;
        nappiAloita.Position = new Vector(0, 50);
        valikko[0] =nappiAloita;
        Add(nappiAloita);

        Label nappiTarina = new Label("Lue Tarina");    // Luo näppäimen "Lue Tarina"
        nappiTarina.Font = valikonFontti;
        nappiTarina.Position = new Vector(0, -50);
        valikko[1] = nappiTarina;
        Add(nappiTarina);

        Label nappiUusiPeli = new Label("Uusi Peli");   // Luo näppäimen "Uusi Peli"
        nappiUusiPeli.Position = new Vector(0, -100);
        valikko[2] = nappiUusiPeli;

        Mouse.ListenOn(nappiAloita, MouseButton.Left, ButtonState.Pressed, UusiPeli, null);
        Mouse.ListenOn(nappiTarina, MouseButton.Left, ButtonState.Pressed, NaytaTarina, null);
        Mouse.ListenMovement(1.0, ValikkoLiike, null);
    }


    /// <summary>
    /// Luo valikon, jonka taustalla teksti Game Over. Tekstin alapuolella painike "Uusi Peli".
    /// </summary>
    private void GameOverValikko()
    {
        ClearAll();

        Font valikonFontti = LoadFont("valikkoFontti.ttf");         
        Level.Background.CreateGradient(Color.Black, Color.White);
        elamatAlussa = 3;
        
        Label gameOver = new Label(600, 300, "GAME OVER");
        gameOver.SizeMode = TextSizeMode.StretchText;
        gameOver.Font = valikonFontti;
        gameOver.Position = new Vector(0, 100);
        Add(gameOver);

        valikko[2].Position = new Vector(0, -100);      // taulukossa valikko[2]= nappiUusiPeli
        valikko[2].Font = valikonFontti;

        Add(valikko[2]);
        Mouse.ListenOn(valikko[2], MouseButton.Left, ButtonState.Pressed, UusiPeli, null);
        Mouse.ListenMovement(1.0, ValikkoLiike, null);
    }


    /// <summary>
    /// Käynnistää uuden pelin.
    /// </summary>
    private void UusiPeli()
    {
        ClearAll();
        LuoKentta();
        LisaaNappaimet();
    }


    private void NaytaTarina()
    {
        ClearAll();
        
        Level.Background.CreateGradient(Color.Black, Color.White);
        Label tarina1 = new Label(600, 600, "Ilkeät ulkoavaruuden oliot ovat kaapanneet sinut. He tekevät sinulla kieroja testejä, joiden avulla tarkkailevat käytöstäsi."
            +"Pysyt hengissä vain kun käyttäydyt testaajien antamien sääntöjen mukaan.");
        tarina1.Font = teksti;
        tarina1.SizeMode = TextSizeMode.Wrapped;
        tarina1.Position = new Vector(0, 100);
        Add(tarina1);
       

        nappiAloita.Position = new Vector(0, -50);
        Add(nappiAloita);
        Mouse.ListenOn(nappiAloita, MouseButton.Left, ButtonState.Pressed, UusiPeli, null);
        Mouse.ListenMovement(1.0, ValikkoLiike, null);
    }


    private void ValikkoLiike()
    {
        foreach (Label nappi in valikko)
        {
            if (Mouse.IsCursorOn(nappi))
            {
                nappi.TextColor = Color.Red;
            }
            else nappi.TextColor = Color.White;
        }
    }


    /// <summary>
    /// Luo kentän tilemap:n avulla. Kentällä liikkuvat tasot luodaan omalla aliohjelmalla.
    /// </summary>
    private void LuoKentta()
    {
        Gravity = new Vector(0, -1000);
        TileMap[] kenttaTaulukko = new TileMap[2];
        kenttaTaulukko[0] = TileMap.FromLevelAsset("kentta1.txt");
        kenttaTaulukko[1] = TileMap.FromLevelAsset("kentta2.txt");

        TileMap kentta = kenttaTaulukko[kenttaNro - 1];

        kentta.SetTileMethod('#', LisaaTaso);
        kentta.SetTileMethod('*', LisaaEliksiiri);
        kentta.SetTileMethod('P', LisaaPelaaja);
        kentta.SetTileMethod('O', LisaaOvi);
        kentta.SetTileMethod('A', LisaaAvain);
        kentta.SetTileMethod('E', LisaaPiikit);
        kentta.Execute(20, 20);
        Level.CreateBorders();
        Level.Background.CreateGradient(Color.Black, Color.White);

        MessageDisplay.Font = teksti;
        LuoTasot();
        LuoMatkaLaskuri();
        HyppyLaskuri();
        LuoEliksiiriLaskuri();
        KuolemanNaytto();
        LuoElamatNaytto();

        Camera.Follow(pelaaja);
        Camera.ZoomFactor = 5;
        Camera.StayInLevel = true;
    }


    // Pelikentän rakentamiseen tarvittavat aliohjelmat
    //-----------------------------------------------------------------------------------------------------------------------------------------------------


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


    /// <summary>
    /// Luo liikkuvan tason.
    /// </summary>
    /// <param name="paikka">Tason keskipisteen paikka</param>
    /// <param name="leveys">Tason leveys</param>
    /// <param name="korkeus">Tason korkeus</param>
    /// <param name="vaihe">Tason värähtelyn vaihe</param>
    /// <param name="suunta"></param>
    /// <returns>Liikkuva taso</returns>
    private PhysicsObject LisaaLiikkuvaTaso(Vector paikka, double leveys, double korkeus, double vaihe, Vector suunta)
    {
        liikkuvaTaso = new PhysicsObject(leveys, korkeus);
        liikkuvaTaso.IgnoresGravity = true;
        liikkuvaTaso.MakeStatic();
        liikkuvaTaso.Position = paikka;
        liikkuvaTaso.Restitution = 0;
        liikkuvaTaso.Color = Color.Black;
        liikkuvaTaso.Height = 5;
        Add(liikkuvaTaso);
        liikkuvaTaso.Oscillate(suunta, 75, 0.2, vaihe);
        return liikkuvaTaso;
    }


    /// <summary>
    /// Aliohjelma joka luo kentälle liikkuvat tasot
    /// </summary>
    private void LuoTasot()
    {

        LisaaLiikkuvaTaso(new Vector(150, 125), 50, 20, 0.5 * Math.PI, Vector.UnitX);
        LisaaLiikkuvaTaso(new Vector(-350, 125), 50, 20, -0.35 * Math.PI, Vector.UnitX);
        LisaaLiikkuvaTaso(new Vector(-100, 250), 50, 20, 0.75 * Math.PI, Vector.UnitY);
        LisaaLiikkuvaTaso(new Vector(-425, 150), 50, 20, -0.5 * Math.PI, Vector.UnitY);
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

    
    /// <summary>
    /// Luo piikit, johon törmäämällä pelaaja kuolee.
    /// </summary>
    /// <param name="paikka">Piikkien paikka</param>
    /// <param name="leveys">Piikkien leveys</param>
    /// <param name="korkeus">Piikkien korkeus</param>
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

    
    /// <summary>
    /// Luo avaimen, joka pelaajan täytyy hakea ennen kuin pääsee kentän läpi.
    /// </summary>
    /// <param name="paikka">Avaimen paikka</param>
    /// <param name="leveys">Avaimen leveys</param>
    /// <param name="korkeus">Avaimen korkeus</param>
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
    

    // Näppäimet ja liikkuminen
    //-----------------------------------------------------------------------------------------------------------------------------------------------------

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
        hyppyAani.Play();
        hahmo.Jump(hyppyVoima);
        hypyt.Value += 1;
    }


    // Toiminnot kun kohdataan erilaisia objekteja
    //-----------------------------------------------------------------------------------------------------------------------------------------------------


    /// <summary>
    /// Aliohjelma kun törmätään hidastuseliksiiriin
    /// </summary>
    /// <param name="hahmo">Törmääjä</param>
    /// <param name="eliksiiri">Törmäyksen kohde</param>
    private void TormaaEliksiiriin(PhysicsObject hahmo, PhysicsObject eliksiiri)
    {
        eliksiiriKeraysAani.Play();
        eliksiirit.Value += 1;
        eliksiiri.Destroy();
    }


    /// <summary>
    /// Aliohjelma kun törmätään oveen.
    /// </summary>
    /// <param name="pelaaja"></param>
    /// <param name="ovi"></param>
    private void PelaajaOvella(PhysicsObject pelaaja, PhysicsObject ovi)
    {
        
        if (avainLoytynyt == 1 && kuolemanLuku < kuolemanLuvunRaja) 
        {
            SoundEffect kenttaLapi = LoadSoundEffect("aplodit");
            kenttaLapi.Play();
            MessageDisplay.Add("Amazing");
            kenttaNro++;
            UusiPeli();
        }
        
        else MessageDisplay.Add("Käyppä etsimässä avain");
        if (avainLoytynyt == 1 && kuolemanLuku > kuolemanLuvunRaja) 
        {
            avainLoytynyt = 0;
            PelaajaKuolee();
        }   
        

    }


    /// <summary>
    /// Aliohjelma kun törmätään avaimeen
    /// </summary>
    /// <param name="pelaaja"></param>
    /// <param name="avain"></param>
    private void PelaajaAvain(PhysicsObject pelaaja, PhysicsObject avain)
    {
        SoundEffect avaimenKeraysAani = LoadSoundEffect("avaimenKerays");
        avaimenKeraysAani.Play();
        avainLoytynyt += 1;
        avain.Destroy();
    }


    /// <summary>
    /// Aliohjelma kun törmätään piikkeihin
    /// </summary>
    /// <param name="pelaaja"></param>
    /// <param name="piikit"></param>
    private void PelaajaPiikit(PhysicsObject pelaaja, PhysicsObject piikit)
    {
        PelaajaKuolee();
    }


    /// <summary>
    /// Ohjelma joka suoritetaan pelaajan kuollessa.
    /// </summary>
    private void PelaajaKuolee()
    {
        SoundEffect kuolemanAani = LoadSoundEffect("kuolema");
        Sound kuolema = kuolemanAani.CreateSound();
        kuolema.Volume = 1.0;
        kuolema.Play();
        elamatAlussa--;
        avainLoytynyt = 0;
        pelaaja.Destroy();
        evilNauru.Play();
        if (elamatAlussa > 0) Timer.SingleShot(1, UusiPeli);
        else GameOverValikko();
        

        
    }





    // Laskurit
    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    
    /// <summary>
    /// Laskuri joka laskee pelaajan kulkemaa matkaa
    /// </summary>
    private void LuoMatkaLaskuri()
    {
        matkaLaskuri = new DoubleMeter(0);

        Timer aikaLaskurinTriggeri = new Timer();
        aikaLaskurinTriggeri.Interval = 0.05;
        aikaLaskurinTriggeri.Timeout += LaskeJosNappiPohjassa;
        aikaLaskurinTriggeri.Start();

        Label aikaNaytto = new Label();
        aikaNaytto.Font = teksti;
        aikaNaytto.X = Screen.Right - 117;
        aikaNaytto.Y = Screen.Top - 150;
        aikaNaytto.TextColor = Color.White;
        aikaNaytto.DecimalPlaces = 1;
        aikaNaytto.BindTo(matkaLaskuri);
        aikaNaytto.DoubleFormatString = "Liikuttu matka {0:N1}m";
        Add(aikaNaytto);
    }

    
    /// <summary>
    /// Aliohjelma jota käytetään kuljettua matkaa laskettaessa. Matka lasketaan left- ja right- nappien painallusten perusteella.
    /// </summary>
    private void LaskeJosNappiPohjassa()
    {
        if (Keyboard.GetKeyState(Key.Left) == ButtonState.Down) matkaLaskuri.Value += 0.1;
        if (Keyboard.GetKeyState(Key.Right) == ButtonState.Down) matkaLaskuri.Value += 0.1;
    }

    
    /// <summary>
    /// Laskuri joka laskee pelaajan hyppyjen lukumäärän ja näyttää sen näytöllä.
    /// </summary>
    private void HyppyLaskuri()
    {
        hypyt = new IntMeter(0);

        Label hypytNaytto = new Label();
        hypytNaytto.Font = teksti;
        hypytNaytto.X = Screen.Right - 177;
        hypytNaytto.Y = Screen.Top - 100;
        hypytNaytto.TextColor = Color.White;
        hypytNaytto.Title = "Hypyt";

        hypytNaytto.BindTo(hypyt);
        Add(hypytNaytto);
        // if (Keyboard.GetKeyState(Key.Up) == ButtonState.Pressed) hypyt.Value += 1;
    }


    /// <summary>
    /// Laskuri joka laskee pelaajan keräämät eliksiirit ja näyttää sen näytöllä.
    /// </summary>
    private void LuoEliksiiriLaskuri()
    {
        eliksiirit = new IntMeter(0);

        Label eliksiiritNaytto = new Label();
        eliksiiritNaytto.Font = teksti;
        eliksiiritNaytto.X = Screen.Right - 165;
        eliksiiritNaytto.Y = Screen.Top - 200;
        eliksiiritNaytto.TextColor = Color.White;
        eliksiiritNaytto.Title = "Eliksiirit";

        eliksiiritNaytto.BindTo(eliksiirit);
        Add(eliksiiritNaytto);
    }


    /// <summary>
    /// Luo näytön joka näyttää sen, selviääkö pelaaja hengissä päästyään ovelle.
    /// </summary>
    private void KuolemanNaytto()
    {
        kuolemanNaytto = new MessageDisplay();
        kuolemanNaytto.Font = teksti;
        kuolemanNaytto.TextColor = Color.White;
        kuolemanNaytto.BackgroundColor = Color.Black;
        kuolemanNaytto.MaxMessageCount = 0;
        kuolemanNaytto.Position = new Vector(350, 190);
        kuolemanNaytto.MessageTime = new TimeSpan(0,0,0,1);
        Add(kuolemanNaytto);

        Timer kuolemanLaskurinTriggeri = new Timer();
        kuolemanLaskurinTriggeri.Interval = 1;
        kuolemanLaskurinTriggeri.Timeout += NaytaKuolemanStatus;
        kuolemanLaskurinTriggeri.Start();
    }


    /// <summary>
    /// Laskee pelaajan liikkeiden ja kerättyjen eliksiirien perusteella sen, tapetaanko pelaaja kokeen lopussa vai ei.
    /// Ohjelma näyttää näytöllä pelaajan statuksen.
    /// </summary>
    private void NaytaKuolemanStatus()
    {
        kuolemanLuku = 1.0 * hypyt.Value + matkaLaskuri.Value - 1.0 * eliksiirit.Value;
        if (kuolemanLuku < kuolemanLuvunRaja)
            kuolemanNaytto.Add(" Selviät hengissä seuraavaan kokeeseen! ");
        else
            kuolemanNaytto.Add(" Sinut tullaan tappamaan! ");
    }


    /// <summary>
    /// Luo tekstikentän, jossa näytetään pelaajan elämät.
    /// </summary>
    private void LuoElamatNaytto()
    {
        string elamat = elamatAlussa.ToString();
        Label elamatNaytto = new Label("Elämät: " + elamat);
        elamatNaytto.Font = teksti;
        elamatNaytto.TextColor = Color.Red;
        elamatNaytto.X = Screen.Right - 172;
        elamatNaytto.Y = Screen.Top - 50;
        Add(elamatNaytto);
    }
}


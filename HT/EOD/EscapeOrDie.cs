using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Input;

/// @author Henri Leinonen
/// @version 26.10.2020
/// <summary>
/// Escape Or Die- tasohyppelypeli.
/// </summary>


public class EscapeOrDie : PhysicsGame
{
    // Pelaajaan liittyvät
    private PlatformCharacter pelaaja;          // PlatformCharacter-tyyppinen pelaaja
    private int elamatJaljella;                 // Pelaajan jäljellä olevat elämät
    private int avainLoytynyt;                  // Muuttuja joka reagoi avaimen löytymiseen
    private int kenttaNro = 1;                  // Kenttänumero


    //Laskureihin liittyvät
    private DoubleMeter matkaLaskuri;           // Pelaajan liikkumaa matkaa laskeva laskuri
    private IntMeter hypyt;                     // Pelaajan hyppyjä laskeva laskuri
    private IntMeter eliksiirit;                // Pelaajan keräämiä eliksiirejä laskeva laskuri
    private double kuolemanLuku;                // Luku joka lasketaan eliksiirien, hyppyjen ja matkan perusteella
    private double kuolemanLuvunRaja = 70;      // Raja-arvo, jonka ylittäminen aiheuttaa pelaajan tuhoutumisen kentän lopussa. 
    private MessageDisplay kuolemanNaytto;      // Näyttö joka näyttää pelaajan selviytymisen laskureista laskettujen arvojen perusteella.


    // Valikkoon liittyvät
    private List<Label> valikko;                // Label-lista valikon napeista. Tämä tarvitaan foreach-silmukkaa varten valikon animoinnissa.
    private Font teksti;                        // Fontti jota käytetään laskureissa ja valikon tarinassa.


    /// <summary>
    /// Käynnistää pelin alkuvalikon.
    /// </summary>
    public override void Begin()
    {
        elamatJaljella = 3;
        Alkuvalikko(); 
    }
    

    /// <summary>
    /// Luo alkuvalikon, josta voidaan käynnistää peli ja lukea pelin tarina
    /// </summary>
    private void Alkuvalikko()
    {
        ClearAll();

        // Valikon ulkoasu, fontit ja taustamusiikki
        Font valikonFontti = LoadFont("valikkoFontti.ttf");        
        MediaPlayer.Play("taustamusa");                             
        MediaPlayer.IsRepeating = true;
        teksti = LoadFont("teksti.ttf");
        Level.Background.CreateGradient(Color.Black, Color.White);
        
        // Valikon näppäimet
        valikko = new List<Label>();

        Label nappiAloita = LuoValikonNappain("Aloita Peli", valikonFontti, 0);     // Luo näppäimen "Aloita Peli"
        Label nappiTarina = LuoValikonNappain("Lue tarina", valikonFontti, 1);      // Luo näppäimen "Lue Tarina"
        Label aaniAsetukset = LuoValikonNappain("Mute Music", valikonFontti, 2);    // Luo näppäimen "Mute Music"

        // Hiiren kuuntelijat
        Mouse.ListenOn(nappiAloita, MouseButton.Left, ButtonState.Pressed, UusiPeli, null);
        Mouse.ListenOn(nappiTarina, MouseButton.Left, ButtonState.Pressed, NaytaTarina, null);
        Mouse.ListenOn(aaniAsetukset, MouseButton.Left, ButtonState.Pressed, taustaMusaPois, null);
        Mouse.ListenMovement(1.0, ValikkoLiike, null);
    }


    /// <summary>
    /// Luo valikon näppäimiä
    /// </summary>
    /// <param name="nappaimenTeksti">Teksti joka näytetään näppäimessä</param>
    /// <param name="fontti">käytettävä fontti</param>
    /// <param name="moneskoNappain">valikon näppäimen järjestysluku ylhäältä alas laskettuna</param>
    /// <returns></returns>
    private Label LuoValikonNappain(string nappaimenTeksti, Font fontti, int moneskoNappain)
    {
        Label nappi = new Label(nappaimenTeksti);
        nappi.Font = fontti;
        nappi.Position = new Vector(0, 100 - moneskoNappain * 100);
        valikko.Add(nappi);
        Add(nappi);
        return nappi;
    }


    /// <summary>
    /// Luo valikon, jonka taustalla teksti Game Over. Tekstin alapuolella painike "Uusi Peli".
    /// </summary>
    private void GameOverValikko()
    {
        ClearAll();
        elamatJaljella = 3;

        //Fontti ja tausta
        Font valikonFontti = LoadFont("valikkoFontti.ttf");         
        Level.Background.CreateGradient(Color.Black, Color.White);
        
        // "Game Over"- teksti
        Label gameOver = new Label(600, 300, "GAME OVER");
        gameOver.SizeMode = TextSizeMode.StretchText;
        gameOver.Font = valikonFontti;
        gameOver.Position = new Vector(0, 100);
        Add(gameOver);

        Label uusiPeli = LuoValikonNappain("Uusi peli", valikonFontti, 2);  // Luo "Uusi Peli"- painikkeen

        // Hiiren kuuntelijat
        Mouse.ListenOn(uusiPeli, MouseButton.Left, ButtonState.Pressed, UusiPeli, null);
        Mouse.ListenMovement(1.0, ValikkoLiike, null);
    }


    /// <summary>
    /// Poistaa taustamusiikin.
    /// </summary>
    private void taustaMusaPois()
    {
        MediaPlayer.IsMuted = true;
    }


    /// <summary>
    /// Ohjelma luo valikkoon näkymän, jossa yläosassa tekstiä
    /// ja sen alapuolella "uusi peli"-painike.
    /// </summary>
    /// <param name="sisalto">Teksti joka näytetään</param>
    private void tarinaValikko(string sisalto)
    {
        ClearAll();
        Font valikonFontti = LoadFont("valikkoFontti.ttf");
        Level.Background.CreateGradient(Color.Black, Color.White);

        // Tekstikenttä tarinalle
        Label tarina = new Label(600, 600, sisalto);
        tarina.Font = teksti;
        tarina.SizeMode = TextSizeMode.Wrapped;
        tarina.Position = new Vector(0, 100);
        Add(tarina);

        // "Uusi Peli"- näppäimen lisääminen 
        Label uusiPeli = LuoValikonNappain("Uusi peli", valikonFontti, 2);  // Luo "Uusi Peli"- painikkeen

        // Hiiren kuuntelijat
        Mouse.ListenOn(uusiPeli, MouseButton.Left, ButtonState.Pressed, UusiPeli, null);
        Mouse.ListenMovement(1.0, ValikkoLiike, null);
    }
    
    
    /// <summary>
    /// Näyttää pelin taustatarinan. Näytetään myös "Uusi Peli"- painike taustatarinan alapuolella.
    /// </summary>
    private void NaytaTarina()
    {
        tarinaValikko("Ilkeät ulkoavaruuden oliot ovat kaapanneet sinut. He tekevät sinulla kieroja testejä, joiden avulla tarkkailevat käytöstäsi."
            + "Pysyt hengissä vain kun käyttäydyt testaajien antamien sääntöjen mukaan.");   
    }


    /// <summary>
    /// Muuttaa valikon tekstien värin, kun hiiri viedään tekstin päälle.
    /// </summary>
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
    /// Käynnistää uuden pelin.
    /// </summary>
    private void UusiPeli()
    {
        ClearAll();
        LuoKentta();
        LisaaNappaimet();
    }


    /// <summary>
    /// Luo kentän tilemap:n avulla. Kentällä liikkuvat tasot luodaan omalla aliohjelmalla.
    /// </summary>
    private void LuoKentta()
    {
        if (kenttaNro == 3) peliLoppu();
        else
        {
            Gravity = new Vector(0, -1000);    // Painovoima

            // Luodaan TileMap-taulukkon, johon tallennetaan kaikki kentät.
            TileMap[] kenttaTaulukko = new TileMap[2];
            kenttaTaulukko[kenttaNro - 1] = TileMap.FromLevelAsset("kentta" + kenttaNro + ".txt");

            // Haetaan oikea kenttä taulukosta kenttaNro avulla ja luodaan kenttä.
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

            //Luodaan Laskurit, näytettävä teksti sekä liikkuvat tasot
            LuoLiikkuvatTasot(); // TODO: tee taulukko, josta haetaan tasojen paikat jokaisen levelin yhteydessä.
            LuoMatkaLaskuri();
            LuoHyppyLaskuri();
            LuoEliksiiriLaskuri();
            LuoKuolemanNaytto();
            LuoElamatNaytto();

            // Kameran asetukset
            Camera.Follow(pelaaja);
            Camera.ZoomFactor = 1;
            Camera.StayInLevel = true;
            //IsFullScreen = true;                          //Tämä päälle, jos halutaan FullScreen.
        }
    }


    /// <summary>
    /// Näyttää pelin lopputarinan. Näytetään myös "Uusi Peli"- painike tarinan alapuolella.
    /// </summary>
    private void peliLoppu()
    {
        kenttaNro = 1;
        tarinaValikko("Ilkeät ulkoavaruuden oliot päästivät sinut vapaaksi. Elät elämääsi onnellisempana kuin ennen kaapausta"
            + " ja toivot, että kaikki saisivat kokea saman kuin sinä!");
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
    private void LisaaLiikkuvaTaso(Vector paikka, double leveys, double korkeus, double vaihe, Vector suunta)
    {
        PhysicsObject liikkuvaTaso = new PhysicsObject(leveys, korkeus);
        liikkuvaTaso.IgnoresGravity = true;
        liikkuvaTaso.MakeStatic();
        liikkuvaTaso.Position = paikka;
        liikkuvaTaso.Restitution = 0;
        liikkuvaTaso.Color = Color.Black;
        liikkuvaTaso.Height = 5;
        liikkuvaTaso.Oscillate(suunta, 75, 0.2, vaihe);
        Add(liikkuvaTaso);    
    }


    /// <summary>
    /// Aliohjelma joka luo kentälle liikkuvat tasot
    /// </summary>
    private void LuoLiikkuvatTasot()
    {
        int[,] tasojenPaikat= new int[,]{{ 150, 125 ,-350, 125, -100, 250, -425, 150 },
                                         {   0, -50, 0,-100,  -200,   100, 200, -150  } } ;

        
        LisaaLiikkuvaTaso(new Vector(tasojenPaikat[kenttaNro - 1, 0], tasojenPaikat[kenttaNro - 1, 1]), 50, 20, 0.5 * Math.PI, Vector.UnitX);
        LisaaLiikkuvaTaso(new Vector(tasojenPaikat[kenttaNro - 1, 2], tasojenPaikat[kenttaNro - 1, 3]), 50, 20, -0.35 * Math.PI, Vector.UnitX);
        LisaaLiikkuvaTaso(new Vector(tasojenPaikat[kenttaNro - 1, 4], tasojenPaikat[kenttaNro - 1, 5]), 50, 20, 0.75 * Math.PI, Vector.UnitY);
        LisaaLiikkuvaTaso(new Vector(tasojenPaikat[kenttaNro - 1, 6], tasojenPaikat[kenttaNro - 1, 7]), 50, 20, -0.5 * Math.PI, Vector.UnitY);
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
        AddCollisionHandler(pelaaja, "eliksiiri", PelaajaEliksiiri);
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
    }


    /// <summary>
    /// Aliohjelma jonka avulla pelaajaa voidaan liikuttaa.
    /// </summary>
    /// <param name="hahmo">Objekti jota halutaan liikuttaa</param>
    /// <param name="nopeus">Liikuttelunopeus</param>
    private void Liikuta(PlatformCharacter hahmo, double nopeus)
    {
        hahmo.Walk(nopeus);
    }


    /// <summary>
    /// Aliohjelma jonka avulla pelaaja voi hypätä.
    /// </summary>
    /// <param name="hahmo">Objekti jonka halutaan hyppäävän</param>
    /// <param name="hyppyVoima">Hypyn voimakkuus</param>
    private void Hyppaa(PlatformCharacter hahmo, double hyppyVoima)
    {
        SoundEffect hyppyAani = LoadSoundEffect("hyppy");
        hyppyAani.Play();
        hahmo.Jump(hyppyVoima);
        hypyt.Value += 1;
    }


    // Toiminnot kun kohdataan erilaisia objekteja
    //-----------------------------------------------------------------------------------------------------------------------------------------------------


    /// <summary>
    /// Aliohjelma kun törmätään hidastuseliksiiriin.
    /// </summary>
    /// <param name="hahmo">Törmääjä</param>
    /// <param name="eliksiiri">Törmäyksen kohde</param>
    private void PelaajaEliksiiri(PhysicsObject hahmo, PhysicsObject eliksiiri)
    {
        SoundEffect eliksiiriKeraysAani = LoadSoundEffect("eliksiirinkerays");
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
    /// Aliohjelma kun törmätään avaimeen.
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
    /// Aliohjelma kun törmätään piikkeihin.
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
        SoundEffect evilNauru = LoadSoundEffect("evilNauru");
        Sound kuolema = kuolemanAani.CreateSound();
        kuolema.Volume = 1.0;
        kuolema.Play();
        elamatJaljella --;
        avainLoytynyt = 0;
        pelaaja.Destroy();
        evilNauru.Play();
        if (elamatJaljella > 0) Timer.SingleShot(1, UusiPeli);
        else GameOverValikko();     
    }


    // Laskurit
    //-----------------------------------------------------------------------------------------------------------------------------------------------------


    /// <summary>
    /// Nayttö jota voidaan käyttää IntMeter-mittarin lukeman näyttämiseen
    /// </summary>
    /// <param name="format">Muoto jossa teksti näytetään</param>
    /// <param name="mittari">Mihin mittariin näyttö on liitetty</param>
    /// <param name="moneskoMittari">Luku kertoo mittarin paikan ruudun oikeassa yläreunassa. 0 = ylin mittari, 1 = toisiksi ylin, jne...</param>
    /// <returns>Näyttö johon haluttu mittari on kytketty</returns>
    private Label LuoNaytto(string format, Jypeli.Color vari, IntMeter mittari, int moneskoMittari)
    {
        Label naytto = new Label(format);
        naytto.Font = teksti;
        naytto.IntFormatString = format;
        naytto.BindTo(mittari);
        naytto.Position = new Vector(Screen.Right - naytto.Width / 2 - 20,
                         Screen.Top - naytto.Height / 2 - moneskoMittari * naytto.Height);
        naytto.TextColor = vari;
        Add(naytto);
        return naytto;
    }


    /// <summary>
    /// Overload aliohjelma, jota voidaan käyttää DoubleMeter-mittarin lukeman näyttämiseen.
    /// </summary>
    /// <param name="format">Muoto jossa teksti näytetään</param>
    /// <param name="mittari">Mihin mittariin näyttö on liitetty</param>
    /// <param name="moneskoMittari">Luku kertoo mittarin paikan ruudun oikeassa yläreunassa. 0 = ylin mittari, 1 = toisiksi ylin, jne...</param>
    /// <returns>Näyttö johon haluttu mittari on kytketty</returns>
    private Label LuoNaytto(string format, Jypeli.Color vari, DoubleMeter mittari, int moneskoMittari)
    {
        Label naytto = new Label(format);
        naytto.Font = teksti;
        naytto.DoubleFormatString = format;
        naytto.BindTo(mittari);
        naytto.Position = new Vector(Screen.Right - naytto.Width / 2 - 20,
                         Screen.Top - naytto.Height / 2 - moneskoMittari * naytto.Height);
        naytto.TextColor = vari;
        Add(naytto);
        return naytto;
    }


    /// <summary>
    /// Luo laskurin joka näyttää pelaajan jäljellä olevat elämät.
    /// </summary>
    private void LuoElamatNaytto()
    {
        IntMeter elamaLaskuri = new IntMeter(elamatJaljella);
        LuoNaytto("Elämät: {0:0}", Color.Red, elamaLaskuri, 0);
    }


    /// <summary>
    /// Laskuri joka laskee pelaajan keräämät eliksiirit ja näyttää sen näytöllä.
    /// </summary>
    private void LuoEliksiiriLaskuri()
    {
        eliksiirit = new IntMeter(0);
        LuoNaytto("Eliksiirit: {0:0}", Color.White, eliksiirit, 1);
    }


    /// <summary>
    /// Laskuri joka laskee pelaajan hyppyjen lukumäärän ja näyttää sen näytöllä.
    /// </summary>
    private void LuoHyppyLaskuri()
    {
        hypyt = new IntMeter(0);
        LuoNaytto("Hypyt: {0:0}", Color.White, hypyt, 2);
    }


    /// <summary>
    /// Laskuri joka laskee pelaajan kulkemaa matkaa.
    /// </summary>
    private void LuoMatkaLaskuri()
    {
        matkaLaskuri = new DoubleMeter(0);

        Timer matkaLaskurinTriggeri = new Timer();
        matkaLaskurinTriggeri.Interval = 0.05;
        matkaLaskurinTriggeri.Timeout += LaskeJosNappiPohjassa;
        matkaLaskurinTriggeri.Start();

        LuoNaytto("Liikuttu matka: {0:N1}", Color.White, matkaLaskuri, 3);
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
    /// Luo näytön joka näyttää sen, selviääkö pelaaja hengissä päästyään ovelle.
    /// </summary>
    private void LuoKuolemanNaytto()
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
        kuolemanLuku = LaskeKuolemanLuku(hypyt.Value, matkaLaskuri.Value, eliksiirit.Value);
        if (kuolemanLuku < kuolemanLuvunRaja)
            kuolemanNaytto.Add(" Selviät hengissä seuraavaan kokeeseen! ");
        else
            kuolemanNaytto.Add(" Sinut tullaan tappamaan! ");
    }


    /// <summary>
    /// Laskee kuolemanluvun annettujen parametrien avulla.
    /// </summary>
    /// <param name="hyppyjenMaara">Pelaajan hyppyjen määrä</param>
    /// <param name="liikuttuMatka">Pelaajan liikkuma matka</param>
    /// <param name="keratytEliksiirit">Pelaajan keräämät eliksiirit</param>
    /// <returns>Lukuarvo jota käytetään sen arvioimiseen, selviääkö pelaaja seuraavaan kenttään.</returns>
    private double LaskeKuolemanLuku(int hyppyjenMaara, double liikuttuMatka, int keratytEliksiirit)
    {
       return 1.0 * hyppyjenMaara + liikuttuMatka - 1.0 * keratytEliksiirit;
    }
}
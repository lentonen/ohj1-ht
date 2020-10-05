using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;

public class EscapeOrDie : PhysicsGame
{
    PhysicsObject pelaaja;
    public override void Begin()
    {
       pelaaja = LuoSuorakaide(this, 20, 240, 0, 0);
       LuoKentta();

        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
    }


    void LuoKentta()
    {
        Level.CreateBorders(false);
        LuoSuorakaide(this, 20, 240, 24, 100);
    }
    
    /// <summary>
    /// Luo pelaajan
    /// </summary>
    /// <param name="peli">Peli johon pelaaja lisätään.</param>
    /// <param name="koko">Pelaajan koko (neliön sivun pituus).</param>
    /// <param name="x">Pelaajan aloituspaikan x-koordinaatti.</param>
    /// <param name="y">Pelaajan aloituspaikan y-koordinaatti</param>
    private PhysicsObject LuoSuorakaide(PhysicsGame peli, double leveys,double korkeus, double x, double y)
    {
        PhysicsObject suorakaide = new PhysicsObject(leveys, korkeus, Shape.Rectangle);
        suorakaide.X = x;
        suorakaide.Y = y;
        peli.Add(suorakaide);
        return suorakaide;
    }
}


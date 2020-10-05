using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;

public class EscapeOrDie : PhysicsGame
{
    public override void Begin()
    {
        LuoPelaaja(this, 20, 0, 0);

        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
    }
    /// <summary>
    /// Luo pelaajan
    /// </summary>
    /// <param name="peli">Peli johon pelaaja lisätään.</param>
    /// <param name="koko">Pelaajan koko (neliön sivun pituus).</param>
    /// <param name="x">Pelaajan aloituspaikan x-koordinaatti.</param>
    /// <param name="y">Pelaajan aloituspaikan y-koordinaatti</param>
    private static void LuoPelaaja(PhysicsGame peli, double koko, double x, double y)
    {
        PhysicsObject pelaaja = new PhysicsObject(koko, koko, Shape.Rectangle);
        pelaaja.X = x;
        pelaaja.Y = y;
        peli.Add(pelaaja);
    }
}


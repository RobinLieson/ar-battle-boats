using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AR_Battle_Boats
{
    /// <summary>
    /// Lists the types of Game Modes we have
    /// </summary>
    public enum GameMode
    {
        Single_Player,
        Local_Multiplayer,
        Network_Multiplayer,
        Menu
    }

    /// <summary>
    /// List of different Game States
    /// </summary>
    public enum GameState
    {
        Main_Menu,
        Game_Load,
        In_Game,
        Joining,
        Hosting
    }

    /// <summary>
    /// Enumerable for different ammo types
    /// </summary>
    public enum AmmoType
    {
        Round_Shot,
        Grape_Shot,
        Chain_Shot
    }
}

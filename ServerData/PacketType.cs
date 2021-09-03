using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerData
{
    public enum PacketType
    {
        Registration,
        Chat,
        CloseConnection,
        Battleship_set,
        Battleship_shot,
        Game_over
    }
}

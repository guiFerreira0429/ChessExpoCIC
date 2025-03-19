﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChessLogic;

namespace ChessUI;

public interface IClockUpdater
{
    void UpdateClock(Player player, string formattedTime);
}

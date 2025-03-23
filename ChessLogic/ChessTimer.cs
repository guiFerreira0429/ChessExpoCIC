using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessLogic;

public class ChessTimer
{
    public int InitialTime;
    public int Increment;

    public int RemainingTimeWhite;
    public int RemainingTimeBlack;  

    public Player ActiveClock;

    public bool IsPaused;

    public event Action<Player, int> OnTimeUpdated;

    private Timer _timer;
    private const int TickInterval = 1000;

    public ChessTimer(GameDuration initialTimeInSeconds, GameIncrement incrementInSeconds = 0, Player player = Player.White)
    {
        InitialTime = (int)initialTimeInSeconds;
        Increment = (int)incrementInSeconds;
        ActiveClock = player;
        Reset();
    }

    public void Reset()
    {
        RemainingTimeWhite = InitialTime;
        RemainingTimeBlack = InitialTime;
        IsPaused = true;
        _timer?.Dispose();
    }

    public void Start()
    {
        if (IsPaused)
        {
            IsPaused = false;
            _timer = new Timer(TimerTick, null, 0, TickInterval);
        }
    }

    public void Pause()
    {
        IsPaused = true;
        _timer.Dispose();
    }

    private void TimerTick(object state)
    {
        if (IsPaused)
        {
            return;
        } 

        if (ActiveClock == Player.White)
        {
            RemainingTimeWhite--;
            if (RemainingTimeWhite == 0)
            {
                Pause();
            }
        }
        else
        {
            RemainingTimeBlack--;
            if (RemainingTimeBlack == 0)
            {
                Pause();
            }
        }

        OnTimeUpdated?.Invoke(ActiveClock, ActiveClock == Player.White ? RemainingTimeWhite : RemainingTimeBlack);
    }

    public void ChangePlayer()
    {
        if (ActiveClock == Player.White)
        {
            RemainingTimeWhite += Increment;
            OnTimeUpdated?.Invoke(Player.White, RemainingTimeWhite);
            ActiveClock = Player.Black;
        }
        else if (ActiveClock == Player.Black)
        {
            RemainingTimeBlack += Increment;
            OnTimeUpdated?.Invoke(Player.Black, RemainingTimeBlack);
            ActiveClock = Player.White;
        }
        else
        {
            throw new Exception("Jogador Inválido na troca de Relógio");
        }
    }
}

public enum GameDuration
{
    Default = 600,
    Bullet = 60,
    Blitz = 300,
    Rapid = 900,
    Third = 1200,
    THalf = 1500,
    Half = 1800,
    Long = 3600
}

public enum GameIncrement
{
    Default = 0,
    One = 1,
    Three = 3,
    Five = 5,
    Ten = 10,
    Fifteen = 15,
    Twenty = 20
}

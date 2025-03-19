using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessLogic;

public class ChessTimer
{
    public int InitialTime { get; private set; }

    public int Increment { get; private set; }

    public int RemainingTimeWhite { get; private set; }
    public int RemainingTimeBlack { get; private set; }

    public Player ActiveClock { get; private set; }

    public bool IsPaused { get; private set; } = true;

    private DateTime lastTimestamp;

    public event Action<Player, string> OnTimeUpdated;

    public ChessTimer(int initialTimeInSeconds, int incrementInSeconds = 0)
    {
        InitialTime = initialTimeInSeconds;
        Increment = incrementInSeconds;

        Reset();
    }

    public void Reset()
    {
        RemainingTimeWhite = InitialTime;
        RemainingTimeBlack = InitialTime;
        ActiveClock = Player.None;
        IsPaused = true;
    }

    public void Start(Player player)
    {
        if (player == Player.None)
            return;

        ActiveClock = player;
        IsPaused = false;
        lastTimestamp = DateTime.Now;
    }

    public void Pause()
    {
        if (!IsPaused)
        {
            UpdateRemainingTime();
            IsPaused = true;
        }
    }

    public void Resume()
    {
        if (IsPaused && ActiveClock != Player.None)
        {
            IsPaused = false;
            lastTimestamp = DateTime.Now;
        }
    }

    public void SwitchClock()
    {
        if (!IsPaused)
        {
            UpdateRemainingTime();

            if (ActiveClock == Player.White)
            {
                RemainingTimeWhite += Increment;
                ActiveClock = Player.Black;
            }
            else if (ActiveClock == Player.Black)
            {
                RemainingTimeBlack += Increment;
                ActiveClock = Player.White;
            }

            lastTimestamp = DateTime.Now;
        }
    }

    public void UpdateRemainingTime()
    {
        if (IsPaused || ActiveClock == Player.None)
            return;

        TimeSpan elapsed = DateTime.Now - lastTimestamp;
        int secondsElapsed = (int)elapsed.TotalSeconds;

        if (ActiveClock == Player.White)
        {
            RemainingTimeWhite = Math.Max(0, RemainingTimeWhite - secondsElapsed);
            GetFormattedTime(Player.White);
        }
        else if (ActiveClock == Player.Black)
        {
            RemainingTimeBlack = Math.Max(0, RemainingTimeBlack - secondsElapsed);
            GetFormattedTime(Player.Black);
        }

        lastTimestamp = DateTime.Now;
    }

    public bool IsTimeOut(out Player losingPlayer)
    {
        UpdateRemainingTime();

        if (RemainingTimeWhite <= 0)
        {
            losingPlayer = Player.White;
            return true;
        }

        if (RemainingTimeBlack <= 0)
        {
            losingPlayer = Player.Black;
            return true;
        }

        losingPlayer = Player.None;
        return false;
    }

    private void GetFormattedTime(Player player)
    {
        int seconds = player == Player.White ? RemainingTimeWhite : RemainingTimeBlack;
        int minutes = seconds / 60;
        int remainingSeconds = seconds % 60;
        string formattedTime = $"{minutes:D2}:{remainingSeconds:D2}";

        OnTimeUpdated?.Invoke(player, formattedTime);
    }
}

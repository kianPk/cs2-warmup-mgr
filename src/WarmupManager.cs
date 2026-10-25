namespace WarmupMgr;

public sealed class WarmupConfig
{
    public int DurationSeconds { get; set; } = 90;
    public int ReadyThreshold { get; set; } = 6;
    public bool IdleOnPractice { get; set; } = true;
}

public sealed class WarmupManager
{
    private readonly WarmupConfig _cfg;
    private readonly HashSet<string> _ready = new(StringComparer.OrdinalIgnoreCase);
    private int _playerCount;

    public bool IsActive { get; private set; } = true;
    public bool WarmupLive { get; private set; }
    public DateTime? EndsAt { get; private set; }

    public WarmupManager(WarmupConfig? cfg = null) => _cfg = cfg ?? new();

    public void OnServerType(string t)
    {
        IsActive = !(_cfg.IdleOnPractice && t.Equals("Practice", StringComparison.OrdinalIgnoreCase));
    }

    public void Begin(DateTime now, int playerCount = 0)
    {
        if (!IsActive) return;
        WarmupLive = true;
        _ready.Clear();
        _playerCount = playerCount > 0 ? playerCount : _cfg.ReadyThreshold;
        EndsAt = now.AddSeconds(_cfg.DurationSeconds);
    }

    public void SetPlayerCount(int n) => _playerCount = Math.Max(0, n);

    public void Ready(string steamId) => _ready.Add(steamId);
    public void Unready(string steamId) => _ready.Remove(steamId);
    public int ReadyCount => _ready.Count;

    /// <summary>Percent of players marked ready (0–100).</summary>
    public double ReadyPercent
    {
        get
        {
            var denom = _playerCount > 0 ? _playerCount : _cfg.ReadyThreshold;
            if (denom <= 0) return 0;
            return Math.Round(100.0 * _ready.Count / denom, 1);
        }
    }

    /// <summary>Force-end warmup immediately.</summary>
    public void ForceEnd()
    {
        WarmupLive = false;
        EndsAt = DateTime.UtcNow;
    }

    /// <summary>Extend warmup by additional seconds.</summary>
    public void Extend(int seconds)
    {
        if (!WarmupLive || EndsAt is null || seconds <= 0) return;
        EndsAt = EndsAt.Value.AddSeconds(seconds);
    }

    /// <summary>Returns true when warmup should end.</summary>
    public bool Tick(DateTime now)
    {
        if (!IsActive || !WarmupLive) return false;
        if (_ready.Count >= _cfg.ReadyThreshold || (EndsAt is not null && now >= EndsAt))
        {
            WarmupLive = false;
            return true;
        }
        return false;
    }
}

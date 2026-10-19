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
    public bool IsActive { get; private set; } = true;
    public bool WarmupLive { get; private set; }
    public DateTime? EndsAt { get; private set; }

    public WarmupManager(WarmupConfig? cfg = null) => _cfg = cfg ?? new();

    public void OnServerType(string t)
    {
        IsActive = !(_cfg.IdleOnPractice && t.Equals("Practice", StringComparison.OrdinalIgnoreCase));
    }

    public void Begin(DateTime now)
    {
        if (!IsActive) return;
        WarmupLive = true;
        _ready.Clear();
        EndsAt = now.AddSeconds(_cfg.DurationSeconds);
    }

    public void Ready(string steamId) => _ready.Add(steamId);
    public void Unready(string steamId) => _ready.Remove(steamId);
    public int ReadyCount => _ready.Count;

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

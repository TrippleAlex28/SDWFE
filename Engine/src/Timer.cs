namespace Engine;

public sealed class Timer
{
    public Timer(float timeLimitInSeconds) : base()
    {
        this.TimeLimitInSeconds = timeLimitInSeconds;
    }

    public delegate void TimerEvent();
    public event TimerEvent? Finished;
    public event TimerEvent? Paused;
    public event TimerEvent? Unpaused;

    public bool IsPaused { get; private set; } = false;
    public bool RestartsOnFinish { get; set; } = true;
    public bool IsFinished => this.ElapsedTimeInSeconds >= this._timeLimitInSeconds;
    public float TimeLimitInSeconds
    {
        get => this._timeLimitInSeconds;
        set
        {
            // This is an example of defensive programming: make it easy for your peers
            // to use your code as intended.
            if (value <= 0)
                throw new Exception("Only positive limits are allowed.");

            this._timeLimitInSeconds = value;
        }
    }
    private float _timeLimitInSeconds;
    public float ElapsedTimeInSeconds { get; private set; } = 0;
    public float TimeLeftInSeconds => this._timeLimitInSeconds - this.ElapsedTimeInSeconds;
    public float TimeLeftToTimeLimitRatio => this.TimeLeftInSeconds / this._timeLimitInSeconds;
    public float TimeElapsedToTimeLimitRatio => this.ElapsedTimeInSeconds / this._timeLimitInSeconds;

    /// <summary>
    /// Call this every frame for the timer to work as you would expect.
    /// </summary>
    /// <param name="deltaSeconds">The time between the current and previous frame in seconds.</param>
    public void Update(float deltaSeconds)
    {
        // No update if paused.
        if (this.IsPaused)
            return;

        // If not finished, then simply update the elapsed time.
        if (!this.IsFinished)
        {
            this.ElapsedTimeInSeconds += deltaSeconds;
            return;
        }
        // Else, handle finishing logic.

        // Cap the elapsed time, so that the elapsed time does not overshoot the limit.
        this.ElapsedTimeInSeconds = this._timeLimitInSeconds;

        if (this.RestartsOnFinish)
            this.Restart();
        else
            this.Pause();

        // Broadcast the finished event for listeners on every finish.
        this.Finished?.Invoke();
    }

    public void Pause()
    {
        // Prevent pausing when the timer is already paused.
        if (this.IsPaused)
            return;

        this.IsPaused = true;
        this.Paused?.Invoke();
    }

    public void Unpause()
    {
        // Prevent unpausing when the timer is already unpaused.
        if (!this.IsPaused)
            return;

        this.IsPaused = false;
        this.Unpaused?.Invoke();
    }

    public void Restart()
    {
        this.ElapsedTimeInSeconds = 0;
        this.Unpause();
    }

    /// <summary>
    /// Skips the timer to a provided percentage of the configured limit in seconds.
    /// </summary>
    /// <param name="percentage">Typically a value between 0 and 1, respectively for 0% and 100%.</param>
    public void SkipTo(float percentage)
    {
        this.ElapsedTimeInSeconds = percentage * this._timeLimitInSeconds;
    }
}
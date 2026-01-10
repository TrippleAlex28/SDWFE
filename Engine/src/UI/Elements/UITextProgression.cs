using Engine.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.UI.Elements;

/// <summary>
/// Displays text with a typewriter animation effect, supporting pagination.
/// Pages can be split manually with "|p" or automatically when text overflows.
/// </summary>
public class UITextProgression : UIElement
{
    private const string PageDelimiter = "|p";
    private const string UISelectAction = "UISelect";
    
    private string _text;
    private SpriteFont _font;
    private float _layerDepth;
    
    private List<string> _pages = new();
    private Rectangle _cachedSlot;
    private int _currentPageIndex;
    private double _elapsedTimeSincePageStart;
    
    // Configuration
    public float SecondsPerCharacter { get; set; } = 0.05f;
    public Color TextColor { get; set; } = Color.White;
    public bool AutoAdvanceOnClick { get; set; } = true;
    
    // State
    public int CurrentPage => _currentPageIndex;
    public int TotalPages => _pages.Count;
    public bool IsOnLastPage => _currentPageIndex >= _pages.Count - 1;
    public bool IsPageFullyRevealed => _pages.Count > 0 && 
        GetVisibleCharacterCount() >= _pages[_currentPageIndex].Length;
    
    // Events
    public event Action? OnLastPageReached;
    public event Action? OnTextCompleted;
    public event Action? OnPageChanged;
    
    private bool _lastPageEventFired;

    public UITextProgression(string text, SpriteFont font, float layerDepth = 0.91f)
    {
        _text = text ?? string.Empty;
        _font = font;
        _layerDepth = layerDepth;
        _layoutDirty = true;
    }

    /// <summary>
    /// Changes the displayed text and resets to the first page.
    /// </summary>
    public void SetText(string text)
    {
        _text = text ?? string.Empty;
        Reset();
        RebuildPages();
    }

    /// <summary>
    /// Resets the text progression to the beginning.
    /// </summary>
    public void Reset()
    {
        _currentPageIndex = 0;
        _elapsedTimeSincePageStart = 0;
        _lastPageEventFired = false;
    }

    /// <summary>
    /// Advances to the next page if available.
    /// </summary>
    /// <returns>True if advanced to next page, false if already on last page.</returns>
    public bool NextPage()
    {
        if (_currentPageIndex >= _pages.Count - 1)
            return false;
        
        _currentPageIndex++;
        _elapsedTimeSincePageStart = 0;
        OnPageChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// Goes back to the previous page if available.
    /// </summary>
    /// <returns>True if went to previous page, false if already on first page.</returns>
    public bool PreviousPage()
    {
        if (_currentPageIndex <= 0)
            return false;
        
        _currentPageIndex--;
        _elapsedTimeSincePageStart = 0;
        OnPageChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// Skips the animation and reveals all text on the current page instantly.
    /// </summary>
    public void RevealCurrentPage()
    {
        if (_pages.Count > 0)
        {
            _elapsedTimeSincePageStart = _pages[_currentPageIndex].Length * SecondsPerCharacter;
        }
    }

    /// <summary>
    /// Jumps to a specific page.
    /// </summary>
    public void GoToPage(int pageIndex)
    {
        if (pageIndex < 0 || pageIndex >= _pages.Count)
            return;
        
        _currentPageIndex = pageIndex;
        _elapsedTimeSincePageStart = 0;
        OnPageChanged?.Invoke();
    }

    protected override void UpdateSelf(GameTime gameTime)
    {
        base.UpdateSelf(gameTime);
        
        Rectangle currentSlot = CalculateActualSlot();
        
        // Rebuild pages if size changed
        if (_cachedSlot != currentSlot)
        {
            _cachedSlot = currentSlot;
            RebuildPages();
        }

        _elapsedTimeSincePageStart += gameTime.ElapsedGameTime.TotalSeconds;

        // Fire last page event once
        if (IsOnLastPage && !_lastPageEventFired)
        {
            _lastPageEventFired = true;
            OnLastPageReached?.Invoke();
        }

        // Handle click interaction
        if (AutoAdvanceOnClick && 
            InputManager.Instance.IsActionPressed(UISelectAction) && 
            IsMouseInsideBounds())
        {
            HandleClick();
        }
    }
    
    private bool IsMouseInsideBounds()
    {
        Point mousePos = InputManager.Instance.MousePosition;
        Rectangle bounds = CalculateActualSlot();
        return mousePos.X >= GlobalPosition.X && 
               mousePos.X <= GlobalPosition.X + bounds.Width &&
               mousePos.Y >= GlobalPosition.Y && 
               mousePos.Y <= GlobalPosition.Y + bounds.Height;
    }

    private void HandleClick()
    {
        // If text is still animating, reveal it fully first
        if (!IsPageFullyRevealed)
        {
            RevealCurrentPage();
            return;
        }

        // If on last page and fully revealed, fire completion
        if (IsOnLastPage)
        {
            OnTextCompleted?.Invoke();
            return;
        }

        // Otherwise advance to next page
        NextPage();
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        base.DrawSelf(spriteBatch);

        if (_pages.Count == 0)
            return;

        _currentPageIndex = Math.Clamp(_currentPageIndex, 0, _pages.Count - 1);

        string currentPageText = _pages[_currentPageIndex];
        int visibleChars = GetVisibleCharacterCount();
        string visibleText = currentPageText[..Math.Min(visibleChars, currentPageText.Length)];

        DrawTextLines(spriteBatch, visibleText);
    }

    private void DrawTextLines(SpriteBatch spriteBatch, string text)
    {
        string[] lines = text.Split('\n');
        
        for (int i = 0; i < lines.Length; i++)
        {
            Vector2 position = new(
                GlobalPosition.X + Margin.X,
                GlobalPosition.Y + Margin.Y + _font.LineSpacing * i
            );
            
            spriteBatch.DrawString(
                _font,
                lines[i],
                position,
                TextColor,
                0f,
                Vector2.Zero,
                1f,
                SpriteEffects.None,
                _layerDepth
            );
        }
    }

    private int GetVisibleCharacterCount()
    {
        if (_pages.Count == 0)
            return 0;
        
        int maxChars = _pages[_currentPageIndex].Length;
        return Math.Clamp((int)(_elapsedTimeSincePageStart / SecondsPerCharacter), 0, maxChars);
    }

    private void RebuildPages()
    {
        _pages.Clear();
        
        Rectangle slot = CalculateActualSlot();
        if (string.IsNullOrEmpty(_text) || slot.Width <= 0 || slot.Height <= 0)
            return;

        // Split by manual page breaks first
        string[] manualPages = _text.Split(PageDelimiter);

        foreach (string manualPage in manualPages)
        {
            if (string.IsNullOrWhiteSpace(manualPage))
                continue;

            // Wrap text into lines that fit the width
            List<string> wrappedLines = WrapTextToLines(manualPage.Trim());
            
            // Split lines into pages that fit the height
            List<string> subPages = SplitLinesIntoPages(wrappedLines);
            
            _pages.AddRange(subPages);
        }

        // Ensure current page is valid
        _currentPageIndex = Math.Clamp(_currentPageIndex, 0, Math.Max(0, _pages.Count - 1));
    }

    private List<string> WrapTextToLines(string text)
    {
        List<string> lines = new();
        Rectangle slot = CalculateActualSlot();
        float maxWidth = slot.Width - Margin.X - Margin.Z;
        
        string[] words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string currentLine = "";

        foreach (string word in words)
        {
            string testLine = string.IsNullOrEmpty(currentLine) ? word : $"{currentLine} {word}";
            float testWidth = _font.MeasureString(testLine).X;

            if (testWidth > maxWidth && !string.IsNullOrEmpty(currentLine))
            {
                lines.Add(currentLine);
                currentLine = word;
            }
            else
            {
                currentLine = testLine;
            }
        }

        if (!string.IsNullOrEmpty(currentLine))
        {
            lines.Add(currentLine);
        }

        return lines;
    }

    private List<string> SplitLinesIntoPages(List<string> lines)
    {
        List<string> pages = new();
        Rectangle slot = CalculateActualSlot();
        float maxHeight = slot.Height - Margin.Y - Margin.W;
        int linesPerPage = Math.Max(1, (int)(maxHeight / _font.LineSpacing));

        for (int i = 0; i < lines.Count; i += linesPerPage)
        {
            int count = Math.Min(linesPerPage, lines.Count - i);
            string page = string.Join("\n", lines.GetRange(i, count));
            pages.Add(page);
        }

        return pages;
    }
}
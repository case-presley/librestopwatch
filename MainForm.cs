namespace StopwatchApp;

using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

public class MainForm : Form
{
    private readonly PrivateFontCollection _fontCollection = new PrivateFontCollection();

    private Label _labelTime = null!;
    private Button _buttonStartStop = null!;
    private TextBox _textboxSet = null!;
    private Button _buttonApply = null!;
    private Button _buttonSave = null!;
    private Timer _timer = null!;

    private bool _running;
    private double _accumulated;
    private int _totalSeconds;
    private DateTime _lastUpdate;

    private const string SaveFile = "time.txt";

    public MainForm()
    {
        LoadEmbeddedFont();
        InitializeUI();
        LoadSavedTime();
    }

    // -------------------------------------------------------------
    // LOAD EMBEDDED FONT
    // -------------------------------------------------------------
    private void LoadEmbeddedFont()
    {
        using Stream? fontStream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("StopwatchApp.font.ttf");

        if (fontStream == null)
        {
            MessageBox.Show("Failed to load font.ttf.");
            return;
        }

        byte[] fontData = new byte[fontStream.Length];
        int read = fontStream.Read(fontData, 0, fontData.Length);
        if (read != fontData.Length)
        {
            MessageBox.Show("Error reading font file.");
            return;
        }

        IntPtr mem = Marshal.AllocCoTaskMem(fontData.Length);
        Marshal.Copy(fontData, 0, mem, fontData.Length);
        _fontCollection.AddMemoryFont(mem, fontData.Length);
        Marshal.FreeCoTaskMem(mem);
    }

    // -------------------------------------------------------------
    // UI SETUP — FIXED 800 × 800 LAYOUT
    // -------------------------------------------------------------
    private void InitializeUI()
    {
        // Window
        Text = "Stopwatch";
        Size = new Size(800, 800);
        MaximizeBox = false;
        MinimizeBox = false;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(20, 20, 20);

        // Large LED clock
        _labelTime = new Label
        {
            Text = "00:00:00",
            Font = new Font(_fontCollection.Families[0], 120, FontStyle.Regular),
            ForeColor = Color.FromArgb(255, 160, 215),
            BackColor = Color.FromArgb(20, 20, 20),
            TextAlign = ContentAlignment.MiddleCenter,
            Width = 800,
            Height = 300,
            Left = 0,
            Top = 20
        };
        Controls.Add(_labelTime);

        // Start/Stop button
        _buttonStartStop = new Button
        {
            Text = "Start",
            Width = 200,
            Height = 60,
            BackColor = Color.FromArgb(45, 45, 45),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Left = (800 - 200) / 2,
            Top = 360
        };
        _buttonStartStop.Click += ButtonStartStop_Click;
        Controls.Add(_buttonStartStop);

        // Manual time input
        _textboxSet = new TextBox
        {
            Text = "00:00:00",
            Width = 200,
            Height = 30,
            Left = 50,
            Top = 700,
            BackColor = Color.FromArgb(35, 35, 35),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };
        Controls.Add(_textboxSet);

        // Apply button
        _buttonApply = new Button
        {
            Text = "Apply",
            Width = 150,
            Height = 30,
            Left = 270,
            Top = 700,
            BackColor = Color.FromArgb(45, 45, 45),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        _buttonApply.Click += ButtonApply_Click;
        Controls.Add(_buttonApply);

        // Save button
        _buttonSave = new Button
        {
            Text = "Save Time",
            Width = 150,
            Height = 30,
            Left = 600,
            Top = 700,
            BackColor = Color.FromArgb(45, 45, 45),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        _buttonSave.Click += ButtonSave_Click;
        Controls.Add(_buttonSave);

        // Timer
        _timer = new Timer { Interval = 100 };
        _timer.Tick += Timer_Tick;
    }

    // -------------------------------------------------------------
    // LOAD SAVED TIME
    // -------------------------------------------------------------
    private void LoadSavedTime()
    {
        if (!File.Exists(SaveFile))
            return;

        if (int.TryParse(File.ReadAllText(SaveFile).Trim(), out int s))
        {
            _totalSeconds = s;
            UpdateDisplay();
        }
    }

    // -------------------------------------------------------------
    // SAVE TIME
    // -------------------------------------------------------------
    private void ButtonSave_Click(object? sender, EventArgs e)
    {
        File.WriteAllText(SaveFile, _totalSeconds.ToString());
    }

    // -------------------------------------------------------------
    // START/STOP BUTTON
    // -------------------------------------------------------------
    private void ButtonStartStop_Click(object? sender, EventArgs e)
    {
        _running = !_running;

        if (_running)
        {
            _lastUpdate = DateTime.Now;
            _buttonStartStop.Text = "Stop";
            _timer.Start();
        }
        else
        {
            _buttonStartStop.Text = "Start";
            _timer.Stop();
        }
    }

    // -------------------------------------------------------------
    // TIMER TICK
    // -------------------------------------------------------------
    private void Timer_Tick(object? sender, EventArgs e)
    {
        DateTime now = DateTime.Now;
        double delta = (now - _lastUpdate).TotalSeconds;
        _lastUpdate = now;

        _accumulated += delta;

        while (_accumulated >= 1.0)
        {
            _totalSeconds++;
            _accumulated -= 1.0;
        }

        UpdateDisplay();
    }

    // -------------------------------------------------------------
    // UPDATE DISPLAY
    // -------------------------------------------------------------
    private void UpdateDisplay()
    {
        int hours = _totalSeconds / 3600;
        int minutes = (_totalSeconds % 3600) / 60;
        int seconds = _totalSeconds % 60;

        _labelTime.Text = $"{hours:D2}:{minutes:D2}:{seconds:D2}";
    }

    // -------------------------------------------------------------
    // APPLY MANUAL TIME
    // -------------------------------------------------------------
    private void ButtonApply_Click(object? sender, EventArgs e)
    {
        var parts = _textboxSet.Text.Split(':');
        if (parts.Length != 3) return;

        if (int.TryParse(parts[0], out int h) &&
            int.TryParse(parts[1], out int m) &&
            int.TryParse(parts[2], out int s))
        {
            _totalSeconds = h * 3600 + m * 60 + s;
            _accumulated = 0;
            UpdateDisplay();
        }
    }
}

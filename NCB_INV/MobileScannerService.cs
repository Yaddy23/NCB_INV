using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static NCB_INV.BookScanner;

public class MobileScannerService
{
    private HttpListener _listener;
    private readonly Func<string, ScanResult> _onScanReceived;

    public MobileScannerService(Func<string, ScanResult> onScanReceived)
    {
        _onScanReceived = onScanReceived;
    }

    public void Start(int port = 8080)
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://*:{port}/");
        _listener.Start();
        Task.Run(() => ListenAsync());
    }

    public void Stop()
    {
        _listener?.Stop();
    }

    private async Task ListenAsync()
    {
        while (_listener != null && _listener.IsListening)
        {
            try
            {
                var context = await _listener.GetContextAsync();
                ProcessRequest(context);
            }
            catch { /* Listener stopped */ }
        }
    }

    private void ProcessRequest(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;

        if (request.HttpMethod == "GET" && request.Url.AbsolutePath == "/")
        {
            string html = GetScannerHtml();
            byte[] buffer = Encoding.UTF8.GetBytes(html);
            response.ContentType = "text/html";
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.Close();
        }
        else if (request.HttpMethod == "POST" && request.Url.AbsolutePath == "/api/scan")
        {
            using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
            {
                string isbn = reader.ReadToEnd().Trim();

                // Execute ProcessScan on WinForms UI Thread
                ScanResult result = _onScanReceived?.Invoke(isbn) ?? new ScanResult { Success = false };

                string jsonResponse = $"{{\"success\":{result.Success.ToString().ToLower()}, \"title\":\"{result.Title}\", \"oldQty\":{result.OldQty}, \"newQty\":{result.NewQty}}}";
                byte[] buffer = Encoding.UTF8.GetBytes(jsonResponse);

                response.ContentType = "application/json";
                response.OutputStream.Write(buffer, 0, buffer.Length);
                response.Close();
            }
        }
    }

    private string GetScannerHtml()
    {
        // Continuous mobile scanner HTML with audio feedback
        return @"<!DOCTYPE html>
<html>
<head>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>NCB_INV Mobile Scanner</title>
    <script src='https://unpkg.com/html5-qrcode'></script>
    <style>
        body { font-family: sans-serif; text-align: center; padding: 12px; margin: 0; background: #121212; color: #fff; }
        #reader { width: 100%; max-width: 480px; margin: 10px auto; background: #000; border-radius: 8px; overflow: hidden; }
        #status { margin: 10px; font-weight: bold; font-size: 1.1em; color: #4dabf7; }
        #log { max-width: 480px; margin: 10px auto; text-align: left; max-height: 160px; overflow-y: auto; background: #1e1e1e; padding: 10px; border-radius: 8px; font-family: monospace; font-size: 0.85em; }
        .success { color: #51cf66; }
        .error { color: #ff6b6b; }
    </style>
</head>
<body>
    <h3>NCB_INV Mobile Scanner</h3>
    <div id='reader'></div>
    <div id='status'>Tap camera feed once to enable sound</div>
    <div id='log'></div>

    <script>
        let lastIsbn = '';
        let lastScanTime = 0;
        const cooldown = 2000;
        const audioCtx = new (window.AudioContext || window.webkitAudioContext)();

        function playSound(freq, duration, type='sine') {
            if (audioCtx.state === 'suspended') audioCtx.resume();
            try {
                const osc = audioCtx.createOscillator();
                const gain = audioCtx.createGain();
                osc.type = type;
                osc.frequency.setValueAtTime(freq, audioCtx.currentTime);
                gain.gain.setValueAtTime(0.2, audioCtx.currentTime);
                gain.gain.exponentialRampToValueAtTime(0.01, audioCtx.currentTime + duration);
                osc.connect(gain);
                gain.connect(audioCtx.destination);
                osc.start();
                osc.stop(audioCtx.currentTime + duration);
            } catch(e) {}
        }

        const scanner = new Html5QrcodeScanner('reader', { fps: 15, qrbox: { width: 250, height: 140 } });

        function onScanSuccess(isbn) {
            const now = Date.now();
            if (isbn === lastIsbn && (now - lastScanTime) < cooldown) return;
            
            lastIsbn = isbn;
            lastScanTime = now;

            fetch('/api/scan', {
                method: 'POST',
                headers: { 'Content-Type': 'text/plain' },
                body: isbn
            })
            .then(res => res.json())
            .then(data => {
                const log = document.getElementById('log');
                const timeStr = new Date().toLocaleTimeString();
                
                if (data.success) {
                    playSound(1046.50, 0.12); // High beep
                    document.getElementById('status').innerText = `✅ ${data.title} (New Qty: ${data.newQty})`;
                    log.innerHTML = `<div class='log-item success'>[${timeStr}] +1 to ${data.title} (Qty: ${data.newQty})</div>` + log.innerHTML;
                } else {
                    playSound(300, 0.15, 'sawtooth'); // Error buzz
                    document.getElementById('status').innerText = `❌ ISBN: ${isbn} NOT FOUND`;
                    log.innerHTML = `<div class='log-item error'>[${timeStr}] NOT FOUND: ${isbn}</div>` + log.innerHTML;
                }
            })
            .catch(() => {
                playSound(300, 0.15, 'sawtooth');
                document.getElementById('status').innerText = '⚠️ Network Error connecting to PC';
            });
        }

        document.body.addEventListener('click', () => { if (audioCtx.state === 'suspended') audioCtx.resume(); }, { once: true });
        scanner.render(onScanSuccess);
    </script>
</body>
</html>";
    }
}
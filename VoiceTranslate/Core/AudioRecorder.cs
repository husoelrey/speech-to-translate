using System;
using System.IO;
using NAudio.Wave;

namespace VoiceTranslate.Core;

public class AudioRecorder : IDisposable
{
    private WaveInEvent? _waveIn;
    private MemoryStream? _memoryStream;
    private WaveFileWriter? _waveWriter;

    public void StartRecording()
    {
        _memoryStream = new MemoryStream();
        _waveIn = new WaveInEvent
        {
            WaveFormat = new WaveFormat(16000, 16, 1) // 16kHz, 16-bit, Mono
        };

        _waveWriter = new WaveFileWriter(_memoryStream, _waveIn.WaveFormat);

        _waveIn.DataAvailable += (s, a) =>
        {
            _waveWriter.Write(a.Buffer, 0, a.BytesRecorded);
            _waveWriter.Flush();
        };

        _waveIn.StartRecording();
    }

    public byte[] StopRecording()
    {
        if (_waveIn != null)
        {
            _waveIn.StopRecording();
            _waveIn.Dispose();
            _waveIn = null;
        }

        if (_waveWriter != null)
        {
            _waveWriter.Dispose();
            _waveWriter = null;
        }

        byte[] wavBytes = _memoryStream?.ToArray() ?? Array.Empty<byte>();

        _memoryStream?.Dispose();
        _memoryStream = null;

        return wavBytes;
    }

    public void Dispose()
    {
        _waveIn?.Dispose();
        _waveWriter?.Dispose();
        _memoryStream?.Dispose();
    }
}

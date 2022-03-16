using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// Reference: http://soundfile.sapp.org/doc/WaveFormat/

public class WavHandler
{
    private byte[] _wavData; // All wav data in the given byte array (PCM header + audio samples)
    private string _chunkID; // "RIFF"
    private int _chunkSize;  // Size of the entire file in bytes minus 8 bytes
    private string _format;  // "WAVE"
    private string _subchunk1ID; // "fmt "
    private int _subchunk1Size; // Size of the rest of the Subchunk which follows this number
    private short _audioFormat; // PCM = 1, other values indicate compression
    private short _numChannels; // Mono = 1, Stereo = 2, etc.
    private int _frequency; // 8,000 , 44,100 etc.
    private int _byteRate; // _frequency * _numChannels * _bitDepth / 8 (specified in wave data already)
    private short _blockAlign; // _numChannels * _bitDepth / 8 (specified in wave data already)
    private short _bitDepth; // Bits in a sample (max amplitude of the sample -> 8 bit _bitDepth == 255 max amplitude)
    private string _subchunk2ID; // "data"
    private int _subchunk2Size; // Number of bytes of audio samples
    private byte[] _rawAudioSamples; // Unscaled audio samples in bytes
    private float[] _scaledAudioSamples; // Audio samples scaled down to be used in Unity
    private bool _corruptionDetected; // Indicates whether the file failed the integrity check or not

    public WavHandler(byte[] data)
    {
        _wavData = data;
        _chunkID = System.Text.Encoding.UTF8.GetString(data, 0, 4);
        _chunkSize = BitConverter.ToInt32(data, 4);
        _format = System.Text.Encoding.UTF8.GetString(data, 8, 4); 
        _subchunk1ID = System.Text.Encoding.UTF8.GetString(data, 12, 4); 
        _subchunk1Size = BitConverter.ToInt32(data, 16);
        _audioFormat = BitConverter.ToInt16(data, 20);
        _numChannels = BitConverter.ToInt16(data, 22);
        _frequency = BitConverter.ToInt32(data, 24);
        _byteRate = BitConverter.ToInt32(data, 28);
        _blockAlign = BitConverter.ToInt16(data, 32);
        _bitDepth = BitConverter.ToInt16(data, 34);
        _subchunk2ID = System.Text.Encoding.UTF8.GetString(data, 20 + _subchunk1Size, 4); 
        _subchunk2Size = BitConverter.ToInt32(data, 24 + _subchunk1Size);
        _rawAudioSamples = new byte[_subchunk2Size];
        Array.Copy(data, 28 + _subchunk1Size, _rawAudioSamples, 0,  _rawAudioSamples.Length);
        _scaledAudioSamples = ScaleAudioSamples();
        _corruptionDetected = false;
        IntegrityCheck();
    }

    // Parses unscaled audio data, grabs each sample, and passes to ScaleSample() 
    private float[] ScaleAudioSamples()
    {
        float[] scaledSamples = new float[_rawAudioSamples.Length / 4]; // 1 float = 4 bytes
        int unscaledSamplesIndex;
        float unscaledSample = 2147483700; // Max value for signed int 32 is 2,147,483,648 so sentinel value of 2,147,483,700 used to satisfy C#'s restrictions on passing uninitialized variables as function parameters

        // Parse the unscaledSamples and convert each to float
        for (int scaledSamplesIndex = 0; scaledSamplesIndex < scaledSamples.Length; scaledSamplesIndex++)
        {
            unscaledSamplesIndex = scaledSamplesIndex * _blockAlign; // Get next unscaled sample

            // Determine bit depth
            switch (_bitDepth)
            {
                case 8:
                    unscaledSample = _rawAudioSamples[unscaledSamplesIndex];
                    break;
                case 16:
                    unscaledSample = BitConverter.ToInt16(_rawAudioSamples, unscaledSamplesIndex);
                    break;
                case 32:
                    unscaledSample = BitConverter.ToInt32(_rawAudioSamples, unscaledSamplesIndex);
                    break;
                default:
                    Debug.Log("Unsupported bit depth (only supports 8, 16, and 32)");
                    break;
            }
            if (unscaledSample != 2147483700)
                scaledSamples[scaledSamplesIndex] = ScaleSample(unscaledSample); // Scale the sample down to be between 1 and -1

        }
        return scaledSamples;
    }

    // Scales a single audio sample value down to be between -1 and 1 (required or audio result is TV static)
    private float ScaleSample(float unscaledSample)
    {
        float scaledSample = 0f;
        float divisor = 1f;
        switch (_bitDepth)
        {
            case 8:
                divisor = Mathf.Pow(2f, _bitDepth) - 1f; // (2 ^ _bitDepth) - 1
                break;
            case 16:
            case 32:
                divisor = Mathf.Pow(2f, _bitDepth - 1f); // 2 ^ (_bitDepth - 1) if unscaled sample is negative
                if (unscaledSample > 0)
                    divisor -= 1f; // (2 ^ (_bitDepth - 1)) - 1 if unscaled sample is positive
                break;
        }
        scaledSample = unscaledSample / divisor;
        return scaledSample;
    }

    private void IntegrityCheck()
    {
        // Check chunk and subchunk IDs
        if (_chunkID != "RIFF")
            _corruptionDetected = true;
        else if (_format != "WAVE")
            _corruptionDetected = true;
        else if (_subchunk1ID != "fmt ")
            _corruptionDetected = true;
        else if (_subchunk2ID != "data")
            _corruptionDetected = true;

        // Check chunk and subchunk sizes
        else if (_wavData.Length != (_chunkSize + 8))
            _corruptionDetected = true;
        else if (_chunkSize != (20 + _subchunk1Size + _subchunk2Size))
            _corruptionDetected = true;
        else if (_subchunk1Size != (_chunkSize - _subchunk2Size - 20))
            _corruptionDetected = true;
        else if (_subchunk2Size != (_chunkSize - _subchunk1Size - 20))
            _corruptionDetected = true;

    }

    // Getters
    public byte[] GetWavData() { return _wavData; }
    public string GetChunkID() { return _chunkID; }
    public int GetChunkSize() { return _chunkSize; }
    public string GetFormat() { return _format; }
    public string GetSubchunk1ID() { return _subchunk1ID; }
    public int GetSubchunk1Size() { return _subchunk1Size; }
    public short GetAudioFormat() { return _audioFormat; }
    public short GetNumChannels() { return _numChannels; }
    public int GetFrequency() { return _frequency; }
    public int GetByteRate() { return _byteRate; }
    public short GetBlockAlign() { return _blockAlign; }
    public short GetBitDepth() { return _bitDepth; }
    public string GetSubchunk2ID() { return _subchunk2ID; }
    public int GetSubchunk2Size() { return _subchunk2Size; }
    public byte[] GetRawAudioSamples() { return _rawAudioSamples; }
    public float[] GetScaledAudioSamples() { return _scaledAudioSamples; }
    public int GetScaledAudioSamplesLength() { return _scaledAudioSamples.Length; }
    public bool CheckFileIntegrity() { return !_corruptionDetected; }
}

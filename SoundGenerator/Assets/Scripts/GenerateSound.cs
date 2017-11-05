using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD;
using System;

public class GenerateSound : MonoBehaviour {

    FMOD.Studio.System studioSystem;
    FMOD.System lowlevelSystem;

    FMOD.Channel channel;
    FMOD.ChannelGroup channelGroup;
    FMOD.DSP fft;
    const int windowSize = 1024;

    FMOD.CREATESOUNDEXINFO soundInfo;
    FMOD.Sound generatedSound;
    int sampleRate = 44100;
    int channels = 2;
    int soundLength = 5; //sec
    bool sampleCreated = false; //temp
    int frequency = 800; //Hz
    float volume = 0.2f; // 1-0
    int samplesGenerated = 0;

    LineRenderer lineRenderer;

    // Use this for initialization
    void Start () {

        //referencja do komponentow FMOD - wysokiego poziomu (studio) i niskiego
        studioSystem = FMODUnity.RuntimeManager.StudioSystem;
        lowlevelSystem = FMODUnity.RuntimeManager.LowlevelSystem;

        //odniesienie do glownego kanalu - do niego bedziemy przesylac nasz dzwiek
        channel = new Channel();
        FMODUnity.RuntimeManager.LowlevelSystem.getMasterChannelGroup(out channelGroup);

        //inicjalizacja FFT (w FMODzie jako komponent DSP) i linerenderera do wyswietlania equalizera
        FMODUnity.RuntimeManager.LowlevelSystem.createDSPByType(FMOD.DSP_TYPE.FFT, out fft);
        fft.setParameterInt((int)FMOD.DSP_FFT.WINDOWTYPE, (int)FMOD.DSP_FFT_WINDOW.HANNING);
        fft.setParameterInt((int)FMOD.DSP_FFT.WINDOWSIZE, windowSize * 2);
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = windowSize;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;        
        channelGroup.addDSP(FMOD.CHANNELCONTROL_DSP_INDEX.HEAD, fft);

        //Debug - sprawdzamy czy dobrze udalo nam sie zlapac kanal dzwiekowy (nie mamy jeszcze
        //obslugi bledow FMOD_OK)
        uint version;
        lowlevelSystem.getVersion(out version);
        bool channelIsPlaying;
        channel.isPlaying(out channelIsPlaying);
        UnityEngine.Debug.Log(channelIsPlaying);

        //Wczytujemy i odtwarzamy testowy plik do obiektu 'sound'
        //Przypisujemy nasz kanal do tej samej grupy co glowny kanal dzwieku
        FMOD.Sound sound;
        string nametest = "";
        lowlevelSystem.createSound("Assets\\Sounds\\test2.mp3", FMOD.MODE.DEFAULT, out sound);
        sound.getName(out nametest, 20);
        UnityEngine.Debug.Log(nametest);

        createSample();

        //debug
        if (sampleCreated)
        {
            lowlevelSystem.playSound(generatedSound, channelGroup, false, out channel);
            channel.setLoopCount(-1);
            channel.setMode(MODE.LOOP_NORMAL);
            channel.setPosition(0, TIMEUNIT.MS);
            channel.setPaused(false);

        } else
        {
            lowlevelSystem.playSound(sound, channelGroup, true, out channel);
            channel.setLoopCount(-1);
            channel.setMode(MODE.LOOP_NORMAL);
            channel.setPosition(0, TIMEUNIT.MS);
            channel.setPaused(false);
        } 

    }
	
	// Update is called once per frame
	void Update () {

        IntPtr unmanagedData;
        uint length;
        fft.getParameterData((int)FMOD.DSP_FFT.SPECTRUMDATA, out unmanagedData, out length);
        //Musimy zrzutować dane z pointera unmanagedData na typ DSP_PARAMETER_FFT
        FMOD.DSP_PARAMETER_FFT fftData = 
            (FMOD.DSP_PARAMETER_FFT)System.Runtime.InteropServices.Marshal.PtrToStructure(unmanagedData, typeof(FMOD.DSP_PARAMETER_FFT));
        var spectrum = fftData.spectrum;

        //UnityEngine.Debug.Log("Number of channels in track:" + fftData.numchannels);

        //Wrzucamy dane do linerenderera
        if (fftData.numchannels > 0)
        {
            float width = 80.0f;
            float height = 0.1f;
            var pos = Vector3.zero;
            pos.x = width * -0.5f;

            for (int i = 0; i < windowSize; ++i)
            {
                pos.x += (width / windowSize);
                //Elementy tablicy spectrum zwracane sa w log dB
                //TODO: fft w skali logarytmicznej a nie liniowej
                float level = Lin2dB(spectrum[0][i]);
                pos.y = (80 + level) * height;

                lineRenderer.SetPosition(i, pos);
            }
        }
    }

    void createSample()
    {
        soundInfo = new FMOD.CREATESOUNDEXINFO();
        soundInfo.cbsize = System.Runtime.InteropServices.Marshal.SizeOf(soundInfo);
        soundInfo.decodebuffersize = (uint)sampleRate;

        // Sample rate * number of channels * bits per sample per channel * number of seconds
        soundInfo.length = (uint)(sampleRate * channels * sizeof(short) * soundLength);
        //UnityEngine.Debug.Log(soundInfo.length);
        soundInfo.numchannels = channels;
        soundInfo.defaultfrequency = sampleRate;
        soundInfo.format = SOUND_FORMAT.PCM16;
        soundInfo.pcmreadcallback = PCMReadCallbackImpl;
        soundInfo.pcmsetposcallback = PCMSetPosCallbackImpl;

        //zwiekszenie bufora
        lowlevelSystem.setStreamBufferSize(65536, TIMEUNIT.RAWBYTES);
        lowlevelSystem.createStream("generatedSound", MODE.OPENUSER, ref soundInfo, out generatedSound);
        sampleCreated = true;
        
    }
    private RESULT PCMReadCallbackImpl(IntPtr soundraw, IntPtr data, uint length)
    {
        Int16[] buffer = new Int16[length/4];
        UnityEngine.Debug.Log("Samples length:" +  length + ", short size:" + sizeof(Int16));
        int i = 0;
        for (i = 0; i < length / 4; i++)
        {
            //pozycja w probce
            double position = frequency * (float)samplesGenerated / (float)sampleRate;

            try
            {
                var currentPtr = new IntPtr(data.ToInt32() + (i * sizeof(Int16)));
                buffer[i] = (Int16)System.Runtime.InteropServices.Marshal.PtrToStructure(
                currentPtr, typeof(Int16));
                buffer[i] = 15000;
            } catch (NullReferenceException nre)
            {
                UnityEngine.Debug.Log("Samples broke at sample:" + i);
                throw new NullReferenceException(nre.Message);
            }

            //zamiana z -1 - 1 na zakres 16bit (+/-32767)
            //buffer
        }

        UnityEngine.Debug.Log("Finished at sample no. " + i);
        System.Runtime.InteropServices.Marshal.Copy(buffer, 0, data, (int)(length/4));

        //testing last table index to see if it copied OK
        int testPointerIndex = (int)(length / 4) - 1;
        var currentPtr2 = new IntPtr(data.ToInt32() + (testPointerIndex * sizeof(Int16)));
        var test = (Int16)System.Runtime.InteropServices.Marshal.PtrToStructure(
        currentPtr2, typeof(Int16));

        UnityEngine.Debug.Log("sample test: buffer[" + testPointerIndex + "] = " + test);
        return FMOD.RESULT.OK;
    }
    private RESULT PCMSetPosCallbackImpl(IntPtr soundraw, int subsound, uint position, TIMEUNIT postype)
    {
        return FMOD.RESULT.OK;
    }

    float Lin2dB(float linear)
    {
        return Mathf.Clamp(Mathf.Log10(linear) * 20.0f, -80.0f, 0.0f);
    }
}

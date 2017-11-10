using UnityEngine;
using FMOD;
using System;

public class GenerateSound : MonoBehaviour {

    public enum WaveType {SINE, TRIANGLE, SAW, SQUARE, WHITE_NOISE, PINK_NOISE};
    private delegate double GenerateSample(double position);
    GenerateSample sampleGenerator;
    WaveType wave;
    double[] pinkNoiseBuffer = new double[7];
    System.Random rand = new System.Random();

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
    int soundLength = 1; //sec
    bool sampleCreated = false; //temp
    public int frequency { get; set; } //Hz
    public float amplitude { get; set; } // 0 - 2, 1 is default
    float volume = 0.2f; // 1-0
    int samplesGenerated = 0;
    bool debugDrawFFT = true; //false - draw sample instead

    LineRenderer lineRendererFFT;
    [SerializeField]
    GameObject lineRendererHolder;
    LineRenderer lineRendererSamples;
    float[] lineRendererSamplesData;

    // Use this for initialization
    void Start () {
        frequency = 800;
        amplitude = 1.0f;
        sampleGenerator = GenerateSineSample;

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
        lineRendererFFT = gameObject.AddComponent<LineRenderer>();
        lineRendererFFT.positionCount = windowSize;
        lineRendererFFT.startWidth = 0.1f;
        lineRendererFFT.endWidth = 0.1f;        
        channelGroup.addDSP(FMOD.CHANNELCONTROL_DSP_INDEX.HEAD, fft);

        lineRendererSamples = lineRendererHolder.AddComponent<LineRenderer>();
        lineRendererSamples.positionCount = sampleRate / 100;
        lineRendererSamples.startWidth = 0.1f;
        lineRendererSamples.endWidth = 0.1f;

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

        InitSampleGeneration();

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
        if (debugDrawFFT)
        {
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
                    pos.y = (80 + level) * height - 20;

                    lineRendererFFT.SetPosition(i, pos);
                }
            }
        }

        lineRendererSamples.positionCount = lineRendererSamplesData.Length;
        for (int i = 0; i < lineRendererSamplesData.Length; i++)
        {
            lineRendererSamples.SetPosition(i, new Vector3(-40 + i * 0.5f, lineRendererSamplesData[i] -25, 0));
        }

    }

    internal void SetWaveType(WaveType wavetype)
    {
        switch (wavetype)
        {
            case WaveType.SINE:
                sampleGenerator = GenerateSineSample;
                wave = WaveType.SINE;
                break;
            case WaveType.TRIANGLE:
                sampleGenerator = GenerateTriangleSample;
                wave = WaveType.TRIANGLE;
                break;
            case WaveType.SAW:
                sampleGenerator = GenerateSawSample;
                wave = WaveType.SAW;
                break;
            case WaveType.SQUARE:
                sampleGenerator = GenerateSquareSample;
                wave = WaveType.SQUARE;
                break;
            case WaveType.WHITE_NOISE:
                sampleGenerator = GenerateWhiteNoise;
                wave = WaveType.WHITE_NOISE;
                break;
            case WaveType.PINK_NOISE:
                sampleGenerator = GeneratePinkNoise;
                wave = WaveType.PINK_NOISE;
                break;
            default:
                break;
        }
    }

    private double GenerateSineSample(double position)
    {
        return Math.Sin(position * Math.PI * 2)*amplitude;
    }

    private double GenerateTriangleSample(double position)
    {
        double sampleVal;
        if (position == 0)
        {
            sampleVal = - 1.0f;
        } else if (position == 0.5d)
        {
            sampleVal = 1.0f;
        } else if(position < 0.5)
        {
            sampleVal = (2.0d * (position*2)) - 1.0d;
        } else
        {
            sampleVal = (2.0d * (2.0d-(position*2))) - 1.0d; //not sure
        }
        return sampleVal * amplitude;
    }

    private double GenerateSawSample(double position)
    {
        double sampleVal;
        if (position == 0)
        {
            sampleVal = - 1.0f;
        } else
        {
            sampleVal = (2.0d * position) - 1.0d;
        }
        return sampleVal * amplitude;
    }

    private double GenerateSquareSample(double position)
    {
        double sampleVal;
        if (position < 0.5d)
        {
            sampleVal = 1.0d;
        }
        else
        {
            sampleVal = -1.0d;
        }
        return sampleVal * amplitude;
    }


    private double WhiteNoise()
    {
        return rand.NextDouble() * 2.0d - 1.0d;
    }

    private double GenerateWhiteNoise(double position)
    {
        return position * WhiteNoise() * amplitude;
    }

    private double GeneratePinkNoise(double position)
    {
        double white = WhiteNoise();

        pinkNoiseBuffer[0] = 0.99886 * pinkNoiseBuffer[0] + white * 0.0555179;
        pinkNoiseBuffer[1] = 0.99332 * pinkNoiseBuffer[1] + white * 0.0750759;
        pinkNoiseBuffer[2] = 0.96900 * pinkNoiseBuffer[2] + white * 0.1538520;
        pinkNoiseBuffer[3] = 0.86650 * pinkNoiseBuffer[3] + white * 0.3104856;
        pinkNoiseBuffer[4] = 0.55000 * pinkNoiseBuffer[4] + white * 0.5329522;
        pinkNoiseBuffer[5] = -0.7616 * pinkNoiseBuffer[5] - white * 0.0168980;
        double pink = pinkNoiseBuffer[0] + pinkNoiseBuffer[1] + pinkNoiseBuffer[2] + pinkNoiseBuffer[3] + pinkNoiseBuffer[4] + pinkNoiseBuffer[5] + pinkNoiseBuffer[6] + white * 0.5362;
        pinkNoiseBuffer[6] = white * 0.115926;

        double sample = position * amplitude;
        return Mathf.Clamp((float)(sample * (pink / 7)), -1.0f, 1.0f);
    }

    void InitSampleGeneration()
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
        //Tutaj przechowujemy probki
        short[] buffer = new short[length/sizeof(short)];
        //UnityEngine.Debug.Log("Samples length:" +  length + ", short size:" + sizeof(short));
        int i = 0;
        samplesGenerated = 0;
        int lineRendererResolution = 5; // co ile probek dodajemy punkt do wykresu
        lineRendererSamplesData = new float[(int)(length / sizeof(short)) / lineRendererResolution];

        for (i = 0; i < length / sizeof(short); i += 2)
        {
            double position = frequency * (double)samplesGenerated / (double)sampleRate;
            //obecna pozycja w probce
            if (wave <= WaveType.SQUARE)
                position = frequency * (double)samplesGenerated / (double)sampleRate;
            else
                position = (rand.NextDouble() * 0.999d + 0.001d) * 20000 * (double)samplesGenerated / (double)sampleRate;

            try
            {
                //test dostepu pamieci
                var currentPtr = new IntPtr(data.ToInt32() + (i * sizeof(short)));
                buffer[i] = (short)System.Runtime.InteropServices.Marshal.PtrToStructure(
                currentPtr, typeof(short));

                //i - lewy kanal, i+1 - prawy
                buffer[i] = (short)(sampleGenerator(position - Math.Floor(position)) * 32767.0f * volume);
                buffer[i + 1] = (short)(sampleGenerator(position - Math.Floor(position)) * 32767.0f * volume);
            }
            catch (NullReferenceException nre) {
                UnityEngine.Debug.Log("Samples broke at sample:" + i);
                throw new NullReferenceException(nre.Message);
            }

            catch (IndexOutOfRangeException ioore)
            {
                UnityEngine.Debug.Log("Samples broke at sample:" + i);
                throw new NullReferenceException(ioore.Message);
            }
            samplesGenerated++;

            if(samplesGenerated% lineRendererResolution == 0)
            {
                lineRendererSamplesData[samplesGenerated / lineRendererResolution] = (float)sampleGenerator(position - Math.Floor(position));
            }
            //zamiana z -1 - 1 na zakres 16bit (+/-32767)
            //buffer
        }

        //kopiujemy caly bufor do wskaznika data
        System.Runtime.InteropServices.Marshal.Copy(buffer, 0, data, (int)(length / 2));

        ///////////
        //Testowanie ostatniej probki po skopiowaniu w dwie strony
        //Odkomentuj jezeli masz problem ze sprawdzeniem poprawnych wartosci probek.
        ///////////
        //UnityEngine.Debug.Log("Finished at sample no. " + i);
        /*short[] BUFFTEST = new short[length / sizeof(short)]; //Do przegladania w visualu


        for (i = 0; i < length / sizeof(short); i ++)
        {
            var currentPtr = new IntPtr(data.ToInt32() + (i * sizeof(short)));
            BUFFTEST[i] = (short)System.Runtime.InteropServices.Marshal.PtrToStructure(
                currentPtr, typeof(short));
        }

        int testPointerIndex = (int)(length / 4) - 1;
        var currentPtr2 = new IntPtr(data.ToInt32() + (testPointerIndex * sizeof(short)));
        var test = (short)System.Runtime.InteropServices.Marshal.PtrToStructure(
        currentPtr2, typeof(short));
        UnityEngine.Debug.Log("sample test: buffer[" + testPointerIndex + "] = " + test);*/

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

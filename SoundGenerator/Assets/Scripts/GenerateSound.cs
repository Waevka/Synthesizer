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
        lowlevelSystem.playSound(sound, channelGroup, false, out channel);
        
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

        //Wrzucamy dane do linerenderera
        if(fftData.numchannels > 0)
        {
            UnityEngine.Debug.Log("Number of channels in track:" + fftData.numchannels);
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
    float Lin2dB(float linear)
    {
        return Mathf.Clamp(Mathf.Log10(linear) * 20.0f, -80.0f, 0.0f);
    }
}

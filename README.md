Japanese Emergency Warning Broadcast System (EWBS) Parser
====

This project contains a number of utilities for generating and decoding broadcasts from the Japanese Emergency Warning Broadcast System (EWBS) or 緊急警報放送 in Japanese.

What is EWBS?
----

If you've lived in Japan for a while, you will have heard these broadcasts with their characteristic sound as part of an emergency tsunami warning. They appear on TV and on FM radio. Take a look at http://www.youtube.com/watch?v=OuZ0O71Lw24 to get an idea. 

The beeps are actually two quickly alternating frequencies; 640Hz and 1024Hz. They represent binary code; 640Hz is a `0` and 1024Hz is a `1`.

The system used to turn the binary code into tones is called Frequency Shift Keying (FSK). Zeros are referred to as _Stop_ tones, and ones are called _Mark_ tones.

If you load the audio into Audacity and switch to Spectrogram view, you'll immediately understand how it works:

![000110010010010011](http://i.imgur.com/7WiNgFD.png "000110010010010011")

FSK is essentially what old 1200 Baud modems used to talk to other machines. If you're too young to have experienced modems, Caller ID in the United States also uses the same technique to make the caller's name and phone number appear on your phone, but the frequencies are a little different, and the process is a lot faster.

EWBS in Japan sends a block of 96 bits of data in 1.5 seconds. That isn't a whole lot of space to work with (12 bytes), but contained inside the block is information about the incident category, the day, month, year and hour, and the general location (by prefecture).

The block needs to be repeated several times in order to make sure that receivers can decode them correctly, because there is no other form of error correction.

There is also a more advanced digital version that is broadcast as part of the ISDB TS, but the analog broadcast is still sent. Among other things, it activates sirens placed in remote areas, wakes up radio receivers on constant stand-by, triggers automatic shutdown of heavy machinery, and can even automatically turn on certain TV receivers (though that is done using ISDB TS now).

The system was introduced on September 1, 1985. There is a reason for that date. On September 1, 1923, a very large earthquake occurred in the Tokyo area, and more than 100,000 people died in it. It became a trigger to commence radio broadcasting in Japan, and left such a big impact that September 1 was made Disaster Prevention Day.

What is this Project About?
----

I have no background in signal processing, but I wanted to learn about the beeps. Beep beep boop.


What's Where
----

* The FSK demodulator and all related functionality is in the `ParseEwbsSignal/AudioProcessor.cs` class.
* A light-weight PCM wave file reader is in the `ParseEwbsSignal/AudioFileReader.cs` class.
* The actual EWBS binary data block decoder is in the `ParseEwbsSignal/BlockDecoder.cs` class.
* The tone generator and wave file writer code can be found in `GenerateEwbsSignal/MainForm.cs`.

Some literature about FSK demodulation (German and English), as well as an article about the contents of EWBS broadcasts (Japanese) is included in the `Documentation` folder.


How to Use the Code
----

Compile and start _ParseEwbsSignal_ and point it at a WAV file. If it contains an EWBS broadcast, it will display the demodulated binary data, and then display the decoded information. Sample wave files are in the `sample_recordings.7z` file. You can also generate your own wave files using the _GenerateEwbsSignal_ program.

The YouTube video linked to a little earlier would produce this output:

    Scanning for silence.
    Found 2,873ms of silence. Ending silence scan.
    Demodulating FSK signal.
    Found 801ms of silence after reading 6301008 samples. Ending FSK demodulation.
    Decoding received EWBS data.
    
    00110000111001101101010011010011011100001110011011011001110101100111000011100110
    11011011010100101000111001101101010011010011011100001110011011011001110101100111
    00001110011011011011010100101100001110011011010100110100110111000011100110110110
    01110101100111000011100110110110110101001010000111001101101010011010011011100001
    11001101101100111010110011100001110011011011011010100101011100110110101001101001
    10111000011100110110110011101011001110000111001101101101101010010100011100110110
    10100110100110111000011100110110110011101011001110000111001101101101101010010110
    00011100110110101001101001101110000111001101101100111010110011100001110011011011
    01101010010100001110011011010100110100110111000011100110110110011101011001110000
    11100110110111001101101010011010011011100001110011011011001110101100111000011100
    11011011011010100101000111001101101010011010011011100001110011011011001110101100
    11100001110011011011011010100101110000111001101101010011010011011100001110011011
    01100
    (965 bit(s))
    
    Decoded EWBS Data:
    -----------------------------------------------
    Fixed Code: 0000111001101101
    Confidence: Strong
    Category:   End
    Location:   地域共通
    Day:        29
    Month:      12
    Year:       6
    Hour:       13
    -----------------------------------------------
    
    Processing of WAV file completed.


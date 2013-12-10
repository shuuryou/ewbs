#region AGPL License Block
/* ParseEwbsSignal- Parse Japanese Emergency Warning Broadcast System signal.
 * Copyright (C) 2013
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 */
#endregion

using System;

namespace ParseEwbsSignal
{
	/// <summary>
	/// Contains a variety of methods that are used together in order to perform demodulation of
	/// a binary AFSK signal. FSK tone detection, and silence detection are used to ensure the
	/// signal to demodulate actually exists. An implementation of a phase-locked loop (PLL) is
	/// used to synchronize the bit clock.
	/// 
	/// The methods in this class are used together in order to decode the signal broadcasted by
	/// the Japanese Emergency Warning Broadcast System.
	/// </summary>
	/// <remarks>
	/// The FSK demodulation routine uses an algorithm that was referenced as part of a Caller
	/// ID decoder in the following article:
	/// 
	/// Debbasch, Bernard. "ARM-Based Modern Answering Machine." Circuit Cellar May 2006: 40-47.
	/// <http://web.archive.org/web/http://www.handsontec.com/pdf_files/ezine_files/ARM_MODEM.pdf>.
	/// 
	/// The actual algorithm in the above, as well as the PLL implementation used in this class,
	/// comes from:
	/// 
	/// Sailer, Thomas. "DSP Modems." 11. Internationale Packet-Radio Tagung. Darmstadt, Germany. 1995.
	/// <http://web.archive.org/web/http://www.baycom.org/~tom/ham/da95/d_dspmod.pdf>.
	/// 
	/// Further help in implementing the algorithm was obtained from the following website.
	/// It showed that the correct formula for N is, in our case, sampleRate / FREQ_BIT_0.
	/// 
	/// Holder, Wayne. "Bell 202, 1200 Baud Demodulator in an ATTiny10." 7 July 2012. Web. 09 Dec. 2013.
	/// <http://web.archive.org/web/20131209111635/https://sites.google.com/site/wayneholder/attiny-4-5-9-10-assembly-ide-and-programmer/bell-202-1200-baud-demodulator-in-an-attiny10>.
	/// 
	/// The Görtzel algorithm, which isused to confirm the presence of a given frequency, was
	/// implemented based on the description given in the following article:
	/// 
	/// Banks, Kevin. "The Goertzel Algorithm." Embedded. UBM Tech, 28 Aug. 2002. Web. 09 Dec. 2013.
	/// <http://web.archive.org/web/http://www.embedded.com/design/embedded/4024443/The-Goertzel-Algorithm>.
	/// </remarks>
	public class AudioProcessor
	{
		private const int FREQ_BIT_0 = 640;
		private const int FREQ_BIT_1 = 1024;

		private int N;
		private int m_SampleRate;

		private double[] m_CoeffLoI, m_CoeffLoQ, m_CoeffHiI, m_CoeffHiQ;

		private const int PHASE_MAX = 65536; // Original PLL algorithm used a 16bit register
		private int m_Phase, m_PhaseIncrement, m_PhaseCorrect;

		/// <summary>Creates a new instance of the AudioProcessor class.</summary>
		/// <param name="sampleRate">Sample rate used. Must cleanly divide by 640.</param>
		public AudioProcessor(int sampleRate)
		{
			if (sampleRate % FREQ_BIT_0 != 0)
				throw new ArgumentOutOfRangeException("sampleRate");

			N = sampleRate / FREQ_BIT_0;
			m_SampleRate = sampleRate;

			m_CoeffLoI = new double[N]; // 640 Hz Phase
			m_CoeffLoQ = new double[N]; // 640 Hz Amplitude
			m_CoeffHiI = new double[N]; // 1024 Hz Phase
			m_CoeffHiQ = new double[N]; // 1024 Hz Amplitude

			for (int i = 0; i < N; i++)
			{
				m_CoeffLoI[i] = Math.Cos(2 * Math.PI * i / N * FREQ_BIT_0 / FREQ_BIT_0);
				m_CoeffLoQ[i] = Math.Sin(2 * Math.PI * i / N * FREQ_BIT_0 / FREQ_BIT_0);
				m_CoeffHiI[i] = Math.Cos(2 * Math.PI * i / N * FREQ_BIT_1 / FREQ_BIT_0);
				m_CoeffHiQ[i] = Math.Sin(2 * Math.PI * i / N * FREQ_BIT_1 / FREQ_BIT_0);
			}

			m_Phase = 0;
			m_PhaseIncrement = PHASE_MAX / (m_SampleRate / 64); /* 2^(register bits) / samples per baud */
			m_PhaseCorrect = m_PhaseIncrement / 2;
		}

		#region FSK Demodulation
		/// <summary>
		/// Gets the length the sample array used by DemodulateSample must be in order for
		/// the function to work correctly.
		/// </summary>
		public int SampleArrayLength { get { return N; } }

		/// <summary>
		/// Demodulates the specified new sample of FSK data by adding it to the ring buffer
		/// and passing it through two frequency filters. The filter with the strongest
		/// energy decides whether the signal is mark or space.
		/// </summary>
		/// <param name="samples">
		/// A reference to an array of type double that can be used as a ring buffer for
		/// sample data.
		/// </param>
		/// <param name="offset">
		/// A reference to an integer variable that contains the current active slot of the
		/// ring buffer.
		/// </param>
		/// <param name="newSample">
		/// The new sample that is to be added to the ring buffer.
		/// </param>
		/// <returns>
		/// Returns a value less than 0 if the demodulated signal represents bit 0, and
		/// returns a value greater than 0 if the demodulated signal represents bit 1.
		/// </returns>
		public double DemodulateSample(ref double[] samples, ref int offset, double newSample)
		{
			if (samples.Length != SampleArrayLength)
				throw new ArgumentOutOfRangeException("samples", "Length of sample array is incorrect.");

			double outLoI = 0, outLoQ = 0, outHiI = 0, outHiQ = 0;

			samples[offset] = newSample;
			offset = (offset + 1) % N;

			for (int i = 0; i < N; i++)
			{
				double sample = samples[(offset + i) % N];

				outLoI += (sample * m_CoeffLoI[i]); // 640Hz Phase 
				outHiI += (sample * m_CoeffHiI[i]); // 1024Hz Phase

				outLoQ += (sample * m_CoeffLoQ[i]); // 640Hz Amplitude
				outHiQ += (sample * m_CoeffHiQ[i]); // 1024Hz Amplitude
			}

			return outHiI * outHiI + outHiQ * outHiQ - outLoI * outLoI - outLoQ * outLoQ;
		}
		#endregion

		#region Silence Detection
		/// <summary>
		/// Gets the default silence threshold value that can be passed to IsSilence in
		/// order to get acceptable results.
		/// </summary>
		public double DefaultSilenceThreshold { get { return 0.01D; } }

		/// <summary>
		/// Determines whether the specified array of sample data contains silence or not.
		/// Silence is determined using the signal's energy.
		/// </summary>
		/// <param name="samples">An array of samples that is to be tested for silence.</param>
		/// <param name="silenceThreshold">The power level that is considered to be non-silence.</param>
		/// <returns>Returns true if the array of samples is silence; false otherwise.</returns>
		public bool IsSilence(double[] samples, double silenceThreshold)
		{
			// power_RMS = sqrt(sum(x^2) / N)

			double sum = 0;

			for (int i = 0; i < samples.Length; i++)
				sum += samples[i] * samples[i];

			double power_RMS = Math.Sqrt(sum / samples.Length);

			return power_RMS < silenceThreshold;
		}
		#endregion

		#region FSK Tone Detection
		/// <summary>
		/// Gets the default tone threshold value that can be passed to IsTone in order to
		/// get acceptable results.
		/// </summary>
		public double DefaultToneThreshold { get { return 0.5D; } }

		/// <summary>
		/// Uses the Görtzel algorithm to decide whether the specified array of sample data
		/// contains valid tones that represent FSK data.
		/// </summary>
		/// <param name="samples">An array of samples that is to be tested for FSK tones.</param>
		/// <param name="toneThreshold">How much power is required to consider a tone detected.</param>
		/// <returns>Returns true if the sample data contains tones that represent FSK data.</returns>
		public bool IsTone(double[] samples, double toneThreshold)
		{
			return
				GoertzelMagnitude(samples, FREQ_BIT_0, m_SampleRate) > toneThreshold ||
			GoertzelMagnitude(samples, FREQ_BIT_1, m_SampleRate) > toneThreshold;
		}

		/// <summary>
		/// Detects the strength of the given target frequency in the specified block of samples
		/// using the Görtzel algorithm. Does not consider phase information to save time.
		/// </summary>
		/// <param name="samples">An array of samples that is to be tested for the frequency.</param>
		/// <param name="targetFrequency">The frequency to test for.</param>
		/// <param name="sampleRate">The sample rate that was used when recording the samples.</param>
		/// <returns>Returns the strength of the target frequency in the specified block of samples. </returns>
		private static double GoertzelMagnitude(double[] samples, double targetFrequency, int sampleRate)
		{
			double n = samples.Length;

			int k = (int)(0.5D + ((double)n * targetFrequency) / (double)sampleRate);

			double w = (2.0D * Math.PI / n) * k;
			double cosine = Math.Cos(w);
			double coeff = 2.0D * cosine;

			double q0 = 0, q1 = 0, q2 = 0;

			for (int i = 0; i < samples.Length; i++)
			{
				double sample = samples[i];

				q0 = coeff * q1 - q2 + sample;
				q2 = q1;
				q1 = q0;
			}

			double magnitude = Math.Sqrt(q1 * q1 + q2 * q2 - q1 * q2 * coeff);

			return magnitude;
		}
		#endregion

		#region Phase-Locked Loop
		/// <summary>
		/// Part of the phase-locked loop (PLL). This is to be called every time a sample
		/// was processed by the DemodulateSample function. It increments the internal
		/// counter.
		/// </summary>
		public void IncrementPhase()
		{
			m_Phase = (m_Phase % PHASE_MAX) + m_PhaseIncrement;
		}

		/// <summary>
		/// Part of the phase-locked loop (PLL). This is to be called every time the last
		/// bit does not equal the current bit. It checks if the change came too early or
		/// too late, and then corrects the phase value appropriately.
		/// </summary>
		public void CorrectPhase()
		{
			if (m_Phase < PHASE_MAX / 2)
				m_Phase += m_PhaseCorrect; // Change came too early
			else
				m_Phase -= m_PhaseCorrect; // Change came too late
		}

		/// <summary>
		/// Part of the phase-locked loop (PLL). Use this to check if enough time has
		/// passed and the current bit can be considered and used for further processing.
		/// </summary>
		public bool CanUseBit { get { return m_Phase >= PHASE_MAX; } }
		#endregion
	}
}
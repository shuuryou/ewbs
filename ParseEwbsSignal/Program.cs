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
using System.Text;
using System.Windows.Forms;

namespace ParseEwbsSignal
{
	public static class Program
	{
		[STAThread()]
		public static void Main()
		{
			string audioFile;

			#region Browse for Audio File
			using (OpenFileDialog dialog = new OpenFileDialog())
			{
				dialog.Title = "Select Audio File";
				dialog.Filter = "Wave files (*.wav)|*.wav|All files|*.*";
				dialog.AutoUpgradeEnabled = true;
				dialog.CheckFileExists = true;

				if (dialog.ShowDialog() != DialogResult.OK)
					return;

				audioFile = dialog.FileName;
			}
			#endregion

			using (AudioFileReader reader = new AudioFileReader(audioFile))
			{
				AudioProcessor processor = new AudioProcessor(reader.SampleRate);

				double silenceMs;
				double[] buffer;
				
			ScanSilence:
				silenceMs = 0;
				buffer = new double[reader.SamplesPerMillisecond / 2];

				Console.WriteLine("Scanning for silence.");

				#region Scan for Silence
				while (reader.SamplesAvailable)
				{
					for (int i = 0; i < buffer.Length && reader.SamplesAvailable; i++)
						buffer[i] = reader.ReadSample();

					if (processor.IsSilence(buffer, processor.DefaultSilenceThreshold))
						silenceMs += ((double)buffer.Length / (double)reader.SamplesPerMillisecond);
					else
					{
						// If over 1.8 seconds of silence have been found, exit the loop.
						if (silenceMs > 1800)
						{
							Console.WriteLine("Found {0:n0}ms of silence. Ending silence scan.", silenceMs);

							goto DemodulateFSK; // could just use break...
						}

						// Otherwise, just reset the counter.
						silenceMs = 0;
					}
				}

				if (!reader.SamplesAvailable)
				{
					Console.WriteLine("Nothing left to scan for.");
					goto Done;
				}
				#endregion

			DemodulateFSK:
				silenceMs = 0;
				buffer = new double[reader.SamplesPerMillisecond];

				Console.WriteLine("Demodulating FSK signal.");

				#region Demodulate FSK
				double nonToneMs = 0;


				double[] demodulate_data = new double[processor.SampleArrayLength];
				int demodulate_data_index = 0; 
				
				string receivedBit;
				string previousBit = "0";

				StringBuilder receivedBits = new StringBuilder();

				while (reader.SamplesAvailable)
				{
					// Fill the buffer to check whether the next part is silence or non-tones.
					for (int i = 0; i < buffer.Length && reader.SamplesAvailable; i++)
						buffer[i] = reader.ReadSample();

					#region Detect Silence
					if (processor.IsSilence(buffer, processor.DefaultSilenceThreshold))
					{
						silenceMs += ((double)buffer.Length / (double)reader.SamplesPerMillisecond);

						if (silenceMs > 800)
						{
							Console.WriteLine("Found {0:n0}ms of silence after reading {1} samples. Ending FSK demodulation.",
									silenceMs, reader.SamplesRead);

							goto DecodeBits; // could just use break...
						}

						continue;
					}
					else
						silenceMs = 0;
					#endregion

					#region Check for FSK Tones
					if (!processor.IsTone(buffer, processor.DefaultToneThreshold))
					{
						nonToneMs += ((double)buffer.Length / (double)reader.SamplesPerMillisecond);

						if (nonToneMs >= 5)
						{
							Console.WriteLine("Found {0:n0}ms of non-tone samples. Aborting FSK demodulation.",
									nonToneMs, reader.SamplesRead);

							goto ScanSilence;
						}

						continue;
					}
					else
						nonToneMs = 0;
					#endregion

					// Not silence and probably FSK tones; run all samples through the demodulator.
					foreach (double sample in buffer)
					{
						double result = processor.DemodulateSample(ref demodulate_data,
							ref demodulate_data_index, sample);

						if (result < 0)
							receivedBit = "0";
						else
							receivedBit = "1";

						if (receivedBit != previousBit) // PLL
							processor.CorrectPhase();	// PLL

						if (processor.CanUseBit)		// PLL
							receivedBits.Append(receivedBit);

						previousBit = receivedBit;
						processor.IncrementPhase();		// PLL - these parts implement the phase-locked loop.
					}
				}

				if (!reader.SamplesAvailable)
				{
					Console.WriteLine("Nothing left to demodulate for.");

					if (receivedBits.Length == 0)
						goto Done;
					else
						goto DecodeBits;
				}
				#endregion

			DecodeBits:

				Console.WriteLine("Decoding received EWBS data.");

				#region Decode EWBS Data
				if (receivedBits.Length == 0)
				{
					Console.WriteLine("No EWBS data received?!"); // Should never get here...
					goto Done;
				}

				BlockDecoder decoder = new BlockDecoder(receivedBits.ToString());

				decoder.Decode();

				if (decoder.Confidence == ConfidenceLevel.None)
				{
					Console.WriteLine("Failed to decode EWBS data.");
					goto Done;
				}

				Console.WriteLine();
				Console.WriteLine(decoder.Bits);
				Console.Write("({0:n0} bit(s))", decoder.Bits.Length);
				Console.WriteLine();
				Console.WriteLine();

				Console.WriteLine("Decoded EWBS Data:");
				Console.WriteLine("-----------------------------------------------");
				Console.WriteLine("Fixed Code: {0}", decoder.FixedCode);
				Console.WriteLine("Confidence: {0}", decoder.Confidence);
				Console.WriteLine("Category:   {0}", decoder.Category);
				Console.WriteLine("Location:   {0}", decoder.Location);
				Console.WriteLine("Day:        {0}", decoder.Day);
				Console.WriteLine("Month:      {0}", decoder.Month);
				Console.WriteLine("Year:       {0}", decoder.Year);
				Console.WriteLine("Hour:       {0}", decoder.Hour);
				Console.WriteLine("-----------------------------------------------");
				Console.WriteLine();
				#endregion

			Done:
				Console.WriteLine("Processing of WAV file completed.");
				Console.ReadLine();
			}
		}
	}
}
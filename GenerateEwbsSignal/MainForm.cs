#region AGPL License Block
/* GenerateEwbsSignal - Generate Japanese Emergency Warning Broadcast System signal.
 * Copyright (C) 2013, 2021
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace GenerateEwbsSignal
{
	/// <summary>
	/// This is just a very simple signal generator that generates an Emergeny Warning
	/// Broadcast System FSK signal. The ParseEwbsSignal code was originally tested with
	/// recordings of acutal broadcasts, and this interface was only developed after the
	/// code was already able to demodulate and decode real-world broadcasts.
	/// 
	/// There is nothing too fancy here; just some basic WinForms for the GUI, a wave file
	/// writer (parameters are hardcoded), a silence generator, and a simple tone generator.
	/// 
	/// generateButton_Click() contains the GUI-to-binary-to-WAV-file translator.
	/// </summary>
	public partial class MainForm : Form
	{
		private Dictionary<string, string> m_Locations;

		public MainForm()
		{
			InitializeComponent();
			InitializeLocations();

			preceedingCodeComboBox.SelectedIndex = 0;
			fixedCodeComboBox.SelectedIndex = 0;
			locationCodeComboBox.SelectedIndex = 26;
			dateTimePicker.Value = DateTime.Now;
		}

		private void InitializeLocations()
		{
			// ARIB-STD-B10-2-D 表D-2 地域符号

			m_Locations = new Dictionary<string, string>();

			#region Add Locations
			m_Locations.Add("静岡県", "101001011010");
			m_Locations.Add("愛知県", "100101100110");
			m_Locations.Add("三重県", "001011011100");
			m_Locations.Add("滋賀県", "110011100100");
			m_Locations.Add("京都府", "010110011010");
			m_Locations.Add("大阪府", "110010110010");
			m_Locations.Add("兵庫県", "011001110100");
			m_Locations.Add("奈良県", "101010010011");
			m_Locations.Add("和歌山県", "001110010110");
			m_Locations.Add("鳥取県", "110100100011");
			m_Locations.Add("島根県", "001100011011");
			m_Locations.Add("岡山県", "001010110101");
			m_Locations.Add("広島県", "101100110001");
			m_Locations.Add("山口県", "101110011000");
			m_Locations.Add("徳島県", "111001100010");
			m_Locations.Add("香川県", "100110110100");
			m_Locations.Add("愛媛県", "000110011101");
			m_Locations.Add("高知県", "001011100011");
			m_Locations.Add("福岡県", "011000101101");
			m_Locations.Add("佐賀県", "100101011001");
			m_Locations.Add("長崎県", "101000101011");
			m_Locations.Add("熊本県", "100010100111");
			m_Locations.Add("大分県", "110010001101");
			m_Locations.Add("宮崎県", "110100011100");
			m_Locations.Add("鹿児島県", "110101000101");
			m_Locations.Add("沖縄県", "001101110010");
			m_Locations.Add("地域共通", "001101001101");
			m_Locations.Add("関東広域圏", "010110100101");
			m_Locations.Add("中京広域圏", "011100101010");
			m_Locations.Add("近畿広域圏", "100011010101");
			m_Locations.Add("鳥取・島根圏", "011010011001");
			m_Locations.Add("岡山・香川圏", "010101010011");
			m_Locations.Add("北海道", "000101101011");
			m_Locations.Add("青森県", "010001100111");
			m_Locations.Add("岩手県", "010111010100");
			m_Locations.Add("宮城県", "011101011000");
			m_Locations.Add("秋田県", "101011000110");
			m_Locations.Add("山形県", "111001001100");
			m_Locations.Add("福島県", "000110101110");
			m_Locations.Add("茨城県", "110001101001");
			m_Locations.Add("栃木県", "111000111000");
			m_Locations.Add("群馬県", "100110001011");
			m_Locations.Add("埼玉県", "011001001011");
			m_Locations.Add("千葉県", "000111000111");
			m_Locations.Add("東京都", "101010101100");
			m_Locations.Add("神奈川県", "010101101100");
			m_Locations.Add("新潟県", "010011001110");
			m_Locations.Add("富山県", "010100111001");
			m_Locations.Add("石川県", "011010100110");
			m_Locations.Add("福井県", "100100101101");
			m_Locations.Add("山梨県", "110101001010");
			m_Locations.Add("長野県", "100111010010");
			m_Locations.Add("岐阜県", "101001100101");
			#endregion

			foreach (string key in m_Locations.Keys)
				locationCodeComboBox.Items.Add(key);
		}

		private void preceedingCodeComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (preceedingCodeComboBox.SelectedIndex == 1)
			{
				fixedCodeComboBox.SelectedIndex = 0;
				fixedCodeComboBox.Enabled = false;
			}
			else
			{
				fixedCodeComboBox.Enabled = true;
			}
		}

		private void fixedCodeComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (fixedCodeComboBox.SelectedIndex == 1)
			{
				preceedingCodeComboBox.SelectedIndex = 0;
				preceedingCodeComboBox.Enabled = false;
			}
			else
			{
				preceedingCodeComboBox.Enabled = true;
			}
		}

		private void generateButton_Click(object sender, EventArgs e)
		{
			if (saveFileDialog.ShowDialog(this) != DialogResult.OK)
				return;

			byte[] audioSignal;

			#region Generate Audio Signal
			using (MemoryStream ms = new MemoryStream(50000))
			{
				GenerateSilence(ms, 3000);

				StringBuilder bits = new StringBuilder(64);

				bool isStartSignal;
				string fixedCode;
				
				#region Preparation
				isStartSignal = preceedingCodeComboBox.SelectedIndex == 0;

				switch (fixedCodeComboBox.SelectedIndex)
				{
					case 0:
						fixedCode = "0000111001101101";
						break;
					case 1:
						fixedCode = "1111000110010010";
						break;
					default:
						Trace.Fail("Fixed Code ComboBox has invalid state.");
						return;
				}
				#endregion

				// Preceeding Code
				if (isStartSignal)
					bits.Append("1100");
				else
					bits.Append("0011");

				#region Create Block
				// Fixed Code
				bits.Append(fixedCode);

				// Location Code
				if (isStartSignal)
					bits.Append("10");
				else
					bits.Append("01");

				bits.Append(m_Locations[locationCodeComboBox.SelectedItem.ToString()]);

				if (isStartSignal)
					bits.Append("00");
				else
					bits.Append("11");

				// Fixed Code
				bits.Append(fixedCode);

				// Month Day Division
				if (isStartSignal)
					bits.Append("010");
				else
					bits.Append("100");

				bits.Append(Reverse(Convert.ToString(dateTimePicker.Value.Day, 2).PadLeft(5, '0')));

				bits.Append("0");

				bits.Append(Reverse(Convert.ToString(dateTimePicker.Value.Month, 2).PadLeft(4, '0')));

				if (isStartSignal)
					bits.Append("100");
				else
					bits.Append("111");

				// Fixed Code
				bits.Append(fixedCode);

				// Year Hour Division
				if (isStartSignal)
					bits.Append("011");
				else
					bits.Append("101");

				int hour = dateTimePicker.Value.Hour;

				if (hour >= 16)
					hour += 8;
				else
					hour -= 8;

				bits.Append(Reverse(Convert.ToString(hour, 2).PadLeft(5, '0')));

				bits.Append("0");

				bits.Append(Reverse(Convert.ToString(dateTimePicker.Value.Year % 10, 2).PadLeft(4, '0')));

				if (isStartSignal)
					bits.Append("100");
				else
					bits.Append("111");
				#endregion

				Trace.Assert(bits.Length == 100, "Length of bit stream is not 100 bits.");

				GenerateTone(ms, bits.ToString()); // Output preceeding code + block

				string loopBits = bits.ToString().Substring(4); // Retain block

				if (isStartSignal)
				{
					for (int i = 0; i < 9; i++) // Output it 9 more times
						GenerateTone(ms, loopBits);
				}
				
				if (!isStartSignal)
				{
					for (int i = 0; i < 3; i++) // Output it 3 more times with silence
					{
						GenerateSilence(ms, 1530);
						GenerateTone(ms, loopBits);
					}

					GenerateSilence(ms, 1530); // Final silence
				}

				// Generate final silence
				GenerateSilence(ms, 1000);

				audioSignal = ms.ToArray();
			}
			#endregion

			try
			{
				#region Write Wave File
				using (BinaryWriter bw = new BinaryWriter(File.OpenWrite(saveFileDialog.FileName)))
				{
					byte[] buffer;

					#region RIFF Chunk
					// ChunkID
					buffer = Encoding.ASCII.GetBytes("RIFF");
					bw.Write(buffer, 0, buffer.Length);

					// ChunkSize
					buffer = BitConverter.GetBytes((int)36 + audioSignal.Length);
					bw.Write(buffer, 0, buffer.Length);

					// Format
					buffer = Encoding.ASCII.GetBytes("WAVE");
					bw.Write(buffer, 0, buffer.Length);
					#endregion

					#region fmt  Chunk
					// ChunkID
					buffer = Encoding.ASCII.GetBytes("fmt ");
					bw.Write(buffer, 0, buffer.Length);

					// Subchunk1Size
					buffer = BitConverter.GetBytes((int)16);
					bw.Write(buffer, 0, buffer.Length);

					// AudioFormat
					buffer = BitConverter.GetBytes((short)1);
					bw.Write(buffer, 0, buffer.Length);

					// NumChannels
					buffer = BitConverter.GetBytes((short)1);
					bw.Write(buffer, 0, buffer.Length);

					// SampleRate
					buffer = BitConverter.GetBytes((int)32000);
					bw.Write(buffer, 0, buffer.Length);

					// ByteRate
					buffer = BitConverter.GetBytes((int)(32000 * 1 * (16 / 8)));
					bw.Write(buffer, 0, buffer.Length);

					// BlockAlign
					buffer = BitConverter.GetBytes((short)(1 * (16 / 8)));
					bw.Write(buffer, 0, buffer.Length);

					// BitsPerSample
					buffer = BitConverter.GetBytes((short)16);
					bw.Write(buffer, 0, buffer.Length);
					#endregion

					#region data Chunk
					// Subchunk2ID
					buffer = Encoding.ASCII.GetBytes("data");
					bw.Write(buffer, 0, buffer.Length);

					// Subchunk2Size
					buffer = BitConverter.GetBytes((int)audioSignal.Length);
					bw.Write(buffer, 0, buffer.Length);

					// Data
					bw.Write(audioSignal);
					#endregion

					bw.Flush();
					bw.Close();
				}
				#endregion

				MessageBox.Show(this, Resources.SaveSucceeded, Resources.SaveTitle, MessageBoxButtons.OK);
			}
			catch (IOException ex)
			{
				string message = string.Format(CultureInfo.CurrentUICulture, Resources.SaveFailed, ex.Message);
				MessageBox.Show(this, message, Resources.SaveTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private static string Reverse(string s)
		{
			char[] charArray = s.ToCharArray();
			Array.Reverse(charArray);
			return new string(charArray);
		}

		#region Silence and Tone Generator
		private static void GenerateSilence(Stream stream, double length)
		{
			if (!stream.CanWrite)
				throw new InvalidOperationException("Stream is not writable.");

			length /= 1000;
			length = length * 32000 * 2;

			Trace.Assert(length % 2 == 0, "Length is not cleanly divisible by 2.");

			byte[] data = new byte[(int)length];
			stream.Write(data, 0, data.Length);
		}

		private static void GenerateTone(Stream stream, string bits)
		{
			const int FREQ_BIT_0 = 640;
			const int FREQ_BIT_1 = 1024;

			double amplitude = 0.5 * short.MaxValue;
			double length = 32000 / 64;

			Trace.Assert(length % 2 == 0, "Length is not cleanly divisible by 2.");

			int counter = 0;

			foreach (char bit in bits)
			{
				short value;

				if (bit != '0' && bit != '1')
					throw new InvalidDataException("The bit string can only contain '0' and '1'.");

				double frequency = bit == '0' ? FREQ_BIT_0 : FREQ_BIT_1;

				for (int i = 0; i < length; i++)
				{
					value = (short)(amplitude * Math.Sin((2 * Math.PI * counter++ * frequency) / 32000));
		
					byte[] data = BitConverter.GetBytes(value);
					stream.Write(data, 0, data.Length);
				}
			}
		}
		#endregion
	}
}
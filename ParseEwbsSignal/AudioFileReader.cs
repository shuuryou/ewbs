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
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ParseEwbsSignal
{
	/// <summary>A poor man's wave file parser. Supports reading simple PCM wave files
	/// (compression is not supported) that have a single channel (mono). Files can be
	/// read sample-by-sample, and it is possible to backtrack.
	/// </summary>
	/// <remarks>
	/// The implementation of this class is closely based on the specification found at:
	/// <http://web.archive.org/web/https://ccrma.stanford.edu/courses/422/projects/WaveFormat/>.
	/// </remarks>
	public class AudioFileReader : IDisposable
	{
		private BinaryReader m_Reader = null;

		private int m_ChunkSize;
		private int m_SubChunk1Size;
		private short m_AudioFormat;
		private short m_NumChannels;
		private int m_SampleRate;
		private int m_ByteRate;
		private short m_BlockAlign;
		private short m_BitsPerSample;
		private int m_SubChunk2Size;

		private long m_ReadFrom;
		private long m_ReadTo;

		private long m_samplesRead;

		/// <summary>
		/// Releases all resources used by the current instance of the WavFileReader class.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (m_Reader != null)
				{
					m_Reader.Close();
					m_Reader.Dispose();
				}
			}

			m_Reader = null;
		}

		/// <summary>
		/// Creates a new instance of the WavFileReader class for the specified file.
		/// </summary>
		/// <param name="wavFile">The full path to a single-channel (mono) PCM wave file.</param>
		/// <exception cref="InvalidDataException">
		/// Thrown if the specified file contains an unsupported audio format, an unsupported
		/// number of audio channels, or an unsupported header.
		/// </exception>
		public AudioFileReader(string wavFile)
		{
			m_Reader = new BinaryReader(File.OpenRead(wavFile));

			ParseHeader();

			if (m_AudioFormat != 1)
				throw new InvalidDataException("Unsupported audio format. Only PCM is supported.");

			if (m_NumChannels != 1)
				throw new InvalidDataException("Unsupported number of audio channels. Only mono is supported.");

			Trace.Assert(m_Reader.BaseStream.Position == 44, "Not at start of PCM data for some reason.");

			m_ReadFrom = m_Reader.BaseStream.Position;

			// We can't just read to end of file. There may be additional subchunks.
			m_ReadTo = m_Reader.BaseStream.Position + m_SubChunk2Size;

			m_samplesRead = 0;
		}

		/// <summary>
		/// Parses the header of a WAV file. Assumes that there are only two sub-chunks after
		/// the RIFF Chunk; "fmt " and "data". Any other sub-chunks will cause an exception to
		/// be thrown.
		/// </summary>
		/// <exception cref="InvalidDataException">Thrown if the chunk or sub-chunk IDs do not
		/// match the expected values.</exception>
		private void ParseHeader()
		{
			byte[] data; // Holds ASCII characters; used to compare text values

			#region RIFF Chunk Descriptor
			// ChunkID field
			data = m_Reader.ReadBytes(4);

			if (Encoding.ASCII.GetString(data) != "RIFF")
				throw new InvalidDataException(@"ChunkID field is not ""RIFF"".");

			// ChunkSize field
			m_ChunkSize = m_Reader.ReadInt32();

			// Format field
			data = m_Reader.ReadBytes(4);

			if (Encoding.ASCII.GetString(data) != "WAVE")
				throw new InvalidDataException(@"Format field is not ""WAVE"".");
			#endregion

			#region Fmt Sub-Chunk
			// Subchunk1ID field
			data = m_Reader.ReadBytes(4);

			if (Encoding.ASCII.GetString(data) != "fmt ")
				throw new InvalidDataException(@"Subchunk1ID field is not ""fmt "".");

			// Subchunk1Size field
			m_SubChunk1Size = m_Reader.ReadInt32();

			// AudioFormat field
			m_AudioFormat = m_Reader.ReadInt16();

			// NumChannels field
			m_NumChannels = m_Reader.ReadInt16();

			// SampleRate field
			m_SampleRate = m_Reader.ReadInt32();

			// ByteRate field
			m_ByteRate = m_Reader.ReadInt32();

			// BlockAlign field
			m_BlockAlign = m_Reader.ReadInt16();

			// BitsPerSample field
			m_BitsPerSample = m_Reader.ReadInt16();
			#endregion

			#region Data Sub-Chunk
			data = m_Reader.ReadBytes(4);

			if (Encoding.ASCII.GetString(data) != "data")
				throw new InvalidDataException(@"Subchunk2ID field is not ""data"".");

			m_SubChunk2Size = m_Reader.ReadInt32();
			#endregion
		}

		/// <summary>Gets the number of samples per millisecond of audio.</summary>
		public int SamplesPerMillisecond
		{
			get { return (m_SampleRate * m_NumChannels) / 1000; }
		}

		/// <summary>Gets the sample rate.</summary>
		public int SampleRate
		{
			get { return m_SampleRate; }
		}

		/// <summary>Gets the number of samples read so far.</summary>
		public long SamplesRead
		{
			get { return m_samplesRead; }
		}

		/// <summary>Gets whether additional samples are available for reading.</summary>
		public bool SamplesAvailable
		{
			get
			{
				if (m_Reader == null || !m_Reader.BaseStream.CanRead)
					return false;

				return m_Reader.BaseStream.Position < m_ReadTo;
			}
		}

		/// <summary>Reads a single sample and returns it as a normalized value.</summary>
		/// <returns>The normalized double representation of the sample.</returns>
		public double ReadSample()
		{
			if (m_Reader.BaseStream.Position >= m_ReadTo)
				throw new InvalidOperationException("No more samples available for reading.");

			m_samplesRead++;

			switch (m_BitsPerSample / 8)
			{
				case 1: // 8bit
					return ((double)m_Reader.ReadByte() - 127.5D) / 256D;
				case 2: // 16bit
					return (double)m_Reader.ReadInt16() / 32768D;
				case 4: // 32bit
					return (double)m_Reader.ReadInt32() / 2147483648D;
				case 8: // 64bit ?!?!?!?!
					return (double)m_Reader.ReadInt64() / 9223372036854775808D;
				default:
					throw new InvalidOperationException("Unsupported number of bits per sample.");
			}
		}

		/// <summary>Rewinds the current position in the WAV file by a single sample.</summary>
		/// <returns>Returns true of the operation succeded and false otherwise.</returns>
		public bool Backtrack()
		{
			if (m_Reader.BaseStream.Position <= m_ReadFrom)
				return false;

			long offset = m_Reader.BaseStream.Position;

			m_Reader.BaseStream.Seek(-1 * (m_BitsPerSample / 8), SeekOrigin.Current);

			if (m_Reader.BaseStream.Position < offset)
			{
				m_samplesRead--;
				return true;
			}

			return false;
		}
	}
}
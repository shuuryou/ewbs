#region AGPL License Block
/* ParseEwbsSignal- Parse Japanese Emergency Warning Broadcast System signal.
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

namespace ParseEwbsSignal
{
	public enum ConfidenceLevel
	{
		/// <summary>The decoder has absolutely no confidence in its result.</summary>
		None = 0,

		/// <summary>
		/// The decoder could only decode the fixed code and has little confidence
		/// in its result.
		/// </summary>
		Poor = 1,

		/// <summary>
		/// The decoder was able to decode some information in addition to the fixed
		/// code, but a majority of information may be missing.
		/// </summary>
		Weak = 2,

		/// <summary>
		/// The decoder was able to decode most information, but had to guess the type
		/// of signal in the block. However, the decoded data should be correct.
		/// </summary>
		Medium = 3,

		/// <summary>
		/// The decoder was able to decode all information in the block and has strong
		/// confidence in the accuracy of its result.
		/// </summary>
		Strong = 4,
	}

	public enum BlockCategory
	{
		/// <summary>No signal type has been determined.</summary>
		None = 0,

		/// <summary>The block contains a Category I start signal.</summary>
		Category1Start = 1,

		/// <summary>The block contains a Category II start signal.</summary>
		Category2Start = 2,

		/// <summary>The block contains a Category I or Category II end signal.</summary>
		End = 3,

		/// <summary>The block contains an unknown signal.</summary>
		Unknown = 4,
	}
	
	/// <summary>
	/// A decoder for blocks of data broadcasted using the Japanese Emergency Warning
	/// Broadcast System. Takes a demodulated string of bits and interprets them.
	/// 
	/// The way the information is decoded is rather unorthodox, but probably a little
	/// more robust than conventional implementations. The approach used here does not
	/// require a perfect block transmission, and can recover missing information as
	/// long as the fixed code is correct and the missing data appears correctly
	/// *somewhere* in the bit stream.
	/// </summary>
	/// <remarks>
	/// The decoder has been implemented based on own studies of the binary data, as
	/// well as on the following technical article (Japanese), which filled in all of
	/// the gaps:
	/// 
	/// 伊藤, 泰宏. "知っておきたいキーワード 第18回 緊急警報放送." 映像情報メディア学会誌 43th ser. 61.6 (2007): 761-63.
	/// </remarks>
	public class BlockDecoder
	{
		/// <summary>
		/// Creates a new instance of the EwbsBlockDecoder class, which can later be used to
		/// decode the specified bits.
		/// </summary>
		/// <param name="bits">The raw demodulated bits from the FSK demodulator.</param>
		public BlockDecoder(string bits)
		{
			Bits = bits;

			InitializeLocations();
		}

		/// <summary>
		/// Decodes the bits received from the FSK demodulator and assigns the decoded
		/// values to their respective class properties.
		/// </summary>
		public void Decode()
		{
			Category = BlockCategory.None;
			Confidence = ConfidenceLevel.None;

			bool gotType = DetermineType();

			bool gotFixedCode = CheckFixedCode();

			// We cannot decode if there is no fixed code at all; probably not EWBS block
			if (!gotFixedCode)
				return;

			// The only sane way to decode the signal is to break it up by the fixed code
			// and hope for the best. There may be bits missing, flipped, etc. FSK is awful.
			// 
			// EWBS has no concept of error correction other than 「頑張ってください」. A
			// block is retransmitted a few times and then the receiver has to interpret
			// whatever it received.

			bool gotLocationCode = false, gotDayMonthCode = false, gotHourYearCode = false;

			string[] parts = Bits.Split(new string[] { FixedCode }, StringSplitOptions.RemoveEmptyEntries);

			foreach (string part in parts)
			{
				if (part.Length != 16) continue; // Invalid; skip it.

				if (!gotHourYearCode && DecodeHourYear(part)) gotHourYearCode = true;
				if (!gotDayMonthCode && DecodeDayMonth(part)) gotDayMonthCode = true;
				if (!gotLocationCode && DecodeLocation(part)) gotLocationCode = true;

				if (gotHourYearCode && gotDayMonthCode && gotLocationCode)
					break;
			}

			// Assign a confidence level. The final else means all we received was the
			// fixed code.

			if (gotType && gotFixedCode && gotHourYearCode && gotDayMonthCode && gotLocationCode)
				Confidence = ConfidenceLevel.Strong;
			else if (gotFixedCode && gotHourYearCode && gotDayMonthCode && gotLocationCode)
				Confidence = ConfidenceLevel.Medium;
			else if (gotFixedCode && (gotHourYearCode || gotDayMonthCode || gotLocationCode))
				Confidence = ConfidenceLevel.Weak;
			else
				Confidence = ConfidenceLevel.Poor;
		}

		/// <summary>Gets the bits that were passed to the class constructor.</summary>
		public string Bits { get; private set; }

		/// <summary>Gets the fixed code (固定符号) that is used to separate entities.</summary>
		public string FixedCode { get; private set; }

		/// <summary>Gets the decoded block category type.</summary>
		public BlockCategory Category { get; private set; }

		/// <summary>Gets the hour the incident occurred.</summary>
		public short Hour { get; private set; }

		/// <summary>Gets the day the incident occurred.</summary>
		public short Day { get; private set; }

		/// <summary>Gets the month the incident occurred.</summary>
		public short Month { get; private set; }

		/// <summary>Gets the year the incident occurred.</summary>
		public short Year { get; private set; }

		/// <summary>
		/// Gets the location (defined by standard ARIB-STD-B10-2-D) the incident occurred at.
		/// </summary>
		public string Location { get; private set; }

		/// <summary>
		/// Gets the level of confidence the block decoderhas in the values returned by the
		/// other properties. Higher confidence is better. Anything lower than Medium should
		/// be interpreted as a "something happened somewhere" warning only.
		/// </summary>
		public ConfidenceLevel Confidence { get; private set; }


		#region Private Decoder Methods
		private bool DetermineType()
		{
			if (Bits.StartsWith("1100", StringComparison.Ordinal))
			{
				Category = BlockCategory.Category1Start;
				return true;
			}

			if (Bits.StartsWith("0011", StringComparison.Ordinal))
			{
				Category = BlockCategory.End;
				return true;
			}

			Category = BlockCategory.Unknown;
			return false;
		}

		private bool CheckFixedCode()
		{
			if (Bits.Contains("0000111001101101"))
			{
				FixedCode = "0000111001101101";
				return true;
			}

			if (Bits.Contains("1111000110010010"))
			{
				FixedCode = "1111000110010010";
				Category = BlockCategory.Category2Start;
				return true;
			}

			return false;
		}

		private bool DecodeDayMonth(string part)
		{
			if (Category == BlockCategory.Unknown)
			{
				// Try to guess it. *sigh*

				if (part.StartsWith("010", StringComparison.Ordinal) && part.EndsWith("100", StringComparison.Ordinal))
					Category = BlockCategory.Category1Start;
				else if (part.StartsWith("100", StringComparison.Ordinal) && part.EndsWith("111", StringComparison.Ordinal))
					Category = BlockCategory.End;
				else
					return false;
			}

			bool ok;

			ok = ((Category == BlockCategory.Category1Start || Category == BlockCategory.Category2Start)
					&& part.StartsWith("010", StringComparison.Ordinal)
					&& part.EndsWith("100", StringComparison.Ordinal))
			||
			(Category == BlockCategory.End && part.StartsWith("100", StringComparison.Ordinal)
				&& part.EndsWith("111", StringComparison.Ordinal));

			if (!ok)
				return false;

			ok = part.Substring(8, 1) == "0";

			if (!ok)
				return false;

			Day = Convert.ToInt16(Reverse(part.Substring(3, 5)), 2);
			Month = Convert.ToInt16(Reverse(part.Substring(9, 4)), 2);

			if (Day > 31 || Month > 12)
			{
				Day = 0;
				Month = 0;
				return false;
			}

			return true;
		}

		private bool DecodeHourYear(string part)
		{
			if (Category == BlockCategory.Unknown)
			{
				// Try to guess it. *sigh*

				if (part.StartsWith("011", StringComparison.Ordinal) && part.EndsWith("100", StringComparison.Ordinal))
					Category = BlockCategory.Category1Start;
				else if (part.StartsWith("101", StringComparison.Ordinal) && part.EndsWith("111", StringComparison.Ordinal))
					Category = BlockCategory.End;
				else
					return false;
			}

			bool ok;

			ok =
				((Category == BlockCategory.Category1Start || Category == BlockCategory.Category2Start)
					&& part.StartsWith("011", StringComparison.Ordinal) 
					&& part.EndsWith("100", StringComparison.Ordinal))
				||
				(Category == BlockCategory.End && part.StartsWith("100", StringComparison.Ordinal)
					&& part.EndsWith("111", StringComparison.Ordinal))
				||
				(Category == BlockCategory.End && part.StartsWith("101", StringComparison.Ordinal)
					&& part.EndsWith("111", StringComparison.Ordinal))
				||
				(Category == BlockCategory.End && part.StartsWith("010", StringComparison.Ordinal)
					&& part.EndsWith("111", StringComparison.Ordinal));

			if (!ok)
				return false;

			ok = part.Substring(8, 1) == "0";

			if (!ok)
				return false;

			Hour = Convert.ToInt16(Reverse(part.Substring(3, 5)), 2);

			// 無線設備規則第九条の三第五号の規定に基づく緊急警報信号の構成
			// 昭和60年06月01日 郵政省告示第405号
			// 別表第4号 時符号
			if (Hour >= 16)
				Hour -= 8;
			else
				Hour += 8;

			Year = Convert.ToInt16(Reverse(part.Substring(9, 4)), 2);

			if (Hour > 23)
			{
				Year = 0;
				Hour = 0;

				return false;
			}

			return true;
		}

		private bool DecodeLocation(string part)
		{
			BlockCategory guessedCategory;

			if (Category == BlockCategory.Unknown)
			{
				// Try to guess it. *sigh*

				if (part.StartsWith("10", StringComparison.Ordinal) && part.EndsWith("00", StringComparison.Ordinal))
					guessedCategory = BlockCategory.Category1Start;
				else if (part.StartsWith("01", StringComparison.Ordinal) && part.EndsWith("11", StringComparison.Ordinal))
					guessedCategory = BlockCategory.End;
				else
					return false;
			}
			else
				guessedCategory = Category;

			string locationCode = part.Substring(2, 12);

			if (!LOCATIONS.ContainsKey(locationCode))
				return false;

			Location = LOCATIONS[locationCode];
			Category = guessedCategory;

			return true;
		}

		#endregion

		#region Static Variables and Methods
		private static Dictionary<string, string> LOCATIONS = null;

		private static void InitializeLocations()
		{
			// ARIB-STD-B10-2-D 表D-2 地域符号

			if (LOCATIONS != null)
				return;

			LOCATIONS = new Dictionary<string, string>();
			
			LOCATIONS.Add("101001011010", "静岡県");
			LOCATIONS.Add("100101100110", "愛知県");
			LOCATIONS.Add("001011011100", "三重県");
			LOCATIONS.Add("110011100100", "滋賀県");
			LOCATIONS.Add("010110011010", "京都府");
			LOCATIONS.Add("110010110010", "大阪府");
			LOCATIONS.Add("011001110100", "兵庫県");
			LOCATIONS.Add("101010010011", "奈良県");
			LOCATIONS.Add("001110010110", "和歌山県");
			LOCATIONS.Add("110100100011", "鳥取県");
			LOCATIONS.Add("001100011011", "島根県");
			LOCATIONS.Add("001010110101", "岡山県");
			LOCATIONS.Add("101100110001", "広島県");
			LOCATIONS.Add("101110011000", "山口県");
			LOCATIONS.Add("111001100010", "徳島県");
			LOCATIONS.Add("100110110100", "香川県");
			LOCATIONS.Add("000110011101", "愛媛県");
			LOCATIONS.Add("001011100011", "高知県");
			LOCATIONS.Add("011000101101", "福岡県");
			LOCATIONS.Add("100101011001", "佐賀県");
			LOCATIONS.Add("101000101011", "長崎県");
			LOCATIONS.Add("100010100111", "熊本県");
			LOCATIONS.Add("110010001101", "大分県");
			LOCATIONS.Add("110100011100", "宮崎県");
			LOCATIONS.Add("110101000101", "鹿児島県");
			LOCATIONS.Add("001101110010", "沖縄県");
			LOCATIONS.Add("001101001101", "地域共通");
			LOCATIONS.Add("010110100101", "関東広域圏");
			LOCATIONS.Add("011100101010", "中京広域圏");
			LOCATIONS.Add("100011010101", "近畿広域圏");
			LOCATIONS.Add("011010011001", "鳥取・島根圏");
			LOCATIONS.Add("010101010011", "岡山・香川圏");
			LOCATIONS.Add("000101101011", "北海道");
			LOCATIONS.Add("010001100111", "青森県");
			LOCATIONS.Add("010111010100", "岩手県");
			LOCATIONS.Add("011101011000", "宮城県");
			LOCATIONS.Add("101011000110", "秋田県");
			LOCATIONS.Add("111001001100", "山形県");
			LOCATIONS.Add("000110101110", "福島県");
			LOCATIONS.Add("110001101001", "茨城県");
			LOCATIONS.Add("111000111000", "栃木県");
			LOCATIONS.Add("100110001011", "群馬県");
			LOCATIONS.Add("011001001011", "埼玉県");
			LOCATIONS.Add("000111000111", "千葉県");
			LOCATIONS.Add("101010101100", "東京都");
			LOCATIONS.Add("010101101100", "神奈川県");
			LOCATIONS.Add("010011001110", "新潟県");
			LOCATIONS.Add("010100111001", "富山県");
			LOCATIONS.Add("011010100110", "石川県");
			LOCATIONS.Add("100100101101", "福井県");
			LOCATIONS.Add("110101001010", "山梨県");
			LOCATIONS.Add("100111010010", "長野県");
			LOCATIONS.Add("101001100101", "岐阜県");
		}
		private static string Reverse(string s)
		{
			char[] charArray = s.ToCharArray();
			Array.Reverse(charArray);
			return new string(charArray);
		}
		#endregion
	}
}
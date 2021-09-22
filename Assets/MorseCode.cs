using System.Collections.Generic;

public class MorseCode
{
	public static readonly Dictionary<char, string> Chart = new Dictionary<char, string>
	{
		{ 'A', ".-" },
		{ 'B', "-..." },
		{ 'C', "-.-." },
		{ 'D', "-.." },
		{ 'E', "." },
		{ 'F', "..-." },
		{ 'G', "--." },
		{ 'H', "...." },
		{ 'I', ".." },
		{ 'J', ".---" },
		{ 'K', "-.-" },
		{ 'L', ".-.." },
		{ 'M', "--" },
		{ 'N', "-." },
		{ 'O', "---" },
		{ 'P', ".--." },
		{ 'Q', "--.-" },
		{ 'R', ".-." },
		{ 'S', "..." },
		{ 'T', "-" },
		{ 'U', "..-" },
		{ 'V', "...-" },
		{ 'W', ".--" },
		{ 'X', "-..-" },
		{ 'Y', "-.--" },
		{ 'Z', "--.." },
		{ '1', ".----" },
		{ '2', "..---" },
		{ '3', "...--" },
		{ '4', "....-" },
		{ '5', "....." },
		{ '6', "-...." },
		{ '7', "--..." },
		{ '8', "---.." },
		{ '9', "----." },
		{ '0', "-----" }
	};

	private static readonly Dictionary<char, int> morseCharacterLengths = new Dictionary<char, int>
	{
		{ '.', 1 },
		{ '-', 3 }
	};

	private static readonly int characterGap = 1;
	private static readonly int letterGap = 3;
	private static readonly int wordGap = 7;

	private static void AddCountToQueue(ref Queue<bool> queue, int count, bool value)
	{
		for (int i = 0; i < count; i++) queue.Enqueue(value);
	}

	public static Queue<bool> CreateMarqueeQueue(string input)
	{
		Queue<bool> queue = new Queue<bool>();
		bool firstInputChar = true;
		foreach (char inputChar in input.ToUpperInvariant())
		{
			if (Chart.ContainsKey(inputChar))
			{
				bool firstMorseChar = true;
				foreach (char morseChar in Chart[inputChar])
				{
					if (firstMorseChar)
					{
						firstMorseChar = false;
						if (firstInputChar) firstInputChar = false;
						else AddCountToQueue(ref queue, letterGap, false);
					}
					else AddCountToQueue(ref queue, characterGap, false);
					AddCountToQueue(ref queue, morseCharacterLengths[morseChar], true);
				}
			}
		}
		AddCountToQueue(ref queue, wordGap, false);
		return queue;
	}
}
using KeepCoding;
using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class MarqueeMorseTPScript : TPScript<MarqueeMorseScript>
{
	public override IEnumerator Process(string command)
	{
		string[] split = command.ToLowerInvariant().Split();
		if (Regex.IsMatch(split[0], "submit|trans|transmit|tx|xmit"))
		{
			int submitIndex = -1;

			string[] stringFrequencies = MarqueeMorseScript.frequencies.Select(f => f.ToString()).ToArray();
			if (stringFrequencies.Contains(split[1])) submitIndex = Array.IndexOf(stringFrequencies, split[1]);
			else if (MarqueeMorseScript.words.Contains(split[1])) submitIndex = Array.IndexOf(MarqueeMorseScript.words.ToArray(), split[1]);

			if (submitIndex > -1)
			{
				yield return null;
				bool moveRight = submitIndex > Module.currentFrequency;
				while (Module.currentFrequency != submitIndex)
				{
					if (moveRight) Module.MoveRight();
					else Module.MoveLeft();
					yield return new WaitForSeconds(0.1f);
				}
				Module.Transmit();
			}
		}
		yield break;
	}

	public override IEnumerator ForceSolve()
	{
		bool moveRight = Module.correctFrequency > Module.currentFrequency;
		while (Module.currentFrequency != Module.correctFrequency)
		{
			if (moveRight) Module.MoveRight();
			else Module.MoveLeft();
			yield return new WaitForSeconds(0.1f);
		}
		Module.Transmit();
	}
}

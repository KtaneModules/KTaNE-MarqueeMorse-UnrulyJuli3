using KeepCoding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class MarqueeMorseScript : ModuleScript
{
	public Transform LightSpawn;
	public MarqueeLight LightPrefab;
	public KMSelectable LeftButton;
	public KMSelectable RightButton;
	public KMSelectable TransmitButton;
	public Transform Needle;
	public TextMesh InputLabel;
	public Renderer[] LitLabels;

	private readonly MarqueeLight[] marqueeLights = new MarqueeLight[10];
	private Queue<bool> currentMarqueeQueue;

	private static readonly List<float> frequencies = new List<float>
	{
		99.9f,
		100.1f,
		100.3f,
		100.5f,
		100.7f,
		100.9f,
		101.1f,
		101.3f,
		101.5f,
		101.7f,
		101.9f,
		102.1f,
		102.3f,
		102.5f,
		102.7f,
		102.9f,
		103.1f,
		103.3f,
		103.5f,
		103.7f
	};

	private static readonly List<string> words = new List<string>
	{
		"intervention",
		"consistently",
		"optimization",
		"spirituality",
		"contributors",
		"collectables",
		"consultation",
		"construction",
		"registration",
		"agricultural",
		"manufactured",
		"implications",
		"subscription",
		"capabilities",
		"jurisdiction",
		"illustration",
		"laboratories",
		"descriptions",
		"publications",
		"representing"
	};

	private int correctFrequency;
	private int currentFrequency;

	private void Start()
	{
		CreateMarqueeLights();
		correctFrequency = UnityEngine.Random.Range(0, frequencies.Count);
		currentMarqueeQueue = MorseCode.CreateMarqueeQueue(words[correctFrequency]);
		SetNeedleAtLerp(0f);
		DisplayCurrentFrequency();
		LeftButton.Assign(onInteract: MoveLeft);
		RightButton.Assign(onInteract: MoveRight);
		TransmitButton.Assign(onInteract: Transmit);

		LitLabels.ForEach(r => r.enabled = false);
		Needle.GetComponent<Renderer>().enabled = false;

		Log("Correct word: {0} ({1} MHz)", words[correctFrequency], frequencies[correctFrequency]);
	}

	public override void OnActivate()
	{
		DisplayMarqueeQueue();
		StartCoroutine(ScrollMarqueeQueue());
		LitLabels.ForEach(r => r.enabled = true);
		Needle.GetComponent<Renderer>().enabled = true;
	}

	private void CreateMarqueeLights()
	{
		for (int i = 0; i < marqueeLights.Length; i++)
		{
			MarqueeLight light = Instantiate(LightPrefab, LightSpawn);
			light.transform.localPosition += new Vector3(i * 1.18f, 0f, 0f);
			light.gameObject.SetActive(true);
			light.Set(false);
			marqueeLights[i] = light;
		}
	}

	private bool scrollingQueue;

	private IEnumerator ScrollMarqueeQueue()
	{
		if (scrollingQueue) yield break;
		yield return new WaitForSeconds(1f);
		if (scrollingQueue) yield break;
		scrollingQueue = true;
		while (scrollingQueue)
		{
			yield return new WaitForSecondsRealtime(0.15f);
			if (!scrollingQueue) yield break;
			currentMarqueeQueue.Enqueue(currentMarqueeQueue.Dequeue());
			DisplayMarqueeQueue();
		}
		yield break;
	}

	private void DisplayMarqueeQueue()
	{
		for (int i = 0; i < marqueeLights.Length; i++) marqueeLights[i].Set(i < currentMarqueeQueue.Count() && currentMarqueeQueue.ElementAt(i));
	}

	private void SetNeedleAtLerp(float lerp)
	{
		Needle.localScale = new Vector3(lerp, 1f, 1f);
		Needle.localPosition = new Vector3((lerp - 1f) / 2f, 0f, 0f);
	}

	private float currentNeedleLerp = 0f;
	private Coroutine currentNeedleCoroutine;

	private IEnumerator NeedleToLerp(float toLerp)
	{
		float fromLerp = currentNeedleLerp;
		float duration = 2f;
		float initialTime = Time.time;
		while (Time.time - initialTime < duration)
		{
			currentNeedleLerp = Mathf.Lerp(fromLerp, toLerp, (Time.time - initialTime) / duration);
			SetNeedleAtLerp(currentNeedleLerp);
			yield return null;
		}
		SetNeedleAtLerp(currentNeedleLerp);
		yield break;
	}

	private void MoveNeedle(float toLerp)
	{
		if (currentNeedleCoroutine != null) StopCoroutine(currentNeedleCoroutine);
		currentNeedleCoroutine = StartCoroutine(NeedleToLerp(toLerp));
	}

	private void DisplayCurrentFrequency()
	{
		MoveNeedle((float)currentFrequency / (frequencies.Count - 1));
		InputLabel.text = frequencies[currentFrequency] + " MH<size=90>Z</size>";
	}

	private void MoveLeft()
	{
		ButtonEffect(LeftButton);
		MoveOffset(-1);
	}

	private void MoveRight()
	{
		ButtonEffect(RightButton);
		MoveOffset(1);
	}

	private void MoveOffset(int offset)
	{
		if (!IsActive) return;
		currentFrequency = Mathf.Clamp(currentFrequency + offset, 0, frequencies.Count - 1);
		DisplayCurrentFrequency();
	}

	private void Transmit()
	{
		ButtonEffect(TransmitButton);
		if (!IsActive || IsSolved) return;
		string submittedFreq = "{0} MHz ({1})".Form(frequencies[currentFrequency], words[currentFrequency]);
		if (currentFrequency == correctFrequency)
		{
			scrollingQueue = false;
			foreach (MarqueeLight marqueeLight in marqueeLights) marqueeLight.Set(false);
			Solve("Submitted " + submittedFreq);
		}
		else Strike("Strike! Submitted " + submittedFreq);
	}

	private void ButtonEffect(KMSelectable selectable)
	{
		ButtonEffect(selectable, 0.4f, KMSoundOverride.SoundEffect.ButtonPress);
	}

	private readonly string TwitchHelpMessage = "!{0} transmit 101.2 | !{0} tx 103.5 mhz | !{0} submit intervention";

	private IEnumerator ProcessTwitchCommand(string command)
	{
		string[] split = command.ToLowerInvariant().Split();
		if (Regex.IsMatch(split[0], "submit|trans|transmit|tx|xmit"))
		{
			int submitIndex = -1;

			string[] stringFrequencies = frequencies.Select(f => f.ToString()).ToArray();
			if (stringFrequencies.Contains(split[1])) submitIndex = Array.IndexOf(stringFrequencies, split[1]);
			else if (words.Contains(split[1])) submitIndex = Array.IndexOf(words.ToArray(), split[1]);

			if (submitIndex > -1)
			{
				yield return null;
				bool moveRight = submitIndex > currentFrequency;
				while (currentFrequency != submitIndex)
				{
					if (moveRight) MoveRight();
					else MoveLeft();
					yield return new WaitForSeconds(0.1f);
				}
				Transmit();
			}
		}
		yield break;
	}

	private IEnumerator TwitchHandleForcedSolve()
	{
		bool moveRight = correctFrequency > currentFrequency;
		while (currentFrequency != correctFrequency)
		{
			if (moveRight) MoveRight();
			else MoveLeft();
			yield return new WaitForSeconds(0.1f);
		}
		Transmit();
	}
}